using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Pay.HaiBeiPay
{
    public class PayRequestInfo
    {
        public int Amount { get; set; }
        public string Attach { get; set; }
        public string OrderNo { get; set; }
        public string Name { get; set; }

        public string ReturnUrl { get; set; }
        //public string NotifyUrl { get; set; }

    }

    public class PayRequest
    {
        public string once { get; set; }
        public string attach { get; set; }
        public string data { get; set; }
        public string request_url { get; set; }
    }

    public class PayData
    {
        public string waresName { get; set; }
        public string cpOrderId { get; set; }
        public decimal price { get; set; }
        public string returnUrl { get; set; }
        public string notifyUrl { get; set; }
        public string type { get; set; }
    }
    public class ResHaiBeiHu
    {
        public int status { get; set; }
        public string result { get; set; }
        public string errmsg { get; set; }
    }
    public class ResHaiBeiHu_Result
    {
        public string orderSn { get; set; }
        public string payUrl { get; set; }
    }
    /// <summary>
    /// PayClient 的摘要说明
    /// </summary>
    public class PayClient : IPay
    {
        string appid = null;
        string secret_key = null;
        string notifyUrl = null;

        const string __format = "JSON";
        const string __method = "sdk.web";
        const string __sign_type = "MD5";
        const string __version = "1.0.0";
        const string __request_url = "https://api.haibeifu.com/method/";

        public PayClient(PayConfig config)
        {
            this.appid = config.Data["appid"];
            this.secret_key = config.Data["appsecret"];
            this.notifyUrl = config.Data["notifyurl"];
        }


        public PayResult Pay(PayInfo info)
        {
            string payurl = DoPay(new PayRequestInfo()
            {
                Amount = info.Amount,
                Attach = info.Attach,
                Name = info.Name,
                OrderNo = info.OrderNo,
                ReturnUrl = info.ReturnUrl
            });

            var result = new PayResult();
            if (!string.IsNullOrEmpty(payurl))
            {
                result.ResultType = PayResultType.Url;
                result.ResultData = payurl;
                result.Status = ResultStatus.Succ;
            }
            else
            {
                result.Status = ResultStatus.Fail;
            }

            return result;
        }

        public bool DoCallback(NameValueCollection query, NameValueCollection form, string bodydata, Action<CallbackInfo> action)
        {

            NameValueCollection Request = new NameValueCollection();
            foreach (var key in query.AllKeys)
            {
                Request[key] = query[key];
            }

            foreach (var key in form.AllKeys)
            {
                Request[key] = form[key];
            }


            string order_id = Request["order_id"];
            string pay_state = Request["pay_state"] ?? string.Empty;
            string agent_order = Request["agent_order"];
            string pay_fee = Request["pay_fee"];
            string time = Request["time"];
            string remark = Request["remark"];
            string additional = Request["additional"];
            string attach = Request["attach"];
            string sign = Request["sign"] ?? string.Empty;

            Hashtable ht = new Hashtable();
            ht.Add("order_id", order_id);
            ht.Add("pay_state", pay_state);
            ht.Add("agent_order", agent_order);
            ht.Add("pay_fee", pay_fee);
            ht.Add("time", time);
            ht.Add("remark", remark);
            ht.Add("additional", additional);
            ht.Add("attach", attach);
            ht.Add("key", this.secret_key);
            string md5sign = CreateSign(ht);

            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(agent_order) && pay_state == "10000" && md5sign == sign.ToUpper())
            {
                callback.Status = ResultStatus.Succ;
                callback.OrderNo = agent_order;
                callback.AttachData = Base64Helper.Base64Decode(attach);
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
                HttpContext.Current.Response.Write("OK");
            }
            else
            {
                HttpContext.Current.Response.Write("fail");
            }
        }



        const string __pay_type = "wechat_h5";

        string DoPay(PayRequestInfo info)
        {
            PayData pd = new PayData()
            {
                waresName = info.Name,
                cpOrderId = info.OrderNo,
                price = Convert.ToDecimal((info.Amount * 1.0 / 100).ToString("f2")),
                returnUrl = info.ReturnUrl,
                notifyUrl = this.notifyUrl,
                type = __pay_type
            };
            PayRequest pr = new PayRequest()
            {
                once = Guid.NewGuid().ToString(),
                attach = info.Attach,
                data = Newtonsoft.Json.JsonConvert.SerializeObject(pd)
            };


            return Pay(pr);
        }

        string Pay(PayRequest req)
        {
            try
            {
                string result = string.Empty;
                #region 参数
                Hashtable ht = new Hashtable();
                ht.Add("appid", this.appid);
                ht.Add("timestamp", GetTimestamp().ToString());
                ht.Add("once", req.once);
                ht.Add("attach", Base64Helper.Base64Encode(req.attach));
                ht.Add("format", __format);
                ht.Add("method", __method);
                ht.Add("sign_type", __sign_type);
                //sign
                ht.Add("version", __version);
                ht.Add("data", req.data);
                ht.Add("key", this.secret_key);
                string sign = CreateSign(ht);
                ht.Add("sign", sign);


                #endregion

                #region 请求接口                
                string res = PostWebRequest(__request_url, CreateParams(ht), Encoding.UTF8);
                //string res = req.request_url+"?"+ CreateParams(ht);               
                if (!string.IsNullOrEmpty(res))
                {

                    ResHaiBeiHu data = Newtonsoft.Json.JsonConvert.DeserializeObject<ResHaiBeiHu>(res);
                    if (data != null)
                    {
                        ResHaiBeiHu_Result rere = Newtonsoft.Json.JsonConvert.DeserializeObject<ResHaiBeiHu_Result>(data.result);
                        if (rere != null)
                        {
                            result = rere.payUrl;
                        }
                    }
                }
                #endregion
                return result;
            }
            catch (Exception e)
            {
                throw (e);
            }

        }



        long GetTimestamp()
        {

            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }


        string CreateSign(Hashtable parameters)
        {
            string result = string.Empty;
            StringBuilder sb = new StringBuilder();
            ArrayList akeys = new ArrayList(parameters.Keys);
            akeys.Sort();

            bool first = true;
            foreach (string k in akeys)
            {
                if (k.Equals("sign"))
                {
                    continue;
                }

                string v = (string)parameters[k];
                if (!string.IsNullOrEmpty(v))//&& "key".CompareTo(k) != 0
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }
                    sb.Append(k + "=" + v);
                    first = false;
                }
            }
            result = sb.ToString();
            string sign = GetMD5(result).ToUpper();
            return sign;
        }

        string GetMD5(string encypStr, string charset = "utf-8")
        {
            //1.创建一个Md5对象
            MD5 md5Obj = MD5.Create();

            //1.1把字符串转换为一个byte数组
            byte[] byts = System.Text.Encoding.GetEncoding(charset).GetBytes(encypStr);
            //2.使用md5进行字符串处理
            //2.把字符串变为一个byte[]
            //对于中文或者某些字符，采用不同的编码生成的byte[]是不一样的，
            //所以造成了采用不同编码生成的md5值不一样的情况。
            byte[] md5Byts = md5Obj.ComputeHash(byts);

            //3.释放资源
            md5Obj.Clear();//类似于Dispose();
            md5Obj.Dispose();

            StringBuilder sb = new StringBuilder();
            //4.返回处理以后的结果
            for (int i = 0; i < md5Byts.Length; i++)
            {
                //x2:把每个数字转换为16进制，并保留两位数字。
                sb.Append(md5Byts[i].ToString("x2"));
            }

            return sb.ToString();

        }

        string CreateParams(Hashtable parameters)
        {
            string result = string.Empty;
            ArrayList akeys = new ArrayList(parameters.Keys);
            akeys.Sort();

            bool first = true;

            var sb = new StringBuilder();

            foreach (string k in akeys)
            {


                string v = (string)parameters[k];

                if (!string.IsNullOrEmpty(v))
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }

                    sb.Append(k + "=" + HttpUtility.UrlEncode(v));

                    first = false;
                }
            }

            result = sb.ToString();
            return result;
        }
        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="paramData"></param>
        /// <param name="dataEncode"></param>
        /// <returns></returns>
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
