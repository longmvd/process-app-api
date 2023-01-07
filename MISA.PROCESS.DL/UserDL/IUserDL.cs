using Dapper;
using MISA.PROCESS.Common.Constants;
using MISA.PROCESS.Common.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.PROCESS.DL
{
    public interface IUserDL : IBaseDL<User>
    {
        public bool UpdateOneByID(Guid id, string DeleteRoleIDs, string InsertRoleIDs, int NumDeleteRole, int NumInsertRole, string ModifiedBy);

    }
}
