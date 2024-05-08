using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using GXPEngine;
using gxpengine_template.MyClasses;
using TiledMapParser;

public class Level : GameObject
{
    public string Name { get; }

    public Level(string fileName)
    {
        Name = fileName;
    }

    //not in constructor because level has to be parent of game first
    public void Init()
    {
        var loader = new TiledLoader(Name, MyGame.main, addColliders: false, autoInstance: true);

        int index;

        //background
        if (loader.map.ImageLayers != null)
        {
            loader.rootObject = this;
            loader.LoadImageLayers();

        }

        //managers
        if (loader.map.ObjectGroups.TryGetIndex(x => x.Name == "Managers", out index))
        {
            loader.rootObject = MyGame.main;
            loader.addColliders = false;
            loader.LoadObjectGroups(index);
        }

        //background objects
        if (loader.map.ObjectGroups.TryGetIndex(x => x.Name == "ObjectBackgrounds", out index))
        {
            loader.rootObject = this;
            loader.addColliders = false;
            loader.LoadObjectGroups(index);

        }

        //level objects
        if (loader.map.ObjectGroups.TryGetIndex(x => x.Name == "Object Layer 1", out index))
        {
            loader.rootObject = this;
            loader.addColliders = true;
            loader.LoadObjectGroups(index);

        }

        //ui
        if (loader.map.ObjectGroups.TryGetIndex(x => x.Name == "UI", out index))
        {
            loader.addColliders = false;
            loader.rootObject = game;
            loader.LoadObjectGroups(index);
        }

    }

}