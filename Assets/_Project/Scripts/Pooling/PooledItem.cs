using System;
using UnityEngine;

public class PooledItem : MonoBehaviour
{
    private Action _releaseAction;

    public void Initialize(Action releaseAction)
    {
        _releaseAction = releaseAction;
    }

    public void Release()
    {
        _releaseAction?.Invoke();
    }

}
