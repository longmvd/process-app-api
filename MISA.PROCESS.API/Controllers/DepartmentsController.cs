using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.PROCESS.BL;
using MISA.PROCESS.Common.Entities;

namespace MISA.PROCESS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DepartmentsController : BasesController<Department>
    {

        private IDepartmentBL _departmentBL;
        public DepartmentsController(IDepartmentBL _departmentBL) : base(_departmentBL)
        {
            this._departmentBL = _departmentBL;
        }

    }
}
