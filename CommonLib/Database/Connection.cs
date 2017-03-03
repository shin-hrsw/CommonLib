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
            bl.Server = Properties.Settings.Default.DatabaseServer;
            bl.Port = Properties.Settings.Default.DatabasePortNo;
            bl.UserID = Properties.Settings.Default.DatabaseUser;
            bl.Password = Properties.Settings.Default.DatabasePassword;
            bl.Database = Properties.Settings.Default.DatabaseName;
            bl.Pooling = true;
            bl.ConnectionTimeout = 10;
            bl.CharacterSet = "utf8";

            conn = new MySqlConnection(bl.GetConnectionString(true));
            try { conn.Open(); }
            catch(MySqlException ex)
            {
                conn.Dispose();
                throw new ApplicationException("データベースに接続できませんでした：" + ex.Message);
            }
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
