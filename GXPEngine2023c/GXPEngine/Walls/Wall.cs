using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GXPEngine.GlobalVariables;
using TiledMapParser;

public class Wall : AnimationSprite
{
    public readonly bool IsLeft;

    LineSegment lineSegment;

    public Wall(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        IsLeft = obj.GetBoolProperty("isLeft");

        if (IsLeft)
        {
            lineSegment = new LineSegment(new Vec2(obj.X + width, obj.Y - height), new Vec2(obj.X + width, obj.Y), true, true);
        }
        else
        {
            lineSegment = new LineSegment(new Vec2(obj.X, obj.Y), new Vec2(obj.X, obj.Y - height), true, true);
        }

        game.AddChild(lineSegment);
        ((MyGame)game).Lines.Add(lineSegment);
    }
}
