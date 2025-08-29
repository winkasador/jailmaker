using System;
using System.IO;
using System.Linq;
using Jailbreak.Data.Dto;
using Microsoft.Xna.Framework.Audio;

namespace Jailbreak.Content.Handler;

public class SoundEffectContentHandler : IContentHandler<SoundEffect> {

    private DynamicContentManager _contentManager;

    public SoundEffectContentHandler(DynamicContentManager contentManager) {
        _contentManager = contentManager;
    }

    public SoundEffect Handle(byte[] data) {
        var yaml = System.Text.Encoding.UTF8.GetString(data);
        
        SoundReference soundRef = _contentManager.GetDeserializer().Deserialize<SoundReference>(yaml);
        string path = _contentManager.ResolveFilePath(soundRef.Path);

        if(!File.Exists(path)) {
            Console.WriteLine($"[SoundEffectContentHandler] Failed to load file '{path}', this file does not exist.");
            return null;
        }

        switch(path.Split(".").Last()) {
            case "wav": {
                var sound = SoundEffect.FromFile(path);
                return sound;
            }
            default: {
                Console.WriteLine($"[Texture2DContentHandler] Failed to load file '{path}', this type of file is not supported.");
                return null;
            }
        }
    }

}