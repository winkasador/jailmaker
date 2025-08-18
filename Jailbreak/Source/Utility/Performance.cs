using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Jailbreak.Utility;

public class Performance {

    private Game _game;
    private GraphicsDeviceManager _graphics;

    private Stopwatch _updateLoopStopwatch;
    private Stopwatch _drawLoopStopwatch;

    public Monitor<int> CurrentFPS { get; }
    public Monitor<int> TargetFPS { get; }
    public Monitor<bool> IsVSync { get; }
    public Monitor<long> FrameTime { get; }
    public Monitor<long> UpdateTime { get; }
    public Monitor<long> DrawTime { get; }

    private Dictionary<string, IMonitor> _monitors;

    public Performance(Game game, GraphicsDeviceManager graphics) {
        _game = game;
        _graphics = graphics;

        _updateLoopStopwatch = new Stopwatch();
        _drawLoopStopwatch = new Stopwatch();

        CurrentFPS = new Monitor<int>();
        TargetFPS = new Monitor<int>();
        IsVSync = new Monitor<bool>();

        _monitors = new Dictionary<string, IMonitor> {
            { "fps", CurrentFPS },
            { "target_fps", TargetFPS },
            { "vsync", IsVSync }
        };
    }

    public void BeginUpdate() => _updateLoopStopwatch.Restart();
    public void EndUpdate() {
        _updateLoopStopwatch.Stop();
        UpdateTime.Value = _updateLoopStopwatch.Elapsed.Ticks;
    }

    public void BeginDraw() => _drawLoopStopwatch.Restart();
    public void EndDraw() {
        _drawLoopStopwatch.Stop();
        DrawTime.Value = _drawLoopStopwatch.Elapsed.Ticks;
    }

    public void Update(float delta) {
        FrameTime.Value = UpdateTime.Value + DrawTime.Value;
    }

    public void AddCustomMonitor(string monitorName, IMonitor monitor) {
        _monitors.Add(monitorName, monitor);
    }

    public Monitor<T> GetMonitor<T>(string monitorName) {
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