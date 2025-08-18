using System;
using Microsoft.Xna.Framework.Input;

namespace Jailbreak.Input;

public static class KeyMap {

    public static Keys StringToKey(string key) {
        if(key == "") return Keys.None;

        switch (key) {
            case "none": return Keys.None;
            case "back": return Keys.Back;
            case "tab": return Keys.Tab;
            case "enter": return Keys.Enter;
            case "caps_lock": return Keys.CapsLock;
            case "escape": return Keys.Escape;
            case "space": return Keys.Space;
            case "page_up": return Keys.PageUp;
            case "page_down": return Keys.PageDown;
            case "end": return Keys.End;
            case "home": return Keys.Home;
            case "left": return Keys.Left;
            case "up": return Keys.Up;
            case "right": return Keys.Right;
            case "down": return Keys.Down;
            case "select": return Keys.Select;
            case "print": return Keys.Print;
            case "execute": return Keys.Execute;
            case "print_screen": return Keys.PrintScreen;
            case "insert": return Keys.Insert;
            case "delete": return Keys.Delete;
            case "help": return Keys.Help;
            case "d0": return Keys.D0;
            case "d1": return Keys.D1;
            case "d2": return Keys.D2;
            case "d3": return Keys.D3;
            case "d4": return Keys.D4;
            case "d5": return Keys.D5;
            case "d6": return Keys.D6;
            case "d7": return Keys.D7;
            case "d8": return Keys.D8;
            case "d9": return Keys.D9;
            case "a": return Keys.A;
            case "b": return Keys.B;
            case "c": return Keys.C;
            case "d": return Keys.D;
            case "e": return Keys.E;
            case "f": return Keys.F;
            case "g": return Keys.G;
            case "h": return Keys.H;
            case "i": return Keys.I;
            case "j": return Keys.J;
            case "k": return Keys.K;
            case "l": return Keys.L;
            case "m": return Keys.M;
            case "n": return Keys.N;
            case "o": return Keys.O;
            case "p": return Keys.P;
            case "q": return Keys.Q;
            case "r": return Keys.R;
            case "s": return Keys.S;
            case "t": return Keys.T;
            case "u": return Keys.U;
            case "v": return Keys.V;
            case "w": return Keys.W;
            case "x": return Keys.X;
            case "y": return Keys.Y;
            case "z": return Keys.Z;
            case "windows":
            case "left_windows": return Keys.LeftWindows;
            case "right_windows": return Keys.RightWindows;
            case "apps": return Keys.Apps;
            case "sleep": return Keys.Sleep;
            case "numpad0": return Keys.NumPad0;
            case "numpad1": return Keys.NumPad1;
            case "numpad2": return Keys.NumPad2;
            case "numpad3": return Keys.NumPad3;
            case "numpad4": return Keys.NumPad4;
            case "numpad5": return Keys.NumPad5;
            case "numpad6": return Keys.NumPad6;
            case "numpad7": return Keys.NumPad7;
            case "numpad8": return Keys.NumPad8;
            case "numpad9": return Keys.NumPad9;
            case "multiply": return Keys.Multiply;
            case "add": return Keys.Add;
            case "separator": return Keys.Separator;
            case "subtract": return Keys.Subtract;
            case "decimal": return Keys.Decimal;
            case "divide": return Keys.Divide;
            case "f1": return Keys.F1;
            case "f2": return Keys.F2;
            case "f3": return Keys.F3;
            case "f4": return Keys.F4;
            case "f5": return Keys.F5;
            case "f6": return Keys.F6;
            case "f7": return Keys.F7;
            case "f8": return Keys.F8;
            case "f9": return Keys.F9;
            case "f10": return Keys.F10;
            case "f11": return Keys.F11;
            case "f12": return Keys.F12;
            case "f13": return Keys.F13;
            case "f14": return Keys.F14;
            case "f15": return Keys.F15;
            case "f16": return Keys.F16;
            case "f17": return Keys.F17;
            case "f18": return Keys.F18;
            case "f19": return Keys.F19;
            case "f20": return Keys.F20;
            case "f21": return Keys.F21;
            case "f22": return Keys.F22;
            case "f23": return Keys.F23;
            case "f24": return Keys.F24;
            case "num_lock": return Keys.NumLock;
            case "scroll": return Keys.Scroll;
            case "shift":
            case "left_shift": return Keys.LeftShift;
            case "right_shift": return Keys.RightShift;
            case "control":
            case "ctrl":
            case "left_control": return Keys.LeftControl;
            case "right_control": return Keys.RightControl;
            case "alt":
            case "alternate":
            case "left_alt": return Keys.LeftAlt;
            case "right_alt": return Keys.RightAlt;
            case "browser_back": return Keys.BrowserBack;
            case "browser_forward": return Keys.BrowserForward;
            case "browser_refresh": return Keys.BrowserRefresh;
            case "browser_stop": return Keys.BrowserStop;
            case "browser_search": return Keys.BrowserSearch;
            case "browser_favorites": return Keys.BrowserFavorites;
            case "browser_home": return Keys.BrowserHome;
            case "volume_mute": return Keys.VolumeMute;
            case "volume_down": return Keys.VolumeDown;
            case "volume_up": return Keys.VolumeUp;
            case "media_next_track": return Keys.MediaNextTrack;
            case "media_previous_track": return Keys.MediaPreviousTrack;
            case "media_stop": return Keys.MediaStop;
            case "media_play_pause": return Keys.MediaPlayPause;
            case "launch_mail": return Keys.LaunchMail;
            case "select_media": return Keys.SelectMedia;
            case "launch_application1": return Keys.LaunchApplication1;
            case "launch_application2": return Keys.LaunchApplication2;
            case "semicolon": return Keys.OemSemicolon;
            case "plus": return Keys.OemPlus;
            case "comma": return Keys.OemComma;
            case "minus": return Keys.OemMinus;
            case "period": return Keys.OemPeriod;
            case "question": return Keys.OemQuestion;
            case "tilde": return Keys.OemTilde;
            case "open_bracket": return Keys.OemOpenBrackets;
            case "pipe": return Keys.OemPipe;
            case "close_bracket": return Keys.OemCloseBrackets;
            case "quotes": return Keys.OemQuotes;
            case "8": return Keys.Oem8;
            case "backslash": return Keys.OemBackslash;
            case "process_key": return Keys.ProcessKey;
            case "attn": return Keys.Attn;
            case "crsel": return Keys.Crsel;
            case "exsel": return Keys.Exsel;
            case "erase_eof": return Keys.EraseEof;
            case "play": return Keys.Play;
            case "zoom": return Keys.Zoom;
            case "pa1": return Keys.Pa1;
            case "clear": return Keys.OemClear;
            case "chatpad_green": return Keys.ChatPadGreen;
            case "chatpad_orange": return Keys.ChatPadOrange;
            case "pause": return Keys.Pause;
            case "convert": return Keys.ImeConvert;
            case "no_convert": return Keys.ImeNoConvert;
            case "kana": return Keys.Kana;
            case "kanji": return Keys.Kanji;
            case "auto": return Keys.OemAuto;
            case "copy": return Keys.OemCopy;
            case "enlarge_window": return Keys.OemEnlW;
            default: {
                Console.WriteLine($"[KeyMap] Unknown Key \"{key}\".");
                return Keys.None;
            }
        }
    }

}