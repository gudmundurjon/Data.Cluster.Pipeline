using Data.Cluster.Pipeline.Shared.Connection;
using Data.Cluster.Pipeline.Shared.Messages;
using Data.Cluster.Pipeline.Shared.Query.Base;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Cluster.Pipeline.App.Observable2
{
    public class DataProcessObservable : BaseConnectionPattern, IStreamUpdate<Tx>
    {
        public IObservable<TxItem> Update(Tx microBatch)
        {
            var connectionString = this.GetParameterValue(ConnectionPatternParamType.ConnectionString);
            var commandString = "dbo.ProcessMicroBatch";

            var cancelToken = new CancellationTokenSource();

            return this.CreateFromSqlCommand(
                connectionString,
                commandString,
                microBatch,
                cancelToken.Token,
                async r => await Task.FromResult(microBatch.Add(r))
            );
        }

        private IObservable<T> CreateFromSqlCommand<T>(
            string connectionString,
            string command,
            Tx microBatch,
            CancellationToken cancellationToken,
            Func<SqlDataReader, Task<T>> readDataFunc)
        {
            return CreateFromSqlCommand(connectionString, command, microBatch, readDataFunc, cancellationToken);
        }

        private static IEnumerable<SqlDataRecord> TxEnumerable(Tx microBatch)
        {
            var rec = new SqlDataRecord(
                new SqlMetaData("IntValue", SqlDbType.Int),
                new SqlMetaData("DateValue", SqlDbType.DateTime2),
                new SqlMetaData("LongValue", SqlDbType.BigInt)
            );
            foreach (var item in microBatch.Items)
            {
                rec.SetInt32(0, item.IntValue);
                rec.SetDateTime(1, item.DateValue);
                rec.SetInt64(2, item.LongValue);

                yield return rec;
            }
        }

        private static IObservable<T> CreateFromSqlCommand<T>(
            string connectionString,
            string command,
            Tx microBatch,
            Func<SqlDataReader, Task<T>> readDataFunc,
            CancellationToken cancellationToken)
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
                            cmd.CommandType = CommandType.StoredProcedure;
                            var pt1 = cmd.Parameters.AddWithValue("@txTable", TxEnumerable(microBatch));
                            pt1.SqlDbType = SqlDbType.Structured;

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

        IObservable<Tx> IStreamUpdate<Tx>.Update(Tx microBatch)
        {
            throw new NotImplementedException();
        }
    }
}
