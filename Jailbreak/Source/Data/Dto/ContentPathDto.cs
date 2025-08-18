namespace Jailbreak.Data.Dto;

public class ContentPathDto {

    public string Path { get; set; }

    public ContentPathDto(string path) {
        Path = path;
    }

    public ContentPath Resolve(Jailbreak jailbreak) {
        string resolvedPath = Path;
        resolvedPath = resolvedPath.Replace("Content|", "escapists/");
        resolvedPath = resolvedPath.Replace("Escapists|", "C:/Program Files (x86)/Steam/steamapps/common/The Escapists/Data/");

        return new ContentPath(resolvedPath);
    }

}