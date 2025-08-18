using System.Collections.Generic;

namespace Jailbreak.Data.Dto;

public class ImageReference {

    public string Path { get; set; }
    public Dictionary<string, string> Encryption { get; set; }

}