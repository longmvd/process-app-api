using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.PROCESS.BL;
using MISA.PROCESS.Common.Entities;

namespace MISA.PROCESS.API.Controllers
{
    //[Route("api/v1/[controller]")]
    [ApiController]
    public class JobPositionsController : BasesController<JobPosition>
    {
        #region Field
        IJobPositionBL _jobPositionBL;
        #endregion
        #region Constructor
        public JobPositionsController(IJobPositionBL jobPositionBL) : base(jobPositionBL)
        {
            this._jobPositionBL = jobPositionBL;
        }

        #endregion
    }
}
