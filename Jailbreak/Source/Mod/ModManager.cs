using System.Collections.Generic;
using System.IO;
using Jailbreak.Content;
using Jailbreak.Data.Dto;
using Serilog;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Jailbreak.Mod;

public class ModManager(Jailbreak jailbreak)
{

    private readonly ILogger _logger = Log.ForContext<ModManager>();

    public ModDefinition ActiveMod { get; private set; }
    public Dictionary<string, ModDefinition> InstalledMods { get; } = new();

    /// <summary>
    /// Loops through every directory in /Content/ and tries to find manifest.yml files.
    /// </summary>
    public void DiscoverMods() {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
            .Build();

        foreach(string path in Directory.GetDirectories(jailbreak.Content.RootDirectory)) {
            if(File.Exists($"{path}/manifest.yml") || File.Exists($"{path}/manifest.yaml")) {
                string targetFile = File.Exists($"{path}/manifest.yml") ? $"{path}/manifest.yml" : $"{path}/manifest.yaml";
                _logger.Information($"Attempting to read mod manifest: {targetFile}.");
                string yaml = File.ReadAllText(targetFile);

                try {
                    ModDto mod = deserializer.Deserialize<ModDto>(yaml);
                    if(InstalledMods.ContainsKey(mod.Id)) {
                        _logger.Error($"Failed to load mod '{targetFile}': A mod with the id '{mod.Id}' is already loaded,");
                        continue;
                    }

                    _logger.Information($"Loaded Mod '{mod.Id}' at {targetFile}.");
                    InstalledMods.Add(mod.Id, mod.ToModDefinition());
                }
                catch(YamlException e) {
                    _logger.Error(e, $"Failed to load mod '{targetFile}'.");
                }
            }
        }
    }

    /// <summary>
    /// Sets the active base mod used by Jailbreak.
    /// </summary>
    /// <param name="id">The string id of the mod to use as found in manifest.yml.</param>
    /// <returns>The actual instance of the selected mod definition</returns>
    public ModDefinition SelectMod(string id) {
        if(!InstalledMods.TryGetValue(id, out var mod)) {
            _logger.Error($"Failed to initialize mod '{id}': Mod with the specified ID is not installed.");
            return null;
        }

        ActiveMod = mod;

        _logger.Information($"Selected Mod '{ActiveMod.Id}' by '{ActiveMod.Authors[0].Name}'.");

        return ActiveMod;
    }

}