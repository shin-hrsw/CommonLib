using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace CommonLib.Database
{
    /// <summary>
    /// Singletonのデータベース接続クラス
    /// </summary>
    public class Connection
    {
        private static Connection Instance = null;
        private static MySqlConnection conn = null;
        private static MySqlTransaction tran = null;

        #region コンストラクタ
        private Connection()
        {
            var bl = new MySqlConnectionStringBuilder();
            bl.ConnectionProtocol = MySqlConnectionProtocol.Tcp;
        }
        #endregion

        #region メソッド(public)
        public static Connection GetInstance()
        {
            return Instance;
        }
        #endregion
    }
}
