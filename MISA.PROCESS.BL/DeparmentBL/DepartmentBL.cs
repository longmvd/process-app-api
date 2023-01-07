using MISA.PROCESS.Common.Entities;
using MISA.PROCESS.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.BL
{
    public class DepartmentBL : BaseBL<Department>, IDepartmentBL
    {
        private IDepartmentDL _departmentDL;
        public DepartmentBL(IDepartmentDL departmentDL) : base(departmentDL)
        {
            _departmentDL = departmentDL;
        }
    }
}
