using Microsoft.AspNetCore.Mvc.ModelBinding;
using MISA.PROCESS.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MISA.PROCESS.BL
{
    public interface IBaseBL<T>
    {
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created by: MDLONG(23/12/2022)
        public ServiceResponse GetAll();

        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created by: MDLONG(23/12/2022)
        public ServiceResponse GetByFilter(PagingRequest request);

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <returns>1 bản ghi</returns>
        /// Created by: MDLONG(23/12/2022)
        public ServiceResponse GetByID(Guid id);

        /// <summary>
        /// Lấy mã mới
        /// </summary>
        /// <returns></returns>
        public ServiceResponse GetNewCode();

        /// <summary>
        /// Thêm 1 bản ghi
        /// </summary>
        /// <returns>Id bản ghi</returns>
        /// Created by: MDLONG(23/12/2022)
        public ServiceResponse Insert(List<T> entities);

        /// <summary>
        /// Cập nhật bản ghi theo id
        /// </summary>
        /// <returns>1 bản ghi</returns>
        /// Created by: MDLONG(23/12/2022)
        public ServiceResponse UpdateOneByID(Guid id, T entity);

        /// <summary>
        /// Xóa bản ghi theo id
        /// </summary>
        /// <returns>Số bản ghi bị xóa</returns>
        /// Created by: MDLONG(23/12/2022)
        public ServiceResponse DeleteOneByID(Guid id);

        /// <summary>
        /// Kiểm tra mã trùng
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Kết quả kiểm tra</returns>
        public ServiceResponse CheckDupplicatedCode(T entity);

        /// <summary>
        /// validate request gửi lên
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validatePage"></param>
        public ServiceResponse ValidateRequest(PagingRequest request);
    }
}
