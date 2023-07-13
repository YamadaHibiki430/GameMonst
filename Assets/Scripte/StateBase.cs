using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase
{
    public abstract void OnEnter();

    public abstract void OnExit();

    public abstract void Update();
}

public class StateProcessorBase
{
    protected StateBase stateBase = null;

    public virtual void Update()
    {
        stateBase.Update();
    }

    public virtual void ChangeState(StateBase state)
    {
        if(stateBase != null)
        {
            stateBase.OnExit();
        }
        stateBase = state;
        stateBase.OnEnter();
    }
}