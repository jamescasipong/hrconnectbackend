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

            CreateMap<Employee, UpdateEmployeeDTO>();
            CreateMap<UpdateEmployeeDTO, Employee>();

            CreateMap<AuthDTO, Auth>();
            CreateMap<Auth, AuthDTO>();
            CreateMap<AuthDTO, Auth>();

        }
    }
}
