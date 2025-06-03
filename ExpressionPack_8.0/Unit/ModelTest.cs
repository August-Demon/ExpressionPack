using ExpressionPack;
using ExpressionPackDemo.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExpressionPackDemo.Unit
{
    public class ModelTest
    {
        // 测试次数
        public int TestCount { get; set; } = 1000;

        // 测试简单的模型类
        ErrorLog errorLog = new ErrorLog
        {
            Id = 1,
            Context = 2.5f,
            Stack = 3u,
            Message = "Test message",
            Time = DateTime.Now
        };

        // 复杂类型
        ComplexErrorLog complexErrorLog = new ComplexErrorLog
        {
            Id = 2,
            Context = 3.5f,
            Stack = 4u,
            SubLogs = new List<SubLog>
            {
                new SubLog { SubId = 1, Secret = 1.1 },
                new SubLog { SubId = 2, Secret = 2.2 }
            },
            SubLogArray = new SubLog[]
            {
                new SubLog { SubId = 3, Secret = 3.3 },
                new SubLog { SubId = 4, Secret = 4.4 }
            },
            SubLogDict = new Dictionary<string, SubLog>
            {
                { "key1", new SubLog { SubId = 5, Secret = 5.5 } },
                { "key2", new SubLog { SubId = 6, Secret = 6.6 } }
            }


        };
        // list 类型测试
        List<ErrorLog> errorLogs = new List<ErrorLog>();

        // list 复杂类型测试
        List<ComplexErrorLog> complexErrorLogs = new List<ComplexErrorLog>();

        public void Init()
        {
            for (int i = 0; i < 1; i++)
            {
                errorLogs.Add(new ErrorLog
                {
                    Id = i,
                    Context = i * 1.5f,
                    Stack = (uint)(i + 100),
                    Message = $"Test message {i}",
                    Time = DateTime.Now.AddMinutes(i)
                });
            }

            for (int i = 0; i < 1; i++)
            {
                complexErrorLogs.Add(new ComplexErrorLog
                {
                    Id = i,
                    Context = i * 2.5f,
                    Stack = (uint)(i + 200),
                    Message = $"Complex message {i}",
                    Time = DateTime.Now.AddHours(i),
                    SubLogs = new List<SubLog>
                    {
                        new SubLog { SubId = i, Secret = i * 1.1 },
                        new SubLog { SubId = i + 1, Secret = i * 2.2 }
                    },
                    SubLogArray = new SubLog[]
                    {
                        new SubLog { SubId = i + 2, Secret = i * 3.3 },
                        new SubLog { SubId = i + 3, Secret = i * 4.4 }
                    },
                    SubLogDict = new Dictionary<string, SubLog>
                    {
                        { $"key_{i}", new SubLog { SubId = i + 4, Secret = i * 5.5 } },
                        { $"key_{i + 1}", new SubLog { SubId = i + 5, Secret = i * 6.6 } }
                    }
                });
            }
        }

        public ModelTest()
        {
           
        }
        public  string GetFriendlyTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;
            var genericTypeName = type.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
            return $"{genericTypeName}<{genericArgs}>";
        }



        // 执行方法 包含 Newtonsoft 和 ExpressionPack 的测试
        public void RunTests()
        {
            CompareTest(errorLog);
            CompareTest(complexErrorLog);
            CompareTest(errorLogs);
            CompareTest(complexErrorLogs);

        }

        public void CompareTest<T>(T obj)
        {
            //string typeName = typeof(T).FullName;
            string typeName = GetFriendlyTypeName(typeof(T));
            Console.WriteLine($"==== 测试类型: {typeName} ====");
            Console.WriteLine($"==== 测试次数: {TestCount} ====");

            // 使用 ExpressionPack 进行序列化和反序列化测试
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < TestCount; i++)
            {
                var meta = BinaryConverter.Serialize(obj);
                var deserializedLog = BinaryConverter.Deserialize<T>(meta);
            }
            sw.Stop();
            Console.WriteLine($"ExpressionPack test completed in {sw.ElapsedMilliseconds} ms.");

            // 使用 Newtonsoft.Json 进行序列化和反序列化测试
            sw.Restart();
            for (int i = 0; i < TestCount; i++)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                var deserializedLog = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            sw.Stop();
            Console.WriteLine($"Newtonsoft test completed in {sw.ElapsedMilliseconds} ms.");
            Console.WriteLine("\r\n");
        }
    }
}
