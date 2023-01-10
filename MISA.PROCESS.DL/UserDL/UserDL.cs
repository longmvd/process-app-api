﻿using Dapper;
using MISA.PROCESS.Common.Constants;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace MISA.PROCESS.DL
{
    public class UserDL : BaseDL<User>, IUserDL
    {
        /// <summary>
        /// Lấy user theo id
        /// </summary>
        /// <returns>Danh sách user</returns>
        /// CreatedBy: MDLONG(01/01/2022)
        public override User GetByID(Guid id)
        {
            string storedProcedure = String.Format(Procedure.GET_BY_ID, typeof(User).Name);
            var parameters = new DynamicParameters();
            parameters.Add($"@{typeof(User).Name}ID", id);

            Dictionary<Guid, User> result = new Dictionary<Guid, User>();
            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                var users = mySqlConnection.Query<User, Role, User>(storedProcedure, (user, role) =>
                {
                    if (!result.ContainsKey(user.UserID))//nếu user chưa có trong từ điển thì thêm vào
                        result.Add(user.UserID, user);
                    User working = result[user.UserID];//nếu có thì thêm vai trò cho user đó
                    working.Roles.Add(role);
                    return user;
                },
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: "RoleID");
                // trả về user đầu tiên trong từ điển
                if (result.Values.Count > 0)
                    return result.Values.First();
                else//nếu không tồn tại trả về null
                    return null;
            }
        }

        public PagingResult<User> GetByFilter(PagingRequest request)
        {
            PagingResult<User> paging = new PagingResult<User>();
            string storedProcedure = String.Format(Procedure.GET_BY_FILTER, "user");
            var jobPositionIDs = request.JobPositionIDs != null ? String.Join(",", request.JobPositionIDs) : null;
            var parameter = new DynamicParameters();
            parameter.Add("@Keyword", request.Filter);
            parameter.Add("@JobPositionID", jobPositionIDs);
            parameter.Add("@DepartmentID", request.DepartmentID);
            parameter.Add("@RoleID", request.RoleID);
            parameter.Add("@Offset", request.PageNumber);
            parameter.Add("@Limit", request.PageSize);
            parameter.Add("@SortColumn", request.SortColumn != null ? request.SortColumn : "ModifiedDate");
            parameter.Add("@SortOrder", request.Desc ? "Desc" : "Asc");

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                var resultQuery = mySqlConnection.QueryMultiple(storedProcedure, parameter, commandType: CommandType.StoredProcedure);
                var totalRecord = resultQuery.Read<int>().First();
                var users = resultQuery.Read<User>().ToList();
                int? totalPage = request.PageSize != null ? Convert.ToInt32(Math.Ceiling(totalRecord / (decimal)request.PageSize)) : null;
                paging.Data = users;
                if (users.ToArray().Length == 0)
                {
                    paging.TotalRecord = 0;
                    paging.TotalPage = 0;
                }
                else
                {
                    paging.TotalRecord = totalRecord;
                    paging.TotalPage = totalPage;
                }
                return paging;
            }

        }

       /// <summary>
       /// Cập nhật theo id
       /// </summary>
       /// <param name="id">id</param>
       /// <param name="deleteRole">đối tượng cần xóa</param>
       /// <param name="insertRole">đối tượng cần thêm</param>
       /// <param name="modifiedBy">sửa bởi</param>
       /// <returns></returns>
        public bool UpdateOneByID(Guid id, StringObject deleteRole, StringObject insertRole, string modifiedBy)
        {
            string storedProcedureName = String.Format(Procedure.UPDATE, typeof(User).Name);

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", id);
            parameters.Add("@DeleteRoleIDs", deleteRole.Value);
            parameters.Add("@InsertRoleIDs", insertRole.Value);
            parameters.Add("@ModifiedDate", DateTime.Now);
            parameters.Add("@ModifiedBy", modifiedBy);

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                mySqlConnection.Open();
                using (var transaction = mySqlConnection.BeginTransaction())
                {
                    try
                    {
                        var reader = mySqlConnection.QueryMultiple(storedProcedureName, parameters, transaction, commandType: CommandType.StoredProcedure);
                        int roleDeleted = reader.Read<int>().Single();
                        int roleInserted = reader.Read<int>().Single();

                        if (roleDeleted != deleteRole.Count || roleInserted != insertRole.Count)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        transaction.Rollback();
                        return false;
                    }
                }

            }

        }
    }
}
