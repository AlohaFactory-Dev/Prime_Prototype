using UniRx;

namespace Aloha.Coconut
{
    public interface IRegularPassProgressProvider
    {
        public IReadOnlyReactiveProperty<int> GetReactiveProgress(string passType);
    }
}