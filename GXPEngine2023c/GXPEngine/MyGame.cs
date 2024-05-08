using GXPEngine;
using System.Collections.Generic;

public class MyGame : Game
{

    public Level CurrentScene { get; private set; }
    private string _newSceneName = null;

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
        targetFps = 60;
        LoadScene("level1.tmx");
        OnAfterStep += LoadSceneIfNotNull;

        player = new Player("Assets/circle.png", new Vec2(width / 2, height / 2));
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

    void Update()
    {
        if (Input.GetKeyDown(Key.R))
        {
            ReloadScene();
        }
    }


    private void LoadSceneIfNotNull()
    {
        if (_newSceneName == null) return;
        DestroyAll();
        var level = new Level(_newSceneName);
        CurrentScene = level;
        AddChild(level);
        level.Init();

        _newSceneName = null;
    }

    public void LoadScene(string sceneName)
    {
        _newSceneName = sceneName;
    }

    public void ReloadScene()
    {
        _newSceneName = CurrentScene.Name;
    }

    protected override void OnDestroy()
    {
        OnAfterStep -= LoadSceneIfNotNull;
    }
    private void DestroyAll()
    {
        foreach (var child in GetChildren())
        {

            child.LateDestroy();

        }
    }

    static void Main()
    {
        new MyGame().Start();
    }
}