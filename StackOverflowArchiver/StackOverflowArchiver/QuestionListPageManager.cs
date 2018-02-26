using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StackOverflowArchiver
{
    class QuestionListPageManager
    {
        public ArchiveConfig Config;

        public Int32 CurrentPageIndex;

        public String UserQuestionListPageUrlTemplate = "https://stackoverflow.com/users/{0}/{1}?tab=questions&sort=newest&page={2}";
        public String UserAnswersListPageUrlTemplate = "https://stackoverflow.com/users/{0}/{1}?tab=answers&sort=newest&page={2}";
        public String UserQuestionListPageUrl;

        public String NextQuestionListPageFlag = "class=\"page-numbers next\"";

        private static String RegexStringQuestionUrl = "<a\\s*href=\"(.*)\"\\s*class=\"question-hyperlink\\s*\">";
        private static Regex RegexQuestionUrl = new Regex(RegexStringQuestionUrl, RegexOptions.Compiled);

        private static String RegexStringAnswernUrl = "<a\\s*href=\"(.*)\"\\s*class=\"answer-hyperlink\\s*\">";
        private static Regex RegexAnswerUrl = new Regex(RegexStringAnswernUrl, RegexOptions.Compiled);

        private static String RegexStringQuestionTitle = "/([^\"/\\s]*)"; 
        private static Regex RegexQuestionTitle = new Regex(RegexStringQuestionTitle, RegexOptions.Compiled);


        public static String BaseUrl = "https://stackoverflow.com";

        private List<Question> Questions = new List<Question>();

        public QuestionListPageManager(ArchiveConfig config)
        {
            this.Config = config;
            this.CurrentPageIndex = this.Config.StartPageIndex - 1;
        }

        public List<Question> GetOnePageOfQuestions()
        {
            this.Questions.Clear();
            this.CurrentPageIndex++;

            if (this.CurrentPageIndex > this.Config.EndPageIndex)
                return this.Questions;

            switch (this.Config.ArchiveType)
            {
                case ArchiveConfig.ArchiveTypeEnum.Questions:
                    this.UserQuestionListPageUrl = String.Format(this.UserQuestionListPageUrlTemplate, this.Config.UserId, this.Config.UserName, this.CurrentPageIndex);
                    break;
                case ArchiveConfig.ArchiveTypeEnum.Answers:
                    this.UserQuestionListPageUrl = String.Format(this.UserAnswersListPageUrlTemplate, this.Config.UserId, this.Config.UserName, this.CurrentPageIndex);
                    break;
            }

            
            HtmlDocument pageDocument = GetPageContent(this.UserQuestionListPageUrl);
            Int32 delaySecond = 60;
            while(DetectVisitBlocking(pageDocument))
            {
                //ensure only valid page content is used for next execution.
                //delaySecond += 60;
                Console.WriteLine("Blocked...wating for {0} seconds...", delaySecond);
                PretendToBeHuman(delaySecond);
                pageDocument = GetPageContent(this.UserQuestionListPageUrl);
            }

            while (DetectMaintenancePage(pageDocument))
            {
                //ensure only valid page content is used for next execution.
                //delaySecond += 60;
                Console.WriteLine("Maintenance...wating for {0} seconds...", delaySecond);
                PretendToBeHuman(delaySecond);
                pageDocument = GetPageContent(this.UserQuestionListPageUrl);
            }

            String pageContent = pageDocument.ParsedText;
            CollectQuestionsFromQuestionListPage(pageContent);
            if (this.Questions.Count > 0)
                Console.WriteLine("Page {0} scanned.", this.CurrentPageIndex);
            else
                Console.WriteLine("Page scanning finished.");

            return this.Questions;
        }

        private Regex regexTooManyRequests = new Regex("<title>Too Many Requests - Stack Exchange</title>", RegexOptions.Compiled);
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

        [Obsolete]
        private Boolean LastQuestionListPageReached(String pageContent)
        {
            return !pageContent.Contains(this.NextQuestionListPageFlag);
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


        private HtmlDocument GetPageContent(String url)
        {
            HtmlWeb web = new HtmlWeb();
            Int32 userAgentIndex = rand.Next(UserAgentString.Length);
            web.UserAgent = UserAgentString[userAgentIndex];
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        private void CollectQuestionsFromQuestionListPage(String pageContent)
        {
            MatchCollection mc = null;
            switch (this.Config.ArchiveType)
            {
                case ArchiveConfig.ArchiveTypeEnum.Answers:
                    mc = RegexAnswerUrl.Matches(pageContent);
                    break;
                case ArchiveConfig.ArchiveTypeEnum.Questions:
                    mc = RegexQuestionUrl.Matches(pageContent);
                    break;
            }

            foreach(Match m in mc)
            {
                Question q = new Question();
                q.RelativeUrl = m.Groups[1].Value;
                q.Title = RegexQuestionTitle.Matches(q.RelativeUrl)[2].Groups[1].Value;
                q.BaseUrl = BaseUrl;
                this.Questions.Add(q);
            }
        }
    }
}
