using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum ETurnItems
{
    Poisoning,
    Eating
}

/*
* Player state machine that handled poisoning and eating logic.
*/

public class PlayerController : MonoBehaviour
{
    private StateData _data;
    private InputSystem_Actions controls;
    private InputAction _enter;
    [SerializeField] private LayerMask _itemLayer;
    [SerializeField] private SushiBoard _sboard;

    private bool _inTransitioningState;
    private ETurnItems _nextStateKey;
    private BTurnItem _currentState;
    private Dictionary<ETurnItems, BTurnItem> _states= new Dictionary<ETurnItems, BTurnItem>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _nextStateKey = _currentState.GetNextState();

        if (!_inTransitioningState && _nextStateKey.Equals(_currentState.StateKey))
        {
            _currentState.UpdateState();
        }
        else
        {
            TransitionToState(_nextStateKey);
        }    
    }
    
    private void TransitionToState(ETurnItems Statekey){
        _inTransitioningState = true;
        _currentState.ExitState();
        _currentState = _states[Statekey];
        _currentState.ResetStateKey();
        _currentState.EnterState();
        _inTransitioningState = false;
    }

    private void Awake()
    {
        _data = new StateData(_itemLayer, _sboard);

        _states.Add(ETurnItems.Poisoning, new Poisoning(ETurnItems.Poisoning, _data));
        _states.Add(ETurnItems.Eating, new Eating(ETurnItems.Eating, _data));

        _currentState = _states[ETurnItems.Poisoning];

        controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _enter = controls.UI.Click;
        _enter.Enable();
        _enter.canceled += ctx => _currentState.OnLeftClick();
    }

    private void OnDisable()
    {
        _enter.Disable();
    }
}
