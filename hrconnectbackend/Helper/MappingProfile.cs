using AutoMapper;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.Requests;


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
            CreateOtApplication();
            Shift();

        }

        private void Shift(){
            CreateMap<Shift, ShiftDTO>();
            CreateMap<ShiftDTO, Shift>();
        }

        private void CreateOtApplication()
        {
            CreateMap<CreateOtApplicationDto, OtApplication>();
            CreateMap<OtApplication, CreateOtApplicationDto>();

            CreateMap<ReadOtApplicationDto, OtApplication>();
            CreateMap<OtApplication, ReadOtApplicationDto>();

            CreateMap<UpdateOtApplicationDto, OtApplication>();
            CreateMap<OtApplication, UpdateOtApplicationDto>();
        }

        private void CreateLeaveApplicationMappings()
        {
            CreateMap<ReadLeaveApplicationDto, LeaveApplication>();
            CreateMap<LeaveApplication, ReadLeaveApplicationDto>();

            CreateMap<CreateLeaveApplicationDto, LeaveApplication>();
            CreateMap<LeaveApplication, CreateLeaveApplicationDto>();

            CreateMap<UpdateLeaveApplicationDto, LeaveApplication>();
            CreateMap<LeaveApplication,  UpdateLeaveApplicationDto>();
        }

        private void CreateNotificationMappings()
        {
            CreateMap<Notifications, CreateNotificationDto>();
            CreateMap<CreateNotificationDto, Notifications>();

            CreateMap<Notifications, ReadNotificationsDto>();
            CreateMap<ReadNotificationsDto, Notifications>();

            CreateMap<UserNotification, ReadUserNotificationDto>();
            CreateMap<ReadUserNotificationDto, UserNotification>();


            CreateMap<UserNotification, CreateUserNotificationDto>();
            CreateMap<CreateUserNotificationDto, UserNotification>();
        }

        private void CreateAttendanceCertification()
        {
            CreateMap<AttendanceCertification, CreateAttendanceCertificationDto>();
            CreateMap<CreateAttendanceCertificationDto, AttendanceCertification>();
        }
        private void CreateDepartmentMappings()
        {
            CreateMap<Department, ReadDepartmentDto>();
            CreateMap<ReadDepartmentDto, Department>();

            CreateMap<Department, CreateDepartmentDto>();
            CreateMap<CreateDepartmentDto, Department>();
        }

        private void CreateAttendanceMappings()
        {
            CreateMap<Attendance, UpdateAttendanceDto>();
            CreateMap<UpdateAttendanceDto, Attendance>();

            CreateMap<Attendance, ReadAttendanceDto>();
            CreateMap<ReadAttendanceDto, Attendance>();

            CreateMap<Attendance, CreateAttendanceDto>();
            CreateMap<CreateAttendanceDto, Attendance>();
        }

        private void CreateEmployeeMappings()
        {
            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<Employee, CreateEmployeeDto>();

            CreateMap<AboutEmployee, CreateAboutEmployeeDto>();
            CreateMap<CreateAboutEmployeeDto, AboutEmployee>();

            CreateMap<AboutEmployee, ReadAboutEmployeeDto>();
            CreateMap<ReadAboutEmployeeDto, AboutEmployee>();

            CreateMap<Employee, ReadEmployeeDto>();
            CreateMap<ReadEmployeeDto, Employee>();

            CreateMap<Employee, UpdateEmployeeDto>();
            CreateMap<UpdateEmployeeDto, Employee>();

            CreateMap<EducationBackground, EducationBackgroundDto>();
            CreateMap<EducationBackgroundDto, EducationBackground>();

        }

        private void CreateAuthMappings()
        {
            CreateMap<UserAccountDto, UserAccount>();
            CreateMap<UserAccount, UserAccountDto>();
            CreateMap<UserAccountDto, UserAccount>();
        }
    }
}
