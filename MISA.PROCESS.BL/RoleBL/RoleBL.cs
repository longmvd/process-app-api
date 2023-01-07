using MISA.PROCESS.Common.Entities;
using MISA.PROCESS.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.BL
{
    public class RoleBL : BaseBL<Role>, IRoleBL
    {
        private IRoleDL _roleDL;
        public RoleBL(IRoleDL roleDL) : base(roleDL)
        {
            _roleDL = roleDL;
        }
    }
}
