using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Utility;

public class AnimatedFrames {

    private List<Texture2D> _frames;

    public Texture2D GetFrame(float _stateTime) {
        int maxStateTime = _frames.Count;
        float adjustedStateTime = _stateTime - ((int)(_stateTime / maxStateTime) * maxStateTime);

        return _frames[(int)adjustedStateTime];
    }

}