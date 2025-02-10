using AutoMapper;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;


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
            CreateNotificationMappings();
            CreateAttendanceCertification();
            CreateLeaveApplicationMappings();
        }

        private void PayrollMappings()
        {
            
        }

        private void CreateLeaveApplicationMappings()
        {
            CreateMap<ReadLeaveApplicationDTO, LeaveApplication>();
            CreateMap<LeaveApplication, ReadLeaveApplicationDTO>();

            CreateMap<CreateLeaveApplicationDTO, LeaveApplication>();
            CreateMap<LeaveApplication, CreateLeaveApplicationDTO>();

            CreateMap<UpdateLeaveApplicationDTO, LeaveApplication>();
            CreateMap<LeaveApplication,  UpdateLeaveApplicationDTO>();
        }

        private void CreateNotificationMappings()
        {
            CreateMap<Notifications, CreateNotificationDTO>();
            CreateMap<CreateNotificationDTO, Notifications>();

            CreateMap<Notifications, ReadNotificationsDTO>();
            CreateMap<ReadNotificationsDTO, Notifications>();
        }

        private void CreateAttendanceCertification()
        {
            CreateMap<AttendanceCertification, CreateAttendanceCertificationDTO>();
            CreateMap<CreateAttendanceCertificationDTO, AttendanceCertification>();
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

            CreateMap<AboutEmployee, CreateAboutEmployeeDTO>();
            CreateMap<CreateAboutEmployeeDTO, AboutEmployee>();

            CreateMap<Employee, ReadEmployeeDTO>();
            CreateMap<ReadEmployeeDTO, Employee>();

            CreateMap<Employee, UpdateEmployeeDTO>();
            CreateMap<UpdateEmployeeDTO, Employee>();

            CreateMap<EducationBackground, EducationBackgroundDTO>();
            CreateMap<EducationBackgroundDTO, EducationBackground>();

        }

        private void CreateAuthMappings()
        {
            CreateMap<UserAccountDTO, UserAccount>();
            CreateMap<UserAccount, UserAccountDTO>();
            CreateMap<UserAccountDTO, UserAccount>();
        }
    }
}
