using GXPEngine;
using System;
using System.Collections.Generic;

public class MyGame : Game
{
    string curSceneName;
    public SoundChannel backgroundMusic;
    public MyGame() : base(1920, 1080, false, false)
    {
        targetFps = 60;

        curSceneName = "startMenu.tmx";

        if (curSceneName == "level1.tmx")
        {
            backgroundMusic = new Sound("Assets/Sounds/background.wav", true).Play(false, 0, 3.5f);
        }
        else
        {
            backgroundMusic = new Sound("Assets/Sounds/menu.wav", true).Play(false, 0, 3.5f);
        }

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

    public void LoadScene(string sceneName)
    {
        List<GameObject> children = GetChildren();
        foreach (GameObject child in children)
        {
            child.LateDestroy();
        }
        children.Clear();
        AddChild(new Level(sceneName, 1));
        curSceneName = sceneName;
        backgroundMusic.Stop();

        if (curSceneName == "level1.tmx")
        {
            backgroundMusic = new Sound("Assets/Sounds/background.wav", true).Play(false, 0, 3.5f);
        }
        else
        {
            backgroundMusic = new Sound("Assets/Sounds/menu.wav", true).Play(false, 0, 3.5f);
        }
    }

    static void Main()
    {
        new MyGame().Start();
    }
}