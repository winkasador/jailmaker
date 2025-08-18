using System;
using System.Collections.Generic;
using Jailbreak.Input;
using Microsoft.Extensions.DependencyInjection;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Jailbreak.Utility;

namespace Jailbreak.Editor;

public class EditorMenuBar : HorizontalMenu {

    private MenuItem _fileMenu;
    private InputManager _inputManager;
    private Dictionary<ContextRequirement, MenuItem> _contextualMenuItems;

    public EditorMenuBar(IServiceProvider services) {
        _contextualMenuItems = new();
        _inputManager = (InputManager)services.GetRequiredService(typeof(InputManager));

        SetupFileMenu();

        foreach (MenuItem item in MenuHelper.GetAllMenuItems(this)) {
            AddShortcutInformation(item);
        }
    }

    private void SetupFileMenu() {
        _fileMenu = new MenuItem("file", "File");

        MenuItem openFileDialog = new MenuItem("editor.open_file", "Open Project...");
        openFileDialog.Selected += (s, a) => {
            //OpenFileSelectDialog();
        };
        MenuItem closeFile = new MenuItem("editor.close_file", "Close");
        closeFile.Selected += (s, a) => {
            //SetMap(null);
        };
        MenuItem exitMenuItem = new MenuItem("editor.quit", "Quit");
        exitMenuItem.Selected += (s, a) => {
           // Game.Exit();
        };

        /*_fileMenu.Items.Add(openFileDialog);
        _fileMenu.Items.Add(new MenuSeparator());
        _fileMenu.Items.Add(closeFile);
        _fileMenu.Items.Add(new MenuSeparator());
        _fileMenu.Items.Add(exitMenuItem);*/

        Items.Add(_fileMenu);
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

    public enum ContextRequirement {
        MapLoaded,
        UndoAvailable,
        RedoAvailable,
        PasteAvailable
    }

}