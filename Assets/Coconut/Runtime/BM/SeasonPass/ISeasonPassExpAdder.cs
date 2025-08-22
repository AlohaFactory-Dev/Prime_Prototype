using UniRx;

namespace Aloha.Coconut
{
    public interface ISeasonPassExpAdder
    {
        Subject<int> OnGetSeasonPassExp { get; }
    }
}
