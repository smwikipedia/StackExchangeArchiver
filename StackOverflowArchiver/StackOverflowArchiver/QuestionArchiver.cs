using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StackOverflowArchiver
{
    class QuestionArchiver
    {
        public static ArchiveConfig Config;

        public QuestionArchiver(ArchiveConfig config)
        {
            Config = config;

            if(!Directory.Exists(Config.ArchiveFolder))
            {
                Directory.CreateDirectory(Config.ArchiveFolder);
            }
        }

        public void ArchiveQuestions()
        {
            QuestionListPageManager qlMgr = new QuestionListPageManager(QuestionArchiver.Config);

            List<Question> questions = qlMgr.GetOnePageOfQuestions();

            while (questions.Count > 0)
            {
                Console.WriteLine("Total questions {0} on page {1}", questions.Count, qlMgr.CurrentPageIndex);
                Console.WriteLine("----------------------");


                QuestionPageManager qMgr = new QuestionPageManager();
                Int32 batchNumber = 10;
                List<Question> questionBatch = null;
                List<Task> batchWaitList = new List<Task>();
                for (Int32 i = 0; i < questions.Count; i += batchNumber)
                {
                    if ((i + batchNumber - 1) <= questions.Count)
                    {
                        Console.WriteLine("-- Batch {0} [{1} ~ {2}]--", i / batchNumber, i, i + batchNumber - 1);
                        questionBatch = questions.GetRange(i, batchNumber);
                    }
                    else
                    {
                        Console.WriteLine("-- Batch {0} [{1} ~ {2}]--", i / batchNumber, i, questions.Count - 1);
                        questionBatch = questions.GetRange(i, questions.Count - i);
                    }

                    batchWaitList.Clear();
                    foreach (Question q in questionBatch)
                    {
                        Task t = qMgr.SaveQuestion(q);
                        batchWaitList.Add(t);
                    }
                    try
                    {
                        Task.WaitAll(batchWaitList.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Int32 beNice = 10;
                    Console.WriteLine("Sleep {0} seconds for next batch...", beNice);
                    Thread.Sleep(beNice * 1000);
                }

               questions = qlMgr.GetOnePageOfQuestions();
            }
        }
    }
}
