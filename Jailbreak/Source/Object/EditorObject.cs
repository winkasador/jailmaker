using System.Linq;

namespace Jailbreak.Object;

public class EditorObject : BaseObject {

    private ObjectDefinition _definition;
    private string _selectedState;

    public EditorObject(ObjectDefinition definition) {
        _definition = definition;
        _selectedState = _definition.States.First().Key;
    }

    public virtual int GetEscapistsId() {
        return _definition.GetObjectState(_selectedState).EscapistsId;
    }

}