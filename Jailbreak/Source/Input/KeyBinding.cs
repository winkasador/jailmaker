using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Jailbreak.Input;

public class KeyBinding {
    
    private string _id;

    private Keys _primaryKey;
    private List<Keys> _primaryModifiers;

    private Keys _secondaryKey;
    private List<Keys> _secondaryModifiers;

    // A length of time after this keybinding has been activated that it will be temporarily disabled.
    private float _timeoutLength;

    // Makes this keybinding activate once after being pressed, and then disable itself until the binding is released.
    private bool _lockUntilRelease;

    public KeyBinding(string id,
                      Keys primaryKey = Keys.None, 
                      Keys secondaryKey = Keys.None, 
                      bool lockoutUntilRelease = false, 
                      float timeoutLength = 0f,
                      List<Keys> primaryKeyModifiers = null, 
                      List<Keys> secondaryKeyModifiers = null) {

        _id = id;

        _primaryKey = primaryKey;
        _secondaryKey = secondaryKey;

        _lockUntilRelease = lockoutUntilRelease;
        _timeoutLength = timeoutLength;

        _primaryModifiers = primaryKeyModifiers == null ? [] : primaryKeyModifiers;
        _secondaryModifiers = secondaryKeyModifiers == null ? [] : secondaryKeyModifiers;
    }

    public string Id {
        get { return _id; }
    }

    public Keys PrimaryKey {
        get { return _primaryKey; }
        set { _primaryKey = value; }
    }

    public Keys SecondaryKey {
        get { return _secondaryKey; }
        set { _secondaryKey = value; }
    }

    public List<Keys> PrimaryModifiers {
        get { return _primaryModifiers; }
        set { _primaryModifiers = value; }
    }

    public List<Keys> SecondaryModifiers {
        get { return _secondaryModifiers; }
        set { _secondaryModifiers = value; }
    }

    public float TimeoutLength {
        get { return _timeoutLength; }
        set { _timeoutLength = value; }
    }

    public bool LockUntilRelease {
        get { return _lockUntilRelease; }
        set { _lockUntilRelease = value; }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("(");

        foreach(Keys modifier in PrimaryModifiers) {
            sb.Append($"{Enum.GetName(typeof(Keys),  modifier)}+");
        }
        sb.Append($"{Enum.GetName(typeof(Keys),  PrimaryKey)}");

        if(SecondaryKey != Keys.None) {
            sb.Append(" / ");
            foreach(Keys modifier in SecondaryModifiers) {
                sb.Append($"{Enum.GetName(typeof(Keys),  modifier)}+");
            }
            sb.Append($"{Enum.GetName(typeof(Keys),  SecondaryKey)}");
        }

        sb.Append($", Timeout: {TimeoutLength}");
        sb.Append($", Lockout: {LockUntilRelease})");

        return sb.ToString();
    }

}