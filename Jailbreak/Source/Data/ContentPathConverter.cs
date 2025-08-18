using System;
using Jailbreak.Data.Dto;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Jailbreak.Data;

public class ContentPathConverter : IYamlTypeConverter {

    private Jailbreak _jailbreak;

    public ContentPathConverter(Jailbreak jailbreak) {
        _jailbreak = jailbreak;
    }

    public bool Accepts(Type type) => type == typeof(ContentPath);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) {
        var scalar = (Scalar)parser.Current;
        parser.MoveNext();
        return new ContentPathDto(scalar.Value).Resolve(_jailbreak);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer) {
        emitter.Emit(new Scalar(value.ToString().ToLower()));
    }

}