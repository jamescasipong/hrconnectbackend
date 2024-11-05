using AutoMapper;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Helper
{
    public class MappingProfile: Profile
    {
        public MappingProfile() {
            CreateMap<EmployeeDTO, Employee>();
            CreateMap<Employee, EmployeeDTO>();
        }
    }
}
