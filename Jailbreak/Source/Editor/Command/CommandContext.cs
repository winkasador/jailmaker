namespace Jailbreak.Editor.Command;

public class CommandContext {

    private EditorScene _scene;

    public CommandContext(EditorScene scene) {
        _scene = scene;
    }

    public EditorState State { get { return _scene.State; } }

    public void ToggleTileWindow() => _scene.ToggleTileWindow();
    public void ShowOpenFileDialog() => _scene.OpenFileSelectDialog();

}