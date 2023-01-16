using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Attributes
{
    public class ColumnAttribute : BaseAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var conditionOp = value.ToString().ToUpper();
                return conditionOp.Equals("LIKE") || conditionOp.Equals("=") || conditionOp.Equals("IN") ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? $"Toán tử '{conditionOp}' không hợp lệ.", GetMemberNames(validationContext));
            }
            return ValidationResult.Success;
        }
    }
}
