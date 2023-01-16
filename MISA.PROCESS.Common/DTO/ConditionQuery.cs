using MISA.PROCESS.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.DTO
{
    public class ConditionQuery
    {
        /// <summary>
        /// Tên quan hệ
        /// </summary>
        /// 
        [Relationship]
        public string? Relationship { get; set; }

        /// <summary>
        /// Tên cột
        /// </summary>
        public string? Column { get; set; }

        /// <summary>
        /// Giá trị
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// toán tử
        /// </summary>
        /// 
        [Operator]
        public string? Operator { get; set; }

        /// <summary>
        /// Truy vấn con
        /// </summary>
        public List<ConditionQuery>? SubQuery { get; set; }
    }
}
