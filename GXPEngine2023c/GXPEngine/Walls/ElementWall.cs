using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledMapParser;
using static GXPEngine.GlobalVariables;

public class ElementWall : Wall
{
    public readonly Element Element;

    public ElementWall(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, obj)
    {
        Element = obj.GetBoolProperty("Fire", true) ? Element.Fire : Element.Ice;
    }
}