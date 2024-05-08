using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledMapParser;
using static GXPEngine.GlobalVariables;

public class ElementWall : Wall
{
    Element element;

    public ElementWall(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, obj)
    {
        element = obj.GetBoolProperty("Fire", true) ? Element.Fire : Element.Ice;
    }
}