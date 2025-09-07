using Jailbreak.Data;
using Jailbreak.Data.Dto;

namespace Jailbreak.Content.Handler;

public class TilesetContentHandler : IContentHandler<TilesetData> {
    
    private DynamicContentManager _contentManager;

    public TilesetContentHandler(DynamicContentManager contentManager) {
        _contentManager = contentManager;
    }

    public TilesetData Handle(byte[] bytes) {
        var yaml = System.Text.Encoding.UTF8.GetString(bytes);
        TilesetDto dto = _contentManager.GetDeserializer().Deserialize<TilesetDto>(yaml);
        return dto.ToTileset(_contentManager);
    }

}