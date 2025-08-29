using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jailbreak.Content.Handler;
using Jailbreak.Data;
using Jailbreak.Mod;
using Microsoft.Xna.Framework.Graphics;
using Serilog;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Jailbreak.Content;

public class DynamicContentManager(Jailbreak jailbreak, ModManager modManager)
{

    private readonly ILogger _logger = Log.ForContext<DynamicContentManager>();

    private Jailbreak _jailbreak = jailbreak;
    private ModManager _modManager = modManager;

    private readonly Dictionary<Type, IBaseContentHandler> _contentHandlers = new();

    private readonly Dictionary<Type, Dictionary<string, object>> _content = new();
    private readonly Dictionary<string, ContentPredicate> _contentPredicates = new();
    private readonly Dictionary<Type, string> _contentDiscoveryPaths = new();
    private readonly Dictionary<Type, string> _typeNames = new();
    private readonly Dictionary<string, string> _filepathMacros = new();

    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .WithTypeConverter(new ContentPathConverter(jailbreak))
        .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
        .Build();

    public T GetContent<T>(string id) {
        _logger.Debug($"Getting Content with ID '{id}'.");
        var type = typeof(T);
        if(!_content.ContainsKey(type)) {
            _logger.Error($"Failed To Get Content with id '{id}': Content Manager does not support '{type}'.");
            return default;
        }

        if(_content[type].ContainsKey(id) && _content[type][id] is T content) {
            return content;
        }

        _logger.Debug($"Content not found in library '{id}' (Type: '{type}'), trying to load from predicates.");
        
        if(_contentPredicates.ContainsKey(id) && _contentPredicates[id].DataType == typeof(T)) {
            return LoadContent<T>(id);
        }

        if(!_contentPredicates.ContainsKey(id)) {
            _logger.Error($"Failed to get content '{id}' because it does not exist.");
        }
        else if(!(_contentPredicates[id].DataType is T)) {
            _logger.Error($"Failed to get content '{id}' because it is the wrong type (Expected '{type}', got '{_contentPredicates[id].DataType}').");
        }

        return default;
    }

    public T LoadContent<T>(string predicateId) {
        _logger.Debug($"Loading predicate: '{predicateId}'");
        var type = typeof(T);
        if(!_contentHandlers.ContainsKey(type)) {
            _logger.Error($"Failed To Load Content with id '{predicateId}': Content Manager has no handler for '{type}'.");
            return default;
        }
        if(!_contentPredicates.ContainsKey(predicateId)) {
            _logger.Error($"Failed To Load Content with id '{predicateId}': No Predicate Loaded for this ID.");
            return default;
        }

        var predicate = _contentPredicates[predicateId];
        if(!File.Exists(predicate.Path)) {
            _logger.Error($"Failed To Load Content with id '{predicateId}': file does not exist. Filepath: '{predicate.Path}'");
            return default;
        }

        byte[] bytes = File.ReadAllBytes(predicate.Path);
        var handler = (IContentHandler<T>)_contentHandlers[type];

        try {
            T content = handler.Handle(bytes);
            _content[type].Add(predicate.Id, content);
            _logger.Debug($"Loaded Content: '{predicate.Id}'.");
            return content;
        }
        catch(YamlException e) {
            _logger.Error(e, $"Failed To Load Content with id '{predicateId}' because of error at {e.End}.");
            return default;
        }
    }

    public void AddFilePathMacro(string macro, string path) {
        _filepathMacros.Add(macro, path);
        _logger.Information($"Registered file path macro '{macro}' for the path {path}.");
    }

    public void DiscoverContent(ModDefinition activeMod) {
        foreach(var kvp in _contentDiscoveryPaths) {
            int predicateCount = 0;
            _logger.Information($"Discovering Content in '{kvp.Value}'.");
            var path = ResolveFilePath(kvp.Value);
            if(Directory.Exists(path)) {
                foreach(string filePath in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
                    if(!filePath.EndsWith(".yml") && !filePath.EndsWith(".yaml")) continue;

                    var name = filePath.Replace("\\", "/").Split("/").Last().Replace(".yml", "").Replace(".yaml", "");
                    var id = $"{activeMod.Id}/{_typeNames[kvp.Key]}.{name}";

                    if(_contentPredicates.ContainsKey(id)) {
                        _logger.Error($"Failed to load Content Predicate, an entry with the ID: '{id}' is already loaded.");
                        continue;
                    }

                    _contentPredicates.Add(id, new ContentPredicate(id, filePath, kvp.Key));
                    _logger.Debug($"Content Predicate Registered: {id}");
                    predicateCount++;
                }
                _logger.Debug($"Discovered {predicateCount} content predicate(s) in {kvp.Value}.");
            }
            else {
                _logger.Warning($"Did not discover any content in {kvp.Value} because the directory doesn't exist.");
            }
        }

        _logger.Information($"Total of {_contentPredicates.Count:n0} content predicates discovered.");
    }

    public void RegisterMod(ModDefinition mod, GraphicsDevice device) {
        AddFilePathMacro("Content|", mod.GetBasePath());
        foreach (var kvp in mod.Macros) {
            AddFilePathMacro(kvp.Key + "|", kvp.Value);
        }

        RegisterContentType(mod.GetContentLocationsFor("tileset"), "tileset", new TilesetContentHandler(this));
        RegisterContentType(mod.GetContentLocationsFor("image"), "image", new Texture2DContentHandler(device, this));
        RegisterContentType(mod.GetContentLocationsFor("sound"), "sound", new SoundEffectContentHandler(this));
        RegisterContentType(mod.GetContentLocationsFor("bindings"), "bindings", new KeybindingsHandler(this));

        DiscoverContent(mod);
    }

    public void RegisterContentType<T>(string directory, string typeName, IContentHandler<T> handler) {
        var type = typeof(T);
        if (_contentHandlers.ContainsKey(type)) {
            _logger.Error($"Failed to register Content Handler: a Handler for '{type}' already exists.");
            return;
        }

        _contentDiscoveryPaths.Add(type, directory);
        _content.Add(type, new());
        _typeNames.Add(type, typeName);

        _logger.Information($"Registered Content Handler '{handler}' for '{typeName}' in '{directory}'.");
        _contentHandlers[type] = handler;
    }

    public void RegisterContentType<T>(List<string> directories, string typeName, IContentHandler<T> handler) {
        var type = typeof(T);
        if(_contentHandlers.ContainsKey(type)) {
            _logger.Error($"Failed to register Content Handler: a Handler for '{type}' already exists.");
            return;
        }

        foreach (string directory in directories) {
            _contentDiscoveryPaths.Add(type, directory);
            _logger.Information($"Registering Content Handler '{handler}' for '{typeName}' in '{directory}'.");
        }
        _content.Add(type, new());
        _typeNames.Add(type, typeName);

        _contentHandlers[type] = handler;
    }

    public string ResolveFilePath(string path) {
        return _filepathMacros.Aggregate(path, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
    }

    public List<Type> GetSupportedContentTypes() {
        return _content.Keys.ToList();
    }

    public int GetAmountOfContentType(Type type) {
        if(_content.TryGetValue(type, out var content)) {
            return content.Count;
        }

        return 0;
    }

    public string GetContentName(Type type) {
        if(_typeNames.TryGetValue(type, out var name)) {
            return name;
        }

        return "?";
    }

    public void Dispose() {
        _logger.Information($"Disposing All Content.");
        foreach(var type in _content.Values) {
            foreach (var entry in type.Values) {
                if (entry is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        }
    }

    public IDeserializer GetDeserializer() {
        return _yamlDeserializer;
    }

} 