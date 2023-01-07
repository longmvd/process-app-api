using System.ComponentModel.DataAnnotations;

namespace MISA.PROCESS.Common.Entities
{
    public class Department : BaseEntity
    {
        #region Properties
        /// <summary>
        /// Id phòng ban
        /// </summary>
        [Key]
        public Guid DepartmentID { get; set; }

        /// <summary>
        /// mã phòng ban
        /// </summary>
        [Required(ErrorMessage = "Mã Không để trống")]
        public string DepartmentCode { get; set; }

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        [Required(ErrorMessage = "Tên Không để trống")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        #endregion

    }
}
