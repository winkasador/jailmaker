using System;
using Jailbreak.Input;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Jailbreak.Data;

public class KeysTypeConverter : IYamlTypeConverter {

    public bool Accepts(Type type) => type == typeof(Microsoft.Xna.Framework.Input.Keys);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) {
        var scalar = (Scalar)parser.Current;
        parser.MoveNext();
        return KeyMap.StringToKey(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer) {
        emitter.Emit(new Scalar(value.ToString().ToLower()));
    }

}