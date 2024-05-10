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
    public LineSegment LineSegment;

    public Wall(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        IsLeft = obj.GetBoolProperty("isLeft");

        if (IsLeft)
        {
            LineSegment = new LineSegment(new Vec2(obj.X + width, obj.Y - height), new Vec2(obj.X + width, obj.Y), false, false);
        }
        else
        {
            LineSegment = new LineSegment(new Vec2(obj.X, obj.Y), new Vec2(obj.X, obj.Y - height), false, false);
        }
    }
}
