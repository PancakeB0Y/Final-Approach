using GXPEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CollisionInfo
{
    public readonly Vec2 normal;
    public readonly GameObject other;
    public readonly GameObject otherReal;
    public readonly float timeOfImpact;

    public CollisionInfo(Vec2 pNormal, GameObject pOther, float pTimeOfImpact, GameObject pOtherReal = null)
    {
        normal = pNormal;
        other = pOther;
        timeOfImpact = pTimeOfImpact;
        otherReal = pOtherReal;
    }
}