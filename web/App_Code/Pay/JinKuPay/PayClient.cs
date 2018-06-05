using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Pay.JinKuPay
{
    public class PayRequest
    {
        public int Amount { get; set; }
        public string Attach { get; set; }
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



        public string Pay(PayRequest info)
        {
            decimal orderAmt = (decimal)(info.Amount) / 100m;

            var dic_params = new Dictionary<string, string>();
            dic_params["merid"] = this.appid;
            dic_params["mchOrderNo"] = info.OrderNo;
            dic_params["paytype"] = "1003";
            dic_params["productName"] = info.Name;
            dic_params["price"] = orderAmt.ToString();
            dic_params["synUrl"] = this.notifyUrl;
            dic_params["returnUrl"] = info.ReturnUrl;
            dic_params["attach"] = info.Attach;

            dic_params["sign"] = GetSign(dic_params);

            var sb_params = new StringBuilder();
            foreach (var p in dic_params)
            {
                if (string.IsNullOrEmpty(p.Value))
                {
                    continue;
                }

                if (sb_params.Length > 0)
                {
                    sb_params.Append("&");
                }

                sb_params.Append(p.Key + "=" + HttpUtility.UrlEncode(p.Value));
            }

            string data = sb_params.ToString();

            return "http://www.pay365.top:8001/upay.aspx?" + data;


        }


        string GetSign(Dictionary<string, string> dict)
        {
            Dictionary<string, string> poststr = dict.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            var sb = new StringBuilder();
            foreach (var p in poststr)
            {

                if (p.Key.Equals("sign"))
                {
                    continue;
                }
                if (p.Key.Equals("attach"))
                {
                    continue;
                }
                if (p.Key.Equals("synUrl"))
                {
                    continue;
                }
                if (p.Key.Equals("returnUrl"))
                {
                    continue;
                }


                string val = p.Value as string ?? string.Empty;

                if (string.IsNullOrEmpty(val))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("&");
                }

                sb.Append(p.Key + "=" + val);
            }


            sb.Append("&key=" + this.key);

            return UserMd5(sb.ToString(), 32).ToUpper();
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


        public PayResult Pay(PayInfo info)
        {
            string payContent = Pay(new PayRequest()
            {
                Amount = info.Amount,
                Attach = info.Attach,
                Name = info.Name,
                OrderNo = info.OrderNo,
                ReturnUrl = info.ReturnUrl
            });

            var result = new PayResult();
            if (!string.IsNullOrEmpty(payContent))
            {
                result.ResultType = PayResultType.Url;
                result.ResultData = payContent;
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



            var dict = new Dictionary<string, string>();
            dict["res"] = query["res"];
            dict["mchOrderNo"] = query["mchOrderNo"];
            dict["order_no"] = query["order_no"];
            dict["price"] = query["price"];
            dict["payType"] = query["payType"];
            dict["attach"] = query["attach"];
            dict["sign"] = query["sign"];


            string sign = GetSign(dict);


            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(dict["mchOrderNo"]) 
                //&& sign.Equals(dict["sign"]) 
                && dict["res"].Equals("success"))
            {
                callback.Status = ResultStatus.Succ;
                callback.OrderNo = dict["mchOrderNo"];
                callback.AttachData = dict["attach"];
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
