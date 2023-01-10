using Microsoft.AspNetCore.Mvc.ModelBinding;
using MISA.PROCESS.Common;
using MISA.PROCESS.Common.Attributes;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Enums;
using MISA.PROCESS.DL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.BL
{
    public class BaseBL<T> : IBaseBL<T>
    {

        #region Field
        private IBaseDL<T> _baseDL;
        #endregion
        #region Constructor
        public BaseBL(IBaseDL<T> baseDL)
        {
            _baseDL = baseDL;
        }
        #endregion
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Tất cả bản ghi</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual ServiceResponse GetAll()
        {
            ServiceResponse response = new ServiceResponse() { StatusCode = System.Net.HttpStatusCode.OK, Success = true };
            response.Data = this._baseDL.GetAll();
            return response;
        }

        /// <summary>
        /// Lấy bản ghi theo filter
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created by: MDLONG(23/12/2022)
        public virtual ServiceResponse GetByFilter(PagingRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lấy bản ghi theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created by: MDLONG(23/12/2022)
        public virtual ServiceResponse GetByID(Guid id)
        {
            ServiceResponse response = new ServiceResponse()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Success = true
            };

            var record = this._baseDL.GetByID(id);
            if (record != null)
            {
                response.Data = record;
            }
            else
            {
                response.Success = false;
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.ErrorCode = Common.Enums.ErrorCode.NotFound;
                response.Data = null;
            }
            return response;
        }

        /// <summary>
        /// Kiểm tra mã trùng
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created by: MDLONG(23/12/2022)
        public virtual ServiceResponse CheckDupplicatedCode(T entity)
        {
            var response = new ServiceResponse() { Success = true };
            response.Success = this._baseDL.CheckDuplicatedField(entity);
            return response;
        }

        /// <summary>
        /// Xóa 1 bản ghi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual ServiceResponse DeleteOneByID(Guid id)
        {
            var response = new ServiceResponse();
            response.Success = this._baseDL.DeleteOneByID(id);
            if (response.Success)
            {
                response.Data = true;
                response.StatusCode = System.Net.HttpStatusCode.OK;
            }
            else
            {
                response.ErrorCode = Common.Enums.ErrorCode.InvalidData;
                response.Data = false;
            }
            return response;
        }

        /// <summary>
        /// Thêm bản ghi
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modelStateDictionary"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created by: MDLONG(23/12/2022)
        public virtual ServiceResponse Insert(List<T> entities)
        {

            var response = ValidateData(entities);
            if (!response.Success)
            {
                return response;
            }
            var detailIDs = new List<object>();
            var values = "";
            string manyToManyName = null;
            entities.ForEach((entity) =>
            {
                var properties = entity.GetType().GetProperties();
                var value = "(";
                foreach (PropertyInfo property in properties)
                {
                    var propName = property.Name;
                    var propValue = property.GetValue(entity, null);
                    var sqlIgnore = property.GetCustomAttribute<SqlIgnoreAttribute>();
                    var manyToMany = property.GetCustomAttribute<ManyToManyAttribute>();
                    //trường nào không bị ignore thì mới lấy dữ liệu để insert database
                    if (sqlIgnore == null)
                    {
                        if (propName.Equals($"{typeof(T).Name}ID"))
                        {
                            propValue = SetValue(entity, property, Guid.NewGuid());
                        }

                        if (propName.Equals("ModifiedDate") || propName.Equals("ModifiedBy"))
                        {
                            continue;
                        }

                        if (propName.Equals($"CreatedDate"))
                        {
                            propValue = SetValue(entity, property, DateTime.Now);
                        }
                        value = ExpandValue(value, propValue);

                    }
                    else
                    {
                        //nếu có bảng nhiều nhiều thì thêm id bảng đó vào để lưu sau khi lưu bảng chính
                        if (manyToMany != null)
                        {
                            detailIDs.Add(propValue);
                            manyToManyName = propName;
                        }
                    }
                }
                value = value.Remove(value.Length - 1);
                value += "),";
                values += value;
            });
            //bỏ dấu phẩy cuối bị thừa
            values = values.Remove(values.Length - 1);
            var ens = new StringObject() { Count = entities.Count, Value = values, Name = typeof(T).Name };

            if (manyToManyName != null)
            {
                var details = new StringObject() { };
                var detailsValue = "";
                int numberOfRole = 0;
                //ghép giá trị id thực thể chính với id thực thể còn lại 
                for (int i = 0; i < entities.Count; i++)
                {
                    var masterID = entities[i].GetType().GetProperty($"{typeof(T).Name}ID").GetValue(entities[i]);
                    var listDetailID = (List<Guid>)detailIDs[i];
                    detailsValue += String.Join(",", listDetailID.Select((id) =>
                    {
                        numberOfRole++;
                        return $"('{masterID}','{id}')";
                    }).ToList());
                    if (i < entities.Count - 1)
                    {
                        detailsValue += ",";
                    }

                }

                details.Count = numberOfRole;
                details.Value = detailsValue;
                details.Name = manyToManyName;

                response.Data = this._baseDL.Insert(ens, details);
            }
            else
            {
                response.Data = this._baseDL.Insert(ens, null);
            }

            if ((int)response.Data > 0)
            {
                response.Success = true;
                response.StatusCode = System.Net.HttpStatusCode.Created;
            }
            else
            {
                Dictionary<string, List<string>> error = new Dictionary<string, List<string>>();
                error.Add($"{entities.GetType().Name}", new List<string>() { Resource.UserMsg_Add_Failed });
                response.Data = error;
                response.Success = false;
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.ErrorCode = ErrorCode.Failed;
            }
            return response;
        }

        /// <summary>
        /// Cập nhật bản ghi
        /// </summary>
        /// <param name="id">Id bản ghi</param>
        /// <param name="entity">thông tin bản ghi mới</param>
        /// <param name="modelStateDictionary"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// Created by: MDLONG(23/12/2022)
        public virtual ServiceResponse UpdateOneByID(Guid id, T entity, ModelStateDictionary modelStateDictionary)
        {
            ServiceResponse respone = ValidateData(entity, modelStateDictionary);
            T oldEntity = this._baseDL.GetByID(id);
            //nếu mã trùng nhưng là mã của chính nó thì vẫn đúng
            if (respone.ErrorCode == ErrorCode.Duplicated)
            {
                if (IsTheOne(entity, oldEntity))
                {
                    respone.Success = true;
                }
            }
            if (respone.Success)
            {
                TypeDescriptor.GetProperties(entity)["CreatedDate"].SetValue(entity, TypeDescriptor.GetProperties(oldEntity)["CreatedDate"].GetValue(oldEntity));
                TypeDescriptor.GetProperties(entity)["CreatedBy"].SetValue(entity, TypeDescriptor.GetProperties(oldEntity)["CreatedBy"].GetValue(oldEntity));
                TypeDescriptor.GetProperties(entity)["ModifiedDate"].SetValue(entity, DateTime.Now);
                respone.Data = this._baseDL.UpdateOneByID(id, entity);
                respone.StatusCode = System.Net.HttpStatusCode.OK;
                return respone;
            }
            return respone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modelStateDictionary"></param>
        /// <returns></returns>
        private ServiceResponse ValidateData(T entity, ModelStateDictionary modelStateDictionary)
        {
            var response = new ServiceResponse() { Success = true, StatusCode = System.Net.HttpStatusCode.OK };
            var properties = typeof(T).GetProperties();

            //custom lỗi binding dữ liệu
            var errorObject = new ExpandoObject() as IDictionary<string, object>;
            bool isValidObject = true;
            bool isValidCode = true;
            foreach (var modelState in modelStateDictionary)
            {
                // lấy ra danh sách lỗi
                var errors = modelState.Value.Errors.Select(error => error.ErrorMessage).ToList();
                if (errors.Count > 0)
                {
                    //nếu có lỗi thì gán property có lỗi tương ứng với dánh sách lỗi
                    var key = modelState.Key;
                    if (key.Contains("$"))
                    {
                        //nếu dữ liệu gửi lên sai thì tất cả đều sai
                        isValidObject = false;
                        isValidCode = false;
                    }
                    if (key.Contains("Code"))
                    {
                        isValidCode = false;
                    }

                    errorObject.Add(key, errors);
                }
            }

            //kiểm tra mã trùng khi dữ liệu hợp lệ
            if (isValidCode)
            {
                bool isDupplicate = CheckDupplicatedCode(entity).Success;
                if (isDupplicate)
                {
                    var code = $"{typeof(T).Name}Code";
                    errorObject = new ExpandoObject() as IDictionary<string, object>;
                    errorObject.Add(code, new List<string>() { string.Format(Resource.UserMsg_Dupplicated_Code, typeof(T).GetProperty(code).GetValue(entity)) });
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    response.Data = errorObject;
                    response.Success = false;
                    response.ErrorCode = ErrorCode.Duplicated;
                    return response;
                }

            }


            //Nếu có lỗi
            if (errorObject.Count > 0)
            {
                response.Success = false;
                response.ErrorCode = ErrorCode.InvalidData;
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;

                if (isValidObject)
                {
                    //sắp xếp lỗi theo thứ tự property
                    var orderedErrorObject = new ExpandoObject() as IDictionary<string, object>;
                    foreach (var property in properties)
                    {
                        var propName = property.Name;
                        bool isContainsKey = ((IDictionary<String, object>)errorObject).ContainsKey(propName);
                        if (isContainsKey)
                        {
                            var value = errorObject[propName];
                            orderedErrorObject.Add(propName, value);
                        }
                    }
                    response.Data = orderedErrorObject;
                }
                else
                {
                    //nếu dữ liệu gửi lên không binding được thì trả luôn lỗi các trường không thể binding
                    response.Data = errorObject;
                }
            }

            return response;
        }

        /// <summary>
        /// Validate dữ liệu
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private ServiceResponse ValidateData(List<T> entities)
        {
            var response = new ServiceResponse() { Success = true };
            var fields = new Dictionary<string, string>();
            entities.ForEach(entity =>
            {
                var properties = entity.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var unique = property.GetCustomAttribute<UniqueAttribute>();
                    var propName = property.Name;
                    var propValue = property.GetValue(entity);
                    if (unique != null)
                    {
                        if (fields.ContainsKey(propName))
                        {
                            fields[propName] += propValue.ToString() + ",";
                        }
                        else
                        {
                            fields.Add(propName, $"{propValue.ToString()},");
                        }
                    }
                }
            });
            var errorValue = new Dictionary<string, List<string>>();
            foreach (var key in fields.Keys)
            {
                var values = this._baseDL.CheckDuplicatedField(fields[key], key, typeof(T).Name);
                if (values.Count > 0)
                {
                    errorValue.Add(key, values);
                }
            }
            if (errorValue.Count > 0)
            {
                response.Success = false;
                response.ErrorCode = ErrorCode.Duplicated;
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Data = errorValue;
            };


            return response;
        }

        /// <summary>
        /// Kiểm tra 2 đối tượng là 1 hay không
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldEntity"></param>
        /// <returns></returns>
        private static bool IsTheOne(T entity, T oldEntity)
        {
            var key = typeof(T).GetProperty($"{typeof(T).Name}Code");
            return key.GetValue(entity).Equals(key.GetValue(oldEntity));
        }

        /// <summary>
        /// Cộng dồn giá trị cho value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propValue"></param>
        /// <returns></returns>
        private static string ExpandValue(string value, object? propValue)
        {

            if (propValue != null)
            {
                if (propValue.GetType().Name == "DateTime")
                {
                    value += "NOW(),";
                }
                else if (propValue.GetType().IsEnum)
                {
                    value += $"{(int)propValue},";
                }
                else if (propValue == "")
                {
                    value += "null,";
                }
                else
                {
                    value += $"'{propValue.ToString()}',";
                }

            }
            else
            {
                value += "null,";
            }
            return value;
        }

        /// <summary>
        /// đặt giá trị cho trường dữ liệu của thực thể
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object SetValue(T entity, PropertyInfo property, object value)
        {
            object propValue;
            property.SetValue(entity, value, null);
            propValue = property.GetValue(entity, null);
            return propValue;
        }
    }
}
