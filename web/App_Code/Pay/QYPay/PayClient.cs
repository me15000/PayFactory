using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Pay.QYPay
{
    /// <summary>
    /// PayClient 的摘要说明
    /// </summary>
    public class PayClient : IPay
    {
        string appid;
        string appsecret;
        string notifyUrl;


        public PayClient(PayConfig config)
        {
            this.appid = config.Data["appid"];
            this.appsecret = config.Data["appsecret"];
            this.notifyUrl = config.Data["notifyurl"];
        }


        public bool DoCallback(NameValueCollection query, NameValueCollection form, string bodydata, Action<CallbackInfo> action)
        {
            var parameters = new Dictionary<string, string>();
            parameters["status"] = query["status"];
            parameters["account"] = query["account"];
            parameters["order"] = query["order"];
            parameters["orders"] = query["orders"];
            parameters["paytype"] = query["paytype"];
            parameters["money"] = query["money"];
            parameters["body"] = query["body"];
            parameters["ext"] = query["ext"];
            parameters["sign"] = query["sign"];

            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(parameters["order"]) && parameters["status"] == "1" && parameters["account"] == this.appid)
            {
                callback.Status = ResultStatus.Succ;
                callback.OrderNo = parameters["order"];
                callback.AttachData = Base64Helper.Base64Decode(parameters["ext"]);
            }

            action(callback);

            if (callback.Status == ResultStatus.Succ)
            {
                return true;
            }

            return false;
        }

        public void EchoStatus(ResultStatus status)
        {
            if (status == ResultStatus.Succ)
            {
                HttpContext.Current.Response.Write("success");
            }
            else
            {
                HttpContext.Current.Response.Write("fail");
            }
        }

        public PayResult Pay(PayInfo info)
        {
            var url = "http://pay.dkg88.com/api/pay";

            Dictionary<string, string> nvc = new Dictionary<string, string>();

            nvc["account"] = this.appid;
            nvc["order"] = info.OrderNo;
            nvc["paytype"] = "wxwap"; //微信WAP
            //nvc["type"] = "";
            nvc["money"] = ((decimal)(info.Amount) * 0.01m).ToString();
            //nvc["body"] = "";
            nvc["ext"] = Base64Helper.Base64Encode(info.Attach);
            nvc["notify"] = this.notifyUrl;
            nvc["callback"] = info.ReturnUrl;

            nvc["ip"] = info.IP;

            nvc["sign"] = getSign(nvc);


            string ps = CreateParams(nvc);


            var resultString = PostWebRequest(url, ps, Encoding.GetEncoding("utf-8"));
            var result = new PayResult();
            result.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(resultString))
            {

                dynamic obj = JsonConvert.DeserializeObject<dynamic>(resultString);
                if (obj != null)
                {

                    int code = obj.code;
                    if (code == 1)
                    {
                        string payurl = obj.payurl;
                        result.ResultType = PayResultType.Url;
                        result.ResultData = payurl;
                        result.Status = ResultStatus.Succ;

                    }
                }
            }


            return result;
        }

        string CreateParams(Dictionary<string, string> parameters)
        {
            string result = string.Empty;
            var sb = new StringBuilder();
            bool first = true;

            foreach (var item in parameters)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }
                    sb.Append(item.Key + "=" + System.Web.HttpUtility.UrlEncode(item.Value));
                    first = false;
                }
            }
            result = sb.ToString();
            return result;
        }

        string getSign(Dictionary<string, string> dict)
        {
            Dictionary<string, string> poststr = dict.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            var sb = new StringBuilder();
            foreach (var p in poststr)
            {

                if (p.Key.Equals("sign"))
                {
                    continue;
                }
                if (p.Key.Equals("ip"))
                {
                    continue;
                }
                if (p.Key.Equals("ext"))
                {
                    continue;
                }
                if (p.Key.Equals("body"))
                {
                    continue;
                }

                if (p.Key.Equals("type"))
                {
                    continue;
                }



                if (string.IsNullOrEmpty(p.Value))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("&");
                }

                //notify callback

                if (p.Key.Equals("notify") || p.Key.Equals("callback"))
                {
                    sb.Append(p.Key + "=" + UrlEncode(p.Value,Encoding.ASCII));

                }
                else
                {
                    sb.Append(p.Key + "=" + p.Value);

                }

            }


            sb.Append("&" + this.appsecret);


            return GetMD5(sb.ToString());
        }

        static string UrlEncode(string temp, Encoding encoding)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < temp.Length; i++)
            {
                string t = temp[i].ToString();
                string k = HttpUtility.UrlEncode(t, encoding);
                if (t == k)
                {
                    stringBuilder.Append(t);
                }
                else
                {
                    stringBuilder.Append(k.ToUpper());
                }
            }
            return stringBuilder.ToString();
        }

        string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {



            string ret = string.Empty;
            try
            {


                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), dataEncode);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        string GetMD5(string encypStr, string charset = "utf-8")
        {
            return UserMd5(encypStr, 32);

        }

        public static string UserMd5(string str, int code)
        {
            if (code == 16) //16位MD5加密（取32位加密的9~25字符）
            {
                return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower().Substring(8, 16);
            }
            if (code == 32)
            {
                return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower();
            }

            return str;
        }
    }


    class Base64Helper
    {
        /// <summary>
        /// Base64加密，采用utf8编码方式加密
        /// </summary>
        /// <param name="source">待加密的明文</param>
        /// <returns>加密后的字符串</returns>
        public static string Base64Encode(string source)
        {
            return Base64Encode(Encoding.UTF8, source);
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="encodeType">加密采用的编码方式</param>
        /// <param name="source">待加密的明文</param>
        /// <returns></returns>
        public static string Base64Encode(Encoding encodeType, string source)
        {
            string encode = string.Empty;
            byte[] bytes = encodeType.GetBytes(source);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }

        /// <summary>
        /// Base64解密，采用utf8编码方式解密
        /// </summary>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(string result)
        {
            return Base64Decode(Encoding.UTF8, result);
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="encodeType">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(Encoding encodeType, string result)
        {
            string decode = string.Empty;
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encodeType.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }
    }
}