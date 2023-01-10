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
    public class User : BaseEntity
    {
        #region Constructor
        public User()
        {
            Roles = new List<Role>();
        }
        #endregion

        /// <summary>
        /// ID người dùng
        /// </summary>
        [Key]        
        public Guid UserID { get; set; }

        /// <summary>
        /// Mã người dùng
        /// </summary>
        [Required]
        [Unique]
        public string UserCode { get; set; }

        /// <summary>
        /// Tên người dùng
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Vị trí
        /// </summary>
        [Required]
        public Guid JobPositionID { get; set; }

        /// <summary>
        /// Phòng ban
        /// </summary>
        [Required]
        public Guid DepartmentID { get; set; }

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        [SqlIgnore]
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Tên vị trí
        /// </summary>
        [SqlIgnore]
        public string? JobPositionName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        /// 
        [Required]
        [EmailAddress]
        [Unique]
        public string Email { get; set; }

        /// <summary>
        /// Danh sách vai trò
        /// </summary>
        [Required]
        [SqlIgnore]
        public List<Role>? Roles { get; set; }

        /// <summary>
        /// Danh sách id vai trò
        /// </summary>
        [SqlIgnore]
        [ManyToMany]
        public List<Guid>? RoleIDs { get; set; }

        /// <summary>
        /// Tên các vai trò
        /// </summary>
        [SqlIgnore]
        public string? RoleNames { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        /// 
        [Required]
        public Status Status { get; set; }

    }
}
