using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Jailbreak.Content;
using Jailbreak.Data.Encryption;
using Jailbreak.World;
using Serilog;

namespace Jailbreak.Data;

public class MapLoader {

    private readonly ILogger _logger = Log.ForContext<MapLoader>();

    public MapLoadResult LoadMap(DynamicContentManager content, string path, bool isEncrypted) {
        if (!File.Exists(path)) {
            _logger.Error($"Failed to decrypt map at {path} because it does not exist!");
            return MapLoadResult.Failure(MapLoadStatus.FileNotFoundError);
        }

        try {
            PropertyContainer file;
            if (isEncrypted) {
                file = OpenPrivateFile(path);
            }
            else {
                file = OpenFile(path);
            }

            // Initialize map and return
            try {
                string id = path.Split("\\").Last().Split(".")[0];

                Map map = new Map(content, file, id);
                return MapLoadResult.Success(map);
            }
            catch (Exception e) {
                _logger.Error($"Failed to initialize map at {path}", e);
                return MapLoadResult.Failure(MapLoadStatus.MapInitializationError, e);
            }
        }
        catch (CryptographicException e) {
            _logger.Error($"Failed to decrypt map at {path}", e);
            return MapLoadResult.Failure(MapLoadStatus.DecryptionError, e);
        }
        catch (Exception e) {
            _logger.Error($"Failed to load map at {path}", e);
            return MapLoadResult.Failure(MapLoadStatus.UnknownError, e);
        }
    }

    private PropertyContainer OpenFile(string path) {
        _logger.Information($"Opening File: \"{path}\".");
        string contents = File.ReadAllText(path);

        return new PropertyContainer(contents);
    }

    private PropertyContainer OpenPrivateFile(string path) {
        _logger.Information($"Opening Private File: \"{path}\".");
        BlowfishCompat blowfishCompat = new BlowfishCompat("mothking");
        byte[] bytes = File.ReadAllBytes(path);
        byte[] decryptedBytes = blowfishCompat.Decrypt(bytes);

        string decryptedData = System.Text.Encoding.UTF8.GetString(decryptedBytes);

        return new PropertyContainer(decryptedData);
    }

    public class MapLoadResult {

        public MapLoadStatus Status { get; }
        public Map Map { get; }

        public Exception? Exception { get; }

        private MapLoadResult(MapLoadStatus status, Map map = null, Exception exception = null) {
            Status = status;
            Map = map;
            Exception = exception;
        }

        public static MapLoadResult Success(Map map) =>
            new(MapLoadStatus.Success, map ?? throw new ArgumentNullException(nameof(map)), null);

        public static MapLoadResult Failure(MapLoadStatus status, Exception exception = null) =>
            new MapLoadResult(status, null, exception);

    }

    public enum MapLoadStatus {
        Success,
        DecryptionError,
        FileNotFoundError,
        MapInitializationError,
        UnknownError
    }

}