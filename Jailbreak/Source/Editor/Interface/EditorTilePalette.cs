using Jailbreak.Editor.History;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Jailbreak.Editor.Interface;

public class EditorTilePalette : Widget {

    private Color _selectedTileBorderColor = Color.White;
    private Color _selectedTileHighlightColor = new Color(100, 100, 100, 20);

    private Color _highlightedTileColorBright = Color.Yellow;
    private Color _highlightedTileColorDark = Color.Orange;

    private EditorState _state;
    private EditorMapRenderer _mapRenderer;
    private int _highlightedTileIndex;

    private int _tileSize = 20;
    private int _paddingSize = 1;
    private int _rows = 13;

    public EditorTilePalette(EditorState state, EditorMapRenderer mapRenderer) {
        _state = state;
        _mapRenderer = mapRenderer;
    }

    public override void InternalRender(RenderContext context)
    {
        // TODO: Abort render if map is not available.

        int columns = _mapRenderer.TileTextures.Count / _rows + 1;

        int paletteHeight = _rows * _tileSize;
        paletteHeight += _paddingSize * (_rows + 1);

        int paletteWidth = columns * _tileSize;
        paletteWidth += _paddingSize * (columns + 1);

        Width = paletteWidth;
        Height = paletteHeight;

        context.FillRectangle(new Rectangle(0, 0, paletteWidth, paletteHeight), Color.Black);

        for(int i = 0; i < _mapRenderer.TileTextures.Count; i++) {
            int columnOffset = i / _rows;
            int rowOffset = i % _rows;

            int offsetX = _paddingSize * (columnOffset + 1);
            offsetX += columnOffset * _tileSize;
            int offsetY = _paddingSize * (rowOffset + 1);
            offsetY += rowOffset * _tileSize;

            if(i + 1 == _state.selectedTile && _state.editMode != EditMode.Erase) {
                context.FillRectangle(new Rectangle(offsetX - _paddingSize, offsetY - _paddingSize, _tileSize + _paddingSize * 2, _tileSize + _paddingSize * 2), _selectedTileBorderColor);
            }
            if(i == _highlightedTileIndex) {
                var color = (int)(_state.stateTime * 10) % 2 == 0 ? _highlightedTileColorBright : _highlightedTileColorDark;
                context.FillRectangle(new Rectangle(offsetX - _paddingSize, offsetY - _paddingSize, _tileSize + _paddingSize * 2, _tileSize + _paddingSize * 2), color);
            }
            
            context.Draw(_mapRenderer.GroundTexture, new Rectangle(offsetX, offsetY, _tileSize, _tileSize), new Rectangle(0, 0, 16, 16), Color.White);
            context.Draw(_mapRenderer.TileTextures[i], new Rectangle(offsetX, offsetY, _tileSize, _tileSize), Color.White);

            if(i + 1 == _state.selectedTile && _state.editMode != EditMode.Erase) {
                context.FillRectangle(new Rectangle(offsetX, offsetY, _tileSize, _tileSize), _selectedTileHighlightColor);
            }
        }
    }

    protected override void ProcessInput(InputContext inputContext)
    {
        base.ProcessInput(inputContext);

        if(LocalMousePosition.HasValue) {
            int adjustedTileSize = _tileSize + _paddingSize;
            var mousePosition = LocalMousePosition.GetValueOrDefault();
            var mouseTilePosition = new Point(mousePosition.X / adjustedTileSize, mousePosition.Y / adjustedTileSize);

            _highlightedTileIndex = (mouseTilePosition.X * _rows) + mouseTilePosition.Y;
        }
        else {
            _highlightedTileIndex = -1;
        }
    }

    public override void OnTouchDown()
    {
        base.OnTouchDown();

        if(_highlightedTileIndex != -1) {
            SelectTileAction action = new SelectTileAction(_state, EditMode.Paint, _highlightedTileIndex + 1);
            _state.History.PostAndExecuteAction(action);
        }
    }

}