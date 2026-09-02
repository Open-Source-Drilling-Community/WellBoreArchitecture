using Microsoft.Extensions.Configuration;
using OSDC.Drilling.WellBoreArchitecture.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ArchitectureModel = OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitecture;

namespace OSDC.Drilling.WellBoreArchitecture.Service;

public interface IWellBoreArchitectureExternalReferenceValidator
{
    Task<IReadOnlyList<WellBoreArchitectureExternalReferenceValidation>> ValidateAsync(
        IReadOnlyCollection<ArchitectureModel> architectures, CancellationToken cancellationToken);
}

internal sealed class UnavailableWellBoreArchitectureExternalReferenceValidator : IWellBoreArchitectureExternalReferenceValidator
{
    public Task<IReadOnlyList<WellBoreArchitectureExternalReferenceValidation>> ValidateAsync(
        IReadOnlyCollection<ArchitectureModel> architectures, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset checkedAt = DateTimeOffset.UtcNow;
        return Task.FromResult<IReadOnlyList<WellBoreArchitectureExternalReferenceValidation>>(architectures.Select(value => new WellBoreArchitectureExternalReferenceValidation
        {
            WellBoreArchitectureID = value.MetaInfo?.ID ?? Guid.Empty,
            WellBoreID = value.WellBoreID,
            CheckedAtUtc = checkedAt,
            Status = value.WellBoreID == null ? WellBoreArchitectureExternalReferenceValidationStatus.Valid : WellBoreArchitectureExternalReferenceValidationStatus.Unavailable,
            Issues = value.WellBoreID == null ? [] : [new()
            {
                Property = "WellBoreID", Code = "external_reference_validation_unavailable",
                Message = "WellBore reference validation is unavailable in this host."
            }]
        }).ToList());
    }
}

/// <summary>Reads WellBore resources for diagnostics only; it never participates in architecture writes.</summary>
public sealed class WellBoreArchitectureExternalReferenceValidator : IWellBoreArchitectureExternalReferenceValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IHttpClientFactory _clients;
    private readonly IConfiguration _configuration;

    public WellBoreArchitectureExternalReferenceValidator(IHttpClientFactory clients, IConfiguration configuration)
    {
        _clients = clients;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<WellBoreArchitectureExternalReferenceValidation>> ValidateAsync(
        IReadOnlyCollection<ArchitectureModel> architectures, CancellationToken cancellationToken)
    {
        DateTimeOffset checkedAt = DateTimeOffset.UtcNow;
        var resolutions = new Dictionary<Guid, ReferenceResolution>();
        foreach (Guid id in architectures.Where(value => value.WellBoreID is Guid candidate && candidate != Guid.Empty)
                     .Select(value => value.WellBoreID!.Value).Distinct())
            resolutions[id] = await ReadAsync(id, cancellationToken);
        return architectures.Select(value => Validate(value, checkedAt, resolutions)).ToList();
    }

    private async Task<ReferenceResolution> ReadAsync(Guid id, CancellationToken cancellationToken)
    {
        string? host = _configuration["WellBoreHostURL"];
        if (string.IsNullOrWhiteSpace(host))
            return ReferenceResolution.Unavailable("well_bore_service_not_configured", "WellBoreHostURL is not configured.");
        try
        {
            using HttpClient client = _clients.CreateClient(nameof(WellBoreArchitectureExternalReferenceValidator));
            client.BaseAddress = new Uri(host.EndsWith('/') ? host : host + "/");
            using HttpResponseMessage response = await client.GetAsync($"WellBore/api/WellBore/{id:D}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound) return ReferenceResolution.NotFound();
            if (!response.IsSuccessStatusCode)
                return ReferenceResolution.Unavailable("well_bore_service_error", $"WellBore service returned HTTP {(int)response.StatusCode}.");
            ExternalResourceDto? resource = await response.Content.ReadFromJsonAsync<ExternalResourceDto>(JsonOptions, cancellationToken);
            return resource?.MetaInfo?.ID == id ? ReferenceResolution.Found() :
                ReferenceResolution.Unavailable("well_bore_response_invalid", "WellBore service returned a malformed or mismatched resource.");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException or TaskCanceledException)
        {
            return ReferenceResolution.Unavailable("well_bore_service_unavailable", "WellBore reference validation is temporarily unavailable.");
        }
    }

    private static WellBoreArchitectureExternalReferenceValidation Validate(ArchitectureModel architecture, DateTimeOffset checkedAt,
        IReadOnlyDictionary<Guid, ReferenceResolution> resolutions)
    {
        var result = new WellBoreArchitectureExternalReferenceValidation
        {
            WellBoreArchitectureID = architecture.MetaInfo?.ID ?? Guid.Empty,
            WellBoreID = architecture.WellBoreID,
            CheckedAtUtc = checkedAt,
            Status = WellBoreArchitectureExternalReferenceValidationStatus.Valid
        };
        if (architecture.WellBoreID == null) return result;
        if (architecture.WellBoreID == Guid.Empty)
        {
            result.Status = WellBoreArchitectureExternalReferenceValidationStatus.Invalid;
            result.Issues.Add(new() { Property = "WellBoreID", Code = "empty_uuid", Message = "WellBoreID is empty." });
            return result;
        }
        if (!resolutions.TryGetValue(architecture.WellBoreID.Value, out ReferenceResolution? resolution) || resolution.IsUnavailable)
        {
            result.Status = WellBoreArchitectureExternalReferenceValidationStatus.Unavailable;
            result.Issues.Add(new() { Property = "WellBoreID", Code = resolution?.Code ?? "well_bore_service_unavailable",
                Message = resolution?.Message ?? "WellBore reference validation is unavailable." });
        }
        else
        {
            result.WellBoreExists = resolution.Exists;
            if (!resolution.Exists)
            {
                result.Status = WellBoreArchitectureExternalReferenceValidationStatus.Invalid;
                result.Issues.Add(new() { Property = "WellBoreID", Code = "well_bore_not_found",
                    Message = $"WellBore UUID '{architecture.WellBoreID}' does not exist." });
            }
        }
        return result;
    }

    private sealed class ExternalResourceDto { public MetaInfoDto? MetaInfo { get; set; } }
    private sealed class MetaInfoDto { public Guid ID { get; set; } }
    private sealed record ReferenceResolution(bool Exists, bool IsUnavailable, string? Code, string? Message)
    {
        public static ReferenceResolution Found() => new(true, false, null, null);
        public static ReferenceResolution NotFound() => new(false, false, null, null);
        public static ReferenceResolution Unavailable(string code, string message) => new(false, true, code, message);
    }
}
