using System;
using System.Collections.Generic;
using System.Data;
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
            if(Instance == null) { Instance = new Connection(); }
            return Instance;
        }

        public static DataTable ExecuteQuery(string sql)
        {
            if(conn == null) { Instance = new Connection(); }

            var dt = new DataTable();
            var cmd = new MySqlCommand(sql, conn);
            var adapter = new MySqlDataAdapter();
            try
            {
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
            }
            catch (MySqlException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                adapter.Dispose();
            }

            return dt;
        }

        public static DataTable ExecuteQuery(string sql, SQLParameter param)
        {
            if(conn == null) { Instance = new Connection(); }

            var dt = new DataTable();
            var cmd = new MySqlCommand(sql, conn);
            var adapter = new MySqlDataAdapter();
            try
            {
                cmd.Prepare();
                var dbparam = new MySqlParameter();
                dbparam.ParameterName = param.ParameterName;
                dbparam.MySqlDbType = param.DbType;
                dbparam.Value = param.ParameterValue;
                cmd.Parameters.Add(dbparam);

                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
            }
            catch(MySqlException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                adapter.Dispose();
            }

            return dt;
        }
        
        public static DataTable ExecuteQuery(string sql, List<SQLParameter> param)
        {
            if (conn == null) { Instance = new Connection(); }

            var cmd = new MySqlCommand(sql, conn);
            var adapter = new MySqlDataAdapter();
            var dt = new DataTable();
            try
            {
                cmd.Prepare();
                foreach(var p in param)
                {
                    cmd.Parameters.Add(
                        new MySqlParameter()
                        {
                            ParameterName = p.ParameterName,
                            MySqlDbType = p.DbType,
                            Value = p.ParameterValue
                        });
                }

                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
            }
            catch(MySqlException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                adapter.Dispose();
            }
            return dt;
        }

        public static void ExecuteNonQuery(string sql, List<SQLParameter> param)
        {
            if(conn == null) { Instance = new Connection(); }

            var cmd = new MySqlCommand(sql, conn);
            try
            {
                cmd.Prepare();
                if (param != null)
                {
                    param.ForEach(x =>
                    {
                        cmd.Parameters.Add(
                            new MySqlParameter()
                            {
                                ParameterName = x.ParameterName,
                                MySqlDbType = x.DbType,
                                Value = x.ParameterValue
                            });
                    });
                }
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public static object ExecuteScalar(string sql)
        {
            if(conn == null) { Instance = new Connection(); }

            var cmd = new MySqlCommand(sql, conn);
            var adapter = new MySqlDataAdapter();
            try
            {
                adapter.SelectCommand = cmd;
                var dt = new DataTable();
                adapter.Fill(dt);

                return dt.Rows[0][0];
            }
            catch(MySqlException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                adapter.Dispose();
            }
        }

        public static MySqlTransaction BeginTransaction()
        {
            if(conn == null) { Instance = new Connection(); }
            tran = conn.BeginTransaction();
            return tran;
        }
        #endregion
    }
}
