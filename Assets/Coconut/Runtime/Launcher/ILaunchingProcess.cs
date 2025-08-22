using System;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut.Launcher
{
    public interface ILaunchingProcess
    {
        // 작을 수록 먼저 실행
        public int Order { get; }
        public string Message { get; }
        public bool IsBlocker { get; }
        public UniTask Run(ITitleScreen titleScreen);
    }
}