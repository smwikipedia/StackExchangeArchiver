using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflowArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
            //String userName = "smwikipedia";
            //Int32 userId = 264052;
            //String archiveFolder = @"c:\d\Archive";
            //Task t = Task.Factory.StartNew(() => {
            //    QuestionArchiver qa = new QuestionArchiver(userName, userId, archiveFolder);
            //    qa.ArchiveQuestions().Wait();
            //});

            //t.Wait();

            ArchiveConfig config = new ArchiveConfig();
            config.UserId = Int32.Parse(args[0]);
            config.UserName = args[1];
            if (args[2].ToLower() == "q")
                config.ArchiveType = ArchiveConfig.ArchiveTypeEnum.Questions;
            if (args[2].ToLower() == "a")
                config.ArchiveType = ArchiveConfig.ArchiveTypeEnum.Answers;

            config.ArchiveFolder = args[3];

            try
            {
                config.StartPageIndex = Int32.Parse(args[4]);
            }
            catch
            {
                config.StartPageIndex = 1;
            }

            try
            {
                config.EndPageIndex = Int32.Parse(args[5]);
            }
            catch
            {
                config.EndPageIndex = Int32.MaxValue;
            }


            QuestionArchiver qa = new QuestionArchiver(config); //"smwikipedia", 264052, @"c:\d\Archive"
            qa.ArchiveQuestions();
            Console.WriteLine("All done...");
            
        }
    }
}
