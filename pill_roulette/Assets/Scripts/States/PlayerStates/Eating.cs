using UnityEngine;
using System;

/*
* Eating class that handles the selection of a sushi GameObject for consumption 
* and appropriate health logic
*/
public class Eating : BTurnItem
{
    public Eating(ETurnItems stateKey, StateData Data) : base(stateKey, Data)
    {
        // _data = Data;
    }
    
    /*
    * Called when player hits left click
    * if they're hovering over a sushi, grab that sushi GameObject's Item script from the SushiBoard class, 
    * if the sushi is poisoned, subtract 1 from current player health then eat the sushi object
    */
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