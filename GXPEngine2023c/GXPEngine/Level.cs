using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using GXPEngine;
using gxpengine_template.MyClasses;
using TiledMapParser;

public class Level : GameObject
{
    public string Name { get; }

    int id;
    TiledLoader tiledLoader;

    Player player;

    List<LineSegment> lines;
    List<LineSegment> wallLines;

    public Level(string fileName, int id)
    {
        this.id = id;
        Name = fileName;

        tiledLoader = new TiledLoader(fileName);

        CreateLevel();
    }

    void CreateLevel()
    {
        lines = new List<LineSegment>();
        wallLines = new List<LineSegment>();

        // boundary:
        AddLine(new Vec2(50, game.height - 20), new Vec2(game.width - 50, game.height - 20));  //bottom
        AddLine(new Vec2(game.width - 50, game.height - 20), new Vec2(game.width - 50, 20));
        AddLine(new Vec2(game.width - 50, 20), new Vec2(50, 20));
        AddLine(new Vec2(50, 20), new Vec2(50, game.height - 20));  //right

        tiledLoader.autoInstance = true;
        tiledLoader.rootObject = this;
        tiledLoader.addColliders = false;

        tiledLoader.LoadObjectGroups(0);

        player = FindObjectOfType<Player>();

        foreach (var wall in GetChildren().Where(c => c is Wall))
        {
            AddWallLine(((Wall)wall).LineSegment);
        }
    }

    void AddLine(Vec2 start, Vec2 end, bool lineCapStart = false, bool lineCapEnd = false)
    {
        LineSegment line = new LineSegment(start, end, lineCapStart, lineCapEnd, 0xff00ff00, 4);
        AddChild(line);
        lines.Add(line);
    }

    public int GetNumberOfLines()
    {
        return lines.Count;
    }

    public LineSegment GetLine(int index)
    {
        if (index >= 0 && index < lines.Count)
        {
            return lines[index];
        }
        return null;
    }

    public void AddWallLine(LineSegment wallLine)
    {
        AddChild(wallLine);
        wallLines.Add(wallLine);
    }

    public int GetNumberOfWallLines()
    {
        return wallLines.Count;
    }

    public LineSegment GetWallLine(int index)
    {
        if (index >= 0 && index < wallLines.Count)
        {
            return wallLines[index];
        }
        return null;
    }
}