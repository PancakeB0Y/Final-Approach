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
            LineSegment = new LineSegment(new Vec2(obj.X + obj.Width, obj.Y - obj.Height), new Vec2(obj.X + obj.Width, obj.Y), true, true);
        }
        else
        {
            LineSegment = new LineSegment(new Vec2(obj.X, obj.Y), new Vec2(obj.X, obj.Y - obj.Height), true, true);
        }
    }
}
