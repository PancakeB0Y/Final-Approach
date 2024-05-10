using GXPEngine;
using System.Collections.Generic;

public class MyGame : Game
{
    string curSceneName;
    public MyGame() : base(1500, 900, false, false)
    {
        targetFps = 60;

        curSceneName = "level1.tmx";
        LoadScene(curSceneName);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(Key.R))
        {
            LoadScene(curSceneName);
        }
    }

    void LoadScene(string sceneName)
    {
        List<GameObject> children = GetChildren();
        foreach (GameObject child in children)
        {
            child.LateDestroy();
        }
        children.Clear();
        AddChild(new Level(sceneName, 1));
        curSceneName = sceneName;
    }

    static void Main()
    {
        new MyGame().Start();
    }
}