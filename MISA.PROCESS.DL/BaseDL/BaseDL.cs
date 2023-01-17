using Dapper;
using MISA.PROCESS.Common.Constants;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MISA.PROCESS.DL
{
    public class BaseDL<T> : IBaseDL<T>
    {
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách bản ghi</returns>
        /// Author: MDLONG(13/11/2022)
        public virtual IEnumerable<T> GetAll()
        {
            string storedProcedure = String.Format(Procedure.GET_ALL, typeof(T).Name);
            OpenDB();
            var result = mySqlConnection.Query<T>(storedProcedure, commandType: CommandType.StoredProcedure);
            CloseDB();
            return result;
        }

        /// <summary>
        /// Lấy bản ghi theo filter
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual PagingResult<T> GetByFilter(PagingRequest request)
        {
            var paging = new PagingResult<T>();
            string storedProcedure = String.Format(Procedure.GET_BY_FILTER, typeof(T).Name);
            var parameter = new DynamicParameters();
            parameter.Add("@Where", request.Filter);
            parameter.Add("@OrderLimit", request.OrderLimit);
            OpenDB();
            var resultQuery = mySqlConnection.QueryMultiple(storedProcedure, parameter, commandType: CommandType.StoredProcedure);
            var totalRecord = resultQuery.Read<int>().First();
            var records = resultQuery.Read<T>().ToList();
            CloseDB();

            int? totalPage = request.PageSize != null ? Convert.ToInt32(Math.Ceiling(totalRecord / (decimal)request.PageSize)) : null;
            paging.Data = records;
            if (records.ToArray().Length == 0)
            {
                paging.TotalRecord = 0;
                paging.TotalPage = 0;
            }
            else
            {
                paging.TotalRecord = totalRecord;
                paging.TotalPage = totalPage;
            }
            return paging;


        }

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>1 Bản ghi</returns>
        /// Author: MDLONG(13/11/2022)
        public virtual T GetByID(Guid id)
        {
            string storedProcedure = String.Format(Procedure.GET_BY_ID, typeof(T).Name);
            var parameters = new DynamicParameters();
            parameters.Add($"@{typeof(T).Name}ID", id);
            OpenDB();
            var result = mySqlConnection.QueryFirstOrDefault<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            CloseDB();
            return result;
        }

        /// <summary>
        /// Xóa 1 bản ghi theo id
        /// </summary>
        /// <param name="id">ID bản ghi cần xóa</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// Author: MDLONG(13/11/2022)
        public virtual bool DeleteOneByID(Guid id)
        {
            string storedProcedure = String.Format(Procedure.DELETE_BY_ID, typeof(T).Name);
            var parameters = new DynamicParameters();
            parameters.Add($"@{typeof(T).Name}ID", id);
            OpenDB();
            if (mySqlConnection != null)
            {
                using (var transaction = mySqlConnection.BeginTransaction())
                {
                    int numberOfRow = mySqlConnection.Query<int>(storedProcedure, parameters, transaction, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (numberOfRow != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                    transaction.Commit();
                    CloseDB();
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// Thêm 1 bản ghi
        /// </summary>
        /// <param name="entity">Đối tượng thêm mới</param>
        /// <returns>Id bản ghi mới</returns>
        ///  Created by: MDLONG(13/11/2022)
        public virtual int Insert(StringObject entities, List<StringObject>? detailEntities)
        {
            string storedProcedureName = String.Format(Procedure.INSERT, typeof(T).Name);
            var parameters = new DynamicParameters();
            parameters.Add($"@{typeof(T).Name}", entities.Value);
            if (detailEntities != null)
            {
                foreach (var entity in detailEntities)
                {
                    parameters.Add($"@{entity.Name}", entity.Value);
                }
            }

            OpenDB();
            using (var transaction = mySqlConnection.BeginTransaction())
            {
                using (var result = mySqlConnection.QueryMultiple(storedProcedureName, parameters, transaction, commandType: CommandType.StoredProcedure))
                {
                    int RowEntitiesEffect = result.Read<int>().First();
                    bool isRollback = false;
                    if (detailEntities != null)//nếu có bảng join n-n thì đọc số cột insert vào
                    {
                        var isNext = false;
                        int index = 0;
                        do
                        {
                            var RowDetailsEffect = result.Read<int>().Single();
                            if (RowDetailsEffect != detailEntities[index].Count || RowEntitiesEffect != entities.Count) // Số lượng insert thành công khác số lượng cần insert
                            {
                                isRollback = true;
                            }
                            index++;
                            isNext = result.IsConsumed;
                        }
                        while (!isNext);
                        if (isRollback)
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                    else
                    {
                        if (RowEntitiesEffect != entities.Count)
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                    transaction.Commit();
                    CloseDB();
                    return RowEntitiesEffect;
                }
            }
        }

        /// <summary>
        /// Cập nhật bản ghi theo id
        /// </summary>
        /// <param name="id">Id bản ghi</param>
        /// <param name="entity">Thông tin mới của bản ghi</param>
        /// <returns>kết quả cập nhật</returns>
        /// Created by: MDLONG(13/11/2022)
        public virtual bool UpdateOneByID(Guid id, T entity)
        {
            string storedProcedureName = String.Format(Procedure.UPDATE, typeof(T).Name);
            DynamicParameters parameters = SetParameter(id, entity);

            bool state = true;
            OpenDB();
            if (mySqlConnection != null)
            {
                using var transaction = mySqlConnection.BeginTransaction();
                try
                {
                    int result = mySqlConnection.Execute(storedProcedureName, parameters, transaction, commandType: CommandType.StoredProcedure);
                    transaction.Commit();
                    state = result > 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    state = false;
                }
                CloseDB();
            }
            return state;

        }

        /// <summary>
        /// Đặt parameter cho câu query
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static DynamicParameters SetParameter(Guid id, T entity)
        {
            var parameters = new DynamicParameters();
            if (entity != null)
            {
                foreach (var property in entity.GetType().GetProperties())
                {
                    string propName = property.Name;
                    object value = property.GetValue(entity);

                    string idField = $"{typeof(T).Name}ID";
                    if (propName.Equals(idField))
                        parameters.Add($"@{propName}", id);
                    else
                        parameters.Add($"@{propName}", value);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Kiểm tra mã TRÙNG
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Kết quả kiểm tra</returns>
        ///Created by: MDLONG(28/12/2022)
        public virtual bool CheckDuplicatedField(T entity)
        {
            string storedProcedure = String.Format(Procedure.CHECK_DUPLICATED_CODE, typeof(T).Name);
            var parameters = new DynamicParameters();
            foreach (var property in entity.GetType().GetProperties())
            {
                //Lấy tên của prop
                string propName = property.Name;
                //Nếu prop id
                string codeField = $"{typeof(T).Name}Code";
                if (propName.Equals(codeField))
                {
                    object value = property.GetValue(entity);
                    parameters.Add($"@{propName}", value);
                    break;
                }
            }

            OpenDB();
            var result = mySqlConnection.QueryFirstOrDefault<int>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            CloseDB();
            return result > 0 ? true : false;
        }

        /// <summary>
        /// Kiểm tra trường TRÙNG
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Kết quả kiểm tra</returns>
        ///Created by: MDLONG(28/12/2022)
        public List<string> CheckDuplicatedField(string values, string field, string entityName)
        {
            string storedProcedure = Procedure.CHECK_DUPLICATED;
            var parameters = new DynamicParameters();
            parameters.Add($"@Values", values);
            parameters.Add($"@Column", field);
            parameters.Add($"@Table", entityName);
            OpenDB();
            var result = mySqlConnection.Query<string>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList();
            CloseDB();
            return result;

        }

        /// <summary>
        /// Lấy mã mới
        /// </summary>
        /// <returns></returns>
        public int GetNewCode()
        {
            string storedProcedure = String.Format(Procedure.GET_MAX_CODE, typeof(T).Name);
            OpenDB();
            var result = mySqlConnection.QueryFirstOrDefault<int>(storedProcedure, commandType: CommandType.StoredProcedure);
            CloseDB();
            return result;
        }

        /// <summary>
        /// Kết nối database
        /// </summary>
        protected IDbConnection? mySqlConnection;

        /// <summary>
        /// Khởi tạo và mở connection tới database
        /// </summary>
        public void OpenDB()
        {
            mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString);
            mySqlConnection.Open();
        }

        /// <summary>
        /// Đóng connection tới database
        /// </summary>
        public void CloseDB()
        {
            if (mySqlConnection != null)
            {
                mySqlConnection.Close();
            }
        }
    }
}
