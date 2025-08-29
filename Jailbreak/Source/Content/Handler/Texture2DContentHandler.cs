using System;
using System.IO;
using System.Linq;
using Jailbreak.Data.Dto;
using Jailbreak.Data.Encryption;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Content.Handler;

public class Texture2DContentHandler : IContentHandler<Texture2D> {

    private GraphicsDevice _device;
    private DynamicContentManager _contentManager;

    public Texture2DContentHandler(GraphicsDevice device, DynamicContentManager contentManager) {
        _device = device;
        _contentManager = contentManager;
    }

    public Texture2D Handle(byte[] bytes) {
        var yaml = System.Text.Encoding.UTF8.GetString(bytes);
        
        ImageReference imageDto = _contentManager.GetDeserializer().Deserialize<ImageReference>(yaml);
        string path = _contentManager.ResolveFilePath(imageDto.Path);

        if(!File.Exists(path)) {
            Console.WriteLine($"[Texture2DContentHandler] Failed to load file '{path}', this file does not exist.");
            return null;
        }

        switch(path.Split(".").Last()) {
            case "gif": {
                if(imageDto.Encryption == null) {
                    using(FileStream stream = new FileStream(path, FileMode.Open)) {
                        return Texture2D.FromStream(_device, stream);
                    }
                }
                else if(imageDto.Encryption.ContainsKey("mode")) {
                    switch(imageDto.Encryption["mode"]) {
                        case "blowfish-compat": {
                            BlowfishCompat blowfishCompat = new BlowfishCompat(imageDto.Encryption["key"]);
                            byte[] imageBytes = File.ReadAllBytes(path);
                            byte[] decryptedBytes = blowfishCompat.Decrypt(imageBytes);

                            using (MemoryStream stream = new MemoryStream(decryptedBytes)) {
                                return Texture2D.FromStream(_device, stream);
                            }
                        }
                    }
                }
                break;
            }
            case "jpg": 
            case "jpeg":
            case "png": {
                using(FileStream stream = new FileStream(path, FileMode.Open)) {
                    return Texture2D.FromStream(_device, stream);
                }
            }
            default: {
                Console.WriteLine($"[Texture2DContentHandler] Failed to load file '{path}', this type of file is not supported.");
                break;
            }
        }

        return null;
    }

}