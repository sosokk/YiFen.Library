using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using YiFen.DataHelper;

namespace YiFen.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //aa a = new aa();
            Console.WriteLine(Query.Select<Case>().ToList(2, 9));
            Console.Read();
        }
    }

    public class Case : Entity
    {
        public Guid ID { get; set; }

        public string Title { get; set; }
    }

    public class CaseDetail : Entity
    {
        public Guid ID { get; set; }

        [NameInDatabase("CaseID")]
        public Case Case { get; set; }

        public string Title { get; set; }
    }
}
