namespace Jailbreak.Editor.History;

public class ChangeLayerAction : IAction
{

    private EditorState _state;
    private int _previousFloor;
    private int _newFloor;

    public ChangeLayerAction(EditorState state, int newLayer) {
        _state = state;
        _previousFloor = state.activeFloor;
        _newFloor = newLayer;
    }

    public void PerformAction()
    {
        _state.activeFloor = _newFloor;
        if(_newFloor < _previousFloor) EditorSoundEffects.CLOSE_MENU.Play();
        else EditorSoundEffects.OPEN_MENU.Play();
    }

    public void UndoAction()
    {
        _state.activeFloor = _previousFloor;
        if(_previousFloor < _newFloor) EditorSoundEffects.CLOSE_MENU.Play();
        else EditorSoundEffects.OPEN_MENU.Play();
    }

}