using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Enums
{
    public enum Status
    {
        /// <summary>
        /// Chưa kích hoạt
        /// </summary>
        NotActivated = 1,

        /// <summary>
        /// Chờ xác nhận
        /// </summary>
        Pending = 2,

        /// <summary>
        /// Đang hoạt động
        /// </summary>
        Active = 3,

        /// <summary>
        /// Ngừng kích hoạt
        /// </summary>
        Inactive = 4,

    }
}
