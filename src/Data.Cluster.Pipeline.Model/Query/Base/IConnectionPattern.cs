namespace Data.Cluster.Pipeline.App.Query.Base
{
    using System;

    public interface IConnectionPattern<out T>
    {
        IObservable<T> Queue();
    }
}
