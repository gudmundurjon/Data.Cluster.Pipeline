namespace Data.Cluster.Pipeline.Shared.Connection
{
    using Data.Cluster.Pipeline.Shared.Messages;
    using System;

    public interface IStreamUpdate<out T>
    {
        IObservable<T> Update(Tx tx);
    }
}
