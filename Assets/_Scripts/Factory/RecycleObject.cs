using System;
using UnityEngine;

public class RecycleObject : MonoBehaviour
{
    public int PoolSize = 5;
    private Action<RecycleObject> release;

    public virtual void InitializeByFactory(Action<RecycleObject> releaseAction)
    {
        release = releaseAction;
    }

    public void Release()
    {
        release(this);
    }
}