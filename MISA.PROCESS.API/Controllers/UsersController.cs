using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.PROCESS.BL;
using MISA.PROCESS.Common;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Entities;
using MISA.PROCESS.Common.Enums;

namespace MISA.PROCESS.API.Controllers
{
    [ApiController]
    public class UsersController : BasesController<User>
    {
        #region Field
        IUserBL _userBL;
        #endregion

        #region Constructor
        public UsersController(IUserBL userBL) : base(userBL)
        {
            _userBL = userBL;
        }
        #endregion


    }
}
