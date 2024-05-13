using GXPEngine;
using GXPEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LineSegment : GameObject
{
    public Vec2 start;
    public Vec2 end;
    public LineCap lineCapStart;
    public LineCap lineCapEnd;

    public uint color = 0xffffffff;
    public uint lineWidth = 1;

    public LineSegment(Vec2 pStart, Vec2 pEnd, bool lineCapStart = false, bool lineCapEnd = false, uint pColor = 0xffffffff, uint pLineWidth = 1)
    {
        start = pStart;
        end = pEnd;
        this.lineCapStart = lineCapStart ? new LineCap(pStart) : null;
        this.lineCapEnd = lineCapEnd ? new LineCap(pEnd) : null;
    }

    public void MoveLine(float moveAmount)
    {
        start.y += moveAmount;
        end.y += moveAmount;
        if (lineCapStart != null)
            lineCapStart.position.y += moveAmount;
        if (lineCapEnd != null)
            lineCapEnd.position.y += moveAmount;
    }

    //------------------------------------------------------------------------------------------------------------------------
    //														RenderSelf()
    //------------------------------------------------------------------------------------------------------------------------
    override protected void RenderSelf(GLContext glContext)
    {
        if (game != null)
        {
            Gizmos.RenderLine(start.x, start.y, end.x, end.y, color, lineWidth);
        }
    }
}