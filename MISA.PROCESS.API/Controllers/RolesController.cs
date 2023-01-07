using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.PROCESS.BL;
using MISA.PROCESS.Common.Entities;

namespace MISA.PROCESS.API.Controllers
{
    [ApiController]
    public class RolesController : BasesController<Role>
    {
        #region Field
        IRoleBL _roleBL;
        #endregion
        #region Constructor
        public RolesController(IRoleBL roleBL) : base(roleBL)
        {
            this._roleBL = roleBL;
        }

        #endregion
    }
}
