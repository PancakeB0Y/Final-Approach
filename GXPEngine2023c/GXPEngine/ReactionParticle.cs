using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ReactionParticle : AnimationSprite
{
    public ReactionParticle(string filename, int cols, int rows) : base(filename, cols, rows, -1, false, false)
    {
        SetOrigin(width / 2, height / 2);
        _animationDelay = 3;
    }

    void Update()
    {
        Animate();
        if (currentFrame == frameCount - 1)
        {
            Destroy();
        }
    }
}
