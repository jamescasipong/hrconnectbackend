using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OvertimeController : ControllerBase
    {
        private readonly IOTApplicationServices _oTApplicationServices;
        private readonly IMapper _mapper;
        public OvertimeController(IOTApplicationServices oTApplicationServices, IMapper mapper) 
        {
            _oTApplicationServices = oTApplicationServices;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> RetrieveOTApplication([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            var oTApplications = new List<OTApplication>();

            try
            {
                if (pageIndex == null && pageSize == null)
                {
                    oTApplications = await _oTApplicationServices.GetAllAsync();

                    if (!oTApplications.Any()) return Ok(new ApiResponse<List<OTApplication>>(true, $"Leave applications not found.", oTApplications));

                    return Ok(new ApiResponse<List<OTApplication>>(true, $"Leave applications retrieved successfully", oTApplications));
                }
                else
                {
                    if (pageIndex <= 0)
                    {
                        return BadRequest(new ApiResponse(false, $"Page index must be greater than 0"));
                    }

                    if (pageSize <= 0)
                    {
                        return BadRequest(new ApiResponse(false, $"Page size must be greater than 0"));
                    }

                    oTApplications = await _oTApplicationServices.GetAllAsync();
                    oTApplications = oTApplications.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();

                    if (!oTApplications.Any()) return Ok(new ApiResponse<List<OTApplication>>(true, $"Leave applications not found.", oTApplications));

                    return Ok(new ApiResponse<List<OTApplication>>(true, $"Leave applications retrieved successfully", oTApplications));
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            } 
        }
    }
}
