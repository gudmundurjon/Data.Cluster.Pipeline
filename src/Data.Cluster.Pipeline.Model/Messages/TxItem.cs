namespace Data.Cluster.Pipeline.Shared.Messages
{
    using System;
    using System.Linq;

    [Serializable]
    public class TxItem
    {
        public DateTime DateValue { get; set; }

        public int IntValue { get; set; }

        public long LongValue { get; set; }

        public static TxItem Demo(long daysFromNow)
        {
            Random r = new Random();
            return new TxItem
            {
                IntValue = r.Next(1, 100000),
                DateValue = DateTime.Now.AddDays(r.Next(1, 365)),
                LongValue = r.Next(1, 1000000)
            };
        }
    }
}
