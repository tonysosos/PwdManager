using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using log4net;

namespace PwdManager.DAO.MySQLTest
{
    public class SQLHelper
    {
        //数据库连接字符串（注意：这里的“DBConnectionString”一定要与web.config文件中connectionStrings节点值一致）
        public static readonly string connectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString();

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().GetType());

        private static SQLHelper _instance;

        static object locked;

        static SQLHelper()
        {
            locked = new object();
        }

        public static SQLHelper GetInstance()
        {
            lock (locked)
            {
                if (_instance == null)
                {
                    _instance = new SQLHelper();
                }
            }
            return _instance;
        }

        public int InsertTest()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            int result = 0;
            try
            {
                string sql = string.Format("insert into appinfo (APPID, APPNAME, APPINFO) values ('{0}', '{1}', '{2}')", "test1", "test2", "test3");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log.Error("连接数据库异常: " + e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return result;
        }
    }
}
