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

    public End(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        NextScene = obj.GetStringProperty("NextScene", "endMenu.tmx");

        SetOrigin(obj.Width / 2, obj.Height / 2);
        position = new Vec2(obj.X + obj.Width / 2, obj.Y - obj.Height / 2);
        SetXY(position.x, position.y);

        Radius = obj.Width / 2;

        SetCycle(0, frameCount-1);
    }

    void Update()
    {
        AnimateFixed();
    }
}
