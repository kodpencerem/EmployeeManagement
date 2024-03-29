﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EmployeeManagement.BusinessEngine.Contracts;
using EmployeeManagement.Common.ConstantsModels;
using EmployeeManagement.Common.Extentsion;
using EmployeeManagement.Common.ResultModels;
using EmployeeManagement.Common.SessionOperations;
using EmployeeManagement.Common.VModels;
using EmployeeManagement.Data.Contracts;
using EmployeeManagement.Data.DbModels;

namespace EmployeeManagement.BusinessEngine.Implementaion
{
    public class EmployeeLeaveRequestBusinessEngine : IEmployeeLeaveRequestBusinessEngine
    {
        #region Variables
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public EmployeeLeaveRequestBusinessEngine(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #endregion

        #region CustomMethods

        public Result<List<EmployeeLeaveRequestVm>> GetAllLeaveRequestByUserId(string userId)
        {
            var data = _unitOfWork.employeeLeaveRequestRepository.GetAll(
                u => u.RequestingEmployeeId == userId
                && u.Cancelled == false,
                includeProperties: "RequestingEmployee,EmployeeLeaveType").ToList();

            if (data != null)
            {
                List<EmployeeLeaveRequestVm> returnData = new List<EmployeeLeaveRequestVm>();
                foreach (var item in data)
                {
                    returnData.Add(new EmployeeLeaveRequestVm()
                    {
                        Id = item.Id,
                        ApprovedStatus = (EnumEmployeeLeaveRequestStatus)item.Approved,
                        ApprovedText = EnumExtension<EnumEmployeeLeaveRequestStatus>.GetDisplayValue((EnumEmployeeLeaveRequestStatus)item.Approved),
                        ApprovedEmployeeId = item.ApprovedEmployeeId,
                        Cancelled = item.Cancelled,
                        DateRequested = item.DateRequested,
                        EmployeeLeaveTypeId = item.EmployeeLeaveTypeId,
                        LeaveTypeText = item.EmployeeLeaveType.Name,
                        EndDate = item.EndDate,
                        StartDate = item.StartDate,
                        RequestComments = item.RequestComments,
                        RequestingEmployeeId = item.RequestingEmployeeId
                    });
                }
                return new Result<List<EmployeeLeaveRequestVm>>(true, ResultConstant.RecordFound, returnData);
            }
            else
                return new Result<List<EmployeeLeaveRequestVm>>(false, ResultConstant.RecordNotFound);

        }

        /// <summary>
        /// Create Employee Leave Request(Çalışan İzin Talebi oluşturma Methodu)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Result<EmployeeLeaveRequestVm> CreateEmployeeLeaveRequest(EmployeeLeaveRequestVm model, SessionContext user)
        {
            if (model != null)
            {
                try
                {
                    var leaveRequest = _mapper.Map<EmployeeLeaveRequestVm, EmployeeLeaveRequest>(model);
                    leaveRequest.RequestingEmployeeId = user.LoginId;
                    leaveRequest.Cancelled = false;
                    leaveRequest.Approved = (int)EnumEmployeeLeaveRequestStatus.Send_Approved;
                    leaveRequest.DateRequested = DateTime.Now;
                    _unitOfWork.employeeLeaveRequestRepository.Add(leaveRequest);
                    _unitOfWork.Save();
                    return new Result<EmployeeLeaveRequestVm>(true, ResultConstant.RecordCreateSuccessfully);
                }
                catch (Exception ex)
                {
                    return new Result<EmployeeLeaveRequestVm>(false, ResultConstant.RecordCreateNotSuccessfully + "=>" + ex.Message.ToString());
                }
            }
            else
                return new Result<EmployeeLeaveRequestVm>(false, "Parametre Olarak Geçilen Data Boş Olamaz!");
        }

        /// <summary>
        /// Edit To Employee Leave Request(Çalışan İzin Talep Güncelleme)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Result<EmployeeLeaveRequestVm> EditEmployeeLeaveRequest(EmployeeLeaveRequestVm model, SessionContext user)
        {
            if (model != null)
            {
                try
                {
                    var leaveRequest = _mapper.Map<EmployeeLeaveRequestVm, EmployeeLeaveRequest>(model);
                    leaveRequest.Approved = (int)model.ApprovedStatus;
                    leaveRequest.RequestingEmployeeId = user.LoginId;
                    _unitOfWork.employeeLeaveRequestRepository.Update(leaveRequest);
                    _unitOfWork.Save();
                    return new Result<EmployeeLeaveRequestVm>(true, ResultConstant.RecordCreateSuccessfully);
                }
                catch (Exception ex)
                {
                    return new Result<EmployeeLeaveRequestVm>(false, ResultConstant.RecordCreateNotSuccessfully + "=>" + ex.Message.ToString());
                }
            }
            else
                return new Result<EmployeeLeaveRequestVm>(false, "Parametre Olarak Geçilen Data Boş Olamaz!");
        }

        public Result<EmployeeLeaveRequestVm> GetAllLeaveRequestById(int id)
        {
            var data = _unitOfWork.employeeLeaveRequestRepository.Get(id);
            if (data != null)
            {
                var leaveRequest = _mapper.Map<EmployeeLeaveRequest, EmployeeLeaveRequestVm>(data);
                leaveRequest.ApprovedStatus = (EnumEmployeeLeaveRequestStatus)data.Approved;
                leaveRequest.ApprovedText = EnumExtension<EnumEmployeeLeaveRequestStatus>.GetDisplayValue((EnumEmployeeLeaveRequestStatus)data.Approved);
                return new Result<EmployeeLeaveRequestVm>(true, ResultConstant.RecordFound, leaveRequest);
            }
            else
                return new Result<EmployeeLeaveRequestVm>(false, ResultConstant.RecordNotFound);
        }

        public Result<EmployeeLeaveRequestVm> RemoveEmployeeRequest(int id)
        {
            var data = _unitOfWork.employeeLeaveRequestRepository.Get(id);
            if (data != null)
            {
                data.Cancelled = true;
                _unitOfWork.employeeLeaveRequestRepository.Update(data);
                _unitOfWork.Save();
                return new Result<EmployeeLeaveRequestVm>(true, ResultConstant.RecordCreateSuccessfully);
            }
            else
                return new Result<EmployeeLeaveRequestVm>(false, ResultConstant.RecordCreateNotSuccessfully);
        }

        public Result<List<EmployeeLeaveRequestVm>> GetSendApprovedLeaveRequests()
        {
            var data = _unitOfWork.employeeLeaveRequestRepository.GetAll(
                u => u.Approved == (int)EnumEmployeeLeaveRequestStatus.Send_Approved
                && u.Cancelled == false,
                includeProperties: "RequestingEmployee,EmployeeLeaveType").ToList();

            if (data != null)
            {
                List<EmployeeLeaveRequestVm> returnData = new List<EmployeeLeaveRequestVm>();
                foreach (var item in data)
                {
                    returnData.Add(new EmployeeLeaveRequestVm()
                    {
                        Id = item.Id,
                        ApprovedStatus = (EnumEmployeeLeaveRequestStatus)item.Approved,
                        ApprovedText = EnumExtension<EnumEmployeeLeaveRequestStatus>.GetDisplayValue((EnumEmployeeLeaveRequestStatus)item.Approved),
                        ApprovedEmployeeId = item.ApprovedEmployeeId,
                        Cancelled = item.Cancelled,
                        DateRequested = item.DateRequested,
                        EmployeeLeaveTypeId = item.EmployeeLeaveTypeId,
                        LeaveTypeText = item.EmployeeLeaveType.Name,
                        EndDate = item.EndDate,
                        StartDate = item.StartDate,
                        RequestComments = item.RequestComments,
                        RequestingEmployeeId = item.RequestingEmployeeId,
                        RequestEmployeeName = item.RequestingEmployee.Email
                    });
                }
                return new Result<List<EmployeeLeaveRequestVm>>(true, ResultConstant.RecordFound, returnData);
            }
            else
                return new Result<List<EmployeeLeaveRequestVm>>(false, ResultConstant.RecordNotFound);
        }

        public Result<bool> RejectEmployeeLeaveRequest(int id)
        {
            var data = _unitOfWork.employeeLeaveRequestRepository.Get(id);
            if (data != null)
            {
                try
                {
                    data.Approved = (int)EnumEmployeeLeaveRequestStatus.Rejected;
                    _unitOfWork.employeeLeaveRequestRepository.Update(data);
                    _unitOfWork.Save();
                    return new Result<bool>(true, ResultConstant.RecordCreateSuccessfully);
                }
                catch (Exception ex)
                {
                    return new Result<bool>(false, ex.Message.ToString());
                }
            }
            else
                return new Result<bool>(false, ResultConstant.RecordCreateNotSuccessfully);
        }
        #endregion
    }
}
