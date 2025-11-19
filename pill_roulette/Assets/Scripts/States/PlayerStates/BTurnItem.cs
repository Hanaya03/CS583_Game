using UnityEngine;
using System;

public abstract class BTurnItem
{
    protected RaycastHit hit;
    protected StateData _data{ get; set; }
    public ETurnItems StateKey { get; private set; }
    protected ETurnItems _nextState;


    public BTurnItem(ETurnItems key, StateData data)
    {
        StateKey = key;
        _nextState = key;
        _data = data;
    }
    
    public void ResetStateKey(){_nextState = StateKey;}

    public abstract void OnLeftClick();
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract ETurnItems GetNextState();
}