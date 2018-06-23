using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowLab
{
    public class RandomNumbersPipeline : IDataFlow<int>
    {
        private TransformBlock<int, int> _square;

        // Separates the specified text into an array of words.
        private ActionBlock<int> _printOdd;

        private ActionBlock<int> _printEven;

        public void BuildPipeline()
        {
            _square = new TransformBlock<int, int>(n =>
            {
                Console.WriteLine($"N = {n}");

                return n * n;
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4 });

            _printOdd = new ActionBlock<int>(n =>
            {
                if (n % 2 == 0)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }

                Console.WriteLine($"Odd number is {n}");
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });

            _printEven = new ActionBlock<int>(n =>
            {
                if (n % 2 == 1)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }

                Console.WriteLine($"Even number is {n}");
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });
        }

        public void Link()
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            _square.LinkTo(_printEven, linkOptions, n => n % 2 == 0);
            _square.LinkTo(_printOdd, linkOptions, n => n % 2 == 1);
        }

        public async Task SendAsyn(int item)
        {
            await _square.SendAsync(item);
        }

        public async Task Complete()
        {
            _square.Complete();
            await _square.Completion;
        }
    }
}
