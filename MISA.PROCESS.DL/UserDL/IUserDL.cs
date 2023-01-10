using Dapper;
using MISA.PROCESS.Common.Constants;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.DL
{
    public interface IUserDL : IBaseDL<User>
    {
        /// <summary>
        /// Cập nhật theo id
        /// </summary>
        /// <param name="id">id bản ghi</param>
        /// <param name="deleteRole">vai trò cần xóa</param>
        /// <param name="insertRole">vai trò cần thêm</param>
        /// <param name="ModifiedBy"></param>
        /// <returns></returns>
        public bool UpdateOneByID(Guid id, StringObject deleteRole, StringObject insertRole, string modifiedBy);

        /// <summary>
        /// Tìm kiếm phân trang
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Phân trang</returns>
        public PagingResult<User> GetByFilter(PagingRequest request);


    }
}
