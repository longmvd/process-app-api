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
        [HttpPost("filter")]
        public IActionResult GetByFilter([FromBody] PagingRequest request)
        {
            try
            {
                var response = this._userBL.GetByFilter(request);
                if (response.Success)
                {
                    return StatusCode((int)response.StatusCode, response.Data);
                }
                return BadRequest(response.Data);
            }catch(Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResult
                {
                    ErrorCode = ErrorCode.Exception,
                    DevMsg = Resource.DevMsg_Exception,
                    UserMsg = Resource.UserMsg_Exception,
                    MoreInfo = Resource.DevMsg_Exception,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
        
    }
}
