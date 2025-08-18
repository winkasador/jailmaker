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

namespace Jailbreak.Editor;

public class EditorScene : Scene.Scene {

    private const float CAMERA_MOVE_SPEED = 400f;
    private const float FAST_CAMERA_MOVE_MODIFIER = 2f;
    private const float CAMERA_ZOOM_SPEED = 2f;
    private const int DEFAULT_TILE_SIZE = 16;
    private const int EMPTY_TILE = 0;
    private const int INVALID_TILE = -1;
    
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
    private MapLoader _mapLoader;

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
    private FileDialog _openFileDialog;

    // Menubar
    private HorizontalMenu _menubar;

    // Contains the IDs to menuitems which are only available when a map is currently loaded.
    private List<string> _mapSpecificMenuItems;

    public EditorScene(Jailbreak game, IServiceProvider services) : base(game, services) {
        _batch = new SpriteBatch(Game.GraphicsDevice);

        _contentManager = GetService<DynamicContentManager>();
        _inputManager = GetService<InputManager>();

        _inputManager.LoadBindingGroup(_contentManager.GetContent<List<KeyBinding>>("escapists/bindings.editor"));

        _state = new EditorState();
        _state.activeFloor = 1;
        _state.selectedTile = 1;

        _state.History = new EditHistory();

        _camera = new Camera(Game.GraphicsDevice.Viewport);
        _renderer = new EditorMapRenderer(Game.GraphicsDevice, _contentManager);

        _debugCameraPositionTexture = _contentManager.GetContent<Texture2D>("escapists/image.camera_position_hint");
        _debugCameraTargetPositionTexture = _contentManager.GetContent<Texture2D>("escapists/image.camera_target_hint");

        _pixelTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);

        _backgroundColor = new Color(19, 20, 19);
        _background = new SolidColorBackground(Game.GraphicsDevice, _backgroundColor);

        _mapLoader = new MapLoader();
        _state.selection = Rectangle.Empty;

        _font = Game.Content.Load<SpriteFont>("escapists/Fonts/Escapists");
        _titleFont = Game.Content.Load<SpriteFont>("escapists/Fonts/8BitSnobbery");

        EditorSoundEffects.LoadSounds(_contentManager);
        _desktop = new Desktop();

        CreateWindows();

        _inputManager.Desktop = _desktop;

        CreateMenuBar();
        UpdateWindowTitle();
        SetMapSpecificMenuItems(false);
    }

    private void CreateMenuBar() {
        _mapSpecificMenuItems = new List<string>();

        _menubar = new HorizontalMenu();

        // File Menu
        MenuItem fileMenu = new MenuItem("file", "File");
        MenuItem openFileDialog = new MenuItem("editor.open_file", "Open Project...");
        openFileDialog.ShortcutColor = Color.Gray;
        openFileDialog.ShortcutText = "Ctrl+O";
        openFileDialog.Selected += (s, a) => {
            OpenFileSelectDialog();
        };
        MenuItem closeFile = new MenuItem("editor.close_file", "Close");
        closeFile.ShortcutColor = Color.Gray;
        closeFile.ShortcutText = "Ctrl+W";
        closeFile.Selected += (s, a) => {
            SetMap(null);
        };
        MenuItem exitMenuItem = new MenuItem("editor.quit", "Quit");
        exitMenuItem.Selected += (s, a) => {
            Game.Exit();
        };

        fileMenu.Items.Add(openFileDialog);
        fileMenu.Items.Add(new MenuSeparator());
        fileMenu.Items.Add(closeFile);
        fileMenu.Items.Add(new MenuSeparator());
        fileMenu.Items.Add(exitMenuItem);

        // Edit Menu
        MenuItem editMenu = new MenuItem("edit", "Edit");
        MenuItem undoItem = new MenuItem("editor.undo", "Undo");
        undoItem.ShortcutColor = Color.Gray;
        undoItem.ShortcutText = "Ctrl+Z";
        undoItem.Selected += (s, a) => {
            _state.History.UndoAndRemoveLatestAction();
        };
        MenuItem redoItem = new MenuItem("editor.redo", "Redo");
        redoItem.ShortcutColor = Color.Gray;
        redoItem.ShortcutText = "Ctrl+Y";
        redoItem.Selected += (s, a) => {
            //
        };

        editMenu.Items.Add(undoItem);
        editMenu.Items.Add(redoItem);

        // View Menu
        MenuItem viewMenu = new MenuItem("view", "View");
        MenuItem showTilesItem = new MenuItem("editor.window.tiles", "Show Tile Palette");
        showTilesItem.ShortcutColor = Color.Gray;
        showTilesItem.ShortcutText = "T";
        showTilesItem.Selected += (s, a) => {
            ToggleTileWindow();
        };
        MenuItem showPropertiesItem = new MenuItem("editor.window.properties", "Show Properties Window");
        showPropertiesItem.ShortcutColor = Color.Gray;
        showPropertiesItem.ShortcutText = "P";
        showPropertiesItem.Selected += (s, a) => {
            TogglePropertiesWindow();
        };
        MenuItem viewMenuToggleGrid = new MenuItem("editor.toggle_grid", "Show Grid");
        viewMenuToggleGrid.ShortcutColor = Color.Gray;
        viewMenuToggleGrid.ShortcutText = "G";
        viewMenuToggleGrid.Selected += (s, a) => {
            _state.drawGrid = !_state.drawGrid;
        };

        viewMenu.Items.Add(showTilesItem);
        viewMenu.Items.Add(showPropertiesItem);
        viewMenu.Items.Add(new MenuSeparator());
        viewMenu.Items.Add(viewMenuToggleGrid);

        _mapSpecificMenuItems.AddRange(["editor.undo", "editor.redo", "editor.toggle_grid", "editor.window.tiles", "editor.window.properties", "editor.close_file"]);

        // Menubar
        _menubar.Items.Add(fileMenu);
        _menubar.Items.Add(editMenu);
        _menubar.Items.Add(viewMenu);

        _desktop.Widgets.Add(_menubar);
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
                    if(tileSelection != INVALID_TILE) {
                        EditMode editMode = tileSelection == EMPTY_TILE ? EditMode.Erase : EditMode.Paint;
                        var action = new SelectTileAction(_state, editMode, tileSelection == EMPTY_TILE ? _state.selectedTile : tileSelection);
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
                        if(_state.Map.GetTileAt(_mouseTilePosition, _state.activeFloor) == EMPTY_TILE) break;
                        var eraseAction = new SetTileAction(_state.Map, _mouseTilePosition, _state.activeFloor, EMPTY_TILE);
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
            OpenFileSelectDialog();
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
            int tileWidth = _state.Map == null ? DEFAULT_TILE_SIZE : _state.Map.GetTilesetTileWidth();
            int tileHeight = _state.Map == null ? DEFAULT_TILE_SIZE : _state.Map.GetTilesetTileHeight();

            _batch.Draw(_debugCameraTargetPositionTexture, _camera.TargetPosition - new Vector2(tileWidth / 2, tileHeight / 2), Color.White);
            _batch.Draw(_debugCameraPositionTexture, _camera.Position - new Vector2(tileWidth / 2, tileHeight / 2), Color.White);
        }

        _batch.End();

        if (_state.Map == null) {
            _batch.Begin();
            DrawNoMapLoadedText();
            _batch.End();
        }

        if (_state.drawDebugWidgets) {
            _batch.Begin();
            DrawDebugInfo();
            _batch.End();
        }

        _desktop.Render();
    }

    public void DrawNoMapLoadedText() {
        Vector2 middle = new Vector2(
            Game.GraphicsDevice.Viewport.Width / 2f,
            Game.GraphicsDevice.Viewport.Height / 2f
        );

        string line1 = "No map loaded!";
        string line2 = "Create a New Project or Open an Existing One";
        int width = (int)Math.Max(_font.MeasureString(line1).X, _font.MeasureString(line2).X);
        int height = (int)(_font.MeasureString(line1).Y + _font.MeasureString(line2).Y);

        Rectangle border = new Rectangle((int)(middle.X - width / 2), (int)(middle.Y - height / 2), width, height);
        Rectangle innerBorder = border;
        innerBorder.Inflate(8, 8);
        Rectangle outerBorder = border;
        outerBorder.Inflate(10, 10);

        _batch.Draw(_pixelTexture, outerBorder, Color.White);
        _batch.Draw(_pixelTexture, innerBorder, _backgroundColor);

        DrawCenteredText(_batch, _font, line1, middle + new Vector2(0, -12), Color.White);
        DrawCenteredText(_batch, _font, line2, middle + new Vector2(0, 12), Color.White);

        int horizontalPadding = 8;
        int topPadding = 48;
        int itemMargin = 8;

        DrawCenteredText(_batch, _titleFont, "JAILMAKER", new Vector2(middle.X, topPadding), Color.LightGray);

        // OFFICIAL PRISONS

        string[] prisons = ["Tutorial", "Centre Perks", "Stalag Flucht", "Shankton State Pen", "Jungle Compound", "San Pancho", "HMP Irongate"];

        int yOffset = topPadding;
        string officialPrisons = "Official Prisons";
        DrawRightAlignedText(_batch, _font, officialPrisons, new Vector2(Game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.LightGray);
        yOffset += (int)_font.MeasureString(officialPrisons).Y + itemMargin;

        foreach (string prison in prisons) {
            DrawRightAlignedText(_batch, _font, prison, new Vector2(Game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.Gray);
            yOffset += (int)_font.MeasureString(prison).Y + itemMargin;
        }

        yOffset += (int)_font.MeasureString("C").Y + itemMargin;

        // BONUS PRISONS

        string[] bonusPrisons = ["Alcatraz", "Banned Camp", "Camp Epsilon", "Duck Tapes Are Forever", "Escape Team", "Fhurst Peak", "Fort Bamford", "Jingle Cells", "Paris Central Pen", "Santa's Sweatshop", "Tower of London"];

        string bonusPrisonsTitle = "Bonus Prisons";
        DrawRightAlignedText(_batch, _font, bonusPrisonsTitle, new Vector2(Game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.LightGray);
        yOffset += (int)_font.MeasureString(bonusPrisonsTitle).Y + itemMargin;

        foreach (string prison in bonusPrisons) {
            DrawRightAlignedText(_batch, _font, prison, new Vector2(Game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.Gray);
            yOffset += (int)_font.MeasureString(prison).Y + itemMargin;
        }

        yOffset += (int)_font.MeasureString("C").Y + itemMargin;

        // CUSTOM PRISONS

        string customPrisonsTitle = "Custom Prisons";
        DrawRightAlignedText(_batch, _font, customPrisonsTitle, new Vector2(Game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.LightGray);
        yOffset += (int)_font.MeasureString(customPrisonsTitle).Y + itemMargin;

        DrawRightAlignedText(_batch, _font, "(No custom maps installed.)", new Vector2(Game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.Gray);

        yOffset = (int)(middle.Y * 2 - _font.MeasureString("C").Y / 2 - 6);

        string attribution = "The Escapists and Included Assets (c) Mouldy Toof Studios and Team17 Digital Ltd.";
        DrawCenteredText(_batch, _font, attribution, new Vector2(middle.X, yOffset), Color.Gray);

        yOffset -= (int)_font.MeasureString(attribution).Y + 6;

        string authorText = "~ A Winkassador! Product ~";
        DrawCenteredText(_batch, _font, authorText, new Vector2(middle.X, yOffset), Color.Gray);
    }

    public void DrawCenteredText(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color) {
        Vector2 textSize = font.MeasureString(text);
        batch.DrawString(font, text, position, color, 0f, textSize / 2f, 1f, SpriteEffects.None, 0f);
    }

    public void DrawRightAlignedText(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color) {
        Vector2 textSize = font.MeasureString(text);
        batch.DrawString(font, text, position, color, 0f, textSize, 1f, SpriteEffects.None, 0f);
    }

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

    private void DrawDebugInfo() {
        int verticalOffset = 4;
        _batch.DrawString(_font, $"Jailbreak Editor 0.1", new Vector2(5, verticalOffset), Color.White);
        _batch.DrawString(_font, $"Mod: '{Game.Mod.Id}' by {Game.Mod.Authors[0].Name}", new Vector2(5, verticalOffset += 25), Color.White);
        _batch.DrawString(_font, $"FPS: {Game.FPS}{(Game.TargetFPS == -1 ? "" : "/" + Game.TargetFPS)}{(Game.Vsync ? " (Vsync)" : "")}", new Vector2(5, verticalOffset += 25), Color.White);
        _batch.DrawString(_font, $"Frame Time: {Game.FrameTime / 10000:F1}ms (Update: {Game.UpdateFrameTime / 10000:F1}ms, Draw: {Game.DrawFrameTime / 10000:F1}ms)", new Vector2(5, verticalOffset += 25), Color.White);
        double memoryUsage = Game.CurrentMemoryUsage / (1024 * 1024);
        double memoryLimit = Game.MaximumAvailableMemory / (1024 * 1024);
        _batch.DrawString(_font, $"Memory: {memoryUsage:F1}MB / {memoryLimit:F1}MB", new Vector2(5, verticalOffset += 25), Color.White);

        verticalOffset += 25;

        _batch.DrawString(_font, "Content:", new Vector2(5, verticalOffset += 25), Color.White);
        foreach (Type contentType in _contentManager.GetSupportedContentTypes()) {
            _batch.DrawString(_font, $"- {_contentManager.GetContentName(contentType)}: {_contentManager.GetAmountOfContentType(contentType)}", new Vector2(5, verticalOffset += 25), Color.White);

        }

        verticalOffset += 25;

        _batch.DrawString(_font, "Editor", new Vector2(5, verticalOffset += 25), Color.White);
        _batch.DrawString(_font, $"Selected Tile: {_state.selectedTile} (Index: {_state.selectedTile - 1})", new Vector2(5, verticalOffset += 25), Color.White);
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
            var fallbackTileset = _contentManager.GetContent<Tileset>("escapists/tileset.perks");
            if (fallbackTileset == null) {
                _logger.Error("Could not load fallback tileset! Aborting map load.");
                SetMap(null);
                ShowMessage("Something Went Wrong...", "The map you tried to load has an invalid tileset and\nJailmaker was unable to load a fallback map.");
                return;
            }
            else {
                map.ChangeTileset(_contentManager.GetContent<Tileset>("escapists/tileset.perks"));
                ShowMessage("Warning", $"The tileset this map is asking for \"{map.TilesetId}\" does not exist.\nJailmaker has selected a fallback one, however it may not look right.\n\nYou can select a valid tileset in the properties menu.");
            }
        }

        var undergroundTexture = _state.Map.IsCustom ?
            _contentManager.GetContent<Texture2D>("escapists/image.ground_soil_custom") :
            _contentManager.GetContent<Texture2D>("escapists/image.ground_soil");

        _renderer.UndergroundTexture = undergroundTexture;
        _renderer.GroundTexture = _contentManager.GetContent<Texture2D>("escapists/image.ground_" + map.TilesetData.Id);
        _renderer.TilesetTexture = _contentManager.GetContent<Texture2D>("escapists/image." + map.TilesetData.Id);


        _camera.Bounds = new Rectangle(0, 0, map.Width * map.GetTilesetTileWidth(), map.Height * map.GetTilesetTileHeight());

        MoveCameraToMapCentre(map);
        SetEditorWindowsVisibility(true);
        SetMapSpecificMenuItems(true);

        UpdateWindowTitle();
    }

    /// <summary>
    /// Enable or disable commands in the menu bar that require a map to be loaded.
    /// </summary>
    public void SetMapSpecificMenuItems(bool enabled) {
        int count = 0;

        foreach (string item in _mapSpecificMenuItems) {
            MenuItem menuItem = _menubar.FindMenuItemById(item);
            if (menuItem != null) {
                menuItem.Enabled = enabled;
                count++;
            }
            else {
                _logger.Warning($"Cannot change activity of menu item \"{item}\" because no such menu item exists.");
            }
        }

        if (enabled) {
            _logger.Information($"Enabled {count} menu item(s).");
        }
        else {
            _logger.Information($"Disabled {count} menu item(s).");
        }
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
