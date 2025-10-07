using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.WellBoreArchitecture.Service.Managers;

namespace NORCE.Drilling.WellBoreArchitecture.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class WellBoreArchitectureController : ControllerBase
    {
        private readonly ILogger<WellBoreArchitectureManager> _logger;
        private readonly WellBoreArchitectureManager _wellBoreArchitectureManager;

        public WellBoreArchitectureController(ILogger<WellBoreArchitectureManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _wellBoreArchitectureManager = WellBoreArchitectureManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all WellBoreArchitecture present in the microservice database at endpoint WellBoreArchitecture/api/WellBoreArchitecture
        /// </summary>
        /// <returns>the list of Guid of all WellBoreArchitecture present in the microservice database at endpoint WellBoreArchitecture/api/WellBoreArchitecture</returns>
        [HttpGet(Name = "GetAllWellBoreArchitectureId")]
        public ActionResult<IEnumerable<Guid>> GetAllWellBoreArchitectureId()
        {
            var ids = _wellBoreArchitectureManager.GetAllWellBoreArchitectureId();
            if (ids != null)
            {
                return Ok(ids);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the list of MetaInfo of all WellBoreArchitecture present in the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all WellBoreArchitecture present in the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllWellBoreArchitectureMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllWellBoreArchitectureMetaInfo()
        {
            var vals = _wellBoreArchitectureManager.GetAllWellBoreArchitectureMetaInfo();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the WellBoreArchitecture identified by its Guid from the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the WellBoreArchitecture identified by its Guid from the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetWellBoreArchitectureById")]
        public ActionResult<Model.WellBoreArchitecture?> GetWellBoreArchitectureById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _wellBoreArchitectureManager.GetWellBoreArchitectureById(id);
                if (val != null)
                {
                    return Ok(val);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns the list of all WellBoreArchitectureLight present in the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/LightData
        /// </summary>
        /// <returns>the list of all WellBoreArchitectureLight present in the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/LightData</returns>
        [HttpGet("LightData", Name = "GetAllWellBoreArchitectureLight")]
        public ActionResult<IEnumerable<Model.WellBoreArchitectureLight>> GetAllWellBoreArchitectureLight()
        {
            var vals = _wellBoreArchitectureManager.GetAllWellBoreArchitectureLight();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the list of all WellBoreArchitecture present in the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/HeavyData
        /// </summary>
        /// <returns>the list of all WellBoreArchitecture present in the microservice database, at endpoint WellBoreArchitecture/api/WellBoreArchitecture/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllWellBoreArchitecture")]
        public ActionResult<IEnumerable<Model.WellBoreArchitecture?>> GetAllWellBoreArchitecture()
        {
            var vals = _wellBoreArchitectureManager.GetAllWellBoreArchitecture();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Performs calculation on the given WellBoreArchitecture and adds it to the microservice database, at the endpoint WellBoreArchitecture/api/WellBoreArchitecture
        /// </summary>
        /// <param name="wellBoreArchitecture"></param>
        /// <returns>true if the given WellBoreArchitecture has been added successfully to the microservice database, at the endpoint WellBoreArchitecture/api/WellBoreArchitecture</returns>
        [HttpPost(Name = "PostWellBoreArchitecture")]
        public ActionResult PostWellBoreArchitecture([FromBody] Model.WellBoreArchitecture? data)
        {
            // Check if wellBoreArchitecture exists in the database through ID
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _wellBoreArchitectureManager.GetWellBoreArchitectureById(data.MetaInfo.ID);
                if (existingData == null)
                {   
                    //  If wellBoreArchitecture was not found, call AddWellBoreArchitecture, where the wellBoreArchitecture.Calculate()
                    // method is called. 
                    if (_wellBoreArchitectureManager.AddWellBoreArchitecture(data))
                    {
                        return Ok(); // status=OK is used rather than status=Created because NSwag auto-generated controllers use 200 (OK) rather than 201 (Created) as return codes
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
                else
                {
                    _logger.LogWarning("The given WellBoreArchitecture already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given WellBoreArchitecture is null, badly formed, or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given WellBoreArchitecture and updates it in the microservice database, at the endpoint WellBoreArchitecture/api/WellBoreArchitecture/id
        /// </summary>
        /// <param name="wellBoreArchitecture"></param>
        /// <returns>true if the given WellBoreArchitecture has been updated successfully to the microservice database, at the endpoint WellBoreArchitecture/api/WellBoreArchitecture/id</returns>
        [HttpPut("{id}", Name = "PutWellBoreArchitectureById")]
        public ActionResult PutWellBoreArchitectureById(Guid id, [FromBody] Model.WellBoreArchitecture? data)
        {
            // Check if WellBoreArchitecture is in the data base
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _wellBoreArchitectureManager.GetWellBoreArchitectureById(id);
                if (existingData != null)
                {
                    if (_wellBoreArchitectureManager.UpdateWellBoreArchitectureById(id, data))
                    {
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
                else
                {
                    _logger.LogWarning("The given WellBoreArchitecture has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given WellBoreArchitecture is null, badly formed, or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the WellBoreArchitecture of given ID from the microservice database, at the endpoint WellBoreArchitecture/api/WellBoreArchitecture/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the WellBoreArchitecture was deleted from the microservice database, at the endpoint WellBoreArchitecture/api/WellBoreArchitecture/id</returns>
        [HttpDelete("{id}", Name = "DeleteWellBoreArchitectureById")]
        public ActionResult DeleteWellBoreArchitectureById(Guid id)
        {
            if (_wellBoreArchitectureManager.GetWellBoreArchitectureById(id) != null)
            {
                if (_wellBoreArchitectureManager.DeleteWellBoreArchitectureById(id))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                _logger.LogWarning("The WellBoreArchitecture of given ID does not exist");
                return NotFound();
            }
        }
    }
}
