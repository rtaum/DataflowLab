using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowLab.Recursive
{
    public class RecursiveProcessingPipeline : IDataFlow<int>
    {
        private TransformBlock<int, int> _initial;

        private TransformBlock<int, int> _recursive;


        public void BuildPipeline()
        {
            var options = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 100,
                BoundedCapacity = 100000,
                MaxMessagesPerTask = 10000000
            };
            Random rand = new Random(DateTime.UtcNow.Millisecond);

            _initial = new TransformBlock<int, int>(n =>
            {
                Console.WriteLine($"Number is {n}");
                return n;
            });

            _recursive = new TransformBlock<int, int>(n =>
            {
                Console.WriteLine($"Recursive call...");
                return ++n;
            });
        }

        public async Task Complete()
        {
            _initial.Complete();
            await _initial.Completion;
        }

        public void Link()
        {
            var linkOptions = new DataflowLinkOptions()
            {

            };

            _initial.LinkTo(_recursive, linkOptions);
            _recursive.LinkTo(_initial, linkOptions, n => n < 5);
        }

        public async Task SendAsyn(int item)
        {
            await _initial.SendAsync(item);
        }
    }
}
