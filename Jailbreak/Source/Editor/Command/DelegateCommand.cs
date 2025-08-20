using System;

namespace Jailbreak.Editor.Command;

public class DelegateCommand : ICommand {

    private readonly Action<CommandContext> _action;

    public DelegateCommand(Action<CommandContext> action) {
        _action = action;
    }

    public void Execute(CommandContext context)
        => _action(context);

}