using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Managers;

public sealed class WellBoreArchitectureIdentityManager
{
    private static readonly string[] Defaults = ["NameForPlanning", "NameForCompanyReporting", "NameForRegulatoryReporting", "Nickname", "NameForOperationReporting"];
    private readonly ArchitectureCatalogStore<Model.WellBoreArchitectureIdentity> store;
    private readonly SqlConnectionManager connections;
    public WellBoreArchitectureIdentityManager(SqlConnectionManager connections)
    {
        this.connections = connections;
        store = new(connections, "WellBoreArchitectureIdentityTable", "WellBoreArchitectureIdentity",
            value => value.MetaInfo, value => value.Name, (value, date) => value.CreationDate = date,
            (value, date) => value.LastModificationDate = date);
    }
    public List<Model.WellBoreArchitectureIdentity> GetAll() { EnsureDefaults(); return store.All(); }
    public Model.WellBoreArchitectureIdentity? Get(Guid id) => store.ById(id);
    public bool Add(Model.WellBoreArchitectureIdentity value) => store.Add(value);
    public bool Update(Guid id, Model.WellBoreArchitectureIdentity value) => store.Update(id, value);
    public bool Delete(Guid id) => !IsReferenced(id) && store.Delete(id);
    public bool IsReferenced(Guid id) => ReadArchitectures().Any(value => value.WellBoreArchitectureIdentityAssignments?.Any(a => a.IdentityID == id) == true);
    private IEnumerable<Model.WellBoreArchitecture> ReadArchitectures()
    {
        using var connection = connections.GetConnection(); using var command = connection!.CreateCommand();
        command.CommandText = "SELECT WellBoreArchitecture FROM WellBoreArchitectureTable";
        using var reader = command.ExecuteReader();
        while (reader.Read()) { var value = System.Text.Json.JsonSerializer.Deserialize<Model.WellBoreArchitecture>(reader.GetString(0), JsonSettings.Options); if (value != null) yield return value; }
    }
    private void EnsureDefaults()
    {
        List<Model.WellBoreArchitectureIdentity> existing = store.All();
        if (existing.Count > 0) return;
        foreach (string name in Defaults)
            store.Add(new() { MetaInfo = new MetaInfo { ID = Guid.NewGuid() }, Name = name });
    }
}
