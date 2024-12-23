using AutoMapper;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.EmployeeDTOs;

namespace hrconnectbackend.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateDepartmentMappings();
            CreateAttendanceMappings();
            CreateEmployeeMappings();
            CreateAuthMappings();
            CreateLeavesApprovalMappings();
            SupervisorMappings();
        }

        private void SupervisorMappings()
        {
            CreateMap<Supervisor, CreateSupervisorDTO>();
            CreateMap<CreateSupervisorDTO, Supervisor>();


        }

        private void CreateLeavesApprovalMappings()
        {
            CreateMap<LeaveApproval, CreateLeavesApprovalDTO>();
            CreateMap<CreateLeavesApprovalDTO, LeaveApproval>();
        }

        private void CreateDepartmentMappings()
        {
            CreateMap<Department, ReadDepartmentDTO>();
            CreateMap<ReadDepartmentDTO, Department>();

            CreateMap<Department, CreateDepartmentDTO>();
            CreateMap<CreateDepartmentDTO, Department>();
        }

        private void CreateAttendanceMappings()
        {
            CreateMap<Attendance, UpdateAttendanceDTO>();
            CreateMap<UpdateAttendanceDTO, Attendance>();

            CreateMap<Attendance, ReadAttendanceDTO>();
            CreateMap<ReadAttendanceDTO, Attendance>();

            CreateMap<Attendance, CreateAttendanceDTO>();
            CreateMap<CreateAttendanceDTO, Attendance>();
        }

        private void CreateEmployeeMappings()
        {
            CreateMap<CreateEmployeeDTO, Employee>();
            CreateMap<Employee, CreateEmployeeDTO>();

            CreateMap<EmployeeInfo, EmployeeInfoDTO>();
            CreateMap<EmployeeInfoDTO, EmployeeInfo>();

            CreateMap<Employee, ReadEmployeeDTO>();
            CreateMap<ReadEmployeeDTO, Employee>();

            CreateMap<Employee, UpdateEmployeeDTO>();
            CreateMap<UpdateEmployeeDTO, Employee>();
        }

        private void CreateAuthMappings()
        {
            CreateMap<AuthDTO, Auth>();
            CreateMap<Auth, AuthDTO>();
            CreateMap<AuthDTO, Auth>();
        }
    }
}
