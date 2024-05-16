using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GXPEngine.GlobalVariables;
using TiledMapParser;

public class End : AnimationSprite
{
    public string NextScene;
    public Vec2 position;
    public float Radius;

    bool forwards = true;
    public End(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        NextScene = obj.GetStringProperty("NextScene", "endMenu.tmx");

        SetOrigin(obj.Width / 2, obj.Height / 2);
        position = new Vec2(obj.X + obj.Width / 2, obj.Y - obj.Height / 2);
        SetXY(position.x, position.y);

        Radius = obj.Width / 2;

        SetCycle(0);
    }

    void Update()
    {
        Animate(0.10f);
        if(currentFrame == 5)
        {
            forwards = false;
        }else if(currentFrame == 0) {
            forwards = true;
        }

        if (forwards)
        {
            SetCycle(currentFrame + 1, 1, 255, false);
        }
        else
        {
            SetCycle(currentFrame - 1, 1, 255, false);
        }
        position.x = x;
        position.y = y;
    }
}
