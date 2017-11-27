using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwdManager.DAO
{
    public class UserInfoDAO
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().GetType());
        private static object locked;
        private static UserInfoDAO _instance;
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString();

        static UserInfoDAO()
        {
            locked = new object();
        }

        public static UserInfoDAO GetInstance()
        {
            lock (locked)
            {
                if (_instance == null)
                {
                    _instance = new UserInfoDAO();
                }
            }
            return _instance;
        }

        /// <summary>
        /// 根据OpenID查询数据库
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public DataTable GetUserInfoByOpenId(string openid)
        {
            DataTable result = new DataTable();
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            log.Debug("UserInfoDAO.GetUserInfoByOpenId Enter. OpenID: " + openid);
            try
            {
                string sql = string.Format("select * from userinfo where OPENID = '{0}'", openid);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader dr = cmd.ExecuteReader();
                result.Load(dr);
            }
            catch (Exception e)
            {
                log.Error("UserInfoDAO.GetUserInfoByOpenId ERROR: " + e.Message);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        /// <summary>
        /// 插入新注册用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="mainkey">主密钥</param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public int InsertUserInfo(string username, string password, string mainkey, string openid)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            log.Debug("UserInfoDAO.InsertUserInfo Enter: UserName: " + username + ",Password: " + password + ",OpenId: " + openid);
            try
            {
                DateTime now = DateTime.Now;
                string sql = string.Format("insert into userinfo (USERID, USERNAME, PASSWORD, MAINKEY, CREATETIME, USERSTATE, OPENID) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')", now.ToString("yyyyMMddHHmmssffff"), username, password, mainkey, now.ToString("yyyyMMddHHmmss"), "1", openid);
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log.Error("UserInfoDAO.InsertUserInfo ERROR: " + e.Message);
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
