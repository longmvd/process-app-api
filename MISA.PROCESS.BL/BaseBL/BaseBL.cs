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

        #region Method
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Tất cả bản ghi</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual ServiceResponse GetAll()
        {
            var response = new ServiceResponse() { StatusCode = System.Net.HttpStatusCode.OK, Success = true };
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
            var response = ValidateRequest(request);
            var conditions = request.ConditionQueries;
            if (response.Success && conditions != null)
            {
                StringBuilder afterWhere = new StringBuilder();
                foreach (var condition in conditions)
                {
                    string whereCondition;
                    if (condition.SubQuery != null)
                    {
                        //chuẩn hóa giá trị
                        condition.SubQuery.ForEach(query => StandardizeValue(query));

                        //lấy các câu query con
                        var conditionSubQuery = condition.SubQuery.Select((subQuery, index) =>
                                                        (index == 0 ? IsFirstConditionValid(subQuery) : IsValidCondition(subQuery))
                                                        ? $"{subQuery.Relationship} {subQuery.Column} {subQuery.Operator} {subQuery.Value}" : "").ToList();
                        bool isValidSubQuery = false;
                        //nếu duyệt qua các câu query con đều rỗng thì không thêm vào where
                        for (int i = 0; i < conditionSubQuery.Count; i++)
                        {
                            //bỏ quan hệ đầu tiên để đúng cú pháp vd: (AND column LIKE ...) cần bỏ AND
                            if (!string.IsNullOrEmpty(conditionSubQuery[i]))
                            {
                                var splitCondition = conditionSubQuery[i].Split(" ");
                                splitCondition[0] = "";
                                conditionSubQuery[i] = String.Join(" ", splitCondition);
                                isValidSubQuery = true;
                                break;
                            }
                        };
                        //nếu query con hợp lệ thì mới thêm vào
                        if (isValidSubQuery)
                        {
                            whereCondition = $"({String.Join(" ", conditionSubQuery)})";
                            afterWhere.Append($"{condition.Relationship} {whereCondition}");
                        }
                    }
                    else if (IsValidCondition(condition))
                    {
                        StandardizeValue(condition);

                        whereCondition = $" {condition.Relationship} {condition.Column} {condition.Operator} {condition.Value}";
                        afterWhere.Append(whereCondition);
                    }
                }

                request.Filter = afterWhere.ToString();

                var paging = this._baseDL.GetByFilter(request);
                response.Data = paging;
            }

            return response;
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
                response.ErrorCode = ErrorCode.InvalidData;
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

            //BeforeSave();
            var response = ValidateData(entities);
            if (!response.Success)
            {
                return response;
            }

            response.Data = DoSave(entities);

            if ((int)response.Data > 0)
            {
                response.Success = true;
                response.StatusCode = System.Net.HttpStatusCode.Created;
            }
            else
            {
                var error = new Dictionary<string, List<string>>();
                error.Add($"{entities.GetType().Name}", new List<string>() { Resource.UserMsg_Add_Failed });
                response.Data = error;
                response.Success = false;
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.ErrorCode = ErrorCode.Failed;
            }
            return response;
        }

        /// <summary>
        /// Thực hiện xử lý trước khi lưu
        /// </summary>
        /// <param name="entities"></param>
        public virtual void BeforeSave(List<T> entities)
        {
            //
        }

        /// <summary>
        /// Lưu các thực thể
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int DoSave(List<T> entities)
        {
            var detailObjects = new Dictionary<string, StringObject>(); //table name, values, tổng số bản ghi trong bảng cần insert
            // xử lý thực thể trước khi lưu
            StringObject ens = HandleEntityValueToSave(entities, detailObjects);

            if (detailObjects.Count > 0) // xử lý lưu các bảng detail
            {
                //lấy ra danh sách các giá trị của các bảng detail cần insert vào db
                var detailStringObject = detailObjects.ToList().Select((detailObject) => detailObject.Value).ToList();
                return this._baseDL.Insert(ens, detailStringObject);
            }
            else
            {
                return this._baseDL.Insert(ens, null);
            }
        }

        /// <summary>
        /// Xử lý sau lưu
        /// </summary>
        /// <returns></returns>
        public virtual void AfterSave()
        {

        }

        /// <summary>
        /// Xử lý dữ liệu thực thể để lưu
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="detailObjects"></param>
        /// <returns></returns>
        private static StringObject HandleEntityValueToSave(List<T> entities, Dictionary<string, StringObject> detailObjects)
        {
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
                        if (manyToMany != null)
                        {
                            HandleDetailValueToSave(detailObjects, entityID, propName, propValue);
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
            return ens;
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
        public virtual ServiceResponse UpdateOneByID(Guid id, T entity)
        {
            ServiceResponse respone = ValidateData(entity);
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
        public virtual ServiceResponse ValidateData(T entity)
        {
            var response = new ServiceResponse() { Success = true, StatusCode = System.Net.HttpStatusCode.OK };
            var properties = typeof(T).GetProperties();

            //custom lỗi binding dữ liệu
            var errorObject = new ExpandoObject() as IDictionary<string, object>;
            bool isValidCode = true;

            var validationResults = new List<ValidationResult>();
            //validate giá trị đầu vào
            var context = new ValidationContext(entity);
            Validator.TryValidateObject(entity, context, validationResults, true);

            //kiểm tra mã trùng khi dữ liệu hợp lệ
            if (isValidCode)
            {
                bool isDupplicate = true;
                foreach (var property in properties)
                {
                    var unique = property.GetCustomAttribute<UniqueAttribute>();
                    var propName = property.Name;
                    var propValue = property.GetValue(entity);
                    if (propValue != null)
                    {
                        if (unique != null)
                        {
                           isDupplicate = this._baseDL.CheckDuplicatedField((string)propValue, propName, typeof(T).Name).Count > 0;
                            if (isDupplicate)
                            {
                                var code = $"{propName}";
                                errorObject.Add(code, new List<string>() { string.Format(Resource.UserMsg_Dupplicated_Code, typeof(T).GetProperty(code).GetValue(entity)) });
                               
                            }
                        }
                    }
                }
 
                if (isDupplicate)
                {
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
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
            }
            return response;
        }

        /// <summary>
        /// Validate dữ liệu
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual ServiceResponse ValidateData(List<T> entities)
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
                        // 
                        // vd: UserCode: 'NV123', NV234 (nhiều user)
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
                return HandleInValidData(response, validationResults, errorValue);
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
        /// Lấy mã mới
        /// </summary>
        /// <returns></returns>
        public virtual ServiceResponse GetNewCode()
        {
            var response = new ServiceResponse() { StatusCode = System.Net.HttpStatusCode.OK };
            var code = this._baseDL.GetNewCode() + 1;
            string stringCode = code.ToString();
            while (stringCode.Length < (int)LengthRequire.CodeNumberLength)
            {
                stringCode = "0" + stringCode;
            }
            response.Data = $"NV{stringCode}";
            return response;
        }

        /// <summary>
        /// Validate request gửi lên
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validatePage"></param>
        public virtual ServiceResponse ValidateRequest(PagingRequest request)
        {
            var response = new ServiceResponse() { StatusCode = System.Net.HttpStatusCode.OK, Success = true };

            if (request.PageNumber == null)
            {
                request.PageNumber = 1;
            }
            if (request.PageSize == null)
            {
                request.PageSize = 15;
            }
            if (request.Filter == null)
            {
                request.Filter = "";
            }
            if (request.SortColumn == null)
            {
                request.SortColumn = "ModifiedDate";
            }
            var conditions = request.ConditionQueries;
            if (conditions != null)
            {
                var validationResults = new List<ValidationResult>();
                ValidateConditions(validationResults, response, conditions);
                var errorValue = new Dictionary<string, List<string>>();
                if (validationResults.Count > 0)
                {
                    return HandleInValidData(response, validationResults, errorValue);

                }

            }
            int? offset = (request.PageNumber - 1) * request.PageSize;
            if (offset < 0) { offset = 0; }
            request.PageNumber = offset; //sql lấy từ vị trí 0
            return response;
        }

        /// <summary>
        /// Validate condition
        /// </summary>
        /// <param name="response"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private static void ValidateConditions(List<ValidationResult> validationResults, ServiceResponse response, List<ConditionQuery>? conditions)
        {

            conditions.ForEach(condition =>
            {
                if (condition.SubQuery != null)
                {
                    ValidateConditions(validationResults, response, condition.SubQuery);
                }
                //validate giá trị đầu vào
                var context = new ValidationContext(condition);
                response.Success = Validator.TryValidateObject(condition, context, validationResults, true);

            });

        }

        /// <summary>
        /// Xử lý lỗi trả về khi dữ liệu không hợp lệ
        /// </summary>
        /// <param name="response"></param>
        /// <param name="validationResults"></param>
        /// <param name="errorValue"></param>
        /// <returns></returns>
        private static ServiceResponse HandleInValidData(ServiceResponse response, List<ValidationResult> validationResults, Dictionary<string, List<string>> errorValue)
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
                    else
                    {
                        errorValue[errorField].Add(errorMessage);
                    }
                }
            }

            response.Success = false;
            response.ErrorCode = ErrorCode.InvalidData;
            response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            response.Data = errorValue;
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
                    value += "NULL,";
                }
                else
                {
                    value += $"'{propValue}',";
                }

            }
            else
            {
                value += "NULL,";
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
        /// Chuẩn hóa giá trị cho câu điều kiện where
        /// </summary>
        /// <param name="condition"></param>
        public static void StandardizeValue(ConditionQuery condition)
        {
            if (condition.Value != null)
            {
                condition.Value = SanitizeInput(condition.Value);
            }
            if (condition.Column != null)
            {
                condition.Column = SanitizeInput(condition.Column);
            }
            condition.Operator = condition.Operator.ToUpper();
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
        /// Khử kí tự đặc biệt trong input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string SanitizeInput(string input)
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

        /// <summary>
        /// trả về từ điển : tên bảng cần lưu : <tên bảng cân lưu, giá trị cần lưu, số lượng cần lưu>
        /// </summary>
        /// <param name="detailObjects">Từ điển lưu giá trị id </param>
        /// <param name="entityID">ID của người dùng</param>
        /// <param name="propName">Tên trường(RoleID) của bảng user_Role</param>
        /// <param name="propValue">Danh sách id vai trò gửi lên</param>
        private static void HandleDetailValueToSave(Dictionary<string, StringObject> detailObjects, Guid entityID, string propName, object? propValue)
        {
            if (propValue != null)
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

        /// <summary>
        /// Kiểm tra điều kiện truy vấn hợp lệ hay không
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static bool IsValidCondition(ConditionQuery condition)
        {
            return !(String.IsNullOrEmpty(condition.Operator) || String.IsNullOrEmpty(condition.Relationship) || String.IsNullOrEmpty(condition.Column) || String.IsNullOrEmpty(condition.Value));
        }

        /// <summary>
        /// Kiểm tra điều kiện truy vấn đầu tiên hợp lệ hay không
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static bool IsFirstConditionValid(ConditionQuery condition)
        {
            condition.Relationship = "";
            return !(String.IsNullOrEmpty(condition.Operator) || String.IsNullOrEmpty(condition.Column) || String.IsNullOrEmpty(condition.Value));
        }
        #endregion
    }
}
