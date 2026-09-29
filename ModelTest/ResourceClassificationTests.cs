using System.Text.Json;
using System.Text.Json.Nodes;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.ResourceClassification;
using OSDC.Drilling.WellBoreArchitecture.Model;

namespace OSDC.Drilling.WellBoreArchitecture.ModelTest;

public class ResourceClassificationTests
{
    [TestCase(typeof(WellBoreArchitectureIdentity), typeof(IdentityDefinition),
        "{\"MetaInfo\":null,\"Name\":\"Operator code\",\"CreationDate\":null,\"LastModificationDate\":null}")]
    [TestCase(typeof(WellBoreArchitectureIdentityAssignment), typeof(IdentityAssignment),
        "{\"ID\":\"11111111-1111-1111-1111-111111111111\",\"IdentityID\":null,\"Value\":\"A-01\"}")]
    [TestCase(typeof(WellBoreArchitectureFeatureOption), typeof(FeatureOption),
        "{\"ID\":\"22222222-2222-2222-2222-222222222222\",\"Name\":\"Production\"}")]
    [TestCase(typeof(WellBoreArchitectureFeatureAssignment), typeof(FeatureAssignment),
        "{\"ID\":\"33333333-3333-3333-3333-333333333333\",\"FeatureCategoryID\":null,\"FeatureOptionID\":null,\"FromDate\":\"2026-09-29T12:00:00+02:00\",\"ToDate\":null}")]
    [TestCase(typeof(WellBoreArchitectureFeatureCategory), typeof(FeatureCategory<WellBoreArchitectureFeatureOption>),
        "{\"MetaInfo\":null,\"Name\":\"Purpose\",\"IsExclusive\":true,\"HasValidityPeriod\":true,\"Options\":[{\"ID\":\"22222222-2222-2222-2222-222222222222\",\"Name\":\"Production\"}],\"CreationDate\":null,\"LastModificationDate\":null}")]
    public void ExistingStoredJsonRoundTripsThroughSharedImplementation(Type type, Type shared, string json)
    {
        Assert.That(type.BaseType, Is.EqualTo(shared));
        var value = JsonSerializer.Deserialize(json, type);
        var output = JsonSerializer.SerializeToNode(value, type);
        Assert.That(JsonNode.DeepEquals(JsonNode.Parse(json), output), Is.True);
    }

    [Test]
    public void TypedAdapterPreservesOptionsAndNullVersusEmptyCollections()
    {
        var typed = new WellBoreArchitectureFeatureOption { ID = Guid.NewGuid(), Name = "Selected" };
        var other = new FeatureOption { ID = Guid.NewGuid(), Name = "Imported" };
        var category = new WellBoreArchitectureFeatureCategory();
        IFeatureCategory contract = category;
        contract.Options = new List<IFeatureOption> { typed, other };
        Assert.That(category.Options![0], Is.SameAs(typed));
        Assert.That(category.Options[1].ID, Is.EqualTo(other.ID));
        Assert.That(category.Options[1].Name, Is.EqualTo(other.Name));
        contract.Options!.Clear();
        Assert.That(category.Options, Has.Count.EqualTo(2));
        contract.Options = null;
        Assert.That(JsonSerializer.SerializeToNode(category)!["Options"], Is.Null);
        category.Options = [];
        Assert.That(JsonSerializer.SerializeToNode(category)!["Options"]!.AsArray(), Is.Empty);
        Assert.That(new WellBoreArchitectureIdentityAssignment().ID, Is.EqualTo(Guid.Empty));
        Assert.That(new WellBoreArchitectureIdentity().CreationDate, Is.Null);
    }
}
