using MISA.PROCESS.Common;
using MISA.PROCESS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.DTO
{
    public class ServiceResponse
    {
        /// <summary>
        /// Trạng thái phản hồi
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Dữ liệu trả về
        /// </summary>
        public object? Data{ get; set; }

        /// <summary>
        /// Mã lỗi
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Mã lỗi custom
        /// </summary>
        public ErrorCode ErrorCode { get; set; }
    }
}
