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

    EasyDraw instructions;
    EasyDraw sizeMeter;
    EasyDraw canJump;
    EasyDraw canSwitchElement;

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
            Obstacle obstacle1 = (Obstacle)obstacle;

            obstacles.Add(obstacle1);

            foreach (var line in obstacle1.leftRight)
            {
                AddChild(line);
            }
            foreach (var line in obstacle1.topBottom)
            {
                AddChild(line);
            }
        }

        InitEasyDraws();
    }

    void InitEasyDraws()
    {
        sizeMeter = new EasyDraw(400, 300, false);
        sizeMeter.TextAlign(CenterMode.Center, CenterMode.Center);
        sizeMeter.Fill(Color.White);
        sizeMeter.TextSize(20);
        sizeMeter.Text($"Press and drag the left mouse\nbutton to CHARGE and\nrelease to LAUNCH\n\nPress the right mouse\nbutton to SWITCH elements", true, 255, 0, 0, 0);
        sizeMeter.SetOrigin(sizeMeter.width / 2, sizeMeter.height / 2);
        sizeMeter.SetXY((game.width - 800) / 4, game.height / 2);
        AddChild(sizeMeter);

        sizeMeter = new EasyDraw(150, 50, false);
        sizeMeter.TextAlign(CenterMode.Center, CenterMode.Center);
        sizeMeter.Fill(Color.White);
        sizeMeter.TextSize(25);
        sizeMeter.Text($"Size: {player.Mass}/3", true, 255, 0, 0, 0);
        sizeMeter.SetOrigin(sizeMeter.width / 2, sizeMeter.height / 2);
        sizeMeter.SetXY((game.width - 800) / 4, 30f);
        game.AddChild(sizeMeter);

        canJump = new EasyDraw(200, 50, false);
        canJump.TextAlign(CenterMode.Center, CenterMode.Center);
        canJump.Fill(Color.White);
        canJump.TextSize(20);
        canJump.Text($"Can jump: {player.CanJump}", true, 255, 0, 0, 0);
        canJump.SetOrigin(canJump.width / 2, canJump.height / 2);
        canJump.SetXY(game.width - (game.width - 800) / 4, 30f);
        game.AddChild(canJump);

        canSwitchElement = new EasyDraw(310, 50, false);
        canSwitchElement.TextAlign(CenterMode.Center, CenterMode.Center);
        canSwitchElement.Fill(Color.White);
        canSwitchElement.TextSize(20);
        canSwitchElement.Text($"Can switch element: {player.CanSwitchElement}", true, 255, 0, 0, 0);
        canSwitchElement.SetOrigin(canSwitchElement.width / 2, canSwitchElement.height / 2);
        canSwitchElement.SetXY(game.width - (game.width - 800) / 4, 70f);
        game.AddChild(canSwitchElement);
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
        if (player == null) { return; }

        if (player.y + y > game.height / 2)
        {
            y = game.height / 2 - player.y;
        }
        if (player.y + y < game.height / 2)
        {
            Console.WriteLine("In");
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

        foreach (var line in obstacle.leftRight)
        {
            line.Destroy();
        }
        foreach (var line in obstacle.topBottom)
        {
            line.Destroy();
        }
        obstacle.Destroy();
    }

    void Update()
    {
        DrawEasyDraws();
    }

    void DrawEasyDraws()
    {
        sizeMeter.Text($"Size: {(int)player.Mass}/3", true, 255, 0, 0, 0);
        canJump.Text($"Can jump: {player.CanJump}", true, 255, 0, 0, 0);
        canSwitchElement.Text($"Can switch element: {player.CanSwitchElement}", true, 255, 0, 0, 0);
    }
}