using UnityEngine;
using System;

/*
* Poisoning class that handles the selection of a sushi GameObject for poisoning 
* then transitions to eating state
*/
public class Poisoning : BTurnItem
{
    private bool _didAutoPoison;


    public Poisoning(ETurnItems stateKey, StateData Data) : base(stateKey, Data)
    {
        // _data = Data;
    }
    
    public override void EnterState()
    {
        if (_didAutoPoison) return;

        // --- Option A: if you can fetch by known names "Sushi1/2/3" ---
        string[] names = { "Sushi1", "Sushi2", "Sushi3" };
        string chosen = names[UnityEngine.Random.Range(0, names.Length)];
        var itm = _data.Sboard.GetItem(chosen);

        if (itm != null)
        {
            itm.PoisonItem();
            TintRed(itm.gameObject);
        }
        else
        {
            Debug.LogWarning($"Could not find sushi '{chosen}'.");
        }

        _didAutoPoison = true;
        _nextState = ETurnItems.Eating;    // immediately advance
    }

      public override void ExitState(){}


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

    private void TintRed(GameObject go)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null) { r.material.color = Color.red; return; }

        // Colors everything in the poisoned sushi red
        var rs = go.GetComponentsInChildren<Renderer>();
        foreach (var rr in rs) rr.material.color = Color.red;
    }
}