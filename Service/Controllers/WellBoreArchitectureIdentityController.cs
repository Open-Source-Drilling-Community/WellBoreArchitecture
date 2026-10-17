using OSDC.Drilling.WellBoreArchitecture.Service;
using Microsoft.AspNetCore.Mvc;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Controllers;

[Produces("application/json"), Route("[controller]"), ApiController]
public class WellBoreArchitectureIdentityController : ControllerBase
{
        private readonly SqlConnectionManager _discoveryConnections;

        /// <summary>Discover WellBoreArchitectureIdentity resources as bounded light records; retrieve selected full records by ID.</summary>
        [HttpGet("LightData", Name = "DiscoverWellBoreArchitectureIdentity")]
        public ActionResult<OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureIdentityDiscoveryPage> DiscoverWellBoreArchitectureIdentity([FromQuery] int offset = 0, [FromQuery] int limit = 50, [FromQuery] string? name = null)
        {
            UsageStatisticsWellBoreArchitecture.Instance.IncrementDiscoveryRequestsPerDay();
            if (offset < 0 || limit < 1 || limit > 200 || name?.Length > 200)
                return BadRequest(new { error = "invalid_discovery_query", message = "Offset must be non-negative, limit 1–200, name at most 200 characters, and UUID filters non-empty." });
            try
            {
                var records = DiscoveryRecords.ReadPage<OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureIdentityLight>(_discoveryConnections.GetConnection(), "WellBoreArchitectureIdentityTable", "WellBoreArchitectureIdentity", offset, limit, name,
                    new System.Collections.Generic.Dictionary<string, object?> {  }, out int total);
                return Ok(new OSDC.Drilling.WellBoreArchitecture.Model.WellBoreArchitectureIdentityDiscoveryPage { Items = records, Offset = offset, Limit = limit, TotalCount = total,
                    NextOffset = offset < total && records.Count < total - offset ? offset + records.Count : null });
            }
            catch (System.Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException or System.Text.Json.JsonException or System.InvalidOperationException)
            {
                return StatusCode(500, new { error = "discovery_failed" });
            }
        }

    private readonly WellBoreArchitectureIdentityManager manager;
    public WellBoreArchitectureIdentityController(SqlConnectionManager connections) { _discoveryConnections = connections; manager = new(connections); }
    [HttpGet(Name = "GetAllWellBoreArchitectureIdentityId")]
    public ActionResult<IEnumerable<Guid>> GetAllIds() => Ok(manager.GetAll().Select(value => value.MetaInfo!.ID));
    [HttpGet("MetaInfo", Name = "GetAllWellBoreArchitectureIdentityMetaInfo")]
    public ActionResult<IEnumerable<MetaInfo?>> GetAllMetaInfo() => Ok(manager.GetAll().Select(value => value.MetaInfo));
    [NonAction]
        public ActionResult<IEnumerable<WellBoreArchitectureIdentity>> GetAll() => Ok(manager.GetAll());
    [HttpGet("{id}", Name = "GetWellBoreArchitectureIdentityById")]
    public ActionResult<WellBoreArchitectureIdentity> Get(Guid id) => manager.Get(id) is { } value ? Ok(value) : NotFound();
    [HttpPost(Name = "PostWellBoreArchitectureIdentity")]
    public ActionResult Post([FromBody] WellBoreArchitectureIdentity? value) => value?.MetaInfo?.ID is Guid id && id != Guid.Empty
        ? manager.Add(value) ? Ok(value) : Conflict() : BadRequest();
    [HttpPut("{id}", Name = "PutWellBoreArchitectureIdentityById")]
    public ActionResult Put(Guid id, [FromQuery] DateTimeOffset expectedModifiedUtc, [FromBody] WellBoreArchitectureIdentity? value)
    {
        WellBoreArchitectureIdentity? current = manager.Get(id);
        if (value?.MetaInfo?.ID != id) return BadRequest();
        if (current == null) return NotFound();
        if (current.LastModificationDate != expectedModifiedUtc) return Conflict(new { error = "stale_write", currentModifiedUtc = current.LastModificationDate });
        return manager.Update(id, value) ? Ok(value) : Conflict();
    }
    [HttpDelete("{id}", Name = "DeleteWellBoreArchitectureIdentityById")]
    public ActionResult Delete(Guid id, [FromQuery] DateTimeOffset expectedModifiedUtc)
    {
        WellBoreArchitectureIdentity? current = manager.Get(id);
        if (current == null) return NotFound();
        if (current.LastModificationDate != expectedModifiedUtc) return Conflict(new { error = "stale_write", currentModifiedUtc = current.LastModificationDate });
        if (manager.IsReferenced(id)) return Conflict(new { error = "catalog_in_use" });
        return manager.Delete(id) ? Ok() : StatusCode(500);
    }
}
