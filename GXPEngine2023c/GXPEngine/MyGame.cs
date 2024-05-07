using GXPEngine;
using System.Collections.Generic;

public class MyGame : Game {

    Player player;
    List<LineSegment> _lines;

    public int GetNumberOfLines()
    {
        return _lines.Count;
    }

    public LineSegment GetLine(int index)
    {
        if (index >= 0 && index < _lines.Count)
        {
            return _lines[index];
        }
        return null;
    }

    public MyGame() : base(800, 600, false)
	{
        player = new Player("circle.png", new Vec2(width / 2, height / 2));
        AddChild(player);

        _lines = new List<LineSegment>();

        // boundary:
        AddLine(new Vec2(50, height - 20), new Vec2(width - 50, height - 20));  //bottom
        AddLine(new Vec2(width - 50, height - 20), new Vec2(width - 50, 20));
        AddLine(new Vec2(width - 50, 20), new Vec2(50, 20));
        AddLine(new Vec2(50, 20), new Vec2(50, height - 20));  //right
    }

    void AddLine(Vec2 start, Vec2 end, bool lineCapStart = false, bool lineCapEnd = false)
    {
        LineSegment line = new LineSegment(start, end, lineCapStart, lineCapEnd, 0xff00ff00, 4);
        AddChild(line);
        _lines.Add(line);
    }

    void Update() {
	}

	static void Main()       
	{
		new MyGame().Start();
	}
}