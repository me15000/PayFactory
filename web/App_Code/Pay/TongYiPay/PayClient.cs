using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Pay.TongYiPay
{
    /// <summary>
    /// PayClient 的摘要说明
    /// </summary>
    public class PayClient:IPay
    {

        string appid = null;
        string appsecret = null;
        string notifyUrl = null;
        string tongdao = null;

        public PayClient(PayConfig config)
        {
            this.appid = config.Data["appid"];
            this.appsecret = config.Data["appsecret"];
            this.notifyUrl = config.Data["notifyurl"];
            this.tongdao = config.Data["tongdao"];
        }

        public bool DoCallback(NameValueCollection query, NameValueCollection form, string bodydata, Action<CallbackInfo> action)
        {
            var dict = new Dictionary<string, string>();
            dict["memberid"] = query["memberid"];
            dict["orderid"] = query["orderid"];
            dict["amount"] = query["amount"];
            dict["datetime"] = query["datetime"];
            dict["returncode"] = query["returncode"];
            dict["sign"] = query["sign"];


            string signString = "amount=" + dict["amount"] 
                + "&datetime=" + dict["datetime"] 
                + "&memberid=" + dict["memberid"] 
                + "&orderid=" + dict["orderid"] 
                + "&returncode=" + dict["returncode"]
                + "&key=" + this.appsecret;

            string sign = UserMd5(signString,32);




            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(dict["orderid"])
                //&& sign.Equals(dict["sign"])
                && dict["returncode"].Equals("00"))
            {
                callback.Status = ResultStatus.Succ;
                callback.OrderNo = dict["orderid"];
                callback.AttachData = null;
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
                HttpContext.Current.Response.Write("ok");
            }
            else
            {
                HttpContext.Current.Response.Write("fail");
            }
        }

        public PayResult Pay(PayInfo info)
        {
            

            var dict = new Dictionary<string, string>();


            dict["pay_memberid"] = this.appid;
            dict["pay_orderid"] = info.OrderNo;
            dict["pay_amount"] = ((decimal)(info.Amount) * 0.01m).ToString("F2");
            dict["pay_applydate"] =DateTime.Now.ToString();


            dict["pay_bankcode"] = "Wxzf";
            dict["pay_notifyurl"] = this.notifyUrl;
            dict["pay_callbackurl"] = info.ReturnUrl;

            dict["tongdao"] = this.tongdao;


            dict["pay_md5sign"] = GetSign(dict);


            var sb = new StringBuilder();
            sb.Append(@"<!doctype html>\r\n");
            sb.Append(@"<html>");
            sb.Append(@"<head>");
            sb.Append(@"    <meta charset=""utf8"">");
            sb.Append(@"    <title>正在转到付款页</title>");
            sb.Append(@"</head>");
            sb.Append(@"<body onLoad=""document.pay.submit()"">");
            sb.Append(@"    <form name=""pay"" action=""http://www.linkillybb.cn/pay_Index.html"" method=""post"">");

            foreach (var item in dict)
            {
                sb.Append(@"        <input type=""hidden"" name="""+item.Key+@""" value=""" + item.Value + @""">");
            }

            sb.Append(@"    </form>");
            sb.Append(@"</body>");
            sb.Append(@"</html>");

            string payContent = sb.ToString();



            var result = new PayResult();
            if (!string.IsNullOrEmpty(payContent))
            {
                result.ResultType = PayResultType.Html;
                result.ResultData = payContent;
                result.Status = ResultStatus.Succ;
            }
            else
            {
                result.Status = ResultStatus.Fail;
            }

            return result;
        }



        string GetSign(Dictionary<string, string> dict)
        {

            string signString = "pay_amount=" + dict["pay_amount"]
                + "&pay_applydate=" + dict["pay_applydate"]
                + "&pay_bankcode=" + dict["pay_bankcode"]
                + "&pay_callbackurl=" + dict["pay_callbackurl"]
                + "&pay_memberid=" + dict["pay_memberid"]
                + "&pay_notifyurl=" + dict["pay_notifyurl"]
                + "&pay_orderid=" + dict["pay_orderid"]
                + "&key=" + this.appsecret;

            return UserMd5(signString, 32).ToUpper();
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

}