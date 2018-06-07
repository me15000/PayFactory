using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Web;
using System.Collections.Specialized;

namespace Pay.WXPay
{









    public class PayResultInfo
    {
        public string Status { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public Dictionary<string, string> FullData { get; set; }
    }



    public class PayCallbackInfo
    {
        public string Status { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }

    public class DoPayInfo
    {

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public int TotalFee { get; set; }

        public string Attach { get; set; }

        public string IP { get; set; }

        public string OrderName { get; set; }

    }

    public class ConfigInfo
    {

        public string AppID { get; set; }
        public string AppSecret { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string MCH_ID { get; set; }
        /// <summary>
        /// 通知地址
        /// </summary>
        public string NotifyUrl { get; set; }
        public string WapUrl { get; set; }
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string ServerIP { get; set; }
        /// <summary>
        /// 商户Key
        /// </summary>
        public string MCH_KEY { get; set; }
    }



    /// <summary>
    /// PayHelper 的摘要说明
    /// </summary>
    public class PayClient : IPay
    {



        ConfigInfo config;

        public PayClient(PayConfig config)
        {
            this.config = new ConfigInfo()
            {
                AppID = config.Data["appid"],
                AppSecret = config.Data["appsecret"],
                MCH_ID = config.Data["mch_id"],
                MCH_KEY = config.Data["mch_key"],
                NotifyUrl = config.Data["notifyurl"],
                ServerIP = config.Data["serverip"],
                WapUrl = config.Data["wapurl"]
            };
        }




        public PayResult Pay(PayInfo info)
        {
            var result = DoPay(new DoPayInfo
            {

                IP = info.IP,
                OrderName = info.Name,
                TotalFee = info.Amount,
                OrderNo = info.OrderNo,
                Attach = info.Attach
            });

            var ret = new PayResult();
            if (result.Status == "SUCC")
            {
                ret.Status = ResultStatus.Succ;
                ret.Data = new Dictionary<string, object>();

                ret.Data["appId"] = result.Data["appId"];
                ret.Data["timeStamp"] = result.Data["timeStamp"];
                ret.Data["nonceStr"] = result.Data["nonceStr"];
                ret.Data["package"] = result.Data["package"];
                ret.Data["signType"] = result.Data["signType"];
                ret.Data["paySign"] = result.Data["paySign"];
                ret.Data["mweb_url"] = result.FullData["mweb_url"];

                ret.ResultData = result.FullData["mweb_url"];
                ret.ResultType = PayResultType.Url;

            }
            else
            {
                ret.Status = ResultStatus.Fail;
            }

            return ret;
        }


        public bool DoCallback(NameValueCollection query, NameValueCollection form, string bodydata, Action<CallbackInfo> action)
        {

            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;



            var info = ParseCallbackInfo(bodydata);

            if (info != null)
            {
                if (info.Status == "SUCC")
                {
                    bool succ = CheckCallbackInfo(info);
                    if (succ)
                    {
                        if (info.Data["result_code"] == "SUCCESS")
                        {
                            string out_trade_no = info.Data["out_trade_no"];
                            if (!string.IsNullOrEmpty(out_trade_no))
                            {
                                callback.Status = ResultStatus.Succ;
                                callback.OrderNo = out_trade_no;
                                callback.AttachData = info.Data["attach"];
                            }
                        }
                    }
                }
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
                HttpContext.Current.Response.Write("SUCCESS");
            }
            else
            {
                HttpContext.Current.Response.Write("FAIL");
            }

        }


        public bool CheckCallbackInfo(PayCallbackInfo info)
        {

            var keys = info.Data.Keys;
            var dict = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                if (key.Equals("sign"))
                {
                    continue;
                }
                dict[key] = info.Data[key];
            }

            //string[] keys = new string[] { "appid", "bank_type", "cash_fee", "fee_type", "is_subscribe", "mch_id", "nonce_str", "openid", "out_trade_no", "result_code", "return_code", "time_end", "total_fee", "trade_type", "transaction_id" };
            //foreach (var key in keys)
            //{
            //    dict[key] = info.Data[key];
            //}


            string sign = GetSignString(dict, config.MCH_KEY);

            if (sign.Equals(info.Data["sign"], StringComparison.OrdinalIgnoreCase))
            {

                return true;
            }

            return false;
        }

        PayResultInfo DoPay(DoPayInfo info)
        {
            string appid = config.AppID;
            string mch_id = config.MCH_ID;

            string body = info.OrderName; //商品描述

            //string openid = order.OpenID;
            string notify_url = config.NotifyUrl; //通知地址

            string spbill_create_ip = info.IP;//终端IP

            string total_fee = info.TotalFee.ToString();//总金额，单位分


            //订单号
            string out_trade_no = info.OrderNo;



            string scene_info = @"{""h5_info"": {""type"":""Wap"",""wap_url"": """ + config.WapUrl + @""",""wap_name"": ""充值""}}";

            var dic = new Dictionary<string, string>
            {
                {"appid", appid},
                {"mch_id", mch_id},
                {"nonce_str", GetRandomString(20)/*Random.Next().ToString()*/},
                {"scene_info",scene_info },
                {"body",body},
                {"out_trade_no",out_trade_no},//商户自己的订单号码  
                {"total_fee",total_fee},
                {"spbill_create_ip",spbill_create_ip},
                {"notify_url",notify_url},//异步通知的地址，不能带参数  
                {"trade_type","MWEB" },  //H5支付的交易类型为MWEB
                {"attach", info.Attach }
                //{"openid",openid}
            };

            dic.Add("sign", GetSignString(dic, config.MCH_KEY));

            var sb = new StringBuilder();
            sb.Append("<xml>");
            foreach (var d in dic)
            {
                sb.Append("<" + d.Key + "><![CDATA[" + d.Value + "]]></" + d.Key + ">");
            }
            sb.Append("</xml>");


            string postXML = sb.ToString();


            var enc = Encoding.GetEncoding("UTF-8");

            string resultXML = null;

            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            using (var wc = new WebClient())
            {
                byte[] data = enc.GetBytes(postXML);

                byte[] result = wc.UploadData(url, data);

                if (result != null)
                {
                    resultXML = enc.GetString(result);

                }
            }

            if (string.IsNullOrEmpty(resultXML))
            {
                return null;
            }


            var xml = new XmlDocument();


            xml.LoadXml(resultXML);

            if (xml == null)
            {
                return null;
            }

            var orderResult = new PayResultInfo();

            string return_code = xml.SelectSingleNode("/xml/return_code").InnerText;

            if (return_code == "SUCCESS")
            {


                var dict = new Dictionary<string, string>();

                var nodes = xml.DocumentElement.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    dict[node.Name] = node.InnerText;
                }


                if (dict["result_code"] == "SUCCESS")
                {
                    orderResult.Status = "SUCC";

                    var res = new Dictionary<string, string>
                    {
                        {"appId", config.AppID },
                        {"timeStamp", GetTimeStamp() },
                        {"nonceStr", dict["nonce_str"] },
                        {"package",  "prepay_id=" + dict["prepay_id"] },
                        {"signType", "MD5"}
                    };

                    res.Add("paySign", GetSignString(res, config.MCH_KEY));

                    orderResult.FullData = dict;

                    //在服务器上签名                      
                    orderResult.Data = res;
                }
            }

            return orderResult;
        }



        public PayCallbackInfo ParseCallbackInfo(string xmlString)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            if (xml == null)
            {
                return null;
            }

            var callbackInfo = new PayCallbackInfo();

            string return_code = xml.SelectSingleNode("/xml/return_code").InnerText;

            if (return_code == "SUCCESS")
            {
                callbackInfo.Status = "SUCC";

                var dict = new Dictionary<string, string>();

                var nodes = xml.DocumentElement.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    dict[node.Name] = node.InnerText;
                }
                callbackInfo.Data = dict;

            }
            else if (return_code == "FAIL")
            {

                callbackInfo.Status = "FAIL";
                callbackInfo.Data = null;
            }

            return callbackInfo;
        }

        public static string SuccessCallbackInfo
        {
            get
            {
                var dic = new Dictionary<string, string>
                {
                    {"return_code", "SUCCESS"},
                    {"return_msg","OK"}
                };

                var sb = new StringBuilder();
                sb.Append("<xml>");
                foreach (var d in dic)
                {
                    sb.Append("<" + d.Key + ">" + d.Value + "</" + d.Key + ">");
                }
                sb.Append("</xml>");

                return sb.ToString();
            }
        }


        string GetRandomString(int CodeCount)
        {
            string allChar = "1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,i,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            string[] allCharArray = allChar.Split(',');
            string RandomCode = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < CodeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(temp * i * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(allCharArray.Length - 1);
                while (temp == t)
                {
                    t = rand.Next(allCharArray.Length - 1);
                }
                temp = t;
                RandomCode += allCharArray[t];
            }

            return RandomCode;
        }



        string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }


        string GetSignString(Dictionary<string, string> dic, string key)
        {
            //string key = System.Web.Configuration.WebConfigurationManager.AppSettings["key"].ToString();//商户平台 API安全里面设置的KEY  32位长度  
            //排序  
            dic = dic.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            //连接字段  
            var sign = dic.Aggregate("", (current, d) => current + (d.Key + "=" + d.Value + "&"));



            sign += "key=" + key;

            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            sign = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(sign))).Replace("-", null);
            return sign;
        }


    }


}