using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.Common.Constants
{
    /// <summary>
    /// Lớp lưu store procedure
    /// </summary>
    /// Created by: MDLONG(30/10/2022)
    public class Procedure
    {

        #region Field
        /// <summary>
        /// Format tên của procedure lấy tất cả bản ghi
        /// </summary>
        public static string GET_ALL = "Proc_{0}_SelectAll";

        /// <summary>
        /// Format tên của procedure lấy bản ghi theo id
        /// </summary>
        public static string GET_BY_ID = "Proc_{0}_SelectByID";

        /// <summary>
        /// Format tên của procedure lấy bản ghi theo điều kiện
        /// </summary>
        public static string GET_BY_FILTER = "Proc_{0}_SelectFilter";

        /// <summary>
        /// Lấy mã lớn nhất
        /// </summary>
        public static string GET_MAX_CODE = "Proc_{0}_SelectMaxCode";

        /// <summary>
        /// Kiểm tra mã trùng
        /// </summary>
        public static string CHECK_DUPLICATED_CODE = "Proc_{0}_CheckDuplicatedCode";

        /// <summary>
        /// Kiểm tra trường trùng 
        /// </summary>
        public static string CHECK_DUPLICATED = "Proc_GetDuplicated";

        /// <summary>
        /// Kiểm tra mã trùng
        /// </summary>
        public static string CHECK_DUPLICATED_CODES = "Proc_{0}_GetDuplicatedCodes";


        /// <summary>
        /// Kiểm tra email trùng
        /// </summary>
        public static string CHECK_DUPLICATED_EMAILS = "Proc_{0}_GetDuplicatedEmails";

        /// <summary>
        /// Format tên của procedure thêm bản ghi
        /// </summary>
        public static string INSERT = "Proc_{0}_Insert";

        /// <summary>
        /// Format tên của procedure cập nhật bản ghi
        /// </summary>
        public static string UPDATE = "Proc_{0}_Update";

        /// <summary>
        /// Format tên của procedure xóa 1 bản ghi theo id
        /// </summary>
        public static string DELETE_BY_ID = "Proc_{0}_DeleteByID";
        #endregion
    }
}
