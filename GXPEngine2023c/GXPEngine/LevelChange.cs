using GXPEngine;
using TiledMapParser;

public class LevelChange : Sprite
{
    string nextScene;
    public LevelChange(TiledObject data) : base("Assets/UI/small fire.png", true, false)
    {
        alpha = 0f;
        nextScene = data.GetStringProperty("NextScene", "startMenu.tmx");
    }

    void HandleInputs()
    {
        if ((Input.AnyKeyDown() || Input.GetMouseButtonDown(0)) && !Input.GetKey(Key.R))
        {
            ((MyGame)game).LoadScene(nextScene, 0);
        }
    }

    void Update()
    {
        HandleInputs();
    }



}

