using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
namespace Pay.Alipay
{
    public class ConfigInfo
    {
        public string PID { get; set; }
        public string SellerID
        {
            get { return this.PID; }
        }
        public string SCode { get; set; }

        public string NotifyUrl { get; set; }

        public const string ProductUrl = "http://www.baidu.com/";

        // 签名方式
        public const string sign_type = "MD5";

        // 调试用，创建TXT日志文件夹路径，见AlipayCore.cs类中的LogResult(string sWord)打印方法。
        public static string log_path = HttpRuntime.AppDomainAppPath.ToString() + "log\\";

        // 字符编码格式 目前支持utf-8
        public const string input_charset = "utf-8";

        // 支付类型 ，无需修改
        public const string payment_type = "1";

        // 调用的接口名，无需修改
        public const string service = "alipay.wap.create.direct.pay.by.user";
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



        public string OrderName { get; set; }

        public string ReturnUrl { get; set; }

    }


    /// <summary>
    /// PayClient 的摘要说明
    /// </summary>
    public class PayClient : IPay
    {

        ConfigInfo config;

        public PayClient(PayConfig config)
        {
            this.config = new ConfigInfo()
            {
                PID = config.Data["pid"],
                SCode = config.Data["appsecret"],
                NotifyUrl = config.Data["notifyurl"]

            };
        }



        public bool DoCallback(NameValueCollection query, NameValueCollection form, string bodydata, Action<CallbackInfo> action)
        {
            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            var sPara = GetRequestPost(form);

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                var aliNotify = new Notify(config);

                bool verifyResult = aliNotify.Verify(sPara, form["notify_id"], form["sign"]);

                if (verifyResult)//验证成功
                {

                    //商户订单号
                    string out_trade_no = form["out_trade_no"];

                    //支付宝交易号
                    string trade_no = form["trade_no"];

                    //交易状态
                    string trade_status = form["trade_status"];

                    if (form["trade_status"] == "TRADE_SUCCESS")
                    {
                        callback.Status = ResultStatus.Succ;
                        callback.OrderNo = out_trade_no;
                        callback.AttachData = null;
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
                HttpContext.Current.Response.Write("success");
            }
            else
            {
                HttpContext.Current.Response.Write("fail");
            }
        }

        public PayResult Pay(PayInfo info)
        {
            string html = DoPay(new DoPayInfo
            {
                OrderName = info.Name,
                OrderNo = info.OrderNo,
                TotalFee = info.Amount,
                ReturnUrl = info.ReturnUrl
            });

            var payResult = new PayResult()
            {
                Data = null,
                ResultData = html,
                ResultType = PayResultType.Html,
                Status = ResultStatus.Succ
            };

            return payResult;
        }



        SortedDictionary<string, string> GetRequestPost(NameValueCollection form)
        {
            int i = 0;
            var sArray = new SortedDictionary<string, string>();
            var coll = form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], form[requestItem[i]]);
            }

            return sArray;
        }

        string DoPay(DoPayInfo info)
        {


            ////////////////////////////////////////////请求参数////////////////////////////////////////////


            //商户订单号，商户网站订单系统中唯一订单号，必填
            string out_trade_no = info.OrderNo;

            //订单名称，必填
            string subject = info.OrderName;

            //付款金额，必填
            string total_fee = ((decimal)info.TotalFee / 100m).ToString();

            //收银台页面上，商品展示的超链接，必填
            string show_url = ConfigInfo.ProductUrl;

            //商品描述，可空
            string body = string.Empty;



            ////////////////////////////////////////////////////////////////////////////////////////////////

            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", this.config.PID);
            sParaTemp.Add("seller_id", this.config.SellerID);

            sParaTemp.Add("_input_charset", ConfigInfo.input_charset.ToLower());
            sParaTemp.Add("service", ConfigInfo.service);
            sParaTemp.Add("payment_type", ConfigInfo.payment_type);

            sParaTemp.Add("notify_url", this.config.NotifyUrl);

            if (!string.IsNullOrEmpty(info.ReturnUrl))
            {
                sParaTemp.Add("return_url", info.ReturnUrl);
            }


            sParaTemp.Add("out_trade_no", out_trade_no);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("total_fee", total_fee);
            sParaTemp.Add("show_url", show_url);
            sParaTemp.Add("app_pay", "Y");//启用此参数可唤起钱包APP支付。
            sParaTemp.Add("body", body);
            //其他业务参数根据在线开发文档，添加参数.文档地址:https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.2Z6TSk&treeId=60&articleId=103693&docType=1
            //如sParaTemp.Add("参数名","参数值");



            var submit = new Submit(this.config);

            //建立请求
            string sHtmlText = submit.BuildRequest(sParaTemp, "get", "确认");


            return sHtmlText;

        }














    }

}