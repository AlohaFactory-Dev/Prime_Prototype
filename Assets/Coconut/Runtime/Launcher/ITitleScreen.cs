using System;

namespace Aloha.Coconut.Launcher
{
    public interface ITitleScreen : IProgress<float>
    {
        void Show();
        void Hide();
        void SetMessage(string message);
    }
}