using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Pay.SFTPay
{
    public class PayRequest
    {
        public int Amount { get; set; }
        public string Ext { get; set; }
        public string OrderNo { get; set; }
        public string Name { get; set; }

        public string ReturnUrl { get; set; }
        //public string NotifyUrl { get; set; }

    }



    public class PayClient : IPay
    {
        string appid = null;
        string key = null;
        string notifyUrl = null;


        public PayClient(PayConfig config)
        {
            this.appid = config.Data["appid"];
            this.key = config.Data["appsecret"];
            this.notifyUrl = config.Data["notifyurl"];
        }

        public PayClient(string appid, string key)
        {
            this.appid = appid;
            this.key = key;
        }

        public string Pay(PayRequest info)
        {
            int orderAmt = info.Amount;

            var dic_params = new Dictionary<string, string>();
            dic_params["appid"] = this.appid;

            dic_params["amount"] = orderAmt.ToString();



            dic_params["itemname"] = info.Name;
            dic_params["ordersn"] = this.appid + "_" + info.OrderNo;

            dic_params["orderdesc"] = "aaa";
            dic_params["notifyurl"] = this.notifyUrl;


            Dictionary<string, string> dic_SortedByKey = dic_params.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

            var sb_params = new StringBuilder();
            foreach (var p in dic_SortedByKey)
            {
                if (sb_params.Length > 0)
                {
                    sb_params.Append("|");
                }
                sb_params.Append(p.Value as string ?? string.Empty);
            }

            sb_params.Append("|" + this.key);
            string sign = GetMD5(sb_params.ToString());
            dic_params["sign"] = sign;
            dic_params["payway"] = "weixin";
            dic_params["paytype"] = "wap";
            dic_params["returnurl"] = info.ReturnUrl;
            dic_params["ext"] = info.Ext;

            Dictionary<string, string> poststr = dic_params.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            var sb = new StringBuilder();
            foreach (var p in poststr)
            {


                string val = p.Value as string ?? string.Empty;

                if (string.IsNullOrEmpty(val))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("&");
                }

                sb.Append(p.Key + "=" + HttpUtility.UrlEncode(val));

            }
            //api.cdglory.cn 替换 api.qzxczs.cn

            string data = sb.ToString();




            string result = PostWebRequest("http://api.ykhet.cn", data, Encoding.UTF8);

            try
            {
                var r = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
                if (r != null)
                {
                    if (r.status == -1)
                    {
                        HttpContext.Current.Response.Write(data + "\r\n" + result + "\r\n");

                    }
                    else
                    {
                        string url = r.data;

                        return url;
                    }

                }
            }
            catch (Exception ex)
            {

                HttpContext.Current.Response.Write(result);
            }

            

            return null;
        }




        static string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string result = string.Empty;
            try
            {
                byte[] bytes = dataEncode.GetBytes(paramData);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = (long)bytes.Length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), dataEncode);
                result = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                requestStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        static string GetMD5(string encypStr, string charset = "utf-8")
        {
            string retStr;
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch (Exception ex)
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
                Console.WriteLine(ex);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "");
            return retStr;
        }

        public PayResult Pay(PayInfo info)
        {
            string payurl = Pay(new PayRequest()
            {
                Amount = info.Amount,
                Ext = info.Attach,
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
            string payorderno = query["ordersn"];
            string reserved = query["ext"] ?? string.Empty;



            var dict = new Dictionary<string, string>();

            dict["ordersn"] = payorderno.Replace(this.appid + "_", string.Empty);
            dict["ext"] = reserved;
            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(dict["ordersn"]))
            {
                callback.Status = ResultStatus.Succ;
                callback.OrderNo = dict["ordersn"];
                callback.AttachData = dict["ext"];
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
    }
}
