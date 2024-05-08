using System;
using System.Drawing;
using GXPEngine;
using TiledMapParser;

public class Player : AnimationSprite
{
    public readonly float bounciness = 0.5f;

    public Vec2 position;
    public readonly float radius;

    public Vec2 velocity;
    public Vec2 gravity = new Vec2(0, 0.4f);

    Vec2 oldPosition;

    Vec2 mousePosition;

    bool isCharging = false;
    Vec2 chargeMousePos;
    float chargeDistance;
    const float chargeDistanceMax = 100f;

    Arrow chargeIndicator;

    public Player(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows)
    {
        SetOrigin(width / 2, height / 2);

        this.position = new Vec2(obj.X, obj.Y);
        radius = width / 2;

        chargeIndicator = new Arrow(position, new Vec2(0, 0), 10);
        AddChild(chargeIndicator);
        chargeIndicator.visible = false;

        UpdateCoordinates();
    }

    void Update ()
    {

        oldPosition = position;

        Move();
        UpdateCoordinates();


        //Aiming
        UpdateMousePosition();
        CheckForMouseInput();
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

    void UpdateMousePosition()
    {
        mousePosition.SetXY(Input.mouseX, Input.mouseY);
    }

    void CheckForMouseInput()
    {
        if (!isCharging && Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeMousePos = mousePosition;

            chargeIndicator.visible = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isCharging = false;
            Release();
        }

        if (isCharging)
        {
            Charge();
        }
    }

    void Charge()
    {
        Vec2 distanceVec = chargeMousePos - mousePosition;
        chargeDistance = distanceVec.Magnitude();

        chargeDistance = Mathf.Clamp(chargeDistance, 0f, chargeDistanceMax);
        chargeDistance = Mathf.Map(chargeDistance, 0, chargeDistanceMax, 0f, 20f);

        chargeIndicator.startPoint = position;
        chargeIndicator.vector = chargeDistance * distanceVec.Normalized();
        chargeIndicator.lineWidth = (uint)Mathf.Map(chargeDistance, 0, chargeDistanceMax, 1f, 30f);
    }

    void Release()
    {
        //Launch
        velocity = chargeDistance * (chargeMousePos - mousePosition).Normalized();

        chargeIndicator.visible = false;
    }

    void UpdateCoordinates()
    {
        x = position.x;
        y = position.y;
    }
}
