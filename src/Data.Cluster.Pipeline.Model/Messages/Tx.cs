namespace Data.Cluster.Pipeline.Shared.Messages
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Tx
    {
        public List<TxItem> Items { get; set; }

        public TxItem Add(System.Data.SqlClient.SqlDataReader reader)
        {
            var item = new TxItem
            {
                IntValue = reader.GetInt32(0),
                DateValue = reader.GetDateTime(1),
                LongValue = reader.GetInt64(2),
            };

            Items.Add(item);

            return item;
        }
    }
}
