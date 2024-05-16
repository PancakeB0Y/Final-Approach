using System;
using GXPEngine;
using TiledMapParser;

public class SceneChangeButton : Sprite
{
    string _nextLevel;
    public SceneChangeButton(TiledObject obj) : base("Assets/startButton.png", true, false)
    {
        SetOrigin(obj.Width / 2, obj.Height / 2);

        SetXY(obj.X, obj.Y);
        obj.Width = width; 
        obj.Height = height;
        _nextLevel = obj.GetStringProperty("NextLevel", "level1.tmx");
    }

    void HandleInputs()
    {
        if (Input.mouseX >= x - width / 2 && Input.mouseX <= x + width / 2 && Input.mouseY >= y - height / 2 && Input.mouseY <= y + height / 2)
        {
            SetColor(0.5f, 0.5f, 0.5f);
            if (Input.GetMouseButtonDown(0))
            {
                ((MyGame)game).LoadScene(_nextLevel);
            }
        }
        else
        {
            SetColor(1, 1, 1);
        }
    }

    void Update()
    {
        HandleInputs();
    }



}

