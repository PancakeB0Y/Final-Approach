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
    List<Wall> walls;
    List<AABB> obstacles;
    public List<LineSegment> lines;

    public Level(string fileName, int id)
    {
        this.id = id;
        Name = fileName;

        tiledLoader = new TiledLoader(fileName);

        CreateLevel();
    }

    void CreateLevel()
    {
        walls = new List<Wall>();
        obstacles = new List<AABB>();
        lines = new List<LineSegment>();

        tiledLoader.autoInstance = true;
        tiledLoader.rootObject = this;
        tiledLoader.addColliders = false;

        tiledLoader.LoadObjectGroups(0);

        player = FindObjectOfType<Player>();

        foreach (var wall in GetChildren().Where(c => c is Wall))
        {
            walls.Add((Wall)wall);
        }

        foreach (var obstacle in GetChildren().Where(c => c is AABB))
        {
            obstacles.Add((AABB)obstacle);
        }
    }

    public void MoveLevel(Vec2 moveAmount)
    {
        foreach (var child in GetChildren())
        {
            child.Move(moveAmount.x, moveAmount.y);
            if (child is Wall)
            {
                ((Wall)child).LineSegment.MoveWithWall(moveAmount);
            }
            else if (child is AABB)
            {
                ((AABB)child).MoveLineSegments(moveAmount);
            }
        }
    }

    public int GetWallCount()
    {
        return walls.Count;
    }

    public Wall GetWall(int index)
    {
        if (index >= 0 && index < walls.Count)
        {
            return walls[index];
        }
        return null;
    }

    public int GetObstacleCount()
    {
        return obstacles.Count;
    }

    public AABB GetObstacle(int index)
    {
        if (index >= 0 && index < obstacles.Count)
        {
            return obstacles[index];
        }
        return null;
    }
}