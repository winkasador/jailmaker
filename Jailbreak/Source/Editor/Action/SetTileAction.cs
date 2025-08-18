using Microsoft.Xna.Framework;
using Jailbreak.World;

namespace Jailbreak.Editor.History;

public class SetTileAction : IAction
{

    private Map _map;
    private Point _position;
    private int _floor;
    private int _tile;

    private int _previousTile;

    public SetTileAction(Map map, Point position, int floor, int tile) {
        _map = map;
        _position = position;
        _floor = floor;
        _tile = tile;
    }

    public void PerformAction()
    {
        _previousTile = _map.GetTileAt(_position, _floor);
        _map.SetTileAt(_position, _floor, _tile);

        if(_tile == 0) EditorSoundEffects.ERASE_TOOL.Play();
        else EditorSoundEffects.PAINT_FENCE.Play();
    }

    public void UndoAction()
    {
        _map.SetTileAt(_position, _floor, _previousTile);
    }

}