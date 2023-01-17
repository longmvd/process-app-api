using Dapper;
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
                    var working = result[user.UserID];//nếu có thì thêm vai trò cho user đó
                    if (working.Roles != null)
                    {
                        working.Roles.Add(role);
                    }
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

        /// <summary>
        /// Cập nhật theo id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="deleteRole">đối tượng cần xóa</param>
        /// <param name="insertRole">đối tượng cần thêm</param>
        /// <param name="modifiedBy">sửa bởi</param>
        /// <returns></returns>
        public bool UpdateOneByID(Guid id, StringObject deleteRole, StringObject insertRole, string roleNames, string modifiedBy)
        {
            string storedProcedureName = String.Format(Procedure.UPDATE, typeof(User).Name);

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", id);
            parameters.Add("@DeleteRoleIDs", deleteRole.Value);
            parameters.Add("@InsertRoleIDs", insertRole.Value);
            parameters.Add("@RoleNames", roleNames);
            parameters.Add("@ModifiedDate", DateTime.Now);
            parameters.Add("@ModifiedBy", modifiedBy);

            var result = true;
            OpenDB();
            if (mySqlConnection != null)
            {
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
                            result = false;
                        }
                        else
                        {
                            transaction.Commit();
                            result = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        transaction.Rollback();
                        result = false;
                    }
                    CloseDB();
                }
            }

            return result;
        }
    }
}
