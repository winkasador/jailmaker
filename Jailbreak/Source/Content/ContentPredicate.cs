using System;

namespace Jailbreak.Content;

public class ContentPredicate {

    public string Id { get; set; }
    public string Path { get; set; }
    public Type DataType { get; set; }

    public ContentPredicate(string id, string path, Type type) {
        Id = id;
        Path = path;
        DataType = type;
    }

}