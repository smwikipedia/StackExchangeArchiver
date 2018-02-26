using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace StackOverflowArchiver
{
    class QuestionPageManager
    {
        //List<String> FailedDomainsNoMoreTry = new List<String>();
        Int32 TimeoutSecondsValue = 5;

        //private Question q;
        //private HtmlDocument qDoc;
        //private HtmlWeb web = new HtmlWeb();
        //private WebClient wc= null;
        //private HttpClient http = null;

        private String imgSrcBase64Template = "data:{0};base64, {1}"; //{0} content-type, {1} base64 string

        private Regex regexTooManyRequests = new Regex("<title>Too Many Requests - Stack Exchange</title>", RegexOptions.Compiled);
       
        public QuestionPageManager()
        {
            //http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; Touch; rv:11.0) like Gecko");
        }

        public async Task SaveQuestion(Question q)
        {
            //Console.WriteLine("Processing: {0}", q.Title);
            HtmlDocument qDoc = GetQuestionHtmlDocument(q);
            Int32 delaySecond = 60;
            while(DetectVisitBlocking(qDoc))
            {
                //ensure only valid page content is used for next execution.
                //delaySecond += 60;
                Console.WriteLine("Blocked...wating for {0} seconds...", delaySecond);
                PretendToBeHuman(delaySecond);
                qDoc = GetQuestionHtmlDocument(q);
            }

            while (DetectMaintenancePage(qDoc))
            {
                //ensure only valid page content is used for next execution.
                //delaySecond += 60;
                Console.WriteLine("Maintenance...wating for {0} seconds...", delaySecond);
                PretendToBeHuman(delaySecond);
                qDoc = GetQuestionHtmlDocument(q);
            }

            await ProcessQuestionHtmlDocument(q, qDoc);
            SaveQuestionToDisk(q, qDoc);
            Console.WriteLine("Saved question: {0}", q.Title);
            Console.WriteLine("\tUser Agent: {0}", q.UserAgent);
        }

        private Boolean DetectVisitBlocking(HtmlDocument qDoc)
        {
            if (regexTooManyRequests.IsMatch(qDoc.ParsedText))
                return true;
            return false;
        }

        private Regex regexMaintenancePage = new Regex("<h1>We are currently offline for maintenance</h1>", RegexOptions.Compiled);
        private Boolean DetectMaintenancePage(HtmlDocument qDoc)
        {
            if (regexMaintenancePage.IsMatch(qDoc.ParsedText))
                return true;
            return false;
        }

        private void PretendToBeHuman(Int32 delaySecond)
        {
            Thread.Sleep(delaySecond * 1000);
        }


        private String[] UserAgentString = {   "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.85 Safari/537.36",
                                               "Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)",
                                               "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36",
                                               "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)",
                                               "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.10; rv:34.0) Gecko/20100101 Firefox/34.0",
                                               "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0",
                                               "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36",
                                               "mindUpBot (datenbutler.de)",
                                               "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/5.0)",
                                               //"Mozilla/5.0 (iPhone; CPU iPhone OS 7_0 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11A465 Safari/9537.53 (compatible; bingbot/2.0; http://www.bing.com/bingbot.htm)",
                                               "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Media Center PC",
                                               "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0",
                                               "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.112 Safari/535.1",
                                               "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:30.0) Gecko/20100101 Firefox/30.0",
                                               "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
                                               "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko",
                                               "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36",
                                               "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0; Trident/5.0)",
                                               "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0",
                                               "Mozilla/5.0 (iPad; U; CPU OS 5_1 like Mac OS X) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B367 Safari/531.21.10 UCBrowser/3.4.3.532"
                                           };
        private Random rand = new Random(DateTime.Now.Millisecond);

        private HtmlDocument GetQuestionHtmlDocument(Question q)
        {
            HtmlWeb web = new HtmlWeb();
            Int32 userAgentIndex = rand.Next(UserAgentString.Length);
            web.UserAgent = UserAgentString[userAgentIndex];
            q.UserAgent = web.UserAgent;
            String qUrl = q.BaseUrl + q.RelativeUrl;
            HtmlDocument qDoc = web.Load(qUrl);
            return qDoc;
        }

        private async Task ProcessQuestionHtmlDocument(Question q, HtmlDocument qDoc)
        {
            await ProcessImages(q, qDoc);
            await ProcessCss(q, qDoc);
            ProcessAHref(q, qDoc);
            ProcessScript(q, qDoc);
        }

        private void ProcessScript(Question q, HtmlDocument qDoc)
        {
            var imgNodes = qDoc.DocumentNode.Descendants("script").ToList();
            imgNodes.ForEach(n => n.Remove());
        }

        private async Task ProcessImages(Question q, HtmlDocument qDoc)
        {
            var imgNodes = qDoc.DocumentNode.Descendants("img").ToList();

            foreach (var imgNode in imgNodes)
            {
                //Console.WriteLine(imgNode.OuterHtml);
                String imgSrc = String.Empty;
                String imgSrcUrl = String.Empty;
                HtmlAttribute imgAttr = null;
                Boolean imgSrcFound = false;
                var attrs = imgNode.Attributes;
                foreach(var attr in attrs)
                {
                    if (attr.Name.ToLower() == "src")
                    {
                        imgSrc = attr.Value;
                        imgSrcFound = true;
                        imgAttr = attr;
                        imgSrcUrl = imgAttr.Value;
                        break;
                    }
                }
                if(imgSrcFound)
                {
                    //Boolean skipFailedDomain = false;
                    //foreach(var failedDomain in FailedDomainsNoMoreTry)
                    //{
                    //    if (imgSrcUrl.ToLower().Contains(failedDomain))
                    //    {
                    //        skipFailedDomain = true;
                    //        break;
                    //    }
                    //}
                    //if (skipFailedDomain)
                    //    continue;

                    if (!imgSrcUrl.ToLower().StartsWith("http"))
                    {
                        if (imgSrcUrl.ToLower().StartsWith("//")) //Stackoverflow page contains such anomaly: <img src="//i.stack.imgur.com/gfrSH.png"...
                            imgSrcUrl = "https:" + imgSrcUrl;
                        else
                            imgSrcUrl = q.BaseUrl + imgSrcUrl;
                    }
                    try
                    {
                        String contentType = await DetermineImageType(q, imgSrcUrl);
                        String imgBase64String = await GetImageBase64String(q, imgSrcUrl);
                        imgAttr.Value = String.Format(imgSrcBase64Template, contentType, imgBase64String);
                    }
                    catch
                    {
                        //Some domian mail fail to download, such as: https://graph.facebook.com/1711425042468995/picture?type=large
                        //var uri = new Uri(imgSrcUrl);
                        //if (!FailedDomainsNoMoreTry.Contains(uri.Host))
                        //    FailedDomainsNoMoreTry.Add(uri.Host);
                        Console.WriteLine("\t[Warning]: failed to download image from: {0}", imgSrcUrl);
                    }
                }
            }
        }

        
        private async Task ProcessCss(Question q, HtmlDocument qDoc)
        {
            var cssNodes = qDoc.DocumentNode.Descendants("link").Where(n => n.Attributes.Where(a => a.Name.ToLower() == "type" && a.Value.ToLower() == "text/css").Count() > 0).ToList();
            foreach (var cssNode in cssNodes)
            {
                String cssHref = String.Empty;
                String cssDoc = String.Empty;
                Boolean cssHrefFound = false;
                HtmlAttribute cssHrefAttr = null;
                cssNode.Name = "style";  // change node name from "link" to "style"
                var attrs = cssNode.Attributes;
                foreach (var attr in attrs)
                {
                    if (attr.Name.ToLower() == "href")
                    {
                        cssHref = attr.Value;
                        if(!cssHref.ToLower().StartsWith("http"))
                        {
                            cssHref = q.BaseUrl + cssHref;
                        }
                        cssHrefFound = true;
                        cssHrefAttr = attr;
                        break;
                    }
                }
                if(cssHrefFound)
                {
                    cssHrefAttr.Name = "data-href"; //change attr name from "href" to "data-href"
                    cssDoc = await GetCssDoc(q, cssHref);
                    cssNode.InnerHtml = cssDoc;
                }
            }
        }

        private async Task<String> GetCssDoc(Question q, String cssUrl)
        {
            HttpClient http = new HttpClient();
            HttpResponseMessage cssResponse = await http.GetAsync(cssUrl);
            String cssDoc = await cssResponse.Content.ReadAsStringAsync();
            return cssDoc;
        }


        private void ProcessAHref(Question q, HtmlDocument qDoc)
        {
            var aHrefNodes = qDoc.DocumentNode.Descendants("a").Where(n => n.Attributes.Where(a => a.Name.ToLower() == "href").Count() > 0).ToList();
            foreach (var aHrefNode in aHrefNodes)
            {
                String href = String.Empty;
                var attrs = aHrefNode.Attributes;
                foreach (var attr in attrs)
                {
                    if (attr.Name.ToLower() == "href" && attr.Value.Length > 0)
                    {
                        href = attr.Value;
                        if (!href.ToLower().StartsWith("http"))
                        {
                            attr.Value = q.BaseUrl + href;
                        }
                        break;
                    }
                }
            }
        }

        private async Task<String> DetermineImageType(Question q, String imgUrl)
        {
            HttpClient http = new HttpClient();
            http.Timeout = new TimeSpan(0, 0, TimeoutSecondsValue);
            HttpResponseMessage response = await http.GetAsync(imgUrl);
            String contentType = response.Content.Headers.ContentType.ToString();
            //Console.WriteLine(imgUrl);
            //Console.WriteLine(contentType);
            return contentType;
        }


        private async Task<String> GetImageBase64String(Question q, String imgUrl)
        {
            WebClient wc = new WebClient(); //? why need a new instance? otherwise, NotSupportedException.
            Uri imgUri = new Uri(imgUrl);
            Stream s = await wc.OpenReadTaskAsync(imgUri);
            MemoryStream ms = new MemoryStream();
            Byte[] buffer = new Byte[1024];
            Int32 byteRead = 0;
            while ((byteRead = s.Read(buffer, 0, 1024)) > 0)
            {
                ms.Write(buffer, 0, byteRead);
            }
            ms.Position = 0;
            Byte[] imageBytes = ms.ToArray();
            s.Close(); // This stream must be closed. Otherwise other following request will hang.
            ms.Close(); 
            return System.Convert.ToBase64String(imageBytes);
        }




        private void SaveQuestionToDisk(Question q, HtmlDocument qDoc)
        {
            String qFileFullPath = Path.Combine(QuestionArchiver.Config.ArchiveFolder, q.Title) + ".html";
            qDoc.Save(qFileFullPath);
        }
    }
}
