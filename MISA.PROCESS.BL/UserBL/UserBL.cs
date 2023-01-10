using Microsoft.AspNetCore.Mvc.ModelBinding;
using MISA.PROCESS.Common;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Entities;
using MISA.PROCESS.Common.Enums;
using MISA.PROCESS.DL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace MISA.PROCESS.BL
{
    public class UserBL : BaseBL<User>, IUserBL
    {
        #region Field
        private IUserDL _userDL;
        #endregion

        #region Constructor
        public UserBL(IUserDL userBL) : base(userBL)
        {
            _userDL = userBL;
        }

        /// <summary>
        /// Lấy người dùng theo điều kiện và phân trang
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Paging</returns>
        ///Created by: MDLONG(18/11/2022)
        //public ServiceResponse GetByFilter(PagingRequest request)
        //{
        //    ValidateRequest(request);

        //    return _userDL.GetByFilter(request);
        //}

        public override ServiceResponse UpdateOneByID(Guid id, User entity, ModelStateDictionary modelStateDictionary)
        {
            ServiceResponse response = new ServiceResponse() { Success = true, StatusCode = System.Net.HttpStatusCode.OK };
            var errorObject = new ExpandoObject() as IDictionary<string, object>;
            var insertRoles = new List<string>();
            var deleteRoles = new List<string>();
            foreach (var role in entity.Roles)
            {
                if(role.State == State.Add)
                {
                    insertRoles.Add(role.RoleID.ToString());
                }else if(role.State == State.Delete)
                {
                    deleteRoles.Add(role.RoleID.ToString());
                }
            }
            if (insertRoles.Count == 0 && deleteRoles.Count == 0)
            {
                response.Success = true;
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Data = true;
            }
            else
            {
                //lấy tất cả id vai trò cũ
                //var oldRoleIDs = this._userDL.GetByID(id).Roles.Select((role) => role.RoleID.ToString()).ToList();

                var deleteRoleIDs = String.Join(",", deleteRoles);

                var insertRoleIDs = String.Join(",", insertRoles.Select((roleID) => $"('{id}', '{roleID}')"));
                var deleteRole = new StringObject() {Value = deleteRoleIDs, Count = deleteRoles.Count };
                var insertRole = new StringObject() {Value = insertRoleIDs, Count = insertRoles.Count };

                response.Success = this._userDL.UpdateOneByID(id, deleteRole, insertRole, entity.UserName);
                response.Data = true;
                if (!response.Success)
                {
                    response.Data = false;
                }
                

            }
            return response;

        }

        public override ServiceResponse GetByFilter(PagingRequest request)
        {
            ValidateRequest(request);
            var response = new ServiceResponse() { StatusCode = System.Net.HttpStatusCode.OK, Success = true};
            var paging = this._userDL.GetByFilter(request);
            response.Data = paging;
            

            return response;
        }

        /// <summary>
        /// xử lý validate request
        /// </summary>
        /// <returns></returns>
        /// Created by: MDLONG(20/11/2022)
        private void ValidateRequest(PagingRequest request, bool validatePage = true)
        {
            if (validatePage)
            {
                if (request.PageNumber == null)
                {
                    request.PageNumber = 1;
                }
                if (request.PageSize == null)
                {
                    request.PageSize = 15;
                }
            }
            if (request.Filter == null)
            {
                request.Filter = "";
            }
            if(request.JobPositionIDs?.Count == 0)
            {
                request.JobPositionIDs = null;
            }
            if(request.DepartmentID == "")
            {
                request.DepartmentID = null;
            }
            if (request.RoleID == "")
            {
                request.RoleID = null;
            }
            request.Filter = request.Filter.Trim();
            int? offset = (request.PageNumber - 1) * request.PageSize;
            if(offset < 0 ) { offset = 0; }
            request.PageNumber = offset; //sql lấy từ vị trí 0
        }
        #endregion
    }
}
