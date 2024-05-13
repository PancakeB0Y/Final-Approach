using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    List<Obstacle> obstacles;
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
        obstacles = new List<Obstacle>();
        lines = new List<LineSegment>();

        tiledLoader.autoInstance = true;
        tiledLoader.rootObject = this;
        tiledLoader.addColliders = false;

        tiledLoader.LoadObjectGroups(0);

        player = FindObjectOfType<Player>();

        foreach (var wall in GetChildren().Where(c => c is Wall))
        {
            walls.Add((Wall)wall);

            AddChild(((Wall)wall).LineSegment);
        }

        foreach (var obstacle in GetChildren().Where(c => c is Obstacle))
        {
            obstacles.Add((Obstacle)obstacle);

            foreach (var wall in ((Obstacle)obstacle).topBottom)
            {
                AddChild(wall);
            }
            foreach (var wall in ((Obstacle)obstacle).leftRight)
            {
                AddChild(wall.LineSegment);
            }
        }
    }

    public void ReloadLevel()
    {
        foreach (GameObject child in GetChildren())
        {
            child.Destroy();
        }
        CreateLevel();
    }

    public void MoveLevel(float moveAmount)
    {
        var children = GetChildren();
        foreach (var child in children)
        {
            if (child != player && !(child is LineSegment))
            {
                child.y += moveAmount;
            }
            else if (child is LineSegment)
            {
                LineSegment line = (LineSegment)child;
                line.MoveLine(moveAmount);
            }
        }
    }

    public void HandleScrolling()
    {
        if(player == null) { return; }

        if (player.y + y > game.height / 2)
        {
            y = game.height / 2 - player.y;
        }
        if (player.y + y < game.height /2)
        {
            y = game.height / 2 - player.y;
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


    public Obstacle GetObstacle(int index)
    {
        if (index >= 0 && index < obstacles.Count)
        {
            return obstacles[index];
        }
        return null;
    }
    public void RemoveObstacle(Obstacle obstacle)
    {
        obstacles.Remove(obstacle);
    }

    void Update()
    {
        HandleScrolling();
    }
}