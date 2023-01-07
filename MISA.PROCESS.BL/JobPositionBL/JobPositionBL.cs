using MISA.PROCESS.Common;
using MISA.PROCESS.Common.Entities;
using MISA.PROCESS.DL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.BL
{
    public class JobPositionBL : BaseBL<JobPosition>, IJobPositionBL
    {
        private IJobPositionDL _jobPositionDL;
        public JobPositionBL(IJobPositionDL jobPositionDL) : base(jobPositionDL)
        {
            _jobPositionDL = jobPositionDL;
        }
    }
}
