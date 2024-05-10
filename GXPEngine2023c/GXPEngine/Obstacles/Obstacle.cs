using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledMapParser;

public class Obstacle : AnimationSprite
{
    public List<LineSegment> walls;
    public Obstacle(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        walls = new List<LineSegment>
        {
            //top
            new LineSegment(new Vec2(obj.X, obj.Y - obj.Height), new Vec2(obj.X + obj.Width, obj.Y - obj.Height), true),

            //bottom
            new LineSegment(new Vec2(obj.X + obj.Width, obj.Y), new Vec2(obj.X, obj.Y), true),

            //right
            new LineSegment(new Vec2(obj.X, obj.Y), new Vec2(obj.X, obj.Y - obj.Height), true),

            //left
            new LineSegment(new Vec2(obj.X + obj.Width, obj.Y - obj.Height), new Vec2(obj.X + obj.Width, obj.Y), true)
        };
    }
}

