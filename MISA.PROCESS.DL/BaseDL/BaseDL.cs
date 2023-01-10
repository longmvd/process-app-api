using Dapper;
using MISA.PROCESS.Common.Constants;
using MISA.PROCESS.Common.DTO;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                var result = mySqlConnection.Query<T>(storedProcedure, commandType: CommandType.StoredProcedure);
                return result;
            }
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

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                var result = mySqlConnection.QueryFirstOrDefault<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
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
            parameters.Add(String.Format("@{0}ID", typeof(T).Name), id);

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                mySqlConnection.Open();
                var transaction = mySqlConnection.BeginTransaction();
                try
                {
                    int numberOfRow = mySqlConnection.Query<int>(storedProcedure, parameters, transaction, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (numberOfRow == 1)
                    {
                        transaction.Commit();
                        return true;
                    };
                    transaction.Rollback();
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Thêm 1 bản ghi
        /// </summary>
        /// <param name="entity">Đối tượng thêm mới</param>
        /// <returns>Id bản ghi mới</returns>
        ///  Created by: MDLONG(13/11/2022)
        public virtual int Insert(StringObject entities, StringObject? detailEntities)
        {
            string storedProcedureName = String.Format(Procedure.INSERT, typeof(T).Name);
            var parameters = new DynamicParameters();
            parameters.Add($"@{typeof(T).Name}", entities.Value);
            if (detailEntities != null)
            {
                parameters.Add($"@{detailEntities.Name}", detailEntities.Value);
            }
            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                mySqlConnection.Open();
                using (var transaction = mySqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (var result = mySqlConnection.QueryMultiple(storedProcedureName, parameters, transaction, commandType: System.Data.CommandType.StoredProcedure))
                        {
                            int RowEntitiesEffect = result.Read<int>().Single();
                            if (detailEntities != null)//nếu có bảng join n-n thì đọc số cột insert vào
                            {
                                int RowDetailsEffect = result.Read<int>().Single();
                                if (RowEntitiesEffect != entities.Count || RowDetailsEffect != detailEntities.Count)
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
                            return RowEntitiesEffect;

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        transaction.Rollback();
                        return 0;
                    }
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

            var parameters = new DynamicParameters();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(entity))
            {
                string property = descriptor.Name;
                object value = descriptor.GetValue(entity);

                string idField = $"{typeof(T).Name}ID";
                if (property.Equals(idField))
                    parameters.Add($"@{property}", id);
                else
                    parameters.Add($"@{property}", value);
            }

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                mySqlConnection.Open();
                var transaction = mySqlConnection.BeginTransaction();
                try
                {
                    int result = mySqlConnection.Execute(storedProcedureName, parameters, transaction, commandType: CommandType.StoredProcedure);
                    transaction.Commit();
                    return result > 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    transaction.Rollback();
                    return false;
                }
                finally
                {
                    mySqlConnection.Close();
                }

            }
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
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(entity))
            {
                //Lấy tên của prop
                string property = descriptor.Name;
                //Nếu prop id
                string idField = $"{typeof(T).Name}Code";
                if (property.Equals(idField))
                {
                    object value = descriptor.GetValue(entity);
                    parameters.Add($"@{property}", value);
                    break;
                }
            }

            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                var result = mySqlConnection.QueryFirstOrDefault<Int16>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
                return result > 0 ? true : false;
            }
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
            using (var mySqlConnection = new MySqlConnection(DatabaseContext.ConnectionString))
            {
                var result = mySqlConnection.Query<string>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList();
                return result;
            }
        }
    }
}
