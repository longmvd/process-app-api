using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.PROCESS.BL;
using MISA.PROCESS.Common;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Enums;

namespace MISA.PROCESS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BasesController<T> : ControllerBase
    {
        #region Field
        private IBaseBL<T> _baseBL;
        #endregion

        #region Constructor
        public BasesController(IBaseBL<T> baseBL)
        {
            _baseBL = baseBL;
        }
        #endregion

        /// <summary>
        /// Lấy thông tin tất cả phòng ban
        /// </summary>
        /// <returns>Danh sách phòng ban</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var response = _baseBL.GetAll();
                //xử lý kết quả trả về
                return StatusCode((int)response.StatusCode, response.Data);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

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

        /// <summary>
        /// Lấy bản ghi theo filter
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{filter}")]
        public IActionResult GetByFilter(PagingRequest request)
        {
            try
            {
                var response = this._baseBL.GetByFilter(request);
                if (response.Success)
                {
                    return StatusCode((int)response.StatusCode, response.Data);
                }
                return BadRequest(new ErrorResult
                {
                    ErrorCode = response.ErrorCode,
                    DevMsg = Resource.UserMsg_Invalid_Data,
                    UserMsg = Resource.UserMsg_Invalid_Data,
                    MoreInfo = response.Data,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
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

        /// <summary>
        /// Lấy thông tin 1 bản ghi theo id
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns>1 bản ghi</returns>
        /// Author: MDLONG(12/11/2022)
        [HttpGet("{recordId}")]
        public IActionResult GetOneByID([FromRoute] Guid recordId)
        {
            try
            {
                var response = _baseBL.GetByID(recordId);

                if (response.Success)
                {
                    return StatusCode(StatusCodes.Status200OK, response.Data);

                }
                return StatusCode(StatusCodes.Status404NotFound, new ErrorResult
                {
                    ErrorCode = ErrorCode.NotFound,
                    DevMsg = Resource.DevMsg_ID_Not_Exist,
                    UserMsg = Resource.UserMsg_Not_Found,
                    MoreInfo = "",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResult
                {
                    ErrorCode = ErrorCode.Exception,
                    DevMsg = Resource.DevMsg_Exception,
                    UserMsg = Resource.UserMsg_Exception,
                    MoreInfo = "",
                    TraceId = HttpContext.TraceIdentifier
                });

            }
        }

        /// <summary>
        /// API thêm mới bản ghi
        /// </summary>
        /// <param name="entity">Thông tin cần thêm</param>
        /// <returns>ID bản ghi được thêm</returns>
        /// Author: MDLONG(12/11/2022)
        [HttpPost]
        public virtual IActionResult Insert([FromBody] List<T> entities)
        {
            try
            {
                var response = _baseBL.Insert(entities);
                if (response.Success && response.StatusCode !=null)
                {
                    return StatusCode((int)response.StatusCode, response.Data);
                }
                else
                {

                    return StatusCode((int)response.StatusCode, new ErrorResult
                    {
                        ErrorCode = response.ErrorCode != ErrorCode.None ? response.ErrorCode : ErrorCode.InvalidData,
                        DevMsg = Resource.DevMsg_Exception,
                        UserMsg = Resource.UserMsg_Invalid_Data,
                        MoreInfo = response.Data,
                        TraceId = HttpContext.TraceIdentifier
                    });

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResult
                {
                    ErrorCode = ErrorCode.Exception,
                    DevMsg = Resource.DevMsg_Exception,
                    UserMsg = Resource.UserMsg_Exception,
                    MoreInfo = "",
                    TraceId = HttpContext.TraceIdentifier
                });

            }

        }

        /// <summary>
        /// API sửa thông tin phòng ban
        /// </summary>
        /// <param name="depaCodertment">ID phòng ban</paCodem>
        /// <param name="base">Thông tin cần sửa</para, base.BaseNamem>;
        /// <returns>ID bản ghi đã sửa</returns>
        /// Author: MDLONG(12/11/2022)
        [HttpPut("{recordId}")]
        public IActionResult Update([FromRoute] Guid recordId, [FromBody] T entity)
        {
            try
            {
                var response = _baseBL.UpdateOneByID(recordId, entity);
                if (response.Success && response.StatusCode != null)
                {
                    return StatusCode((int)response.StatusCode, response.Data);
                }

                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResult
                {
                    ErrorCode = response.ErrorCode,
                    DevMsg = Resource.UserMsg_Edit_Failed,
                    UserMsg = Resource.UserMsg_Edit_Failed,
                    MoreInfo = response.Data,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResult
                {
                    ErrorCode = ErrorCode.Exception,
                    DevMsg = Resource.DevMsg_Exception,
                    UserMsg = Resource.UserMsg_Exception,
                    MoreInfo = "",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Xóa thông tin 1 phòng ban theo id
        /// </summary>
        /// <param name="recordId">Id bản ghi</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// Author: MDLONG(12/11/2022)
        [HttpDelete("{recordId}")]
        public IActionResult DeleteOneByID([FromRoute] Guid recordId)
        {
            try
            {
                var response = _baseBL.DeleteOneByID(recordId);

                if (response.Success)
                {
                    return StatusCode(StatusCodes.Status200OK, response.Data);
                }

                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResult
                {
                    ErrorCode = ErrorCode.NotFound,
                    DevMsg = Resource.UserMsg_Not_Found,
                    UserMsg = Resource.UserMsg_Delete_Failed,
                    MoreInfo = "",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResult
                {
                    ErrorCode = ErrorCode.Exception,
                    DevMsg = Resource.DevMsg_Exception,
                    UserMsg = Resource.UserMsg_Exception,
                    MoreInfo = "",
                    TraceId = HttpContext.TraceIdentifier
                });

            }
        }

        /// <summary>
        /// Lấy mã mới
        /// </summary>
        /// <returns></returns>
        [HttpGet("newCode")]
        public IActionResult GetNewCode()
        {

            try
            {
                var response = this._baseBL.GetNewCode();
                //xử lý kết quả trả về
                return StatusCode((int)response.StatusCode, response.Data);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

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
