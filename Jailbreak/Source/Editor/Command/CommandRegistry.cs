using System.Collections.Generic;

namespace Jailbreak.Editor.Command;

public class CommandRegistry {

    private Dictionary<string, ICommand> _commands = new();

    public CommandRegistry() {
        _commands.Add("editor.open_file", new DelegateCommand(ctx => ctx.ShowOpenFileDialog()));
        _commands.Add("editor.undo", new DelegateCommand(ctx => ctx.State.History.UndoAndRemoveLatestAction()));
        _commands.Add("editor.toggle_tile_palette", new DelegateCommand(ctx => ctx.ToggleTileWindow()));
        _commands.Add("editor.quit", new DelegateCommand(ctx => ctx.QuitApplication()));
        _commands.Add("editor.show_grid", new DelegateCommand(ctx => ctx.State.drawGrid = !ctx.State.drawGrid));
    }

    public ICommand GetCommand(string commandName) {
        if (_commands.TryGetValue(commandName, out ICommand command)) {
            return command;
        }
        else return null;
    }

    public enum CommandRequirement {
        MapLoaded,
        UndoAvailable,
        RedoAvailable,
        PasteAvailable
    }

}