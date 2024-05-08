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

        tiledLoader.autoInstance = true;
        tiledLoader.rootObject = this;
        tiledLoader.addColliders = false;

        tiledLoader.LoadObjectGroups(0);

        player = FindObjectOfType<Player>();

        foreach (var wall in GetChildren().Where(c => c is Wall))
        {
            walls.Add((Wall)wall);
        }
    }

    public int GetNumberOfWall()
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
}