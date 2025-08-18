namespace Jailbreak.Object;

public class MissingObject : EditorObject {
    
    private int _escapistsId;

    public MissingObject(ObjectDefinition missingObjectDefinition, int escapistsId) : base(missingObjectDefinition) {
        _escapistsId = escapistsId;
    }

    public override int GetEscapistsId()
    {
        return _escapistsId;
    }

}