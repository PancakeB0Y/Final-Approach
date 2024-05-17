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
using static GXPEngine.GlobalVariables;

public class Level : GameObject
{
    public string Name { get; }

    int id;
    TiledLoader tiledLoader;

    Player player;
    List<Wall> walls;
    List<Obstacle> obstacles;
    public End end;

    EasyDraw instructions;

    EasyDraw sizeMeterSmallWater;
    EasyDraw sizeMeterMediumWater;
    EasyDraw sizeMeterLargeWater;
    EasyDraw sizeMeterSmallFire;
    EasyDraw sizeMeterMediumFire;
    EasyDraw sizeMeterLargeFire;

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
        tiledLoader.LoadObjectGroups(1);
        tiledLoader.LoadObjectGroups(2);
        tiledLoader.LoadObjectGroups(3);
        tiledLoader.LoadObjectGroups(4);
        tiledLoader.LoadObjectGroups(5);

        player = FindObjectOfType<Player>();
        end = FindObjectOfType<End>();

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

        if(player != null)
        {
            InitEasyDraws();
        }
    }

    void InitEasyDraws()
    {
        instructions = new EasyDraw(400, 300, false);
        instructions.TextAlign(CenterMode.Center, CenterMode.Center);
        instructions.Fill(Color.White);
        instructions.TextSize(20);
        instructions.Text($"Press and drag the left mouse\nbutton to CHARGE and\nrelease to LAUNCH\n\nPress the right mouse\nbutton to SWITCH elements", true, 255, 0, 0, 0);
        instructions.SetOrigin(instructions.width / 2, instructions.height / 2);
        instructions.SetXY((game.width - 800) / 4, game.height / 2);
        AddChild(instructions);

        InitSizeMeters();
    }

    private void InitSizeMeters()
    {
        sizeMeterSmallWater = new EasyDraw("Assets/UI/small drop.png", false);
        sizeMeterSmallWater.SetOrigin(sizeMeterSmallWater.width / 2, sizeMeterSmallWater.height / 2);
        sizeMeterSmallWater.SetXY((game.width - 800) / 4, 100);
        game.AddChild(sizeMeterSmallWater);

        sizeMeterMediumWater = new EasyDraw("Assets/UI/middle drop.png", false);
        sizeMeterMediumWater.SetOrigin(sizeMeterMediumWater.width / 2, sizeMeterMediumWater.height / 2);
        sizeMeterMediumWater.SetXY((game.width - 800) / 4, 100);
        game.AddChild(sizeMeterMediumWater);

        sizeMeterLargeWater = new EasyDraw("Assets/UI/big drop.png", false);
        sizeMeterLargeWater.SetOrigin(sizeMeterLargeWater.width / 2, sizeMeterMediumWater.height / 2);
        sizeMeterLargeWater.SetXY((game.width - 800) / 4, 100);
        game.AddChild(sizeMeterLargeWater);

        sizeMeterSmallFire = new EasyDraw("Assets/UI/small fire.png", false);
        sizeMeterSmallFire.SetOrigin(sizeMeterSmallFire.width / 2, sizeMeterSmallFire.height / 2);
        sizeMeterSmallFire.SetXY((game.width - 800) / 4, 100);
        game.AddChild(sizeMeterSmallFire);

        sizeMeterMediumFire = new EasyDraw("Assets/UI/middle fire.png", false);
        sizeMeterMediumFire.SetOrigin(sizeMeterMediumFire.width / 2, sizeMeterMediumFire.height / 2);
        sizeMeterMediumFire.SetXY((game.width - 800) / 4, 100);
        game.AddChild(sizeMeterMediumFire);

        sizeMeterLargeFire = new EasyDraw("Assets/UI/big fire.png", false);
        sizeMeterLargeFire.SetOrigin(sizeMeterLargeFire.width / 2, sizeMeterLargeFire.height / 2);
        sizeMeterLargeFire.SetXY((game.width - 800) / 4, 100);
        game.AddChild(sizeMeterLargeFire);

        UpdateUISize();
    }

    public void UpdateUISize()
    {
        switch (player.Mass)
        {
            case 2f:
                sizeMeterSmallWater.visible = player.Element == Element.Ice ? true : false;
                sizeMeterMediumWater.visible = player.Element == Element.Ice ? true : false;
                sizeMeterLargeWater.visible = player.Element == Element.Ice ? true : false;
                sizeMeterSmallFire.visible = player.Element == Element.Ice ? false : true;
                sizeMeterMediumFire.visible = player.Element == Element.Ice ? false : true;
                sizeMeterLargeFire.visible = player.Element == Element.Ice ? false : true;
                break;
            case 1.5f:
                sizeMeterSmallWater.visible = player.Element == Element.Ice ? true : false;
                sizeMeterMediumWater.visible = player.Element == Element.Ice ? true : false;
                sizeMeterLargeWater.visible = false;
                sizeMeterSmallFire.visible = player.Element == Element.Ice ? false : true;
                sizeMeterMediumFire.visible = player.Element == Element.Ice ? false : true;
                sizeMeterLargeFire.visible = false;
                break;
            case 1f:
                sizeMeterSmallWater.visible = player.Element == Element.Ice ? true : false;
                sizeMeterMediumWater.visible = false;
                sizeMeterLargeWater.visible = false;
                sizeMeterSmallFire.visible = player.Element == Element.Ice ? false : true;
                sizeMeterMediumFire.visible = false;
                sizeMeterLargeFire.visible = false;
                break;
            default:
                Console.WriteLine("Icorrect mass in UI switch!!!");
                break;
        }
    }

    public void ReloadLevel()
    {
        foreach (GameObject child in GetChildren())
        {
            child.Destroy();
        }

        sizeMeterSmallWater.Destroy();
        sizeMeterMediumWater.Destroy();
        sizeMeterLargeWater.Destroy();
        sizeMeterSmallFire.Destroy();
        sizeMeterMediumFire.Destroy();
        sizeMeterLargeFire.Destroy();

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

        //SpawnAnimation
        var reactionParticle = new ReactionParticle("Assets/player_animations/smoke.png", 3, 3);
        reactionParticle.SetXY(obstacle.x, obstacle.y);
        AddChild(reactionParticle);

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
}