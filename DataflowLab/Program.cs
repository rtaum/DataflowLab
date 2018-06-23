using System;
using System.Threading.Tasks;

namespace DataflowLab
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IDataFlow<string> dataflow = new InternetWordProcessingPipeline();
            dataflow.BuildPipeline();
            dataflow.Link();

            //Random r = new Random();
            //for(int i = 0; i < 1000; ++i)
            //{
            //    var n = r.Next(1, 10);
            //    await dataflow.SendAsyn(n);
            //}

            await dataflow.SendAsyn("http://www.gutenberg.org/files/1727/1727.txt");

            await dataflow.Complete();
            await dataflow.SendAsyn("http://www.gutenberg.org/files/1727/1727.txt");


            Console.Read();
        }
    }
}
