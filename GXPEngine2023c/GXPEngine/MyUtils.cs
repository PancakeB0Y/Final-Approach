using GXPEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public static class MyUtils
{
    public static MyGame MyGame => (MyGame)MyGame.main;

    public static bool TryGetIndex<T>(this T[] array, Predicate<T> predicate, out int index)
    {
        index = Array.FindIndex(array, predicate);
        return index > -1;
    }
}

