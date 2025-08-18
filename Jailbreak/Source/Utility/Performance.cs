using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Jailbreak.Utility;

public class Performance {

    private Game _game;
    private GraphicsDeviceManager _graphics;

    private Stopwatch _updateLoopStopwatch;
    private Stopwatch _drawLoopStopwatch;

    private float _fps;
    private float _fpsCounter;
    private int _fpsCount;

    public Monitor<int> CurrentFPS { get; }
    public Monitor<int> TargetFPS { get; }
    public Monitor<bool> IsVSync { get; }
    public Monitor<long> FrameTime { get; }
    public Monitor<long> UpdateTime { get; }
    public Monitor<long> DrawTime { get; }
    public Monitor<long> CurrentMemoryUsage { get; }
    public Monitor<long> MaximumAvailableMemory { get; }

    private Dictionary<string, IMonitor> _monitors;

    public Performance(Game game, GraphicsDeviceManager graphics) {
        _game = game;
        _graphics = graphics;

        _updateLoopStopwatch = new Stopwatch();
        _drawLoopStopwatch = new Stopwatch();

        CurrentFPS = new Monitor<int>();
        TargetFPS = new Monitor<int>();
        IsVSync = new Monitor<bool>();
        FrameTime = new Monitor<long>();
        UpdateTime = new Monitor<long>();
        DrawTime = new Monitor<long>();
        CurrentMemoryUsage = new Monitor<long>();
        MaximumAvailableMemory = new Monitor<long>();

        _monitors = new Dictionary<string, IMonitor> {
            { "fps", CurrentFPS },
            { "target_fps", TargetFPS },
            { "vsync", IsVSync },
            { "frame_time", FrameTime },
            { "update_time", UpdateTime },
            { "draw_time", DrawTime },
            { "memory_usage", CurrentMemoryUsage },
            { "maximum_memory", MaximumAvailableMemory },
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
        CurrentMemoryUsage.Value = GC.GetTotalMemory(false);
        MaximumAvailableMemory.Value = Environment.WorkingSet;
        IsVSync.Value = _graphics.SynchronizeWithVerticalRetrace;

        if (!_game.IsFixedTimeStep && !_graphics.SynchronizeWithVerticalRetrace) TargetFPS.Value = -1;
        else TargetFPS.Value = (int)(1 / _game.TargetElapsedTime.TotalSeconds);

        _fpsCounter += delta;
        _fpsCount++;

        if (_fpsCounter >= 1f) {
            _fps = _fpsCount;
            _fpsCount = 0;
            _fpsCounter -= 1f;
        }

        CurrentFPS.Value = (int)_fps;
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