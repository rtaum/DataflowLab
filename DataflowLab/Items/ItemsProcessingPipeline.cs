using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowLab.Items
{
    public class ItemsProcessingPipeline : IDataFlow<ProcessingItem>
    {
        private TransformBlock<Guid, ProcessingItem> _initial;

        private TransformBlock<ProcessingItem, ProcessingItem> _gateway;

        private TransformBlock<ProcessingItem, ProcessingItem> _facebook;

        private TransformBlock<ProcessingItem, ProcessingItem> _google;

        private TransformBlock<ProcessingItem, ProcessingItem> _microsoft;

        private TransformBlock<ProcessingItem, ProcessingItem> _retry;

        private ActionBlock<ProcessingItem> _failed;

        public void BuildPipeline()
        {
            var options = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 100,
                BoundedCapacity = 100000,
                MaxMessagesPerTask = 10000000
            };
            Random rand = new Random(DateTime.UtcNow.Millisecond);

            _initial = new TransformBlock<Guid, ProcessingItem>(guid =>
            {
                var vendor = (Vendors)rand.Next(0, 3);
                return new ProcessingItem(rand.Next(0, 100), vendor)
                {
                    Result = Result.Initial
                };
            }, options);

            _gateway = new TransformBlock<ProcessingItem, ProcessingItem>(item =>
            {
                if (item.FailedAttempts > 0)
                {
                    Console.WriteLine($"Item {item.Value} is reprocessing.");
                }
                //Console.WriteLine($"Item {item.Value} processing has started");

                return item;
            }, options);

            _facebook = new TransformBlock<ProcessingItem, ProcessingItem> (item =>
            {
                var randNumber = Math.Abs(item.Value) % 3;
                if (randNumber == 0)
                {
                    item.Result = Result.Error;
                }
                else if (randNumber == 1)
                {
                    item.Result = Result.Valid;
                }
                else
                {
                    item.Result = Result.Invalid;
                }

                //Console.WriteLine($"Item {item.Value} from the vendor {item.Vendor} processing result is {item.Result.ToString()}");

                return item;
            }, options);

            _google = new TransformBlock<ProcessingItem, ProcessingItem>(item =>
            {
                var randNumber = Math.Abs(item.Value) % 3;
                if (randNumber == 0)
                {
                    item.Result = Result.Error;
                }
                else if (item.Value.GetHashCode() == 1)
                {
                    item.Result = Result.Valid;
                }
                else
                {
                    item.Result = Result.Invalid;
                }

                //Console.WriteLine($"Item {item.Value} from the vendor {item.Vendor} processing result is {item.Result.ToString()}");

                return item;
            }, options);

            _microsoft = new TransformBlock<ProcessingItem, ProcessingItem>(item =>
            {
                var randNumber = Math.Abs(item.Value) % 3;
                if (randNumber == 0)
                {
                    item.Result = Result.Error;
                }
                else if (randNumber == 1)
                {
                    item.Result = Result.Valid;
                }
                else
                {
                    item.Result = Result.Invalid;
                }

                //Console.WriteLine($"Item {item.Value} from the vendor {item.Vendor} processing result is {item.Result.ToString()}");

                return item;
            }, options);

            _retry = new TransformBlock<ProcessingItem, ProcessingItem>(item =>
            {
                ++item.FailedAttempts;
                if (item.FailedAttempts < 3)
                {
                    // if failed less than 3 times, retry
                    item.Result = Result.Initial;
                }
                else
                {
                    Console.WriteLine($"Item {item.Value} is hopelessly failed. Result is {item.Result}");

                }

                return item;
            }, options);

            _failed = new ActionBlock<ProcessingItem>(item =>
            {
                Console.WriteLine($"Failed item {item.Value} from the vendor {item.Vendor.ToString()}");
            }, options);
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

            _initial.LinkTo(_gateway, linkOptions);
            _gateway.LinkTo(_facebook, linkOptions, Items => Items.Vendor == Vendors.Facebook);
            _gateway.LinkTo(_google, linkOptions, Items => Items.Vendor == Vendors.Google);
            _gateway.LinkTo(_microsoft, linkOptions, Items => Items.Vendor == Vendors.Microsoft);

            // retry if error
            _facebook.LinkTo(_retry, linkOptions, item => item.Result == Result.Error);
            _google.LinkTo(_retry, linkOptions, item => item.Result == Result.Error);
            _microsoft.LinkTo(_retry, linkOptions, item => item.Result == Result.Error);

            _retry.LinkTo(_gateway, linkOptions, item => item.Result == Result.Initial);
            _retry.LinkTo(_failed, linkOptions, item => item.Result == Result.Error);
        }

        public async Task SendAsyn(ProcessingItem item)
        {
            _gateway.Post(item);
            await Task.CompletedTask;
        }
    }
}
