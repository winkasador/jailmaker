namespace Jailbreak.Editor.History;

public class SelectTileAction : IAction
{
    private EditorState _editorState;
    private EditMode _editMode;
    private int _selectedTile;

    private EditMode _previousEditMode;
    private int _previousSelectedTile; 

    public SelectTileAction(EditorState state, EditMode newEditMode, int tile) {
        _editorState = state;
        _editMode = newEditMode;
        _selectedTile = tile;

        _previousEditMode = state.editMode;
        _previousSelectedTile = state.selectedTile;
    }

    public void PerformAction() {
        _editorState.editMode = _editMode;
        _editorState.selectedTile = _selectedTile;
        EditorSoundEffects.PICK_TILE.Play();
    }

    public void UndoAction()
    {
        _editorState.editMode = _previousEditMode;
        _editorState.selectedTile = _previousSelectedTile;
    }
}