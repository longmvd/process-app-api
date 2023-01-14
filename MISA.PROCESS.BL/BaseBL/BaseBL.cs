using Microsoft.AspNetCore.Mvc.ModelBinding;
using MISA.PROCESS.Common;
using MISA.PROCESS.Common.Attributes;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.Common.Enums;
using MISA.PROCESS.DL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
            ValidateRequest(request);

            var conditions = request.ConditionQueries;
            StringBuilder afterWhere = new StringBuilder();
            foreach (var condition in conditions)
            {
                string whereCondition;
                if (condition.SubQuery != null)
                {
                    condition.SubQuery.ForEach(query => StandardizeValue(query));
                    whereCondition = $"({String.Join(" ", condition.SubQuery.Select(subQuery => $"{subQuery.Relationship} {subQuery.Column} {subQuery.Operator} {subQuery.Value}"))})";
                    afterWhere.Append($"{condition.Relationship} {whereCondition}");
                }
                else if (IsValidCondition(condition))
                {
                    StandardizeValue(condition);

                    whereCondition = $" {condition.Relationship} {condition.Column} {condition.Operator} {condition.Value}";
                    afterWhere.Append(whereCondition);
                }
            }

            // build câu sắp xếp và phân trang
            //string order = request.Desc ? "DESC" : "ASC";
            //string orderLimit = $" ORDER BY {request.SortColumn} {order} LIMIT {request.PageNumber},{request.PageSize}";


            request.Filter = afterWhere.ToString();

            var response = new ServiceResponse() { StatusCode = System.Net.HttpStatusCode.OK, Success = true };
            var paging = this._baseDL.GetByFilter(request);
            response.Data = paging;


            return response;
        }

        /// <summary>
        /// Chuẩn hóa giá trị cho câu điều kiện where
        /// </summary>
        /// <param name="condition"></param>
        public static void StandardizeValue(ConditionQuery condition)
        {
            if (condition.Value != null)
            {
                condition.Value = SanitizeInput(condition.Value);
            }
            switch (condition.Operator)
            {
                case "LIKE":
                    condition.Value = $"'%{condition.Value}%'";
                    break;
                case "IN":
                    //nếu là IN thì chuyển value về dạng ('abc','cde',...)
                    var values = $"({String.Join(",", condition.Value.Split(",").ToList().Select(value => $"'{value}'"))})";
                    condition.Value = values;
                    break;
                default:
                    condition.Value = $"'{condition.Value}'";
                    break;
            }
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


            var detailObjects = new Dictionary<string, StringObject>(); //table name, values, tổng số bản ghi trong bảng cần insert
            var detailIDs = new List<object>();
            var values = ""; // chuỗi giá trị để lưu vào database của entity

            entities.ForEach((entity) =>
            {
                var properties = entity.GetType().GetProperties();
                var value = "(";
                var entityID = Guid.NewGuid();
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
                            propValue = SetValue(entity, property, entityID);
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
                        if (manyToMany != null && propValue != null)
                        {
                            var detailListValue = (List<Guid>)propValue;// lấy danh sách id bảng detail
                            // chuyển sang dạng chuỗi để insert vào db dạng VALUES ('masterId', 'detailId'),('masterId', 'detailId'),...
                            var detailStringValue = String.Join(",", detailListValue.Select((detail) => $"('{entityID}','{detail}')"));
                            if (detailObjects.ContainsKey(propName))
                            {
                                var valueObject = detailObjects[propName];
                                valueObject.Count += detailListValue.Count;
                                valueObject.Value += $",{detailStringValue}";
                            }
                            else
                            {
                                var valueObject = new StringObject() { Name = propName, Count = detailListValue.Count, Value = detailStringValue };
                                detailObjects.Add(valueObject.Name, valueObject);
                            }
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

            if (detailObjects.Count > 0) // xử lý lưu các bảng detail
            {
                //lấy ra danh sách các giá trị của các bảng detail cần insert vào db
                var detailsStringObject = detailObjects.ToList().Select((detailObject)=> detailObject.Value).ToList();
                response.Data = this._baseDL.Insert(ens, detailsStringObject);
            }
            else
            {
                response.Data = this._baseDL.Insert(ens, null);
            }

            if (response.Data != null && (int)response.Data > 0)
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
        /// Validate dữ liệu
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modelStateDictionary"></param>
        /// <returns></returns>
        public ServiceResponse ValidateData(T entity, ModelStateDictionary modelStateDictionary)
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
        public ServiceResponse ValidateData(List<T> entities)
        {
            var response = new ServiceResponse() { Success = true };
            var fields = new Dictionary<string, string>();
            //duyệt danh sách các thực thể trả về
            var validationResults = new List<ValidationResult>();
            entities.ForEach(entity =>
            {
                //validate giá trị đầu vào
                var context = new ValidationContext(entity);
                bool isValid = Validator.TryValidateObject(entity, context, validationResults, true);

                var properties = entity.GetType().GetProperties();
                //duyệt các trường của thực thể để lưu lại trường cần validate trùng
                foreach (var property in properties)
                {
                    var unique = property.GetCustomAttribute<UniqueAttribute>();
                    var propName = property.Name;
                    var propValue = property.GetValue(entity);
                    if (propValue != null)
                    {
                        //nếu giá trị là string thì loại bỏ các ký tự đặc biệt
                        if (propValue is string)
                        {
                            propValue = SanitizeInput(propValue.ToString());
                            property.SetValue(entity, propValue);
                        }

                        //nếu trường nào là unique thì thêm vào từ điển với key là tên trường của thực thể, value là các giá trị của trường đó
                        // vd: UserCode: 'NV123', NV234
                        if (unique != null)
                        {
                            //nếu trường đã có trong dictionary thì cộng dồn giá trị
                            if (fields.ContainsKey(propName))
                            {
                                fields[propName] += $",'{propValue}'";
                            }
                            else
                            {
                                fields.Add(propName, $"'{propValue}'");
                            }
                        }
                    }
                }
            });
            var errorValue = new Dictionary<string, List<string>>();


            if (validationResults.Count > 0)
            {

                foreach (var result in validationResults)
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        var errorField = result.MemberNames.First();
                        string errorMessage = result.ErrorMessage;
                        if (!errorValue.ContainsKey(errorField))
                        {
                            errorValue.Add(errorField, new List<string>() { errorMessage });
                        }
                    }
                }

                response.Success = false;
                response.ErrorCode = ErrorCode.InvalidData;
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Data = errorValue;
                return response;
            }

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
        /// Validate request gửi lên
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validatePage"></param>
        public void ValidateRequest(PagingRequest request, bool validatePage = true)
        {
            if (validatePage)
            {
                if (request.PageNumber == null)
                {
                    request.PageNumber = 1;
                }
                if (request.PageSize == null)
                {
                    request.PageSize = 15;
                }
            }
            if (request.Filter == null)
            {
                request.Filter = "";
            }
            if (request.SortColumn == null)
            {
                request.SortColumn = "ModifiedDate";
            }
            request.Filter = request.Filter.Trim();
            int? offset = (request.PageNumber - 1) * request.PageSize;
            if (offset < 0) { offset = 0; }
            request.PageNumber = offset; //sql lấy từ vị trí 0
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
                    value += $"'{propValue}',";
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

        /// <summary>
        /// Giải mã chuỗi base64
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Khử kí tự đặc biệt trong input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SanitizeInput(string input)
        {
            // Danh sách kí tự cần loại bỏ
            string[] dangerousChars = new string[] { "'", "--", "/*", "*/", ";", "%", "_", "=" };
            // loại bỏ các ký tự
            foreach (string dangerousChar in dangerousChars)
            {
                input = input.Replace(dangerousChar, "");
            }
            // Trả về input đã khử
            return input;
        }

        public static bool IsValidCondition(ConditionQuery condition)
        {
            return !(String.IsNullOrEmpty(condition.Operator) || String.IsNullOrEmpty(condition.Relationship) || String.IsNullOrEmpty(condition.Column) || String.IsNullOrEmpty(condition.Value));
        }
    }
}
