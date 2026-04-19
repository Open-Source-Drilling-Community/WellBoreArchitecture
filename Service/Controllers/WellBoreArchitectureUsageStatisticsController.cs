using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.WellBoreArchitecture.Model;

namespace NORCE.Drilling.WellBoreArchitecture.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class WellBoreArchitectureUsageStatisticsController : ControllerBase
    {
        private readonly ILogger _logger;

        public WellBoreArchitectureUsageStatisticsController(ILogger<WellBoreArchitectureUsageStatisticsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the usage statistics present in the microservice database at endpoint WellBoreArchitecture/api/WellBoreArchitectureUsageStatistics
        /// </summary>
        [HttpGet(Name = "GetWellBoreArchitectureUsageStatistics")]
        public ActionResult<UsageStatisticsWellBoreArchitecture> GetWellBoreArchitectureUsageStatistics()
        {
            if (UsageStatisticsWellBoreArchitecture.Instance != null)
            {
                return Ok(UsageStatisticsWellBoreArchitecture.Instance);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
