using Microsoft.Xna.Framework;
using Serilog;

namespace Jailbreak.Scene;

public class SceneManager {
    
    private ILogger _logger;

    private Game _game;
    private Scene _currentScene;

    public SceneManager(Game game) {
        _logger = Log.ForContext<SceneManager>();
        _game = game;
    }

    public Scene Scene {
        get { return _currentScene; }
    }

    public void ChangeScene<T>(T scene) where T : Scene {
        _logger.Information($"Changing Scene to '{scene}'.");
        _currentScene = scene;
    }
    
}
