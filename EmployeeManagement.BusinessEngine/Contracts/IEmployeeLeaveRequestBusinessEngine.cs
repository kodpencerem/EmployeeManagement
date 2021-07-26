using EmployeeManagement.Common.ResultModels;
using EmployeeManagement.Common.SessionOperations;
using EmployeeManagement.Common.VModels;
using System.Collections.Generic;

namespace EmployeeManagement.BusinessEngine.Contracts
{
    public interface IEmployeeLeaveRequestBusinessEngine
    {
        Result<List<EmployeeLeaveRequestVm>> GetAllLeaveRequestByUserId(string userId);
        Result<EmployeeLeaveRequestVm> CreateEmployeeLeaveRequest(EmployeeLeaveRequestVm model,SessionContext user);
        Result<EmployeeLeaveRequestVm> EditEmployeeLeaveRequest(EmployeeLeaveRequestVm model, SessionContext user);
        Result<EmployeeLeaveRequestVm> GetAllLeaveRequestById(int id);
        Result<EmployeeLeaveRequestVm> RemoveEmployeeRequest(int id);
        Result<List<EmployeeLeaveRequestVm>> GetSendApprovedLeaveRequests();
        Result<bool> RejectEmployeeLeaveRequest(int id);
    }
}
