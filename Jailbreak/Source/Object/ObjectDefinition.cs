using System.Collections.Generic;

namespace Jailbreak.Object;

public class ObjectDefinition {

    private string _id;
    private Dictionary<string, EditorObjectState> _states;

    public ObjectDefinition(string id, Dictionary<string, EditorObjectState> states) {
        _id = id;
        _states = states;
    }

    public string Id {
        get { return _id; }
    }

    public Dictionary<string, EditorObjectState> States {
        get { return _states; }
    }

    public EditorObjectState GetObjectState(string id) {
        return _states[id];
    }

    public class EditorObjectState {

        private string _id;
        private int _escapistsId;

        public EditorObjectState(string id, int escapistsId = 0) {
            _id = id;
            _escapistsId = escapistsId;
        }

        public int EscapistsId {
            get {
                return _escapistsId;
            }
        }

    }

}