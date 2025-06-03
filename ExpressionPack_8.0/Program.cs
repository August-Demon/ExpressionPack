using ExpressionPackDemo.Unit;

namespace ExpressionPack_8._0
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ModelTest modelTest = new ModelTest();
            modelTest.TestCount = 10_000;
            modelTest.Init();
            // 执行模型测试
            modelTest.RunTests();

            ModelTest modelTest2 = new ModelTest();
            modelTest2.TestCount = 100_000;
            modelTest2.Init();
            // 执行模型测试
            modelTest2.RunTests();
            // 项目“..\ExpressionPack\ExpressionPack.csproj”指向“netstandard2.0,netstandard2.1”。它不能被指向“.NETFramework,Version=v4.6.2”的项目引用。
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey();
        }
    }
}
