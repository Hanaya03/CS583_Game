using UnityEngine;
using System;

public class Poisoning : BTurnItem
{
    public Poisoning(ETurnItems stateKey, StateData Data) : base(stateKey, Data)
    {
        // _data = Data;
    }
    
    public override void OnLeftClick()
    {
        Debug.Log("INPUT");
        if(_data.SelectedGO == null)
            return;

        Debug.Log("GO has been selected");
        _data.SelectedItm = _data.Sboard.GetItem(_data.SelectedGO.name);
        _data.SelectedItm.PoisonItem();
    }
    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState()
    {
        _data.RAY = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_data.RAY, out hit, Mathf.Infinity, _data.ItemLayer))
        {
            _data.SelectedGO = hit.transform.gameObject;
            Debug.Log($"current gameobject {_data.SelectedGO.name}");
        }
        else
        {
            _data.SelectedGO = null;
        }
    }
    public override ETurnItems GetNextState(){return _nextState;}
}