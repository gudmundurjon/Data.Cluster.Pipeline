using Data.Cluster.Pipeline.Shared.Messages;
using System;
using System.Data.SqlClient;
using System.Reactive.Linq;

namespace Data.Cluster.Pipeline.Shared.Query
{
    public interface IWriterObservable<out T>
    {
        IObservable<T> Write(TxItem item, string connectionString, string commandText);
    }

    public class TxWriterObservable : IWriterObservable<int>
    {
        public IObservable<int> Write(TxItem item, string connectionString, string commandText)
        {
            return Observable.Create<int>(
                async o =>
                {
                    try
                    {
                        using (var conn = new SqlConnection(connectionString))
                        {
                            using (var cmd = new SqlCommand(commandText))
                            {
                                await conn.OpenAsync().ConfigureAwait(false);
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        o.OnError(ex);
                    }
                    o.OnCompleted();
                });
        }
    }
}
