using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Attributes
{
    public class OperatorAttribute: BaseAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                var stringValue = value.ToString();
                var conditionOp = stringValue.ToUpper();
                return conditionOp.Equals("LIKE") || conditionOp.Equals("=") || conditionOp.Equals("IN") ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? $"Toán tử '{stringValue}' không hợp lệ.", GetMemberNames(validationContext));
            }
            return ValidationResult.Success;
        }
    }
}
