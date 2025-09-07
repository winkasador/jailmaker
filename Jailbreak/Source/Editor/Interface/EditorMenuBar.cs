using System.Collections.Generic;
using Jailbreak.Input;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Jailbreak.Utility;
using Jailbreak.Editor.Command;

namespace Jailbreak.Editor.Interface;

public class EditorMenuBar : HorizontalMenu {

    private InputManager _inputManager;

    private EditorScene _editor;
    private CommandRegistry _registry;

    private MenuItem _fileMenu;
    private MenuItem _viewMenu;

    private Dictionary<CommandRegistry.CommandRequirement, MenuItem> _contextualMenuItems;

    public EditorMenuBar(EditorScene editor, InputManager inputManager, CommandRegistry registry) {
        _contextualMenuItems = new();
        _inputManager = inputManager;

        _editor = editor;
        _registry = registry;

        SetUpFileMenu();
        SetUpViewMenu();

        foreach (MenuItem item in MenuHelper.GetAllMenuItems(this)) {
            AddShortcutInformation(item);
        }
    }

    private void SetUpFileMenu() {
        _fileMenu = new MenuItem("file", "File");

        MenuItem openFileDialog = new MenuItem("editor.open_file", "Open Project...");
        openFileDialog.Selected += (s, a) => {
            _registry.GetCommand("editor.open_file").Execute(new CommandContext(_editor));
        };
        MenuItem closeFile = new MenuItem("editor.close_file", "Close");
        closeFile.Selected += (s, a) => {
            //SetMap(null);
        };
        MenuItem exitMenuItem = new MenuItem("editor.quit", "Quit");
        exitMenuItem.Selected += (s, a) => {
            _registry.GetCommand("editor.quit").Execute(new CommandContext(_editor));
        };

        _fileMenu.Items.Add(openFileDialog);
        _fileMenu.Items.Add(new MenuSeparator());
        _fileMenu.Items.Add(closeFile);
        _fileMenu.Items.Add(new MenuSeparator());
        _fileMenu.Items.Add(exitMenuItem);

        Items.Add(_fileMenu);
    }

    private void SetUpViewMenu() {
        _viewMenu = new MenuItem("view", "View");

        MenuItem gridVisibilityItem = new MenuItem("editor.toggle_grid", "Show Grid");
        gridVisibilityItem.Selected += (s, a) => {
            _registry.GetCommand("editor.show_grid").Execute(new CommandContext(_editor));
        };

        _viewMenu.Items.Add(gridVisibilityItem);

        Items.Add(_viewMenu);
    }
    
    private void AddShortcutInformation(MenuItem item) {
        string text = "";

        KeyBinding binding = _inputManager.GetKeyBinding(item.Id);
        if (binding.PrimaryKey != Keys.None) {
            foreach (Keys key in binding.PrimaryModifiers) {
                text += key + "+";
            }

            text += binding.PrimaryKey;
        }
        if (binding.SecondaryKey != Keys.None) {
            text += "; ";

            foreach (Keys key in binding.SecondaryModifiers) {
                text += key + "+";
            }

            text += binding.SecondaryKey;
        }

        if (text != "") {
            item.ShortcutText = text;
            item.ShortcutColor = Color.Gray;
        }
    }

}