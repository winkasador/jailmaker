using System.Collections.Generic;
using System.Linq;

namespace Jailbreak.Editor.History;

public class EditHistory {

    private const int MAX_HISTORY_LENGTH = 256;

    private List<IAction> _editHistory = [];

    public int HistoryLength { get { return _editHistory.Count;}}

    public void PostAndExecuteAction(IAction action) {
        if(_editHistory.Count >= MAX_HISTORY_LENGTH) {
            _editHistory.RemoveAt(0);
        }
        _editHistory.Add(action);

        action.PerformAction();
    }

    public void UndoAndRemoveLatestAction() {
        if(_editHistory.Count == 0) return;
        var action = _editHistory.Last();
        action.UndoAction();
        _editHistory.Remove(action);
    }

    public void Clear() {
        _editHistory.Clear();
    }

}