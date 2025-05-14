using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Constants;
using hrconnectbackend.Extensions;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Services.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationServices organizationServices, IUserAccountServices userAccountServices, IAuthService authServices, ILogger<OrganizationController> logger, IMapper mapper) : ControllerBase
    {
        // [UserRole("Admin")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDTO organization)
        {
            var validate = TryValidateModel(organization);

            if (!validate) return BadRequest(ModelState);

            var userId = User.RetrieveSpecificUser(ClaimTypes.NameIdentifier);

            var userIdInt = TypeConverter.StringToInt(userId);

            var newOrg = new Organization
            {
                Name = organization.OrgName,
                Address = organization.Address,
                ContactEmail = organization.ContactEmail,
                Zipcode = organization.Zipcode,
                City = organization.City,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdOrganization = await organizationServices.CreateOrganization(userIdInt, newOrg);

            var user = await userAccountServices.GetByIdAsync(userIdInt);

            if (user == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, $"User not found with id: {userIdInt}"));
            }

            AuthResponse authResponse = await authServices.GenerateTokens(user);
            authServices.SetAccessTokenCookie(authResponse, Response);

            return Ok(new ApiResponse<Organization>(true, $"Account created successfully", createdOrganization));


        }

        [UserRole("Admin,Operator")]
        [HttpPatch("{organizationId}")]
        public async Task<IActionResult> Patch(int organizationId, [FromBody] JsonPatchDocument<Organization> patch)
        {


            // Call the service to apply the patch to the Organization entity
            var (original, patched, isValid) = await organizationServices.ApplyPatchAsync(organizationId, patch);

            // If the organization was not found or if the patch is invalid, return BadRequest
            if (original == null || !isValid || patched == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.OrganizationNotFound, $"Organization with id {organizationId} not found"));
            }


            // Perform additional validation on the patched entity (in the controller)
            if (string.IsNullOrWhiteSpace(patched.Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
            }

            if (string.IsNullOrWhiteSpace(patched.Address))
            {
                ModelState.AddModelError("Address", "Address is required.");
            }

            if (organizationServices.IsNameDuplicate(patched.Name, organizationId))
            {
                ModelState.AddModelError("Name", "An organization with the same name already exists.");
            }

            // If ModelState is invalid, return a BadRequest with validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            // Save changes to the database
            await organizationServices.SaveChangesAsync();

            // Return both the original and patched models
            var model = new
            {
                original,
                patched
            };

            return Ok(new ApiResponse<object>(true, $"Organization with id {organizationId} updated successfully", model));
        }

        [HttpGet("{organizationId}/storage-usage")]
        public async Task<IActionResult> GetStorageUsed(int organizationId)
        {

            long storageUsedBytes = await organizationServices.GetStorageUsedByOrganizationAsync(organizationId);
            var storageUsedMb = storageUsedBytes / (1024 * 1024);  // Convert bytes to MB
            return Ok(new { StorageUsedMB = storageUsedMb });

        }

        [Authorize]
        [HttpGet("my-organization")]
        public async Task<IActionResult> GetMyOrganization()
        {
            var organization = User.RetrieveSpecificUser("OrganizationId");

            var organizationId = TypeConverter.StringToInt(organization);

            var org = await organizationServices.GetByIdAsync(organizationId);

            var mappedOrg = mapper.Map<OrganizationsDto>(org);

            return Ok(new ApiResponse<OrganizationsDto>(true, $"Organization found", mappedOrg));
        }

        [UserRole("Operator")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizations()
        {
            var orgs = await organizationServices.GetAllAsync();

            var mappedOrgs = mapper.Map<List<OrganizationsDto>>(orgs);

            return Ok(new ApiResponse<List<OrganizationsDto>>(true, $"Organizations found", mappedOrgs));
        }

        [UserRole("Operator")]
        [HttpDelete("{organizationId}")]
        public async Task<IActionResult> DeleteOrganization(int organizationId)
        {
            var org = await organizationServices.GetByIdAsync(organizationId);

            if (org == null) return NotFound(new ErrorResponse(ErrorCodes.OrganizationNotFound, $"Organization with id {organizationId} not found"));

            await organizationServices.DeleteAsync(org);

            return Ok(new ApiResponse<Organization>(true, $"Organization with id {organizationId} deleted successfully"));
        }
    }
}
