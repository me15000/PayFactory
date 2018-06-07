using System;
using System.Collections.Generic;
using System.ComponentModel;




public class ProjectConfig
{
    public Dictionary<string, string> Config { get; set; }
    public string CallbackURL { get; set; }
    public string SecretKey { get; set; }
}


public class Config
{

    private static Dictionary<string, PayConfig> PayDict = new Dictionary<string, PayConfig>() {
        {
            "weixin.klzskt",
            new PayConfig{
                Name = "aa",
                Key = "aa",
                ClientType = "Pay.WXPay.PayClient",
                Data = new Dictionary<string, string> {
                    {"appid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"mch_id" ,"aa"},
                    {"mch_key" ,"aa"},
                    {"name" ,"支付"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/weixin.klzskt"},
                    {"wapurl" ,"http://pay.h5.fun5.cn/"},
                    {"serverip" ,"1.1.1.1"}
                }
            }
        },
        {
            "alipay.shwenli",
            new PayConfig{
                Name = "alipay",
                Key = "alipay.shwenli",
                ClientType = "Pay.Alipay.PayClient",
                Data = new Dictionary<string, string> {
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
                Name = "SFT",
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
                Name = "SFT",
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
                Name = "alipay",
                Key = "alipay.guangbiao",
                ClientType = "Pay.Alipay.PayClient",
                Data = new Dictionary<string, string> {
                    {"pid" ,"aa"},
                    {"appsecret" ,"aa"},
                    {"notifyurl" ,"http://pay.h5.fun5.cn/api/OpenApi.ashx/pay/callback.do/alipay.guangbiao"},


                }
            }
        },
        {
            "weixin.haibei",
            new PayConfig{
                Name = "海贝支付",
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
                Name = "金库支付",
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
                Name = "轻云支付",
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
                Name = "云尖支付",
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
                Name = "通易付",
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


    private static Dictionary<string, ProjectConfig> ProjectPayConfig = new Dictionary<string, ProjectConfig>()
    {
        {
            "fzq-h5",
            new ProjectConfig{
                CallbackURL = null,
                Config =  new Dictionary<string,string>(){
                    { "weixin" ,"weixin.klzskt"},
                    { "alipay" ,"alipay.shwenli"}
                }
            }
        },
        {
            "jy-app",
             new ProjectConfig{
                CallbackURL = null,
                Config =  new Dictionary<string,string>(){
                    { "weixin" ,"weixin.jiaoyou"}
                }
            }

        },
        {
            "jz-h5",
            new ProjectConfig{
                CallbackURL = null,
                Config =  new Dictionary<string,string>(){
                    { "weixin" ,"weixin.haibei"},
                    { "alipay" ,"alipay.guangbiao"}
                }
            }


        }

        ,
        {
            "wj-h5",
            new ProjectConfig{
                CallbackURL = null,
                Config =  new Dictionary<string,string>(){
                    { "weixin" ,"weixin.jinku"}
                }
            }

        }
        ,
        {
            "dyzs-app",
            new ProjectConfig{
                CallbackURL = null,
                SecretKey = "abcd",
                Config =  new Dictionary<string,string>(){
                    { "weixin" ,"weixin.yunjian"},
                    { "alipay" ,"alipay.shwenli"}
                }
            }
        }
        ,
        {
            "test",//test
            new ProjectConfig{
                CallbackURL = null,
                Config =  new Dictionary<string,string>(){
                    { "weixin" ,"weixin.tongyi"},
                    { "test" ,"weixin.klzskt"}
                }
            }
        }
    };

    public static bool ExistsProject(string project)
    {
        return ProjectPayConfig.ContainsKey(project);
    }


    public static ProjectConfig GetProjectConfig(string projectKey)
    {
        if (ProjectPayConfig.ContainsKey(projectKey))
        {
            return ProjectPayConfig[projectKey];
        }

        return null;
    }

    public static PayConfig GetPayConfig(string projectKey, string paytype)
    {
        var config = GetProjectConfig(projectKey);

        if (config == null)
        {
            return null;
        }


        if (config.Config.ContainsKey(paytype))
        {
            string paykey = config.Config[paytype];

            if (PayDict.ContainsKey(paykey))
            {
                return PayDict[paykey];
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