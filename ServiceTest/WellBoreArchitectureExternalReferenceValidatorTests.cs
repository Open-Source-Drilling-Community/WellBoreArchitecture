using Microsoft.Extensions.Configuration;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service;
using System.Net;
using System.Net.Http.Json;

namespace ServiceTest;

[TestFixture]
public sealed class WellBoreArchitectureExternalReferenceValidatorTests
{
    [Test]
    public async Task Unlinked_draft_is_valid_without_calling_dependency()
    {
        var handler = new StubHandler(_ => throw new AssertionException("No dependency request was expected."));
        var validator = Validator(handler);
        WellBoreArchitectureExternalReferenceValidation result = (await validator.ValidateAsync([Architecture(null)], default)).Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(WellBoreArchitectureExternalReferenceValidationStatus.Valid));
            Assert.That(result.WellBoreExists, Is.Null);
            Assert.That(result.Issues, Is.Empty);
        });
    }

    [TestCase(HttpStatusCode.OK, WellBoreArchitectureExternalReferenceValidationStatus.Valid, true)]
    [TestCase(HttpStatusCode.NotFound, WellBoreArchitectureExternalReferenceValidationStatus.Invalid, false)]
    [TestCase(HttpStatusCode.ServiceUnavailable, WellBoreArchitectureExternalReferenceValidationStatus.Unavailable, null)]
    public async Task Linked_reference_distinguishes_dependency_outcomes(HttpStatusCode status,
        WellBoreArchitectureExternalReferenceValidationStatus expectedStatus, bool? expectedExists)
    {
        Guid wellBoreId = Guid.NewGuid();
        var handler = new StubHandler(_ => new HttpResponseMessage(status)
        {
            Content = status == HttpStatusCode.OK ? JsonContent.Create(new { MetaInfo = new { ID = wellBoreId } }) : null
        });
        WellBoreArchitectureExternalReferenceValidation result =
            (await Validator(handler).ValidateAsync([Architecture(wellBoreId)], default)).Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(expectedStatus));
            Assert.That(result.WellBoreExists, Is.EqualTo(expectedExists));
        });
    }

    private static WellBoreArchitectureExternalReferenceValidator Validator(HttpMessageHandler handler)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["WellBoreHostURL"] = "https://wellbore.test/" }).Build();
        return new WellBoreArchitectureExternalReferenceValidator(new StubClientFactory(handler), configuration);
    }

    private static WellBoreArchitecture Architecture(Guid? wellBoreId) => new()
    {
        MetaInfo = new MetaInfo { ID = Guid.NewGuid() }, WellBoreID = wellBoreId
    };

    private sealed class StubClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(response(request));
    }
}
