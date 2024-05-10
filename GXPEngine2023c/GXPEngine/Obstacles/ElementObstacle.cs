using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledMapParser;
using static GXPEngine.GlobalVariables;

public class ElementObstacle : Obstacle
{
    public readonly Element Element;
    public readonly float Mass;

    public ElementObstacle(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, obj)
    {
        Element = obj.GetBoolProperty("Fire", true) ? Element.Fire : Element.Ice;
        Mass = obj.GetFloatProperty("Mass", 2);
    }
}