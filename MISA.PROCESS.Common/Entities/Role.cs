using MISA.PROCESS.Common.Attributes;
using MISA.PROCESS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Entities
{
    public class Role : BaseEntity
    {
        /// <summary>
        /// Mã vai trò
        /// </summary>
        [Key]
        public Guid RoleID { get; set; }

        /// <summary>
        /// Tên vai trò
        /// </summary>
        [Required]
        [Unique]
        public string RoleName { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái thao tác
        /// </summary>
        [SqlIgnore]
        public State State { get; set; }
    }
}
