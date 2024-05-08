using GXPEngine;
using System.Collections.Generic;

public class MyGame : Game
{
    public MyGame() : base(1500, 900, false)
    {
        targetFps = 60;

        AddChild(new Level("level1.tmx", 1));
    }

    void Update()
    {
    }

    static void Main()
    {
        new MyGame().Start();
    }
}