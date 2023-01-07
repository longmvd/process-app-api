using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.DTO
{
    public class PagingRequest
    {       

        /// <summary>
        /// Kích thước trang
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Trang số bao nhiêu
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// Điều kiện lọc
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// Danh sách ID vị trí
        /// </summary>
        public List<string>? JobPositionIDs { get; set; }

        /// <summary>
        /// ID Phòng ban
        /// </summary>
        public string? DepartmentID { get; set; }

        /// <summary>
        /// ID vai trò
        /// </summary>
        public string? RoleID { get; set; }

        /// <summary>
        /// Cột cần sắp xếp
        /// </summary>
        public string? SortColumn { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp
        /// </summary>
        public bool Desc { get; set; }
    }
}
