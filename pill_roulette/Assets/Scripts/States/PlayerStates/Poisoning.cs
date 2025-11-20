using UnityEngine;
using System;

/*
* Poisoning class that handles the selection of a sushi GameObject for poisoning 
* then transitions to eating state
*/
public class Poisoning : BTurnItem
{
    public Poisoning(ETurnItems stateKey, StateData Data) : base(stateKey, Data)
    {
        // _data = Data;
    }
    
    /*
    * Called when player hits left click
    * if they're hovering over a sushi, grab that sushi GameObject's Item script from the SushiBoard class, 
    * then poison it. set next state to eating.
    */
    public override void OnLeftClick()
    {
        if(_data.SelectedGO == null)
            return;

        _data.SelectedItm = _data.Sboard.GetItem(_data.SelectedGO.name);
        _data.SelectedItm.PoisonItem();
        _nextState = ETurnItems.Eating;
    }
    
    public override void EnterState(){}
    public override void ExitState(){}

    /*
    * Called every update
    * Find the Sushi GameObject that player's mouse is hovering over
    */
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