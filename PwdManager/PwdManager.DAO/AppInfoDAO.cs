using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace PwdManager.DAO
{
    public class AppInfoDAO
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().GetType());
        private static object locked;
        private static AppInfoDAO _instance;
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString();

        static AppInfoDAO()
        {
            locked = new object();
        }

        public static AppInfoDAO GetInstance()
        {
            lock (locked)
            {
                if (_instance == null)
                {
                    _instance = new AppInfoDAO();
                }
            }
            return _instance;
        }

        public int InsertAppInfo(string AppId, string AppName, string AppInfo)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            log.Debug("AppInfoDAO.InsertAppInfo Enter: Appid: " + AppId + ",AppName: " + AppName + ",AppInfo" + AppInfo);
            try
            {
                string sql = string.Format("insert into appinfo (APPID, APPNAME, APPINFO) values ('{0}', '{1}', '{2}')", AppId, AppName, AppInfo);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log.Error("AppInfoDAO.InsertAppInfo ERROR: " + e.Message);
                throw e;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public DataTable GetAllAppInfo()
        {
            DataTable result = new DataTable();
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            log.Debug("AppInfoDAO.GetAllAppInfo Enter");
            try
            {
                string sql = "select * from appinfo";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader dr = cmd.ExecuteReader();
                result.Load(dr);
            }
            catch(Exception e)
            {
                log.Error("AppInfoDAO.GetAllAppInfo ERROR: " + e.Message);
                throw e;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
    }
}
