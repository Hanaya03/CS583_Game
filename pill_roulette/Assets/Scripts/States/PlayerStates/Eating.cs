using UnityEngine;
using System;

public class Eating : BTurnItem
{
    public Eating(ETurnItems stateKey, StateData Data) : base(stateKey, Data)
    {
        // _data = Data;
    }
    
    public override void OnLeftClick(){}
    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState(){}
    public override ETurnItems GetNextState(){return _nextState;}
}