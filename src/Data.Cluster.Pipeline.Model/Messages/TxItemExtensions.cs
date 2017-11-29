namespace Data.Cluster.Pipeline.Shared.Messages
{
    using System.Data.SqlClient;

    public static class TxItemExtensions
    {
        public static TxItem ToMessage(this SqlDataReader reader)
        {
            return new TxItem
            {
                IntValue = reader.GetInt32(0),
                DateValue = reader.GetDateTime(1),
                LongValue = reader.GetInt64(2)
            };
        }
    }
}
