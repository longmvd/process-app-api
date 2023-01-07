using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Entities
{
    public class JobPosition : BaseEntity
    {
        #region Properties
        /// <summary>
        /// Id vị trí
        /// </summary>
        [Key]
        public Guid JobPositionID { get; set; }

        /// <summary>
        /// Tên vị trí
        /// </summary>
        [Required(ErrorMessage = "Tên Không để trống")]
        public string JobPositionName { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }
        #endregion
    }
}
