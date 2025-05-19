using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Extensions;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/shift")]
    [ApiVersion("1.0")]
    public class ShiftController(
        IShiftServices shiftServices,
        IMapper mapper)
        : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {

            var shift = await shiftServices.GetByIdAsync(id);


            var mappedShift = mapper.Map<ShiftDTO>(shift);
            return Ok(new SuccessResponse<ShiftDTO>(mappedShift, $"Shift with ID {id} retrieved successfully"));

        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllShifts()
        {
            var organizationId = User.RetrieveSpecificUser("organizationId")!;

            if (!int.TryParse(organizationId, out var orgId))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid organization ID");
            }

            var shifts = await shiftServices.GetAllAsync();

            var orgShifts = shifts.Where(a => a.OrganizationId == orgId).ToList();

            return Ok(new SuccessResponse<List<ShiftDTO>>(mapper.Map<List<ShiftDTO>>(orgShifts), "All shifts retrieved successfully"));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateShift(ShiftDTO shiftDTO)
        {

            if (!IsValidDayOfWorked(shiftDTO.DaysOfWorked))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid day of the week");
            }

            var employee = await shiftServices.GetAllAsync();

            bool shiftExisted = employee.Where(a => a.EmployeeShiftId == shiftDTO.EmployeeShiftId && a.DaysOfWorked == shiftDTO.DaysOfWorked).Any();

            if (shiftExisted)
            {
                throw new ConflictException(ErrorCodes.ShiftAlreadyExists, $"Shift already exists for employee with ID {shiftDTO.EmployeeShiftId} on {shiftDTO.DaysOfWorked}");
            }

            var createdShift = await shiftServices.AddAsync(new Shift
            {
                EmployeeShiftId = shiftDTO.EmployeeShiftId,
                DaysOfWorked = BodyRequestCorrection.CapitalLowerCaseName(shiftDTO.DaysOfWorked),
                TimeIn = TimeSpan.Parse(shiftDTO.TimeIn),
                TimeOut = TimeSpan.Parse(shiftDTO.TimeOut)
            });

            return Ok(new SuccessResponse<ShiftDTO>(mapper.Map<ShiftDTO>(createdShift), "Shift created successfully"));

        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] Shift shift)
        {

            var existingShift = await shiftServices.GetByIdAsync(id);

            existingShift.EmployeeShiftId = shift.EmployeeShiftId;
            existingShift.DaysOfWorked = BodyRequestCorrection.CapitalLowerCaseName(shift.DaysOfWorked);
            existingShift.TimeIn = shift.TimeIn;
            existingShift.TimeOut = shift.TimeOut;

            await shiftServices.UpdateAsync(existingShift);

            return Ok(new SuccessResponse<ShiftDTO>(mapper.Map<ShiftDTO>(shift), $"Shift with ID {id} updated successfully"));


        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {

            var shift = await shiftServices.GetByIdAsync(id);

            await shiftServices.DeleteAsync(shift);

            return Ok(new SuccessResponse($"Shift with ID {id} deleted successfully"));

        }
        [Authorize]
        [HttpGet("my-shift")]
        public async Task<IActionResult> GetMyShift()
        {
            var employeeId = int.Parse(User.RetrieveSpecificUser(ClaimTypes.NameIdentifier)!);
            var organizationId = User.RetrieveSpecificUser("organizationId")!;

            if (!int.TryParse(organizationId, out var orgId))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid organization ID");
            }

            var shifts = await shiftServices.GetEmployeeShifts(employeeId, orgId);

            var mappedShift = mapper.Map<List<ShiftDTO>>(shifts);

            return Ok(new SuccessResponse<List<ShiftDTO>>(mappedShift, $"Shifts for employee with ID {employeeId} retrieved successfully"));

        }


        [Authorize(Roles = "Admin")]
        [HttpGet("employee-shift/{employeeId}")]
        public async Task<IActionResult> GetEmployeeShift(int employeeId)
        {
            var organizationId = User.RetrieveSpecificUser("organizationId")!;

            if (!int.TryParse(organizationId, out var orgId))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid organization ID");
            }

            var shifts = await shiftServices.GetEmployeeShifts(employeeId, orgId);

            var mappedShift = mapper.Map<List<ShiftDTO>>(shifts);

            return Ok(new SuccessResponse<List<ShiftDTO>>(mappedShift, $"Shifts for employee with ID {employeeId} retrieved successfully"));

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("shift-today/{employeeId}")]
        public async Task<IActionResult> HasShiftToday(int employeeId)
        {

            var hasShift = await shiftServices.HasShiftToday(employeeId);
            return Ok(new SuccessResponse<bool>(hasShift, $"User with ID {employeeId} has shift today"));

        }

        [Authorize]
        [HttpGet("shift-today")]
        public async Task<IActionResult> HasShiftToday()
        {
            var user = User.RetrieveSpecificUser("EmployeeId")!;

            if (int.TryParse(user, out var employeeId))
            {
                var hasShift = await shiftServices.HasShiftToday(employeeId);
                return Ok(new SuccessResponse<bool>(hasShift, $"User with ID {employeeId} has shift today"));
            }
            else
            {
                throw new UnauthorizedException(ErrorCodes.InvalidRequestModel, "Invalid employee ID");
            }

        }

        private bool IsValidDayOfWorked(string name)
        {
            List<string> daysOfWorked = new List<string>
        {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
        };

            return daysOfWorked.Any(a => a.ToLower() == name.ToLower());
        }
    }

}
