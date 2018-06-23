using DataflowLab.Items;
using DataflowLab.Recursive;
using System;
using System.Threading.Tasks;

namespace DataflowLab
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IDataFlow<int> dataflow = new RecursiveProcessingPipeline();
            dataflow.BuildPipeline();
            dataflow.Link();
            await dataflow.SendAsyn(1);

            //Random rand = new Random(DateTime.UtcNow.Millisecond);
            //for (int i = 0; i < 100; ++i)
            //{
            //    var vendor = (Vendors)rand.Next(0, 3);
            //    var item = new ProcessingItem(rand.Next(0, 100), vendor)
            //    {
            //        Result = Result.Initial
            //    };

            //    //var guid = Guid.NewGuid();
            //    await dataflow.SendAsyn(item);
            //}

            //await dataflow.Complete();

            Console.Read();
        }
    }
}
