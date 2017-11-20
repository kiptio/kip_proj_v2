using Kip.Utils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kip.Test.Sky
{
    class Program
    {
        static void Main(string[] args)
        {

            //new ProductTeset().InitDataSet();
        }


    }

    class ProductTeset
    {
        public struct ProductModel
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string DepartureCity { get; set; }
            public string ParentSpecialLineType { get; set; }
            public string SpecialLineType { get; set; }
            public string Subject { get; set; }
            public int DateCount { get; set; }
            public string Traffic { get; set; }
            public string Transfer { get; set; }
            public DateTime DepartureDate { get; set; }
        }

        /// <summary>
        /// 初始化测试数据集
        /// </summary>
        public void InitDataSet()
        {
            string[] arrDepartureCity = new string[] { "广州", "深圳", "佛山", "香港", "香港岛", "上海", "珠海", "衡阳", "长沙", "湛江" };
            Dictionary<string, string[]> dictSpecialLineType = new Dictionary<string, string[]>();
            dictSpecialLineType.Add("国内游", new string[] { "北京专线", "山东专线", "东北呼伦贝尔专线", "华东专线", "山西内蒙专线", "西安郑州中原专线", "西藏专线", "新疆西北专线"
                , "郴州莽山专线", "福建专线", "广西专线", "海南专线", "湖南专线", "江西专线", "贵州专线", "四川专线", "云南专线", "重庆三峡专线", "湖北专线" });
            dictSpecialLineType.Add("出境游", new string[] { "埃及专线", "巴厘岛专线", "迪拜专线", "柬埔寨专线", "马尔代夫专线", "毛里求斯专线", "美国专线", "南非专线"
                , "日韩专线", "沙巴文莱专线", "土耳其专线", "新马泰专线", "以色列专线", "印度斯里兰卡专线", "越南专线" });
            dictSpecialLineType.Add("周边游", new string[] { "澳门专线", "从化专线", "港澳专线", "广深珠专线", "广州专线", "惠州专线", "清远专线", "深圳专线", "顺德长鹿欢乐专线"
                , "温泉专线", "武夷山专线", "香港专线", "珠海专线", "珠江夜游" });
            string[] arrParentSpecialLineType = dictSpecialLineType.Keys.ToArray();

            string[] arrSubject = new string[] { "摄影", "亲子游", "迪士尼", "毕业游", "蜜月游", "沙滩", "夕阳红", "佛教旅游" };
            string[] arrTraffic = new string[] { "双飞", "双卧", "单飞单卧", "单飞", "单卧", "高铁", "动车", "汽车", "轮船", "其他" };
            string[] arrTransfer = new string[] { "含接送", "单接", "单送", "不含接送" };

            int minDateValue = (int)(new DateTime(2017, 2, 1).Ticks / 1000000000);
            int maxDateValue = (int)(new DateTime(2017, 6, 1).Ticks / 1000000000);

            Random random = new Random();
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < 1000000; i++)
            {
                ProductModel m = new ProductModel();
                m.Name = "Product" + i;
                m.Price = random.Next(100, 8000);
                m.DepartureCity = arrDepartureCity.GetRandom();
                m.ParentSpecialLineType = arrParentSpecialLineType.GetRandom();
                m.SpecialLineType = dictSpecialLineType[m.ParentSpecialLineType].GetRandom();
                m.Subject = arrSubject.GetRandom();
                m.DateCount = random.Next(1, 13);
                m.Traffic = arrTraffic.GetRandom();
                m.Transfer = arrTransfer.GetRandom();
                m.DepartureDate = new DateTime(((long)random.Next(minDateValue, maxDateValue)) * 1000000000);
                //Console.WriteLine(JsonUtils.Serializer(m));
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds);
        }
    }

}
