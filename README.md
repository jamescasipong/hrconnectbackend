
# HRConnect API Documentation

## Overview

The HRConnect API provides a robust system to manage employee data, attendance, payroll, leave applications, and more. This API is built using C# and MongoDB for the backend database management. The core object model revolves around employees, departments, supervisors, and various HR-related activities.

This README provides an overview of the `Employee` model in the HRConnect API, explaining its structure, relationships, and usage.

---

## Employee Model

The `Employee` class represents an employee within the HRConnect system. It includes various properties for managing employee details, including personal information, employment status, and relationships to other entities like departments, supervisors, attendance, and payroll.

### Properties

- **Id** (int): Unique identifier for each employee.
- **Name** (string): Full name of the employee.
- **Email** (string): Email address of the employee.
- **Password** (string): Encrypted password for authentication.
- **IsAdmin** (bool): Boolean indicating whether the employee has admin privileges.
- **Status** (enum `Status`): The current status of the employee. Possible values:
  - `offline` (0): Employee is offline.
  - `online` (1): Employee is online.
- **CreatedAt** (DateOnly): The date the employee was created in the system.
- **UpdatedAt** (DateOnly): The date the employee record was last updated.
- **SupervisorId** (int?): The `Id` of the employee's supervisor (foreign key).
- **DepartmentId** (int?): The `Id` of the department the employee belongs to.
- **Supervisor** (Employee?): The `Employee` object of the employee's supervisor.
- **Department** (Department?): The `Department` object the employee is assigned to.

### Relationships

The `Employee` class also contains relationships to other models in the HRConnect system:

- **Attendance** (List<Attendance>): A list of attendance records associated with the employee.
- **OTApplication** (List<OTApplication>): A list of overtime applications submitted by the employee.
- **OTApproval** (List<OTApproval>): A list of overtime approvals for the employee.
- **LeaveApplication** (List<LeaveApplication>): A list of leave applications submitted by the employee.
- **LeaveApproval** (List<LeaveApproval>): A list of leave approvals for the employee.
- **EmployeeInfo** (EmployeeInfo): Additional detailed information about the employee.
- **Shift** (Shift): The work shift assigned to the employee.
- **Auth** (Auth): Authentication details for the employee (e.g., login credentials).
- **Payroll** (List<Payroll>): A list of payroll records associated with the employee.

### Enum: Status

```csharp
public enum Status
{
    offline = 0,
    online = 1
}
```

The `Status` enum is used to track whether the employee is currently online or offline. This is useful for real-time systems where employee activity needs to be monitored.

---

## Example Usage

Below is an example of how to instantiate an `Employee` object and associate it with a department and supervisor:

```csharp
var employee = new Employee
{
    Id = 1,
    Name = "John Doe",
    Email = "john.doe@example.com",
    Password = "hashed_password",
    IsAdmin = false,
    Status = Employee.Status.online,
    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
    UpdatedAt = DateOnly.FromDateTime(DateTime.Now),
    DepartmentId = 2,
    SupervisorId = 3
};
```

In this example:
- The employee is created with an ID, name, email, and a hashed password.
- The employee is assigned to department 2 and has a supervisor with ID 3.

---

## API Endpoints

### GET /employees

Fetch all employees in the system.

- **Response**: List of `Employee` objects.

### GET /employees/{id}

Fetch a specific employee by ID.

- **Path Parameters**: `id` (int) - The unique ID of the employee.
- **Response**: The `Employee` object.

### POST /employees

Create a new employee record.

- **Request Body**: The `Employee` object (excluding ID, as it is auto-generated).
- **Response**: The created `Employee` object.

### PUT /employees/{id}

Update an existing employee record.

- **Path Parameters**: `id` (int) - The unique ID of the employee.
- **Request Body**: The `Employee` object with updated details.
- **Response**: The updated `Employee` object.

### DELETE /employees/{id}

Delete an employee record.

- **Path Parameters**: `id` (int) - The unique ID of the employee.
- **Response**: Status message indicating success or failure.

---

## Dependencies

This project uses the following dependencies:

- **SQLServer**: Official SQL server driver for working with SQL server databases.
- **System.ComponentModel.DataAnnotations**: For data validation attributes (if used).
- **System.Linq**: For LINQ operations.

---

## Conclusion

The HRConnect API is designed to simplify the management of employee data in an organization. By using this API, you can easily perform CRUD operations on employee records, manage attendance and leave applications, and track payroll, overtime, and other HR-related tasks.
