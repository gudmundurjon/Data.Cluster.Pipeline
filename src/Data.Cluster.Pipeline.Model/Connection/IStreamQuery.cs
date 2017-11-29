namespace Data.Cluster.Pipeline.Shared.Connection
{
    using Data.Cluster.Pipeline.Shared.Messages;
    using System;

    public interface IStreamQuery<out T>
    {
        IObservable<T> Query(Tx tx);
    }
}
