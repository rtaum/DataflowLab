using System;

namespace DataflowLab.Items
{
    public class ProcessingItem
    {
        public int Value { get; set; }

        public Vendors Vendor { get; set; }

        public Result Result { get; set; }

        public int FailedAttempts { get; set; }

        public ProcessingItem(int value, Vendors vendor)
        {
            Vendor = vendor;
            Value = value;
            Result = Result.Initial;
        }
    }
}
