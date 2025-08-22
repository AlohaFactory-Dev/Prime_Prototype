using System;
using System.Collections;
using System.Collections.Generic;
using Aloha.Coconut.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneTransitionViewer : UISlice
{
    public class SceneTransitionOpenArgs : UIOpenArgs
    {
        public Func<UniTask> OnCallback;
        public Action CloseCallback;

        public SceneTransitionOpenArgs(Func<UniTask> onCallback, Action closeCallback)
        {
            OnCallback = onCallback;
            CloseCallback = closeCallback;
        }
    }

    public static string ConfigName => "SceneTransitionViewConfig";
    private readonly int _on = Animator.StringToHash("On");
    private readonly int _off = Animator.StringToHash("Off");
    private SceneTransitionOpenArgs _openArgs;
    [SerializeField] private Animator animator;

    protected override void Open(UIOpenArgs openArgs)
    {
        animator.SetTrigger(_on);
        _openArgs = openArgs as SceneTransitionOpenArgs;
    }

    public async void OnAnimationEnd_Event()
    {
        await _openArgs.OnCallback();
        animator.SetTrigger(_off);
    }

    public void OffAnimationEnd_Event()
    {
        CloseView();
        _openArgs.CloseCallback?.Invoke();
    }
}