namespace Data.Cluster.Pipeline.Shared.Query
{
    using Data.Cluster.Pipeline.Shared.Messages;
    using Data.Cluster.Pipeline.App.Query.Base;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Data.Cluster.Pipeline.Shared.Query.Base;

    public class DatabaseObservable : BaseConnectionPattern, IConnectionPattern<TxItem>
    {
        public IObservable<TxItem> Queue()
        {
            var connectionString = GetParameterValue(ConnectionPatternParamType.ConnectionString);
            var commandString = GetParameterValue(ConnectionPatternParamType.CommandString);

            return CreateFromSqlCommand(connectionString, commandString, async r => await Task.FromResult(r.ToMessage()));
        }

        private IObservable<T> CreateFromSqlCommand<T>(string connectionString, string command, Func<SqlDataReader, Task<T>> readDataFunc)
        {
            return CreateFromSqlCommand(connectionString, command, readDataFunc, CancellationToken.None);
        }

        private static IObservable<T> CreateFromSqlCommand<T>(string connectionString, string command, Func<SqlDataReader, Task<T>> readDataFunc, CancellationToken cancellationToken)
        {
            return Observable.Create<T>(
                async o =>
                {
                    SqlDataReader reader = null;

                    try
                    {
                        using (var conn = new SqlConnection(connectionString))
                        using (var cmd = new SqlCommand(command, conn))
                        {
                            await conn.OpenAsync(cancellationToken);
                            reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);

                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var data = await readDataFunc(reader);
                                o.OnNext(data);
                            }

                            o.OnCompleted();
                        }
                    }
                    catch (Exception ex)
                    {
                        o.OnError(ex);
                    }

                    return reader;
                });
        }
    }
}
