using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledMapParser;

public class Obstacle : AnimationSprite
{
    public List<LineSegment> topBottom;
    public List<LineSegment> leftRight;

    public Obstacle(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        topBottom = new List<LineSegment>
        {
            //top
            new LineSegment(new Vec2(obj.X, obj.Y - obj.Height), new Vec2(obj.X + obj.Width, obj.Y - obj.Height), true),

            //bottom
            new LineSegment(new Vec2(obj.X + obj.Width, obj.Y), new Vec2(obj.X, obj.Y), true),
        };

        leftRight = new List<LineSegment>
        {
            new LineSegment(new Vec2(obj.X, obj.Y), new Vec2(obj.X, obj.Y - obj.Height), true, true),
            new LineSegment(new Vec2(obj.X + obj.Width, obj.Y - obj.Height), new Vec2(obj.X + obj.Width, obj.Y), true, true)
        };
    }
}

