using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Selector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiderAirlineCompany
{
    class Program
    {

        static void Main(string[] args)
        {
            var site = new Site { CycleRetryTimes = 3, SleepTime = 300 };
            var spider = Spider.Create(site, new GithubProfileProcessor()).AddStartUrl("http://data.carnoc.com/corp/airline/").AddPipeline(new MyPipeline());
            //var spider = Spider.Create(site, new GithubProfileProcessor()).AddStartUrl("http://data.carnoc.com/corp/airline/fu.html").AddPipeline(new MyPipeline());
            spider.ThreadNum = 5;
            spider.SkipTargetUrlsWhenResultIsEmpty = false;
            spider.Run();
            Console.Read();
        }
        private class MyPipeline : BasePipeline
        {
            public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
            {
                using (var db = new AppDbContext())
                {
                    foreach (var item in resultItems)
                    {
                        dynamic dict = null;
                        var isdict = item.Results.TryGetValue("dict", out dict);
                        if (isdict)
                        {
                            foreach (var l in dict as Dictionary<string, string>)
                            {
                                var pageurl = l.Key;
                                var shortname = l.Value;
                                var data = db.AirlineCompany.FirstOrDefault(a => a.PageUrl == pageurl);
                                if (data is AirlineCompany)
                                {
                                    data.ShortName = shortname;
                                }
                                else
                                {
                                    db.Add(new AirlineCompany
                                    {
                                        PageUrl = pageurl,
                                        ShortName = shortname
                                    });
                                }
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            string pagurl = item.Results["pageurl"];
                            var data = db.AirlineCompany.FirstOrDefault(a => a.PageUrl == pagurl);
                            if (data is AirlineCompany)
                            {
                                string name = item.Results["name"];
                                string url = item.Results["url"];
                                string country = item.Results["country"];
                                string IATA = item.Results["IATA"];
                                string ICAO = item.Results["ICAO"];
                                data.Name = name;
                                data.Url = url;
                                data.Country = country;
                                data.IATA = IATA;
                                data.ICAO = ICAO;
                            }
                            db.SaveChanges();
                        }

                    }
                }
            }
        }

        private class GithubProfileProcessor : BasePageProcessor
        {
            protected override void Handle(Page page)
            {
                var dict = new Dictionary<string, string>();
                if (page.Url == "http://data.carnoc.com/corp/airline/")
                {
                    var a = page.Selectable.SelectList(new CssSelector(".corp_sub_list>ul>li>a"));
                    foreach (var item in a.Nodes())
                    {
                        var url = item.Links().GetValue();
                        dict.Add(url, item.GetValue());
                    }
                    page.AddResultItem("dict", dict);
                    page.AddTargetRequests(a.Links().GetValues());
                }
                else
                {
                    try
                    {
                        try
                        {
                            var name = page.Selectable.Css("h1").XPath("text()").GetValue();
                            if (string.IsNullOrEmpty(name))
                            {
                                throw new Exception();
                            }
                            page.AddResultItem("name", name);
                        }
                        catch
                        {
                            try
                            {
                                var name = page.Selectable.Regex("<title>(\\w+)</title>",1).GetValue();
                                page.AddResultItem("name", name);

                            }
                            catch (Exception)
                            {
                                page.AddResultItem("name", null);
                            }
                        }
                        try
                        {
                            var url = page.Selectable.Css("h1").Links().GetValue();
                            page.AddResultItem("url", url);

                        }
                        catch
                        {
                            page.AddResultItem("url", null);

                        }
                        try
                        {
                            var country = page.Selectable.Regex("<span class=\"city\"></span>([\\s\\S]+?)(<i>)", 1).GetValue().Trim();
                            page.AddResultItem("country", country);

                        }
                        catch
                        {
                            page.AddResultItem("country", null);

                        }
                        try
                        {
                            var IATA = page.Selectable.Regex("IATA：(\\w+)",1).GetValue();

                            page.AddResultItem("IATA", IATA);
                        }
                        catch
                        {
                            page.AddResultItem("IATA", null);
                        }
                        try
                        {
                            var ICAO = page.Selectable.Regex("ICAO：(\\w+)",1).GetValue();
                            page.AddResultItem("ICAO", ICAO);
                        }
                        catch
                        {
                            page.AddResultItem("ICAO", null);
                        }
                        page.AddResultItem("pageurl", page.Url);
                    }
                    catch (Exception)
                    {
                        page.Skip = true;
                    }
                }
            }
        }
    }
}
