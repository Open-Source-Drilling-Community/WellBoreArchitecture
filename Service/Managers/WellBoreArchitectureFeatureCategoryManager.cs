using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Managers;

public sealed class WellBoreArchitectureFeatureCategoryManager
{
    private static readonly (string Name, bool Exclusive, bool Validity, string[] Options)[] Defaults =
    [
        ("Lifecycle", true, true, ["Planned", "UnderConstruction", "Operational", "Suspended", "Completed", "PluggedBack", "Abandoned", "Decommissioned"]),
        ("ApprovalStatus", true, true, ["Proposed", "UnderReview", "Accepted", "Rejected", "Cancelled", "Superseded"]),
        ("SectionRole", false, false, ["TopHole", "SurfaceSection", "IntermediateSection", "ProductionSection", "ReservoirSection", "HorizontalDrainSection", "PilotSection", "ReliefSection"]),
        ("DrillingMethod", false, true, ["Conventional", "MPD", "Underbalanced", "CasingWhileDrilling", "CoiledTubingDrilling", "Geosteered"])
    ];
    private readonly ArchitectureCatalogStore<Model.WellBoreArchitectureFeatureCategory> store;
    private readonly SqlConnectionManager connections;
    public WellBoreArchitectureFeatureCategoryManager(SqlConnectionManager connections)
    {
        this.connections = connections;
        store = new(connections, "WellBoreArchitectureFeatureCategoryTable", "WellBoreArchitectureFeatureCategory",
            value => value.MetaInfo, value => value.Name, (value, date) => value.CreationDate = date,
            (value, date) => value.LastModificationDate = date, value => value.IsExclusive, value => value.HasValidityPeriod);
    }
    public List<Model.WellBoreArchitectureFeatureCategory> GetAll() { EnsureDefaults(); return store.All(); }
    public Model.WellBoreArchitectureFeatureCategory? Get(Guid id) => store.ById(id);
    public bool Add(Model.WellBoreArchitectureFeatureCategory value) { Prepare(value); return store.Add(value); }
    public bool Update(Guid id, Model.WellBoreArchitectureFeatureCategory value) { Prepare(value); return !RemovesReferencedOptions(id, value) && store.Update(id, value); }
    public bool Delete(Guid id) => !IsReferenced(id) && store.Delete(id);
    public bool IsReferenced(Guid id) => ReadArchitectures().Any(value => value.WellBoreArchitectureFeatureAssignments?.Any(a => a.FeatureCategoryID == id) == true);
    private bool RemovesReferencedOptions(Guid id, Model.WellBoreArchitectureFeatureCategory value)
    {
        HashSet<Guid> retained = (value.Options ?? []).Select(option => option.ID).ToHashSet();
        return ReadArchitectures().Any(a => a.WellBoreArchitectureFeatureAssignments?.Any(x => x.FeatureCategoryID == id && x.FeatureOptionID is Guid option && !retained.Contains(option)) == true);
    }
    private IEnumerable<Model.WellBoreArchitecture> ReadArchitectures()
    {
        using var connection = connections.GetConnection(); using var command = connection!.CreateCommand(); command.CommandText = "SELECT WellBoreArchitecture FROM WellBoreArchitectureTable";
        using var reader = command.ExecuteReader(); while (reader.Read()) { var value = System.Text.Json.JsonSerializer.Deserialize<Model.WellBoreArchitecture>(reader.GetString(0), JsonSettings.Options); if (value != null) yield return value; }
    }
    private void EnsureDefaults()
    {
        List<Model.WellBoreArchitectureFeatureCategory> existing = store.All();
        if (existing.Count > 0) return;
        foreach (var item in Defaults)
            Add(new() { MetaInfo = new MetaInfo { ID = Guid.NewGuid() }, Name = item.Name, IsExclusive = item.Exclusive, HasValidityPeriod = item.Validity,
                Options = item.Options.Select(name => new Model.WellBoreArchitectureFeatureOption { ID = Guid.NewGuid(), Name = name }).ToList() });
    }
    private static void Prepare(Model.WellBoreArchitectureFeatureCategory value)
    {
        value.Options ??= []; foreach (var option in value.Options) if (option.ID == Guid.Empty) option.ID = Guid.NewGuid();
    }
}
