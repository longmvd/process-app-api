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
        /// Lấy tất cả user
        /// </summary>
        /// <returns>Danh sách user</returns>
        /// CreatedBy: MDLONG(01/01/2022)
        public override IEnumerable<User> GetAll()
        {
            //từ điển lưu lại user
            Dictionary<Guid, User> result = new Dictionary<Guid, User>();
            string storedProcedure = String.Format(Procedure.GET_ALL, typeof(User).Name);
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
                commandType: CommandType.StoredProcedure,
                splitOn: "RoleID");
                users = result.Values.ToList();//gán lại danh sách users = users đã xủ lý thêm vai trò cho user đó
                return users;
            }

        }

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

        public override PagingResult<User> GetByFilter(PagingRequest request)
        {
            PagingResult<User> paging = new PagingResult<User>();
            string storedProcedure = String.Format(Procedure.GET_BY_FILTER, "user");
            var jobPositionIDs = request.JobPositionIDs != null ? String.Join(",", request.JobPositionIDs) : null;
            var parameter = new DynamicParameters();
            parameter.Add("@Keyword", request.Filter);
            parameter.Add("@JobPositionID",  jobPositionIDs );
            parameter.Add("@DepartmentID", request.DepartmentID);
            parameter.Add("@RoleID", request.RoleID);
            parameter.Add("@Offset", request.PageNumber);
            parameter.Add("@Limit", request.PageSize);
            parameter.Add("@SortColumn", request.SortColumn != null? request.SortColumn : "ModifiedDate");
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
        /// Cập nhật user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="DeleteRoleIDs">Chuỗi id cần xóa cách nhau bởi dấu phẩy</param>
        /// <param name="InsertRoleIDs">Chuỗi id cần thêm</param>
        /// <param name="NumDeleteRole">số lượng cần xóa</param>
        /// <param name="NumInsertRole">số lượng cần thêm</param>
        /// <param name="ModifiedBy">sửa bởi</param>
        /// <returns>Kết quả thành công thất bại</returns>
        public bool UpdateOneByID(Guid id, string DeleteRoleIDs, string InsertRoleIDs, int NumDeleteRole, int NumInsertRole, string ModifiedBy)
        {
            string storedProcedureName = String.Format(Procedure.UPDATE, typeof(User).Name);

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", id);
            parameters.Add("@DeleteRoleIDs", DeleteRoleIDs);
            parameters.Add("@InsertRoleIDs", InsertRoleIDs);
            parameters.Add("@ModifiedDate", DateTime.Now);
            parameters.Add("@ModifiedBy", ModifiedBy);

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                mySqlConnection.Open();
                using (var transaction = mySqlConnection.BeginTransaction())
                {
                    try
                    {
                        bool rollBack = false;
                        using (var reader = mySqlConnection.ExecuteReader(storedProcedureName, parameters, transaction, commandType: CommandType.StoredProcedure))
                        {
                            var rowEffect = new List<int>();

                            GetRowEffect(reader, rowEffect);
                            int roleDeleted = rowEffect[0];
                            int roleInserted = rowEffect[1];

                            if (roleDeleted != NumDeleteRole || roleInserted != NumInsertRole)
                            {
                                rollBack = true;
                            }

                        };
                        if (rollBack)
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

        private static void GetRowEffect(IDataReader reader, List<int> rowEffect)
        {
            do
            {
                while (reader.Read())
                {
                    rowEffect.Add(reader.GetInt32(0));
                }
            }
            while (reader.NextResult());
        }
    }
}
