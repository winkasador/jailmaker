namespace Jailbreak.Editor.History;

public interface IAction {

    void PerformAction();
    void UndoAction();

}