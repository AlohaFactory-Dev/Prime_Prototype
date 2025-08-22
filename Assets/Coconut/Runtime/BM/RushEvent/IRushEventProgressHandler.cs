using System;

namespace Aloha.Coconut
{
    public interface IRushEventProgressHandler
    {
        IObservable<(string action, int progress)> OnProgressAdded { get; }
    }
}
