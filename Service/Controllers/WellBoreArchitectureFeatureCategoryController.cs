using Microsoft.AspNetCore.Mvc;
using OSDC.Drilling.WellBoreArchitecture.Model;
using OSDC.Drilling.WellBoreArchitecture.Service.Managers;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.Drilling.WellBoreArchitecture.Service.Controllers;

[Produces("application/json"), Route("[controller]"), ApiController]
public class WellBoreArchitectureFeatureCategoryController : ControllerBase
{
    private readonly WellBoreArchitectureFeatureCategoryManager manager;
    public WellBoreArchitectureFeatureCategoryController(SqlConnectionManager connections) => manager = new(connections);
    [HttpGet(Name = "GetAllWellBoreArchitectureFeatureCategoryId")]
    public ActionResult<IEnumerable<Guid>> GetAllIds() => Ok(manager.GetAll().Select(value => value.MetaInfo!.ID));
    [HttpGet("MetaInfo", Name = "GetAllWellBoreArchitectureFeatureCategoryMetaInfo")]
    public ActionResult<IEnumerable<MetaInfo?>> GetAllMetaInfo() => Ok(manager.GetAll().Select(value => value.MetaInfo));
    [HttpGet("HeavyData", Name = "GetAllWellBoreArchitectureFeatureCategory")]
    public ActionResult<IEnumerable<WellBoreArchitectureFeatureCategory>> GetAll() => Ok(manager.GetAll());
    [HttpGet("{id}", Name = "GetWellBoreArchitectureFeatureCategoryById")]
    public ActionResult<WellBoreArchitectureFeatureCategory> Get(Guid id) => manager.Get(id) is { } value ? Ok(value) : NotFound();
    [HttpPost(Name = "PostWellBoreArchitectureFeatureCategory")]
    public ActionResult Post([FromBody] WellBoreArchitectureFeatureCategory? value) => value?.MetaInfo?.ID is Guid id && id != Guid.Empty
        ? manager.Add(value) ? Ok(value) : Conflict() : BadRequest();
    [HttpPut("{id}", Name = "PutWellBoreArchitectureFeatureCategoryById")]
    public ActionResult Put(Guid id, [FromQuery] DateTimeOffset expectedModifiedUtc, [FromBody] WellBoreArchitectureFeatureCategory? value)
    {
        WellBoreArchitectureFeatureCategory? current = manager.Get(id);
        if (value?.MetaInfo?.ID != id) return BadRequest();
        if (current == null) return NotFound();
        if (current.LastModificationDate != expectedModifiedUtc) return Conflict(new { error = "stale_write", currentModifiedUtc = current.LastModificationDate });
        return manager.Update(id, value) ? Ok(value) : Conflict(new { error = "catalog_in_use_or_invalid" });
    }
    [HttpDelete("{id}", Name = "DeleteWellBoreArchitectureFeatureCategoryById")]
    public ActionResult Delete(Guid id, [FromQuery] DateTimeOffset expectedModifiedUtc)
    {
        WellBoreArchitectureFeatureCategory? current = manager.Get(id);
        if (current == null) return NotFound();
        if (current.LastModificationDate != expectedModifiedUtc) return Conflict(new { error = "stale_write", currentModifiedUtc = current.LastModificationDate });
        if (manager.IsReferenced(id)) return Conflict(new { error = "catalog_in_use" });
        return manager.Delete(id) ? Ok() : StatusCode(500);
    }
}
