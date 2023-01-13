using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.DTO
{
    public class ConditionQuery
    {
        public string? Relationship { get; set; }

        public string? Column { get; set; }

        public string? Value { get; set; }

        public string? Operator { get; set; }

        public List<ConditionQuery>? SubQuery { get; set; }
    }
}
