using System;
using Microsoft.Extensions.DependencyInjection;
using Jailbreak.Content;
using Jailbreak.Editor;
using Jailbreak.Input;
using Jailbreak.Scene;
using Microsoft.Xna.Framework;
using Myra;
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

    private IServiceProvider _services;

    private bool _isInitialized = false;
    
    public SceneManager SceneManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public ModManager ModManager { get; private set; }
    public DynamicContentManager ContentManager { get; private set; }
    public Performance Performance { get; private set; }

    public bool IsDebugMode { get; private set; }

    public Jailbreak(string[] args) {
        foreach (string arg in args) {
            if (arg.Equals("--debug")) {
                IsDebugMode = true;
            }
        }

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
        if (IsDebugMode) _logger.Information("Starting Jailbreak in Debug Mode.");
        else _logger.Information("Starting Jailbreak.");

        _logger.Information("Creating Performance Monitoring Service...");
        Performance = new Performance(this, _graphics);
        
        _graphics.PreferredBackBufferWidth = (int)(1920 / 1.2);
        _graphics.PreferredBackBufferHeight = (int)(1080 / 1.2);
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
        _graphics.ApplyChanges();

        base.Initialize();

        _logger.Information("Creating Scene Manager Service...");
        SceneManager = new SceneManager(this);

        ModManager = new ModManager(this);
        ModManager.DiscoverMods();

        _logger.Information("Creating Content Manager Service...");
        ContentManager = new DynamicContentManager(this, ModManager);

        var mods = ModManager.InstalledMods;
        int modCount = ModManager.GetModCount();

        if (modCount == 0) {
            LaunchBootstrapSequence();
            return;
        }
        else if (modCount == 1 && !IsDebugMode) {
            ModManager.SelectMod(mods.First().Key);
            _mod = ModManager.ActiveMod;
        }
        else {
            LaunchModSelector();
            return;
        }

        FinishInitialization();
    }

    public void FinishInitialization() {
        ContentManager.RegisterMod(_mod, GraphicsDevice);

        InputManager = new InputManager();
        InputManager.Game = this;

        _isInitialized = true;

        _logger.Information("Finished Initializing.");

        SceneManager.ChangeScene(new EditorScene(this));
    }

    private void LaunchBootstrapSequence() {
        ModDefinition bootstrapMod = ModManager.InstalledMods["_global"];
        ContentManager.AddFilePathMacro("Global|", bootstrapMod.GetBasePath());
        ContentManager.RegisterContentType("Global|Textures/", "image", new Texture2DContentHandler(GraphicsDevice, ContentManager));
        ContentManager.DiscoverContent(bootstrapMod);

        SceneManager.ChangeScene(new BootstrapScene(this));
    }

    private void LaunchModSelector() {
        ModDefinition bootstrapMod = ModManager.InstalledMods["_global"];
        ContentManager.AddFilePathMacro("Global|", bootstrapMod.GetBasePath());
        ContentManager.RegisterContentType("Global|Textures/", "image", new Texture2DContentHandler(GraphicsDevice, ContentManager));
        ContentManager.DiscoverContent(bootstrapMod);

        SceneManager.ChangeScene(new ModSelectScene(this));
    }

    protected override void LoadContent() {
        MyraEnvironment.Game = this;
    }

    protected override void Update(GameTime gameTime) {
        Performance.BeginUpdate();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_isInitialized) {
            InputManager.Update(deltaTime);
            if (InputManager.IsKeybindingTriggered("window.toggle_fullscreen")) {
                _graphics.ToggleFullScreen();
            }
        }

        SceneManager.Scene?.Update(deltaTime);

        base.Update(gameTime);

        Performance.EndUpdate();
        Performance.Tick(deltaTime);
    }

    protected override void Draw(GameTime gameTime) {
        Performance.BeginDraw();
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        SceneManager.Scene?.Draw(deltaTime);
        base.Draw(gameTime);

        Performance.EndDraw();
    }

    protected override void UnloadContent() {
        _logger.Information("Unloading Content.");

        if (_isInitialized) {
            ContentManager.Dispose();
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