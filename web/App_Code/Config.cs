using System;
using System.Collections.Generic;
using System.ComponentModel;







public class Config
{

    private static Dictionary<string, PayConfig> PayDict = new Dictionary<string, PayConfig>() {
        {
            "weixin.klzskt",
            new PayConfig{
                Name = "aa",
                Key = "weixin.klzskt",
                ClientType = "Pay.WXPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"mch_id" ,"aa"},
                    {"mch_key" ,"aa"},
                    {"name" ,"支付"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.klzskt"},
                    {"wapurl" ,"http://pay.h5.fun5.cn/"},
                    {"serverip" ,"0.0.0.0"}
                }
            }
        },
        {
            "alipay.shwenli",
            new PayConfig{
                Name = "aa-aa",
                Key = "alipay.shwenli",
                ClientType = "Pay.Alipay.PayClient",
                Data = new Dictionary<string, string> {
                    //{"appid" ,"2017063007608308"},
                    {"pid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/alipay.shwenli"},


                }
            }
        }
        ,
        {
            "weixin.jiaoyou",
            new PayConfig{
                Name = "aa",
                Key = "weixin.jiaoyou",
                ClientType = "Pay.SFTPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.jiaoyou"}
                }
            }
        },

        {
            "weixin.sft.duokai",
            new PayConfig{
                Name = "aa",
                Key = "weixin.sft.duokai",
                ClientType = "Pay.SFTPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.sft.duokai"}
                }
            }
        },

        {
            "alipay.guangbiao",
            new PayConfig{
                Name = "aa-广标",
                Key = "alipay.guangbiao",
                ClientType = "Pay.Alipay.PayClient",
                Data = new Dictionary<string, string> {
                    //{"appid" ,"2017063007608308"},
                    {"pid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/alipay.guangbiao"},


                }
            }
        },
        {
            "weixin.haibei",
            new PayConfig{
                Name = "aaa",
                Key = "weixin.haibei",
                ClientType = "Pay.HaiBeiPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.haibei"}
                }
            }
        }
        ,
        {
            "weixin.jinku",
            new PayConfig{
                Name = "aa",
                Key = "weixin.jinku",
                ClientType = "Pay.JinKuPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.jinku"}
                }
            }
        }
        ,
        {
            "weixin.qingyun",
            new PayConfig{
                Name = "aa",
                Key = "weixin.qingyun",
                ClientType = "Pay.QYPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.qingyun"}
                }
            }
        }
        ,
        {
            "weixin.yunjian",
            new PayConfig{
                Name = "aa",
                Key = "weixin.yunjian",
                ClientType = "Pay.YunJianPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.yunjian"}
                }
            }
        }

        ,
        {
            "weixin.tongyi",
            new PayConfig{
                Name = "aa", 
                Key = "weixin.tongyi",
                ClientType = "Pay.TongYiPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"tongdao" ,"WxWap"},
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.tongyi"}
                }
            }
        }

    };

    public static PayConfig GetPayConfig(string paykey)
    {
        if (PayDict.ContainsKey(paykey))
        {
            return PayDict[paykey];
        }

        return null;
    }


    private static Dictionary<string, Dictionary<string, string>> ProjectPayConfig = new Dictionary<string, Dictionary<string, string>>()
    {
        {
            "fzq-h5",//

            new Dictionary<string,string>(){
                { "weixin" ,"weixin.klzskt"},
                { "alipay" ,"alipay.shwenli"}
            }

        },
        {
            "jy-app",//

            new Dictionary<string,string>(){
                { "weixin" ,"weixin.jiaoyou"}
            }

        },
        {
            "jz-h5",//

            new Dictionary<string,string>(){
                { "weixin" ,"weixin.haibei"},
                { "alipay" ,"alipay.guangbiao"}
            }

        }

        ,
        {
            "wj-h5",//
            new Dictionary<string,string>(){
                { "weixin" ,"weixin.jinku"}
            }

        }
        ,
        {
            "dyzs-app",//
            new Dictionary<string,string>(){
                { "weixin" ,"weixin.sft.duokai"},
                { "alipay" ,"alipay.shwenli"}
            }

        }
        ,
        {
            "test",//
            new Dictionary<string,string>(){

                { "weixin" ,"weixin.tongyi"},
                { "test" ,"weixin.klzskt"}
            }

        }
    };

    public static bool ExistsProject(string project)
    {
        return ProjectPayConfig.ContainsKey(project);
    }

 
    public static PayConfig GetPayConfig(string projectKey, string paytype)
    {
        if (ProjectPayConfig.ContainsKey(projectKey))
        {
            var data = ProjectPayConfig[projectKey];
            if (data != null)
            {
                if (data.ContainsKey(paytype))
                {
                    string paykey = data[paytype];

                    if (PayDict.ContainsKey(paykey))
                    {
                        return PayDict[paykey];
                    }
                }
            }
        }

        return null;
    }
}




public class PayConfig
{
    public string Name { get; set; }
    public string Key { get; set; }
    public string ClientType { get; set; }

    Dictionary<string, string> data;
    public Dictionary<string, string> Data
    {
        get { return data; }
        set { this.data = value; }
    }


    public PayConfig()
    {
        this.data = new Dictionary<string, string>();
    }
}