using Jailbreak.Content;
using Microsoft.Xna.Framework.Audio;

namespace Jailbreak.Editor;

public static class EditorSoundEffects {

    public static SoundEffect OPEN_MENU;
    public static SoundEffect CLOSE_MENU;
    public static SoundEffect ASCEND_LAYER;
    public static SoundEffect DESCEND_LAYER;
    public static SoundEffect PICK_TILE;
    public static SoundEffect NOT_ALLOWED;

    public static SoundEffect PAINT_FENCE;
    public static SoundEffect PAINT_FLOOR;
    public static SoundEffect ERASE_TOOL;

    public static void LoadSounds(DynamicContentManager content) {
        OPEN_MENU = content.LoadContent<SoundEffect>("escapists:open");
        CLOSE_MENU = content.LoadContent<SoundEffect>("escapists:close");
        ASCEND_LAYER = content.LoadContent<SoundEffect>("escapists:ascend_floor");
        DESCEND_LAYER = content.LoadContent<SoundEffect>("escapists:descend_floor");
        PICK_TILE = content.LoadContent<SoundEffect>("escapists:pickup_2");
        NOT_ALLOWED = content.LoadContent<SoundEffect>("escapists:not_allowed");

        PAINT_FENCE = content.LoadContent<SoundEffect>("escapists:action_paint_fence");
        PAINT_FLOOR = content.LoadContent<SoundEffect>("escapists:action_paint_floor");
        ERASE_TOOL = content.LoadContent<SoundEffect>("escapists:action_erase");
    }

}