using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace CommonLib.Database
{
    public class SQLParameter
    {
        #region プロパティ
        /// <summary>
        /// パラメータ名
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// データ型
        /// </summary>
        public Type ParameterType { get; private set; }

        /// <summary>
        /// 値
        /// </summary>
        public object ParameterValue { get; private set; }

        /// <summary>
        /// MySQLデータ型
        /// </summary>
        public MySqlDbType DbType
        {
            get
            {
                if (this.ParameterType == typeof(string)) { return MySqlDbType.VarString; }
                else if (this.ParameterType == typeof(bool)) { return MySqlDbType.Int16; }
                else if (this.ParameterType == typeof(int)) { return MySqlDbType.Int32; }
                else if (this.ParameterType == typeof(int?)) { return MySqlDbType.Int32; }
                else if (this.ParameterType == typeof(long)) { return MySqlDbType.Int64; }
                else if (this.ParameterType == typeof(long?)) { return MySqlDbType.Int64; }
                else if (this.ParameterType == typeof(decimal)) { return MySqlDbType.Decimal; }
                else if (this.ParameterType == typeof(decimal?)) { return MySqlDbType.Decimal; }
                else if (this.ParameterType == typeof(DateTime)) { return MySqlDbType.DateTime; }
                else if (this.ParameterType == typeof(DateTime?)) { return MySqlDbType.DateTime; }
                else if (this.ParameterType == typeof(byte[])) { return MySqlDbType.MediumBlob; }
                else { return MySqlDbType.VarString; }
            }
        }
        #endregion

        #region コンストラクタ
        public SQLParameter(string name, Type type, object value)
        {
            this.ParameterName = name;
            this.ParameterType = type;
            this.ParameterValue = value;
        }
        #endregion
    }
}
