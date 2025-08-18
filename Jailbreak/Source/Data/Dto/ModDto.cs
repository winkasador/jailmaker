using System.Collections.Generic;
using Jailbreak.Content;

namespace Jailbreak.Data.Dto;

public class ModDto {

    public string Id { get; set;}
    public List<CreditedUser> Authors { get; set;}
    public List<CreditedUser> Credits { get; set;}
    public Dictionary<string, string> Macros { get; set;}

    public ModDefinition ToModDefinition() {
        return new ModDefinition(Id, Authors, Credits, Macros);
    }

}