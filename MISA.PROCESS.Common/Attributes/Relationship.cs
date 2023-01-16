using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Attributes
{
    public class RelationshipAttribute : BaseAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                var stringValue = value.ToString();
                var relationship = stringValue.ToUpper();
                return relationship.Equals("AND") || relationship.Equals("OR") ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? $"Quan hệ '{stringValue}' không hợp lệ.", GetMemberNames(validationContext));
            }
            return ValidationResult.Success;
        }
    }
}
