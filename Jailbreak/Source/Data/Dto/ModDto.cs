using System.Collections.Generic;
using Jailbreak.Content;

namespace Jailbreak.Data.Dto;

public class ModDto {

    public string Id { get; set;}
    public string Type { get; set; }
    public List<CreditedUser> Authors { get; set; }
    public List<CreditedUser> Credits { get; set;}
    public Dictionary<string, string> Macros { get; set;}
    public Dictionary<string, List<string>> Content { get; set; }

    public ModDefinition ToModDefinition() {
        ModDefinition.ModType typeEnum;
        switch (Type) {
            case "mod":
                typeEnum = ModDefinition.ModType.Mod;
                break;
            case "utility":
                typeEnum = ModDefinition.ModType.Utility;
                break;
            default:
                typeEnum = ModDefinition.ModType.Utility;
                break;
        }

        return new ModDefinition(Id, typeEnum, Authors, Credits, Macros, Content);
    }

}