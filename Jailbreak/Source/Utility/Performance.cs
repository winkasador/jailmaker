using System.Collections.Generic;

namespace Jailbreak.Utility;

public static class Performance {

    public const string FPS = "fps";
    public const string TARGET_FPS = "target_fps";
    public const string VSYNC = "vsync";

    private static Dictionary<string, IMonitor> _monitors;

    static Performance() {
        _monitors = new Dictionary<string, IMonitor> {
            { FPS, new Monitor<int>() },
            { TARGET_FPS, new Monitor<int>() },
            { VSYNC, new Monitor<bool>() }
        };
    }

    public static void RegisterMonitor(string monitorName, IMonitor monitor) {
        _monitors.Add(monitorName, monitor);
    }

    public static Monitor<T> GetMonitor<T>(string monitorName) {
        if(_monitors.TryGetValue(monitorName, out var monitor)) {
            if (monitor is Monitor<T> tMonitor) {
                return tMonitor;
            }
        }

        return null;
    }

    public interface IMonitor {}

    public class Monitor<T> : IMonitor {
        
        private T _value;

        public Monitor(T value = default) {
            _value = value;
        }

        public T Value {
            get { return _value; }
            set { _value = value; }
        }

    }

}