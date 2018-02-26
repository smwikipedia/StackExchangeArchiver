using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflowArchiver
{

    class ArchiveConfig
    {
        public enum ArchiveTypeEnum
        {
            Questions,
            Answers
        }
        
        public Int32 UserId;
        public String UserName;
        public ArchiveTypeEnum ArchiveType;
        public String ArchiveFolder;
        public Int32 StartPageIndex;
        public Int32 EndPageIndex;
    }
}
