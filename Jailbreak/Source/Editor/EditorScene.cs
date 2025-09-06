using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Jailbreak.Input;
using Jailbreak.Content;
using Jailbreak.Editor.History;
using Jailbreak.Data;
using Jailbreak.World;
using Jailbreak.Interface;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Serilog;
using System.Collections.Generic;
using Jailbreak.Render;
using Jailbreak.Editor.Command;
using Jailbreak.Editor.Interface;

namespace Jailbreak.Editor;

public class EditorScene : Scene.Scene {

    private const float CAMERA_MOVE_SPEED = 400f;
    private const float FAST_CAMERA_MOVE_MODIFIER = 2f;
    private const float CAMERA_ZOOM_SPEED = 2f;
    
    private ILogger _logger = Log.ForContext<EditorScene>();
    private Camera _camera;

    private DynamicContentManager _contentManager;
    private InputManager _inputManager;

    private EditorState _state;
    public EditorState State { get { return _state; }}

    private Vector2 _mousePosition = new Vector2();
    private Point _mouseTilePosition = new Point();

    private SpriteBatch _batch;
    private EditorMapRenderer _renderer;
    private EditorDebugHUD _debugHUD;
    private MapLoader _mapLoader;
    private EditorNoMapLoadedScreen _noMapLoadedScreen;

    private Desktop _desktop;
    private Window _tilePaletteWindow;

    #region Textures

    private Texture2D _pixelTexture;

    private Texture2D _debugCameraPositionTexture;
    private Texture2D _debugCameraTargetPositionTexture;

    #endregion

    private SpriteFont _titleFont;
    private SpriteFont _font;
    private Background _background;
    private Color _backgroundColor;


    private EditorMapPropertiesWindow _propertiesWindow;

    private CommandRegistry _commandRegistry;
    private FileDialog _openFileDialog;

    // Menubar
    private HorizontalMenu _menubar;

    public EditorScene(Jailbreak game) : base(game) {
        _batch = new SpriteBatch(Game.GraphicsDevice);

        _contentManager = game.ContentManager;
        _inputManager = game.InputManager;

        _inputManager.LoadBindingGroup(_contentManager.GetContent<List<KeyBinding>>("escapists:bindings.editor"));

        _state = new EditorState();

        _camera = new Camera(Game.GraphicsDevice.Viewport);
        _renderer = new EditorMapRenderer(Game.GraphicsDevice, _contentManager);

        _debugCameraPositionTexture = _contentManager.GetContent<Texture2D>("escapists:image.camera_position_hint");
        _debugCameraTargetPositionTexture = _contentManager.GetContent<Texture2D>("escapists:image.camera_target_hint");

        _pixelTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);

        _backgroundColor = new Color(19, 20, 19);
        _background = new SolidColorBackground(Game.GraphicsDevice, _backgroundColor);

        _commandRegistry = new CommandRegistry();

        _mapLoader = new MapLoader();

        _font = Game.Content.Load<SpriteFont>("escapists/Fonts/Escapists");
        _titleFont = Game.Content.Load<SpriteFont>("escapists/Fonts/8BitSnobbery");

        _debugHUD = new EditorDebugHUD(game.ModManager, game.Performance, game.ContentManager, _font);

        _noMapLoadedScreen = new EditorNoMapLoadedScreen(Game.GraphicsDevice, _font);

        EditorSoundEffects.LoadSounds(_contentManager);
        _desktop = new Desktop();

        CreateWindows();

        _menubar = new EditorMenuBar(this, _inputManager, _commandRegistry);
        _desktop.Widgets.Add(_menubar);

        _inputManager.Desktop = _desktop;

        //CreateMenuBar();
        UpdateWindowTitle();
        SetMapSpecificMenuItems(false);
    }

    private void CreateWindows() {
        _tilePaletteWindow = new ToggleWindow();
        _tilePaletteWindow.Title = "Tiles";
        _tilePaletteWindow.Content = new EditorTilePalette(_state, _renderer);
        _tilePaletteWindow.Visible = false;

        _propertiesWindow = new EditorMapPropertiesWindow();
        _propertiesWindow.Visible = false;

        _desktop.Widgets.Add(_tilePaletteWindow);
        _desktop.Widgets.Add(_propertiesWindow);
    }

    public override void Ready() {}

    public override void Update(float deltaTime)
    {
        _state.stateTime += deltaTime;
        _state.isEditorActive = _openFileDialog == null && _state.Map != null;

        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        _camera.Viewport = Game.GraphicsDevice.Viewport;

        if(_state.isEditorActive) {
            _mousePosition = _camera.Unproject(mouseState.Position.ToVector2());
            _mouseTilePosition = (_mousePosition / 16).ToPoint();

            if(_inputManager.IsKeybindingTriggered("editor.undo")) {
                _state.History.UndoAndRemoveLatestAction();
            }

            if(_inputManager.IsKeybindingTriggered("editor.toggle_mode")) {
                if(_state.editMode == EditMode.Paint) _state.editMode = EditMode.Erase;
                else if(_state.editMode == EditMode.Erase) _state.editMode = EditMode.Paint;
            }

            if(_inputManager.IsKeybindingTriggered("editor.toggle_grid")) {
                _state.drawGrid = !_state.drawGrid;
            }

            if(_inputManager.IsMouseButtonDown(MouseButton.Middle)) {
                if(!_state.isMiddleMouseButtonClicked) {
                    _state.middleClickStartPoint = _mousePosition.ToPoint();
                    _state.middleClickStartTile = _mouseTilePosition;
                }
                _state.isMiddleMouseButtonClicked = true;
            }
            else if(_inputManager.WasMouseButtonDown(MouseButton.Middle)) {
                _state.isMiddleMouseButtonClicked = false;
                if(_state.middleClickStartTile == _mouseTilePosition) {
                    int tileSelection = _state.Map.GetTileAt(_mouseTilePosition, _state.activeFloor);
                    if(tileSelection != Tileset.InvalidTile) {
                        EditMode editMode = tileSelection == Tileset.EmptyTile ? EditMode.Erase : EditMode.Paint;
                        var action = new SelectTileAction(_state, editMode, tileSelection == Tileset.EmptyTile ? _state.selectedTile : tileSelection);
                        _state.History.PostAndExecuteAction(action);
                    }
                }
            }

            if(_inputManager.IsKeybindingTriggered("editor.ascend_floor") && _state.activeFloor < _state.Map.FloorCount - 1) {
                var action = new ChangeLayerAction(_state, _state.activeFloor + 1);
                _state.History.PostAndExecuteAction(action);
            }
            else if(_inputManager.IsKeybindingTriggered("editor.descend_floor") && _state.activeFloor > 0) {
                var action = new ChangeLayerAction(_state, _state.activeFloor - 1);
                _state.History.PostAndExecuteAction(action);
            }

            if(_inputManager.IsKeybindingTriggered("editor.mode.paint")) {
                _state.editMode = EditMode.Paint;
            } else if(_inputManager.IsKeybindingTriggered("editor.mode.erase")) {
                _state.editMode = EditMode.Erase;
            } else if(_inputManager.IsKeybindingTriggered("editor.mode.select")) {
                _state.editMode = EditMode.Select;
            } else if(_inputManager.IsKeybindingTriggered("editor.mode.fill")) {
                _state.editMode = EditMode.Fill;
            } 

            if(_inputManager.IsKeybindingTriggered("editor.window.tiles")) {
                ToggleTileWindow();
            }

            if(_inputManager.IsKeybindingTriggered("editor.next") && _state.editMode == EditMode.Paint && _state.Map.TilesetData != null) {
                int newTile;

                if(_inputManager.IsKeybindingTriggered("editor.negate"))
                    newTile = _state.Map.TilesetData.GetPreviousTileInGroup(_state.selectedTile);
                else newTile = _state.Map.TilesetData.GetNextTileInGroup(_state.selectedTile);

                _state.selectedTile = newTile;
            }

            if(_inputManager.IsMouseButtonDown(MouseButton.Left) && _state.Map.ContainsWorldPos(_mousePosition.ToPoint())) {
                switch(_state.editMode) {
                    case EditMode.Paint:
                        if(_state.Map.GetTileAt(_mouseTilePosition, _state.activeFloor) == _state.selectedTile) break;
                        var action = new SetTileAction(_state.Map, _mouseTilePosition, _state.activeFloor, _state.selectedTile);
                        _state.History.PostAndExecuteAction(action);
                        break;
                    case EditMode.Erase:
                        if(_state.Map.GetTileAt(_mouseTilePosition, _state.activeFloor) == Tileset.EmptyTile) break;
                        var eraseAction = new SetTileAction(_state.Map, _mouseTilePosition, _state.activeFloor, Tileset.EmptyTile);
                        _state.History.PostAndExecuteAction(eraseAction);
                        break;
                    case EditMode.Select:
                        _state.selection = new Rectangle(_mouseTilePosition.X, _mouseTilePosition.Y, 1, 1);
                        break;
                    case EditMode.Fill:
                        break;
                }
            }

            Vector2 inputVector = _inputManager.GetInputVector("editor.move_up", "editor.move_down", "editor.move_left", "editor.move_right");
            Vector2 cameraMoveAmount = inputVector * CAMERA_MOVE_SPEED * deltaTime;
            if(keyboardState.IsKeyDown(Keys.LeftShift))
                cameraMoveAmount *= FAST_CAMERA_MOVE_MODIFIER;

            if(_inputManager.IsKeyDown("editor.zoom_in"))
                _camera.Zoom *= 1 + deltaTime * CAMERA_ZOOM_SPEED;
            if(_inputManager.IsKeyDown("editor.zoom_out"))
                _camera.Zoom *= 1 - deltaTime * CAMERA_ZOOM_SPEED;

            _camera.TargetPosition += cameraMoveAmount;
            _camera.MoveToTarget(deltaTime);
        }

        if(_inputManager.IsKeybindingTriggered("editor.open_file")) {
            _commandRegistry.GetCommand("editor.open_file").Execute(new CommandContext(this));
        }
        if (_inputManager.IsKeybindingTriggered("editor.quit")) {
            Game.Exit();
        } 

        if (_inputManager.IsKeybindingTriggered("debug.toggle_debug_ui")) {
            _state.drawDebugWidgets = !_state.drawDebugWidgets;
            if (_state.drawDebugWidgets) EditorSoundEffects.OPEN_MENU.Play();
            else EditorSoundEffects.CLOSE_MENU.Play();
        }
    }

    public override void Draw(float deltaTime) {
        Game.GraphicsDevice.Clear(Color.Black);

        _batch.Begin();
        _background.Draw(_batch, _camera.GetMatrix(), new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height));
        _batch.End();

        _batch.Begin(transformMatrix: _camera.GetMatrix(), samplerState: SamplerState.PointWrap, blendState: BlendState.NonPremultiplied);
        if (_state.Map != null) {
            _renderer.RenderMap(_batch, _state.Map, _state.activeFloor);
            //_renderer.DebugDrawShadows(_batch, _state.map, _state.activeFloor);

            // Overlays

            if (_state.Map.ContainsWorldPos(_mousePosition.ToPoint()) && !_inputManager.IsMouseOverWidget()) {
                _renderer.DrawCursor(_batch, _state.editMode, _state.selectedTile, _mouseTilePosition, _state.stateTime);
            }
            if (_state.drawGrid) _renderer.DrawGrid(_batch, _state.Map, 16, 16);
            _renderer.DrawSelection(_batch, _state.selection);
        }

        if (_state.drawDebugWidgets) {
            int tileWidth = _state.Map == null ? Tileset.DefaultTileSize : _state.Map.GetTilesetTileWidth();
            int tileHeight = _state.Map == null ? Tileset.DefaultTileSize : _state.Map.GetTilesetTileHeight();

            _batch.Draw(_debugCameraTargetPositionTexture, _camera.TargetPosition - new Vector2(tileWidth / 2, tileHeight / 2), Color.White);
            _batch.Draw(_debugCameraPositionTexture, _camera.Position - new Vector2(tileWidth / 2, tileHeight / 2), Color.White);
        }

        _batch.End();

        if (_state.Map == null) {
            _batch.Begin();
            _noMapLoadedScreen.Draw(_batch);
            _batch.End();
        }

        if (_state.drawDebugWidgets) {
            _batch.Begin();
            _debugHUD.Draw(_batch, new Rectangle(0, 20, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height - 20));
            _batch.End();
        }

        _desktop.Render();
    }

    // TODO: Move into a TextRenderer class.
    public void DrawCenteredText(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color) {
        Vector2 textSize = font.MeasureString(text);
        batch.DrawString(font, text, position, color, 0f, textSize / 2f, 1f, SpriteEffects.None, 0f);
    }

    // TODO: Move into a TextRenderer class.
    public void DrawRightAlignedText(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color) {
        Vector2 textSize = font.MeasureString(text);
        batch.DrawString(font, text, position, color, 0f, textSize, 1f, SpriteEffects.None, 0f);
    }

    // TODO: Move into a OpenFileDialog class.
    public void OpenFileSelectDialog() {
        if (_openFileDialog != null) return;

        _openFileDialog = new FileDialog(FileDialogMode.OpenFile) {
            Filter = "Custom Map (*.proj)|*.proj|Official Map (*.map*)|*.map",
            FilePath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Escapists\\Data\\Maps\\",
        };
        _openFileDialog.ShowModal(_desktop);
        _openFileDialog.Closed += (s, a) => {
            if (_openFileDialog.Result && _openFileDialog.FilePath.Contains(".")) {
                var extension = _openFileDialog.FilePath.Split(".")[1];

                switch (extension) {
                    case "proj":
                        LoadMap(_openFileDialog.FilePath, false); break;
                    case "map":
                        LoadMap(_openFileDialog.FilePath, true); break;
                    default:
                        var dialog = Dialog.CreateMessageBox("Unknown Map Format", $"'.{extension}' is not a supported map format. Please use either '.map' or '.proj' files.");
                        dialog.Show(_desktop);
                        break;
                }
            }
            _openFileDialog = null;
        };
    }

    public void ToggleTileWindow() {
        _tilePaletteWindow.Visible = !_tilePaletteWindow.Visible;

        if (_tilePaletteWindow.Visible) EditorSoundEffects.OPEN_MENU.Play();
        else EditorSoundEffects.CLOSE_MENU.Play();
    }

    public void TogglePropertiesWindow() {
        _propertiesWindow.Visible = !_propertiesWindow.Visible;

        if (_propertiesWindow.Visible) EditorSoundEffects.OPEN_MENU.Play();
        else EditorSoundEffects.CLOSE_MENU.Play();
    }

    public void MoveCameraToMapCentre(Map map) {
        _camera.SnapTo(new Vector2(map.Width * 8, map.Height * 8));
    }

    private void SetEditorWindowsVisibility(bool isVisible) {
        _tilePaletteWindow.Visible = isVisible;
        _propertiesWindow.Visible = isVisible;
    }

    private void LoadMap(string path, bool isEncrypted) {
        MapLoader.MapLoadResult result = _mapLoader.LoadMap(_contentManager, path, isEncrypted);
        if (result.Status == MapLoader.MapLoadStatus.Success) {
            SetMap(result.Map);
        }
        else {
            switch (result.Status) {
                case MapLoader.MapLoadStatus.DecryptionError:
                    ShowMessage("Something Went Wrong...", "An error occured whilst decrypting this map, and it could not be loaded.\nThis file may be corrupt. Try loading it in the original editor.");
                    break;
                case MapLoader.MapLoadStatus.FileNotFoundError:
                    ShowMessage("Something Went Wrong...", "The file you tried to load does not appear to exist.");
                    break;
                case MapLoader.MapLoadStatus.MapInitializationError:
                    ShowMessage("Something Went Wrong...", "Jailmaker could not read the file, it may be corrupt.\nTry loading it in the original editor, this may fix it.");
                    break;
                case MapLoader.MapLoadStatus.UnknownError:
                    ShowMessage("Something Went Wrong...", "Something went wrong and this file could not be opened.");
                    break;
            }
        }
    }

    private void SetMap(Map map) {
        _state.History.Clear();

        if (map == null) {
            _logger.Information($"Set map to null.");
            _state.Map = null;
            SetEditorWindowsVisibility(false);
            UpdateWindowTitle();
            SetMapSpecificMenuItems(false);
            return;
        }

        _logger.Information($"Preparing to load map: \"{map.MapName}\".");

        _state.Map = map;

        if (map.TilesetData == null) {
            _logger.Warning($"Requested tileset \"{map.TilesetId}\" does not exist, falling back to default.");
            var fallbackTileset = _contentManager.GetContent<Tileset>("escapists:tileset.perks");
            if (fallbackTileset == null) {
                _logger.Error("Could not load fallback tileset! Aborting map load.");
                SetMap(null);
                ShowMessage("Something Went Wrong...", "The map you tried to load has an invalid tileset and\nJailmaker was unable to load a fallback map.");
                return;
            }
            else {
                map.ChangeTileset(_contentManager.GetContent<Tileset>("escapists:tileset.perks"));
                ShowMessage("Warning", $"The tileset this map is asking for \"{map.TilesetId}\" does not exist.\nJailmaker has selected a fallback one, however it may not look right.\n\nYou can select a valid tileset in the properties menu.");
            }
        }

        var undergroundTexture = _state.Map.IsCustom ?
            _contentManager.GetContent<Texture2D>("escapists:image.ground_soil_custom") :
            _contentManager.GetContent<Texture2D>("escapists:image.ground_soil");

        _renderer.UndergroundTexture = undergroundTexture;
        _renderer.GroundTexture = _contentManager.GetContent<Texture2D>("escapists:image.ground_" + map.TilesetData.Id);
        _renderer.TilesetTexture = _contentManager.GetContent<Texture2D>("escapists:image." + map.TilesetData.Id);


        _camera.Bounds = new Rectangle(0, 0, map.Width * map.GetTilesetTileWidth(), map.Height * map.GetTilesetTileHeight());

        MoveCameraToMapCentre(map);
        SetEditorWindowsVisibility(true);
        SetMapSpecificMenuItems(true);

        UpdateWindowTitle();
    }

    /// <summary>
    /// Enable or disable commands in the menu bar that require a map to be loaded.
    /// </summary>
    [Obsolete]
    public void SetMapSpecificMenuItems(bool enabled) {
        return;
    }

    public void ShowMessage(string title, string message) {
        var dialog = Dialog.CreateMessageBox(title, message);

        _desktop.Widgets.Add(dialog);
        _logger.Information($"Dialog Box - \"{title}\": \"{message}\".");
    }

    private void UpdateWindowTitle() {
        if (_state.Map == null) {
            Game.Window.Title = "Jailmaker - No Map Loaded";
        }
        else {
            Game.Window.Title = $"Jailmaker - {_state.Map.MapName}" + (_state.Map.IsCustom ? "" : " (Official)");
        }
    }

}
