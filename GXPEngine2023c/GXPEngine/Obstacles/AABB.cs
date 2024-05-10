using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledMapParser;

public class AABB : AnimationSprite
{
    float colW;
    float colH;

    Vec2 position;

    public List<LineSegment> walls;
    public AABB(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        colW = obj.Width;
        colH = obj.Height;
        position = new Vec2(obj.X, obj.Y);

        //Add 4 walls

        //walls.Add(new LineSegment(new Vec2(obj.X), , new Vec2(obj.X + width, obj.Y), true, true);)
    }
}

