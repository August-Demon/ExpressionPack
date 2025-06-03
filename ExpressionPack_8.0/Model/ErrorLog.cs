using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionPackDemo.Model
{

    public class SubLog
    {
        public int SubId { get; set; }
        public double Secret { get; set; }
    }

    public class ErrorLog
    {
        public int Id { get; set; }
        public float Context { get; set; }
        public uint Stack { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }



    }

    // 复合类型的错误日志类
    public class ComplexErrorLog : ErrorLog
    {

        public List<SubLog> SubLogs { get; set; }
        public SubLog[] SubLogArray { get; set; }
        public Dictionary<string, SubLog> SubLogDict { get; set; }
    }

   
}
