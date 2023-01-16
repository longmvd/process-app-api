using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Attributes
{
    public class BaseAttribute : ValidationAttribute
    {
        /// <summary>
        /// Lấy tên thuộc tính đang dùng aAttribute
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns>Tên của thuộc tính đang dùng Attribute</returns>
        public static string[]? GetMemberNames(ValidationContext validationContext)
        {
            string[]? memberNames = validationContext.MemberName is { } memberName
            ? new[] { memberName }
            : null;

            return memberNames;
        }
    }
}
