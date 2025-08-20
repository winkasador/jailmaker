using System.Collections.Generic;

namespace Jailbreak.Editor.Command;

public class CommandRegistry {

    private Dictionary<string, ICommand> _commands = new();

    public CommandRegistry() {
        _commands.Add("editor.open_file", new DelegateCommand(ctx => ctx.ShowOpenFileDialog()));
    }

    public ICommand GetCommand(string commandName) {
        if (_commands.TryGetValue(commandName, out ICommand command)) {
            return command;
        }
        else return null;
    }

}