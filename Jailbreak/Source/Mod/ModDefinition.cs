using System;
using System.Collections.Generic;
using static Jailbreak.Content.ModDefinition;

namespace Jailbreak.Content;

/// <summary>
/// Class which stores basic metadata about a mod; does not contain the actual content of any mod.
/// <br/> Load this using the Jailbreak.Mod.ModManager instance.
/// </summary>
public class ModDefinition(
    string id,
    ModType type,
    List<CreditedUser> authors,
    List<CreditedUser> credits,
    Dictionary<string, string> macros) {

    public string Id { get; } = id;

    public ModType Type { get; } = type;

    /// <summary>
    /// List of people who worked on this mod.
    /// </summary>
    public List<CreditedUser> Authors { get; } = authors;

    /// <summary>
    /// List of people whose work was used in this mod.
    /// </summary>
    public List<CreditedUser> Credits { get; } = credits;

    /// <summary>
    /// Custom macros which shorten file paths.
    /// Content| is a macro and will resolve to /Content/{mod_name}/.
    /// </summary>
    public Dictionary<string, string> Macros { get; } = macros;

    public string GetBasePath() {
        return $"./Content/{Id}/";
    }

    public enum ModType {
        Mod,
        Utility
    }

}