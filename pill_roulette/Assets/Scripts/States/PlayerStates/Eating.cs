using UnityEngine;
using System;

public class Eating : BTurnItem
{
    public Eating(ETurnItems stateKey, StateData Data) : base(stateKey, Data)
    {
        // _data = Data;
    }
    
    public override void OnLeftClick()
    {
        if(_data.SelectedGO == null)
            return;

        _data.SelectedItm = _data.Sboard.GetItem(_data.SelectedGO.name);
        if(_data.SelectedItm.IsPoisoned)
            _data.Health -= 1;

        Debug.Log($"player health is {_data.Health}");
        
        _data.SelectedItm.EatItem();
        _nextState = ETurnItems.Poisoning;
    }
    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState()
    {
        _data.RAY = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_data.RAY, out hit, Mathf.Infinity, _data.ItemLayer))
        {
            _data.SelectedGO = hit.transform.gameObject;
        }
        else
        {
            _data.SelectedGO = null;
        }
    }
    public override ETurnItems GetNextState(){return _nextState;}
}