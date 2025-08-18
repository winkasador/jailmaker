using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Serilog;

namespace Jailbreak.Input;

public class InputManager {

    /// <summary>
    /// This gets returned if a KeyBinding is requested which doesn't exist.
    /// It isn't bound to any keys and so will always be unpressed.
    /// </summary>
    private readonly KeyBinding _emptyKeyBinding = new KeyBinding("empty");

    private ILogger _logger = Log.ForContext<InputManager>();

    private Dictionary<string, KeyBinding> _keyBindings = new Dictionary<string, KeyBinding>();

    // Storing Mouse Buttons is handled differently because MonoGame doesn't provide an easy way to store which ones were pressed.
    private List<MouseButton> _mouseButtonsPressedLastFrame = [];

    private MouseState _mouseState;
    private KeyboardState _keyboardState;

    private Dictionary<string, float> _keybindingRepeatTimeouts = new Dictionary<string, float>();
    private HashSet<string> _keybindingLockouts = new HashSet<string>();

    // Required to check viewport and window focus.
    private Game _game;

    // For preventing input when interacting with UI.
    private Desktop _desktop;

    public Game Game {
        get { return _game; }
        set { _game = value; }
    }

    public Desktop Desktop {
        get { return _desktop; }
        set { _desktop = value; }
    }

    public void RegisterKeyBinding(string key, Keys primaryKey = Keys.None, Keys secondaryKey = Keys.None) {
        _keyBindings.Add(key, new KeyBinding(key, primaryKey, secondaryKey));
    }

    public void LoadBindingGroup(List<KeyBinding> bindings) {
        foreach(KeyBinding binding in bindings) {
            _keyBindings.Add(binding.Id, binding);
        }
    }

    /// <summary>
    /// Gets a Keybinding by it's string id, as specified in the bindings.yaml file.
    /// If the binding does not exist, this method will return an unassigned keybinding which will always be up.
    /// </summary>
    public KeyBinding GetKeyBinding(string key) {
        if(_keyBindings.TryGetValue(key, out var keyBinding)) {
            return keyBinding;
        }
        else {
            return _emptyKeyBinding;
        }
    }

    public Dictionary<string, float> TimedOutKeybindings {
        get { return _keybindingRepeatTimeouts; }
    }

    public HashSet<string> LockedOutKeybindings {
        get { return _keybindingLockouts; }
    }

    /// <summary>
    /// Updates the Input Manager as to what Keys and Buttons are pressed down this frame.
    /// </summary>
    public void Update(float delta) {
        // Apply Timeouts and Lockouts to Last Frame

        foreach(var binding in GetActiveKeybindings()) {
            if(binding.TimeoutLength > 0) {
                _keybindingRepeatTimeouts.TryAdd(binding.Id, binding.TimeoutLength);
            }
            if(binding.LockUntilRelease) {
                _keybindingLockouts.Add(binding.Id);
            }
        }

        // Save from last frame
        _mouseButtonsPressedLastFrame.Clear();

        _keybindingLockouts.RemoveWhere(IsKeyUp);

        var bindingsToBeRemoved = new List<string>();

        foreach(var entry in _keybindingRepeatTimeouts) {
            var id = entry.Key;
            var timer = entry.Value;

            if(timer - delta <= 0) {
                bindingsToBeRemoved.Add(id);
            }
            else {
                _keybindingRepeatTimeouts[id] -= delta;
            }
        }

        foreach(var binding in bindingsToBeRemoved) {
            _keybindingRepeatTimeouts.Remove(binding);
        }

        if(IsMouseButtonDown(MouseButton.Left)) _mouseButtonsPressedLastFrame.Add(MouseButton.Left);
        if(IsMouseButtonDown(MouseButton.Middle)) _mouseButtonsPressedLastFrame.Add(MouseButton.Middle);
        if(IsMouseButtonDown(MouseButton.Right)) _mouseButtonsPressedLastFrame.Add(MouseButton.Right);
        if(IsMouseButtonDown(MouseButton.Extra1)) _mouseButtonsPressedLastFrame.Add(MouseButton.Extra1);
        if(IsMouseButtonDown(MouseButton.Extra2)) _mouseButtonsPressedLastFrame.Add(MouseButton.Extra2);

        // Update for this frame
        _keyboardState = Keyboard.GetState();
        _mouseState = Mouse.GetState();
    }

    private List<KeyBinding> GetActiveKeybindings() {
        List<KeyBinding> bindings = new List<KeyBinding>();

        foreach(var entry in _keyBindings) {
            var binding = entry.Value;
            if(IsKeybindingTriggered(binding.Id)) {
                bindings.Add(binding);
            }
        }

        return bindings;
    }

    public bool IsKeybindingDisabled(string keyId) {
        if(_keybindingLockouts.Contains(keyId)) return true;

        _keybindingRepeatTimeouts.TryGetValue(keyId, out float timeoutLength);
        return timeoutLength > 0;
    }

    public bool IsKeybindingTriggered(string keyId) {
        return !(IsKeyUp(keyId) || IsKeybindingDisabled(keyId));
    }

    #region Keyboard Methods

    private bool IsPrimaryBindingDown(KeyBinding binding) {
        if(_keyboardState.IsKeyUp(binding.PrimaryKey)) return false;

        foreach(var modifier in binding.PrimaryModifiers) {
            if(_keyboardState.IsKeyUp(modifier)) return false;
        }

        return true;
    }

    private bool IsSecondaryBindingDown(KeyBinding binding) {
        if(_keyboardState.IsKeyUp(binding.SecondaryKey)) return false;

        foreach(var modifier in binding.SecondaryModifiers) {
            if(_keyboardState.IsKeyUp(modifier)) return false;
        }

        return true;
    }

    public bool IsKeyDown(string keyId) {
        if(!IsWindowInFocus()) return false;
        KeyBinding keyBinding = GetKeyBinding(keyId);
        return IsPrimaryBindingDown(keyBinding) || IsSecondaryBindingDown(keyBinding);
    }

    public bool IsKeyUp(string keyId) {
        return !IsKeyDown(keyId);
    }

    public Vector2 GetInputVector(string up, string down, string left, string right) {
        Vector2 inputVector = Vector2.Zero;

        if(IsKeyDown(up))
            inputVector += new Vector2(0, -1);
        if(IsKeyDown(down))
            inputVector += new Vector2(0, 1);
        if(IsKeyDown(left))
            inputVector += new Vector2(-1, 0);
        if(IsKeyDown(right))
            inputVector += new Vector2(1, 0);

        if(inputVector.LengthSquared() > 0) {
            inputVector.Normalize();
        }

        return inputVector;
    }

    #endregion

    #region Mouse Methods

    public bool IsMouseButtonDown(MouseButton mouseButton) {
        if(IsMouseOverWidget()) return false;
        if(!IsMouseWithinScreenBounds() || !IsWindowInFocus()) return false;

        switch(mouseButton) {
            case MouseButton.Left:
                return _mouseState.LeftButton == ButtonState.Pressed;
            case MouseButton.Middle:
                return _mouseState.MiddleButton == ButtonState.Pressed;
            case MouseButton.Right:
                return _mouseState.RightButton == ButtonState.Pressed;
            case MouseButton.Extra1:
                return _mouseState.XButton1 == ButtonState.Pressed;
            case MouseButton.Extra2:
                return _mouseState.XButton2 == ButtonState.Pressed;
        }

        return false;
    }
    
    public bool IsMouseButtonUp(MouseButton mouseButton) {
        return !IsMouseButtonDown(mouseButton) && IsMouseWithinScreenBounds() && IsWindowInFocus();
    }

    /// <summary>
    /// Checks if a mouse button was pressed last frame but isn't now.
    /// </summary>
    public bool WasMouseButtonDown(MouseButton mouseButton) {
        return IsMouseButtonUp(mouseButton) && _mouseButtonsPressedLastFrame.Contains(mouseButton);
    }

    public bool IsMouseWithinScreenBounds() {
        Point mousePosition = _mouseState.Position;
        var viewport = Game.GraphicsDevice.Viewport;
        
        return mousePosition.X > 0 && mousePosition.Y > 0 && mousePosition.X < viewport.Width && mousePosition.Y < viewport.Height;
    }

    public bool IsMouseOverWidget() {
        return _desktop != null && _desktop.IsMouseOverGUI;
    }

    #endregion

    public bool IsWindowInFocus() {
        return Game.IsActive;
    }

}