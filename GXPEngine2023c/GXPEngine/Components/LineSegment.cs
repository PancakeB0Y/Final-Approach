using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LineSegment
{
    public Vec2 start;
    public Vec2 end;
    public LineCap lineCapStart;
    public LineCap lineCapEnd;

    public LineSegment(Vec2 pStart, Vec2 pEnd, bool lineCapStart = false, bool lineCapEnd = false)
    {
        start = pStart;
        end = pEnd;
        this.lineCapStart = lineCapStart ? new LineCap(pStart) : null;
        this.lineCapEnd = lineCapEnd ? new LineCap(pEnd) : null;
    }
}