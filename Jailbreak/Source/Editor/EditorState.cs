using Jailbreak.Editor.History;
using Jailbreak.World;
using Microsoft.Xna.Framework;

namespace Jailbreak.Editor;

public class EditorState {

    public int activeFloor;
    public int selectedTile;

    public Map Map { get; set; }

    public EditMode editMode;
    public Rectangle selection;

    public bool isEditorActive = true;

    public bool isMouseDragging;
    public Point mouseClickStartPosition;

    public bool isMiddleMouseButtonClicked;

    public Point middleClickStartPoint;
    public Point middleClickStartTile;

    public bool drawDebugWidgets;
    public bool drawGrid = true;

    public EditHistory History { get; set; }

    public float stateTime;

}