using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

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

    //Mouse look variables
    [SerializeField] public Camera playerCamera;
    [SerializeField] public float lookSensitivity = 2f;
    [SerializeField] public float lookXLimit = 45f;
    CharacterController controller;
    private float _pitch;

    //Crosshair Variables
    [SerializeField] public Image crosshair;
    [SerializeField] public float crosshairSize = 6f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //I'm doing all this to hide the cursor and lock it to the center of the screen
        //When we add menus, we'll need to set Cursor.lockState = CursorLockMode.None; and 
        //  Cursor.visible = true; so that we can 
        //see the cursor and move it around the menu screens.

        EnsureCrosshair();

    }

    private void EnsureCrosshair()
    {
        if (crosshair != null)
        {
            PositionCrosshairCenter();
            return;
        }
        var canvasGO = new GameObject("CrosshairCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

         var imgGO = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
        imgGO.transform.SetParent(canvasGO.transform, false);

        crosshair = imgGO.GetComponent<Image>();
        crosshair.color = Color.white;

        PositionCrosshairCenter();
    }

    private void PositionCrosshairCenter()
    {
        var rt = crosshair.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(crosshairSize, crosshairSize);
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


        //Mouse logic
        
        if (playerCamera != null && UnityEngine.InputSystem.Mouse.current != null)
        {
            //this is raw mouse delta
            Vector2 deltapos = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
            
            //Lets us change the sensitivity of the mouse
            float mouseX = deltapos.x * lookSensitivity;
            float mouseY = deltapos.y * lookSensitivity;

            //left n right
            transform.Rotate(0f, mouseX, 0f);

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, -lookXLimit, lookXLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
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
