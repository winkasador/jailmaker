using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Jailbreak.Content;
using Jailbreak.Editor;
using Jailbreak.Input;
using Jailbreak.Scene;
using Microsoft.Xna.Framework;
using Myra;
using YamlDotNet.Serialization;
using System.IO;
using Jailbreak.Data.Dto;
using Jailbreak.Mod;
using System.Linq;
using Jailbreak.Content.Handler;
using Serilog;
using Jailbreak.Utility;

namespace Jailbreak;

public class Jailbreak : Game {
    
    private ModDefinition _mod;
    private ILogger _logger;

    private GraphicsDeviceManager _graphics;
    private SceneManager _sceneManager;
    private InputManager _inputManager;

    private ModManager _modManager;
    private DynamicContentManager _contentManager;
    private Performance _performance;

    private IServiceProvider _services;

    #region Performance Tracking
    private bool _vsync;
    private int _targetFPS;
    private float _fps;
    private float _fpsCounter;
    private int _fpsCount;

    private float _lastUpdateFrameTime;
    private float _lastDrawFrameTime;

    private long _currentMemoryUsage;
    private long _maximumAvailableMemory;
    #endregion

    public Jailbreak() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content/";
        IsMouseVisible = true;
    }

    [Obsolete]
    public float FPS {
        get { return _performance.CurrentFPS.Value; }
    }

    [Obsolete]
    public bool Vsync {
        get { return _performance.IsVSync.Value; }
    }

    [Obsolete]
    public int TargetFPS {
        get { return _performance.TargetFPS.Value; }
    }

    [Obsolete]
    public float FrameTime {
        get { return _performance.FrameTime.Value; }
    }

    [Obsolete]
    public float UpdateFrameTime {
        get { return _performance.UpdateTime.Value; }
    }

    [Obsolete]
    public float DrawFrameTime {
        get { return _performance.DrawTime.Value; }
    }

    [Obsolete]
    public long CurrentMemoryUsage {
        get { return _currentMemoryUsage; }
    }

    [Obsolete]
    public long MaximumAvailableMemory {
        get { return _maximumAvailableMemory; }
    }

    public ModDefinition Mod {
        get { return _mod; }
    }

    protected override void Initialize() {
        Window.Title = "Jailbreak";
        Window.AllowUserResizing = true;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext:l}) {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("./logs/log.txt")
            .MinimumLevel.Debug()
            .CreateLogger();
        
        _logger = Log.ForContext<Jailbreak>();

        _performance = new Performance(this, _graphics);

        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
            .Build();

        var yaml = File.ReadAllText("./Content/escapists/Manifest.yaml");
        var dto = deserializer.Deserialize<ModDto>(yaml);
        _mod = dto.ToModDefinition();

        _graphics.PreferredBackBufferWidth = (int)(1920 / 1.2);
        _graphics.PreferredBackBufferHeight = (int)(1080 / 1.2);
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
        _graphics.ApplyChanges();

        _modManager = new ModManager(this);
        _modManager.DiscoverMods();
        var mods = _modManager.InstalledMods;

        if(mods.Count == 0) {
            return; // Display Prompt
        }
        else if(mods.Count == 1) {
            _modManager.SelectMod(mods.First().Key);
        }
        else {
            return; // Load Mod Selector
        }

        _contentManager = new DynamicContentManager(this, _modManager);
        _contentManager.AddFilePathMacro("Content|", _modManager.ActiveMod.GetBasePath());
        foreach(var kvp in Mod.Macros) {
            _contentManager.AddFilePathMacro(kvp.Key + "|", kvp.Value);
        }

        _contentManager.RegisterContentType("Content|Data/Tilesets/", "tileset", new TilesetContentHandler(_contentManager));
        _contentManager.RegisterContentType("Content|Data/Textures/", "image", new Texture2DContentHandler(GraphicsDevice, _contentManager));
        _contentManager.RegisterContentType("Content|Data/Sounds/", "sound", new SoundEffectContentHandler(_contentManager));
        _contentManager.RegisterContentType("Content|Data/Bindings/", "bindings", new KeybindingsHandler(_contentManager));

        _contentManager.DiscoverContent(_modManager.ActiveMod);

        _inputManager = new InputManager();
        _inputManager.Game = this;
        
        _sceneManager = new SceneManager(this);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(this);
        serviceCollection.AddSingleton(_inputManager);
        serviceCollection.AddSingleton(_contentManager);
        serviceCollection.AddSingleton(_graphics);
        serviceCollection.AddSingleton(_sceneManager);
        serviceCollection.AddSingleton(_performance);
        _services = serviceCollection.BuildServiceProvider();

        _logger.Information("Finished Initializing.");

        base.Initialize();
    }

    protected override void LoadContent() {
        MyraEnvironment.Game = this;

        _sceneManager.ChangeScene(new EditorScene(this, _services));
        //_sceneManager.ChangeScene(new ValidationScene(this, _services));
    }

    protected override void Update(GameTime gameTime) {
        _performance.BeginUpdate();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _inputManager.Update(deltaTime);

        // Doesn't work quite right.
        if (_inputManager.IsKeybindingTriggered("window.toggle_fullscreen")) {
            _graphics.ToggleFullScreen();
        }

        _sceneManager.Scene?.Update(deltaTime);

        base.Update(gameTime);

        // Performance Tracking

        _currentMemoryUsage = GC.GetTotalMemory(false);
        _maximumAvailableMemory = Environment.WorkingSet;

        if (!IsFixedTimeStep && !_graphics.SynchronizeWithVerticalRetrace) _targetFPS = -1;
        else _targetFPS = (int)(1 / TargetElapsedTime.TotalSeconds);
        _vsync = _graphics.SynchronizeWithVerticalRetrace;

        _fpsCounter += deltaTime;
        _fpsCount++;

        if (_fpsCounter >= 1f) {
            _fps = _fpsCount;
            _fpsCount = 0;
            _fpsCounter -= 1f;
        }

        _performance.CurrentFPS.Value = (int)_fps;
        _performance.TargetFPS.Value = _targetFPS;
        _performance.IsVSync.Value = _vsync;

        _performance.EndUpdate();
        _performance.Update(deltaTime);
    }

    protected override void Draw(GameTime gameTime) {
        _performance.BeginDraw();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _sceneManager.Scene?.Draw(deltaTime);
        base.Draw(gameTime);

        _performance.EndDraw();
    }

    protected override void UnloadContent() {
        _logger.Information("Unloading Content.");

        _contentManager.Dispose();

        Log.CloseAndFlush();

        base.UnloadContent();
    }

}