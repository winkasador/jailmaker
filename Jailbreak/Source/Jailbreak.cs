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

    private bool _isInitialized = false;

    public Jailbreak() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content/";
        IsMouseVisible = true;
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
        _logger.Information("Starting Jailbreak.");

        _logger.Information("Creating Performance Monitoring Service...");
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

        base.Initialize();

        _logger.Information("Creating Scene Manager Service...");
        _sceneManager = new SceneManager(this);

        _modManager = new ModManager(this);
        _modManager.DiscoverMods();

        var mods = _modManager.InstalledMods;

        if (mods.Count == 0) {
            LoadBootstrapScene();
            return;
        }
        else if (mods.Count == 1) {
            _modManager.SelectMod(mods.First().Key);
        }
        else {
            LoadBootstrapScene();
            return;
        }

        FinishInitialization();
    }

    public void FinishInitialization() {
        _logger.Information("Creating Content Manager Service...");
        _contentManager = new DynamicContentManager(this, _modManager);
        _contentManager.AddFilePathMacro("Content|", _modManager.ActiveMod.GetBasePath());
        foreach (var kvp in Mod.Macros) {
            _contentManager.AddFilePathMacro(kvp.Key + "|", kvp.Value);
        }

        _contentManager.RegisterContentType("Content|Data/Tilesets/", "tileset", new TilesetContentHandler(_contentManager));
        _contentManager.RegisterContentType("Content|Data/Textures/", "image", new Texture2DContentHandler(GraphicsDevice, _contentManager));
        _contentManager.RegisterContentType("Content|Data/Sounds/", "sound", new SoundEffectContentHandler(_contentManager));
        _contentManager.RegisterContentType("Content|Data/Bindings/", "bindings", new KeybindingsHandler(_contentManager));

        _contentManager.DiscoverContent(_modManager.ActiveMod);

        _inputManager = new InputManager();
        _inputManager.Game = this;

        _logger.Information("Building Service Provider...");
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(this);
        serviceCollection.AddSingleton(_inputManager);
        serviceCollection.AddSingleton(_contentManager);
        serviceCollection.AddSingleton(_graphics);
        serviceCollection.AddSingleton(_sceneManager);
        serviceCollection.AddSingleton(_performance);
        _services = serviceCollection.BuildServiceProvider();

        _isInitialized = true;

        _logger.Information("Finished Initializing.");

        _sceneManager.ChangeScene(new EditorScene(this, _services));
    }

    private void LoadBootstrapScene() {
        _sceneManager.ChangeScene(new BootstrapScene(this));
    }

    protected override void LoadContent() {
        MyraEnvironment.Game = this;
    }

    protected override void Update(GameTime gameTime) {
        _performance.BeginUpdate();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_isInitialized) {
            _inputManager.Update(deltaTime);
            if (_inputManager.IsKeybindingTriggered("window.toggle_fullscreen")) {
                _graphics.ToggleFullScreen();
            }
        }

        _sceneManager.Scene?.Update(deltaTime);

        base.Update(gameTime);

        _performance.EndUpdate();
        _performance.Tick(deltaTime);
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

        if (_isInitialized) {
            _contentManager.Dispose();
        }

        _logger.Information("Shutting Down.");

        Log.CloseAndFlush();

        base.UnloadContent();
    }

    public enum LoadError {
        NoContent,
        MultipleMods,
        NoEscapists,
        EscapistsInvalid
    }

}