using System;
using TMPro;
using TouchControlsKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerInput : MonoBehaviour
{
    public Vector3 Movement { get; private set; }
    public Vector2 Rotation { get; private set; }
    public float TrainMove { get; private set; }

    public bool Sprint => _sprint;
    public bool JumpTriggered => _jump;
    public bool Interaction => _interaction;
    public bool InteractionHold => _interactionHold;
    public bool UseItem => _attack || _healing;
    public bool Reload => _reload;
    public bool RotationY => _rotationY;
    public bool RotationX => _rotationX;
    public bool Attach => _attach;
    public bool Inventory => _inventory;
    public bool Pause => _pause;

    public bool PickUp
    {
        get
        {
            var value = _pickUp;
            _pickUp = false;
            return value;
        }
    }

    public bool ForcePickUp
    {
        get
        {
            var value = _forcePickUp;
            _forcePickUp = false;
            return value;
        }
        set => _forcePickUp = value;
    }

    private TouchControls _touchControls;

    private bool _jump;
    private bool _sprint;
    private bool _interaction;
    private bool _interactionHold;
    private bool _pickUp;
    private bool _forcePickUp;
    private bool _inTrain;
    private bool _attach;
    private bool _inventory;
    private bool _reload;
    private bool _attack;
    private bool _healing;
    private bool _rotationX;
    private bool _rotationY;
    private bool _pause;
    private bool _roulette;
    private bool _friends;
    private bool _playtime;

    public Action AJump;
    public Action ASprint;
    public Action AInteraction;
    public Action AInteractionHold;
    public Action APickUp;
    public Action AForcePickUp;
    public Action AInTrain;
    public Action AAttach;
    public Action AInventory;
    public Action AUseItem;
    public Action AReload;
    public Action AAttack;
    public Action AHealing;
    public Action APause;
    public Action ARoulette;
    public Action AFriends;
    public Action APlaytime;
    public Action<MonoBehaviour> AOpenWindow;

    private void Awake()
    {
        if (G.Input == null)
        {
            G.Input = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        if (ControlUI.Instance != null)
        {
            _touchControls = ControlUI.Instance.GetTouchControls();
        }
    }

    private void Update()
    {
        if (IsTextInputFocused())
        {
            return;
        }

        CheckControls();
        UpdateMovement();
        UpdateRotation();
    }

    private static bool IsTextInputFocused()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        return selected != null && selected.GetComponent<TMP_InputField>() != null;
    }

    private void CheckControls()
    {
        ResetFrameState();

        if (G.Control.UseTouchControl)
        {
            ReadTouchButtons();
        }
        else
        {
            ReadKeyboardAndMouse();
        }

        if (_jump) AJump?.Invoke();
        if (_sprint) ASprint?.Invoke();
        if (_interaction) AInteraction?.Invoke();
        if (_interactionHold) AInteractionHold?.Invoke();
        if (_pickUp) APickUp?.Invoke();
        if (_forcePickUp) AForcePickUp?.Invoke();
        if (_inTrain) AInTrain?.Invoke();
        if (_attach) AAttach?.Invoke();
        if (_inventory) AInventory?.Invoke();
        if (UseItem) AUseItem?.Invoke();
        if (_reload) AReload?.Invoke();
        if (_attack) AAttack?.Invoke();
        if (_healing) AHealing?.Invoke();
        if (_pause) APause?.Invoke();
        if (_roulette) ARoulette?.Invoke();
        if (_friends) AFriends?.Invoke();
        if (_playtime) APlaytime?.Invoke();
    }

    private void ReadTouchButtons()
    {
        _jump = _touchControls.jumpButton != null && _touchControls.jumpButton.IsTriggered;
        _pickUp = _touchControls.pickUpButton != null && _touchControls.pickUpButton.IsTriggered;
        _interaction = _touchControls.putToInventoryButton != null && _touchControls.putToInventoryButton.IsTriggered;
        _interactionHold = _touchControls.putToInventoryButton != null && _touchControls.putToInventoryButton.IsHolded;
        _sprint = _touchControls.sprintButton != null && _touchControls.sprintButton.IsHolded;
        _attach = _touchControls.attachButton != null && _touchControls.attachButton.IsTriggered;
        _attack = _touchControls.attackButton != null && _touchControls.attackButton.IsHolded;
        _healing = _touchControls.useButton != null && _touchControls.useButton.IsHolded;
        _reload = _touchControls.reloadButton != null && _touchControls.reloadButton.IsTriggered;
        _rotationX = _touchControls.rotateXButton != null && _touchControls.rotateXButton.IsHolded;
        _rotationY = _touchControls.rotateYButton != null && _touchControls.rotateYButton.IsHolded;
    }

    private static bool IsPressed(KeyControl key)
    {
        return key != null && key.isPressed;
    }

    private static bool WasPressedThisFrame(KeyControl key)
    {
        return key != null && key.wasPressedThisFrame;
    }

    private void ReadKeyboardAndMouse()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null)
        {
            return;
        }

        _jump = WasPressedThisFrame(keyboard.spaceKey);
        _interaction = WasPressedThisFrame(keyboard.eKey);
        _interactionHold = IsPressed(keyboard.eKey);
        _sprint = IsPressed(keyboard.leftShiftKey) || IsPressed(keyboard.rightShiftKey);
        _attach = WasPressedThisFrame(keyboard.zKey);
        _inventory = WasPressedThisFrame(keyboard.tabKey);
        _reload = WasPressedThisFrame(keyboard.rKey);
        _rotationY = _interactionHold;
        _rotationX = IsPressed(keyboard.qKey);
        _pause = WasPressedThisFrame(keyboard.pKey);
        _roulette = WasPressedThisFrame(keyboard.kKey);
        _friends = WasPressedThisFrame(keyboard.uKey);
        _playtime = WasPressedThisFrame(keyboard.lKey);

        _pickUp = mouse != null && mouse.rightButton.wasPressedThisFrame;
        _attack = mouse != null && mouse.leftButton.isPressed;
        _healing = _attack;
    }

    private void UpdateMovement()
    {
        if (G.Control.UseTouchControl)
        {
            var move = TCKInput.GetAxis("Joystick");
            Movement = new Vector3(move.x, 0f, move.y);
        }
        else
        {
            Movement = ReadKeyboardMovement();
        }

        if (_inTrain)
        {
            TrainMove = Movement.z;
            Movement = Vector3.zero;
        }
    }

    private static Vector3 ReadKeyboardMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector3.zero;
        }

        var x = 0f;
        var z = 0f;

        if (IsPressed(keyboard.aKey) || IsPressed(keyboard.leftArrowKey)) x -= 1f;
        if (IsPressed(keyboard.dKey) || IsPressed(keyboard.rightArrowKey)) x += 1f;
        if (IsPressed(keyboard.sKey) || IsPressed(keyboard.downArrowKey)) z -= 1f;
        if (IsPressed(keyboard.wKey) || IsPressed(keyboard.upArrowKey)) z += 1f;

        var value = new Vector3(x, 0f, z);
        return value.sqrMagnitude > 1f ? value.normalized : value;
    }

    public void UpdateRotation()
    {
        if (G.Control.UseTouchControl)
        {
            Rotation = TCKInput.GetAxis("Touchpad");
            return;
        }

        var mouse = Mouse.current;
        Rotation = mouse == null ? Vector2.zero : mouse.delta.ReadValue();
    }

    private void ResetFrameState()
    {
        _jump = false;
        _interaction = false;
        _pickUp = false;
        _attach = false;
        _inventory = false;
        _reload = false;
        _pause = false;
        _roulette = false;
        _friends = false;
        _playtime = false;
        _attack = false;
        _healing = false;
        _rotationX = false;
        _rotationY = false;
    }

    public void SitTrain(bool inTrain)
    {
        _inTrain = inTrain;
    }

    public bool InTrain()
    {
        return _inTrain;
    }

    public void UseAttack(bool use)
    {
    }
}
