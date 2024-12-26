using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class LeaveApplicationRepositories : GenericRepository<LeaveApplication>, ILeaveApplicationRepositories
{
    public LeaveApplicationRepositories(DataContext context) : base(context)
    {

    }


    public Task<LeaveApplication> UpdateDate(int id, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<LeaveApplication> UpdateLeaveReason(int id, string leaveReason)
    {
        throw new NotImplementedException();
    }

    public Task<LeaveApplication> UpdateLeaveType(int id, string leaveType)
    {
        throw new NotImplementedException();
    }

}