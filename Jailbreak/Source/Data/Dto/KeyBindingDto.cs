using System.Collections.Generic;
using Jailbreak.Input;

namespace Jailbreak.Data.Dto;

/// <summary>
/// Data Transfer Object (DTO) which stores the raw data for a keybinding and converts it into the appropriate enums.
/// </summary>
public class KeyBindingDto {

    public string Primary { get; set; }
    public List<string> PrimaryModifiers { get; set; } = new();
    public string Secondary { get; set; }
    public List<string> SecondaryModifiers { get; set; } = new();
    public float Timeout { get; set; } = 0f;
    public bool Lockout { get; set; } = false;

    public KeyBinding ToKeyBinding(string id)  {
        return new KeyBinding(
            id,
            KeyMap.StringToKey(Primary),
            KeyMap.StringToKey(Secondary),
            Lockout,
            Timeout,
            PrimaryModifiers.ConvertAll(KeyMap.StringToKey),
            SecondaryModifiers.ConvertAll(KeyMap.StringToKey)
        );
    }

}