namespace hrconnectbackend.Constants
{
    public class ErrorCodes
    {
        // Authorization Errors
        public const string Unauthorized = "UNAUTHORIZED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string TokenExpired = "TOKEN_EXPIRED";
        public const string AccessDenied = "ACCESS_DENIED";

        // User Errors
        public const string UserNotFound = "USER_NOT_FOUND";
        public const string UserAlreadyExists = "USER_ALREADY_EXISTS";

        // Employee Errors
        public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";
        public const string InvalidEmployeeData = "INVALID_EMPLOYEE_DATA";
        public const string EmployeeAlreadyHasAccount = "EMPLOYEE_ALREADY_HAS_ACCOUNT";
        public const string EmployeeInactive = "EMPLOYEE_INACTIVE";
        public const string EmployeeAlreadyExists = "EMPLOYEE_ALREADY_EXISTS";
        public const string EmployeeNotAssignedToDepartment = "EMPLOYEE_NOT_ASSIGNED_TO_DEPARTMENT";
        public const string EmployeeAlreadyAssignedToDepartment = "EMPLOYEE_ALREADY_ASSIGNED_TO_DEPARTMENT";
        public const string EmployeeNoShift = "EMPLOYEE_NO_SHIFT";

        // Attendance Errors
        public const string AlreadyClockedIn = "ALREADY_CLOCKED_IN";
        public const string AttendanceNotFound = "ATTENDANCE_NOT_FOUND";
        public const string InvalidAttendanceData = "INVALID_ATTENDANCE_DATA";

        // Leave Errors
        public const string LeaveNotFound = "LEAVE_NOT_FOUND";
        public const string LeaveAlreadyApplied = "LEAVE_ALREADY_APPLIED";
        public const string LeaveNotApproved = "LEAVE_NOT_APPROVED";
        public const string LeaveAlreadyCancelled = "LEAVE_ALREADY_CANCELLED";
        public const string LeaveAlreadyRejected = "LEAVE_ALREADY_REJECTED";
        public const string LeaveQuotaExceeded = "LEAVE_QUOTA_EXCEEDED";

        // Notification Errors
        public const string NotificationNotFound = "NOTIFICATION_NOT_FOUND";

        // Department Errors
        public const string DepartmentNotFound = "DEPARTMENT_NOT_FOUND";
        public const string DepartmentAlreadyExists = "DEPARTMENT_ALREADY_EXISTS";
        public const string InvalidDepartmentData = "INVALID_DEPARTMENT_DATA";

        // Validation Errors
        public const string DuplicateEmail = "DUPLICATE_EMAIL";
        public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
        public const string InvalidPhoneNumber = "INVALID_PHONE_NUMBER";
        public const string InvalidPhoneNumberFormat = "INVALID_PHONE_NUMBER_FORMAT";
        public const string InvalidPhoneNumberLength = "INVALID_PHONE_NUMBER_LENGTH";
        public const string InvalidPhoneNumberCountryCode = "INVALID_PHONE_NUMBER_COUNTRY_CODE";
        public const string InvalidPhoneNumberRegionCode = "INVALID_PHONE_NUMBER_REGION_CODE";
        public const string InvalidPhoneNumberType = "INVALID_PHONE_NUMBER_TYPE";

        // Data Errors
        public const string InvalidLeaveData = "INVALID_LEAVE_DATA";
        public const string InvalidRoleData = "INVALID_ROLE_DATA";
        public const string InvalidSalaryData = "INVALID_SALARY_DATA";
        public const string InvalidDocumentData = "INVALID_DOCUMENT_DATA";
        public const string InvalidDocumentType = "INVALID_DOCUMENT_TYPE";
        public const string InvalidDocumentFormat = "INVALID_DOCUMENT_FORMAT";

        // System Errors
        public const string DatabaseError = "DATABASE_ERROR";
        public const string ExternalServiceFailed = "EXTERNAL_SERVICE_FAILED";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";

        // Business Logic Errors
        public const string BusinessLogicError = "BUSINESS_LOGIC_ERROR";
        public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";

        // Miscellaneous Errors
        public const string UnknownError = "UNKNOWN_ERROR";
        public const string InvalidInput = "INVALID_INPUT";


    }
}
