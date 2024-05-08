using GXPEngine;
using System.Collections.Generic;

public class MyGame : Game
{
    public List<LineSegment> Lines;

    public int GetNumberOfLines()
    {
        return Lines.Count;
    }

    public LineSegment GetLine(int index)
    {
        if (index >= 0 && index < Lines.Count)
        {
            return Lines[index];
        }
        return null;
    }

    public MyGame() : base(800, 600, false)
    {
        targetFps = 60;

        Lines = new List<LineSegment>();

        // boundary:
        AddLine(new Vec2(50, height - 20), new Vec2(width - 50, height - 20));  //bottom
        AddLine(new Vec2(width - 50, height - 20), new Vec2(width - 50, 20));
        AddLine(new Vec2(width - 50, 20), new Vec2(50, 20));
        AddLine(new Vec2(50, 20), new Vec2(50, height - 20));  //right

        AddChild(new Level("level1.tmx", 1));
    }

    void AddLine(Vec2 start, Vec2 end, bool lineCapStart = false, bool lineCapEnd = false)
    {
        LineSegment line = new LineSegment(start, end, lineCapStart, lineCapEnd, 0xff00ff00, 4);
        AddChild(line);
        Lines.Add(line);
    }

    void Update()
    {
    }

    static void Main()
    {
        new MyGame().Start();
    }
}