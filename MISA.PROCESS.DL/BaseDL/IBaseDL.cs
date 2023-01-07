using MISA.PROCESS.Common;
using MISA.PROCESS.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.DL
{
    public interface IBaseDL<T>
    {

        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created by: MDLONG(11/11/2022)
        public IEnumerable<T> GetAll();

        /// <summary>
        /// Lấy bản ghi theo filter
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created by: MDLONG(11/11/2022)
        public PagingResult<T> GetByFilter(PagingRequest request);

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <returns>1 bản ghi</returns>
        /// Created by: MDLONG(11/11/2022)
        public T GetByID(Guid id);

        /// <summary>
        /// Thêm 1 bản ghi
        /// </summary>
        /// <returns>Id bản ghi</returns>
        /// Created by: MDLONG(11/11/2022)
        public int Insert(StringObject entities, StringObject? detailEntities);

        /// <summary>
        /// Cập nhật bản ghi theo id
        /// </summary>
        /// <returns>1 bản ghi</returns>
        /// Created by: MDLONG(11/11/2022)
        public bool UpdateOneByID(Guid id, T entity);

        /// <summary>
        /// Xóa bản ghi theo id
        /// </summary>
        /// <returns>Số bản ghi bị xóa</returns>
        /// Created by: MDLONG(11/11/2022)
        public bool DeleteOneByID(Guid id);

        /// <summary>
        /// Kiểm tra mã trùng
        /// </summary>
        /// <param name="employee"></param>
        /// <returns>bool</returns>
        /// Created by: MDLONG(18/11/2022)
        public bool CheckDuplicatedField(T entity);

        /// <summary>
        /// Kiểm tra mã trùng
        /// </summary>
        /// <param name="codes">Danh sách mã dạng string</param>
        /// <returns></returns>
        public List<string> CheckDuplicatedField(string values, string field, string entityName);



    }
}
