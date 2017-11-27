using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using log4net;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Configuration;
using System.IO;
using System.Text;
using PwdManager.DAO;
using System.Data;

namespace WebSite.Controllers
{
    public class UserController : ApiController
    {
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().GetType());

        [HttpPost]
        [Route("~/api/User/GetOpenId")]
        public HttpResponseMessage GetOpenId()
        {
            try
            {
                string appid = ConfigurationManager.AppSettings["APPID"].ToString();
                string appsecret = ConfigurationManager.AppSettings["APPSecret"].ToString();

                string jsonStr = Request.Content.ReadAsStringAsync().Result;
                log.Debug("User-GetOpenId Enter: " + jsonStr);

                JObject input = JObject.Parse(jsonStr);
                if (input["code"] == null)
                {
                    log.Error("User-GetOpenId Error: 输入code为空");
                    JObject result = new JObject();
                    result.Add("RetCode", "0005");
                    result.Add("RetMsg", "输入code为空");
                    return Request.CreateResponse(result);
                }
                string code = input["code"].ToString();

                string url = ConfigurationManager.AppSettings["WXOpenIdUrl"].ToString();
                url += string.Format("?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code", appid, appsecret, code);
                log.Debug("请求URL为: " + url);

                //向腾讯用code换openid
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response != null)
                {
                    string retStr = "";
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default))
                    {
                        retStr = sr.ReadToEnd();
                    }
                    log.Debug("腾讯响应结果：" + retStr);
                    JObject WXresp = JObject.Parse(retStr);
                    if (WXresp.Property("openid") != null)
                    {
                        string openid = WXresp["openid"].ToString();
                        string username = "";
                        //查询数据库，看该openid是否为现有用户
                        DataTable dt = UserInfoDAO.GetInstance().GetUserInfoByOpenId(openid);
                        if (dt.Rows.Count > 0)
                        {
                            username = dt.Rows[0]["USERNAME"].ToString();
                        }

                        JObject result = new JObject();
                        result.Add("RetCode", "0000");
                        result.Add("RetMsg", "");
                        result.Add("OpenId", openid);
                        result.Add("UserName", username);
                        return Request.CreateResponse(result);
                    }
                    else
                    {
                        log.Error("User-GetOpenId Error: 未能获取openid");
                        JObject result = new JObject();
                        result.Add("RetCode", "0003");
                        result.Add("RetMsg", "未能获取openid");
                        return Request.CreateResponse(result);
                    }
                }
                else
                {
                    log.Error("User-GetOpenId Error: 网络请求异常");
                    JObject result = new JObject();
                    result.Add("RetCode", "0004");
                    result.Add("RetMsg", "网络请求异常");
                    return Request.CreateResponse(result);
                }
            }
            catch (Exception e)
            {
                log.Error("User-GetOpenId Error: 后台异常. " + e.Message);
                JObject result = new JObject();
                result.Add("RetCode", "0002");
                result.Add("RetMsg", "后台异常");
                return Request.CreateResponse(result);
            }
        }

        [HttpPost]
        [Route("~/api/User/Register")]
        public HttpResponseMessage Register()
        {
            try
            {
                string jsonStr = Request.Content.ReadAsStringAsync().Result;
                log.Debug("User-Register Enter: " + jsonStr);
                JObject input = JObject.Parse(jsonStr);
                if (input["UserName"] == null)
                {
                    log.Error("User-Register Error: 输入用户名为空");
                    JObject result = new JObject();
                    result.Add("RetCode", "0003");
                    result.Add("RetMsg", "输入用户名为空");
                    return Request.CreateResponse(result);
                }
                string username = input["UserName"].ToString();
                if (input["Password"] == null)
                {
                    log.Error("User-Register Error: 输入密码为空");
                    JObject result = new JObject();
                    result.Add("RetCode", "0004");
                    result.Add("RetMsg", "输入密码为空");
                    return Request.CreateResponse(result);
                }
                string password = input["Password"].ToString();
                if (input["OpenId"] == null)
                {
                    log.Error("User-Register Error: 输入OpenId为空");
                    JObject result = new JObject();
                    result.Add("RetCode", "0005");
                    result.Add("RetMsg", "输入OpenId为空");
                    return Request.CreateResponse(result);
                }
                string openid = input["OpenId"].ToString();
                string mainkey = "";
                if (input["MainKey"] != null)
                {
                    mainkey = input["MainKey"].ToString();
                }

                //查询是否为现有用户
                DataTable userinfo = UserInfoDAO.GetInstance().GetUserInfoByOpenId(openid);
                if (userinfo.Rows.Count > 0)
                {
                    log.Error("User-Register Error: 该openid已有用户");
                    JObject result = new JObject();
                    result.Add("RetCode", "0006");
                    result.Add("RetMsg", "已注册用户");
                    return Request.CreateResponse(result);
                }

                //插入数据库
                int sqlre = UserInfoDAO.GetInstance().InsertUserInfo(username, password, mainkey, openid);
                if (sqlre == 1)
                {
                    log.Debug("User-Register : 插入用户信息成功");
                    JObject result = new JObject();
                    result.Add("RetCode", "0000");
                    result.Add("RetMsg", "");
                    return Request.CreateResponse(result);
                }
                else
                {
                    log.Error("User-Register Error: 插入数据库失败");
                    JObject result = new JObject();
                    result.Add("RetCode", "0007");
                    result.Add("RetMsg", "注册失败");
                    return Request.CreateResponse(result);
                }
            }
            catch (Exception e)
            {
                JObject result = new JObject();
                result.Add("RetCode", "0002");
                result.Add("RetMsg", "后台异常");
                return Request.CreateResponse(result);
            }
        }
    }
}