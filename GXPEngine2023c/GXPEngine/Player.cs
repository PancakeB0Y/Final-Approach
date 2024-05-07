﻿using System;
using GXPEngine;

public class Player : Sprite
{
    public readonly float bounciness;

    public Vec2 position;
    public readonly float radius;

    public Vec2 velocity;
    public Vec2 gravity = new Vec2(0, 0.2f);

    Vec2 oldPosition;

    public Player(string filename, Vec2 position) : base(filename, false, false)
    {
        SetOrigin(width / 2, height / 2);

        this.position = position;
        radius = width / 2;

        UpdateCoordinates();
    }

    void Update ()
    {
        oldPosition = position;

        Move();
        UpdateCoordinates();
    }

    void Move()
    {
        velocity += gravity;
        position += velocity;

        CollisionInfo firstCollision = null;
        firstCollision = CheckForBoundariesCollisions(firstCollision);
        if (firstCollision != null)
        {
            ResolveCollision(firstCollision);
        }
    }

    void ResolveCollision(CollisionInfo coll)
    {
        if (coll.other is LineSegment)
        {
            position = oldPosition + velocity * coll.timeOfImpact;
            velocity.Reflect(coll.normal, bounciness);
        }
    }

    CollisionInfo CheckForBoundariesCollisions(CollisionInfo earliestCollision)
    {
        for (int i = 0; i < ((MyGame)game).GetNumberOfLines(); i++)
        {
            LineSegment lineSegment = ((MyGame)game).GetLine(i);

            //Check line caps
            //for (int j = 0; j < 2; j++)
            //{
            //    Ball lineCap = j % 2 == 0 ? lineSegment.lineCapStart : lineSegment.lineCapEnd;
            //    if (lineCap == null)
            //        continue;

            //    earliestCollision = CheckBallCollision(earliestCollision, lineCap);
            //}

            //Check line segment
            earliestCollision = CheckLineSegmentCollision(earliestCollision, lineSegment);
        }

        return earliestCollision;
    }

    CollisionInfo CheckLineSegmentCollision(CollisionInfo earliestColl, LineSegment lineSegment)
    {
        Vec2 lineVector = lineSegment.start - lineSegment.end;
        Vec2 lineNormal = lineVector.Normal();
        float a = Vec2.Dot(oldPosition - lineSegment.start, lineNormal) - radius;
        float b = Vec2.Dot(oldPosition - position, lineNormal);
        if (b <= 0)
        {
            return earliestColl;
        }
        float toi;
        if (a >= 0)
        {
            toi = a / b;
        }
        else if (a >= -radius)
        {
            toi = 0;
        }
        else
        {
            return earliestColl;
        }
        if (toi <= 1)
        {
            Vec2 poi = oldPosition + velocity * toi;
            float d = Vec2.Dot(lineSegment.start - poi, lineVector.Normalized());
            if (d >= 0 && d <= lineVector.Magnitude())
            {
                if (earliestColl == null || toi < earliestColl.timeOfImpact)
                {
                    earliestColl = new CollisionInfo(lineNormal, lineSegment, toi);
                }
            }
        }

        return earliestColl;
    }

    void UpdateCoordinates()
    {
        x = position.x;
        y = position.y;
    }
}
