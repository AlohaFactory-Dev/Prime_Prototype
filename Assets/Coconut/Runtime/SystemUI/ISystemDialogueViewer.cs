using System;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.CoconutMilk
{
    public interface ISystemDialogueViewer
    {
        IObservable<Unit> OnSpecialGesture { get; }
        // no가 null일 경우, yes 선택지만 표시
        UniTask<bool> ShowDialogueYesNo(string title, string content, string yes, string no = null);
    }
}
