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

    public static void LoadSounds(DynamicContentManager assetManager) {
        OPEN_MENU = assetManager.LoadContent<SoundEffect>("escapists/sound.open");
        CLOSE_MENU = assetManager.LoadContent<SoundEffect>("escapists/sound.close");
        ASCEND_LAYER = assetManager.LoadContent<SoundEffect>("escapists/sound.ascend_floor");
        DESCEND_LAYER = assetManager.LoadContent<SoundEffect>("escapists/sound.descend_floor");
        PICK_TILE = assetManager.LoadContent<SoundEffect>("escapists/sound.pickup_2");
        NOT_ALLOWED = assetManager.LoadContent<SoundEffect>("escapists/sound.not_allowed");

        PAINT_FENCE = assetManager.LoadContent<SoundEffect>("escapists/sound.action_paint_fence");
        PAINT_FLOOR = assetManager.LoadContent<SoundEffect>("escapists/sound.action_paint_floor");
        ERASE_TOOL = assetManager.LoadContent<SoundEffect>("escapists/sound.action_erase");
    }

}