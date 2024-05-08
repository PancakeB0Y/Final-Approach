using System;
using System.Drawing;
using System.Linq.Expressions;
using GXPEngine;
using static GXPEngine.GlobalVariables;
using TiledMapParser;

public class Player : AnimationSprite
{
    public readonly float Bounciness = 0.5f;

    public Vec2 Position;
    public float Radius;

    public Vec2 Velocity;
    public Vec2 Gravity = new Vec2(0, 0.4f);

    Vec2 accel = new Vec2(0, 0);
    float mass = 1;

    Vec2 oldPosition;

    Vec2 mousePosition;

    bool isCharging = false;
    Vec2 chargeMousePos;
    float chargeDistance;
    const float chargeDistanceMax = 100f;

    Arrow chargeIndicator;

    Element element;

    public Player(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        SetOrigin(width / 2, height / 2);

        this.Position = new Vec2(obj.X, obj.Y);
        Radius = width / 2;

        element = obj.GetBoolProperty("Fire", true) ? Element.Fire : Element.Ice;

        if(element == Element.Fire)
        {
            SetColor(1, 0, 1);
        }
        else
        {
            SetColor(0, 1, 1);
        }

        chargeIndicator = new Arrow(Position, new Vec2(0, 0), 10);
        AddChild(chargeIndicator);
        chargeIndicator.visible = false;

        UpdateCoordinates();
    }

    void Update ()
    {

        oldPosition = Position;

        Move();
        UpdateCoordinates();

        UpdateSize();

        //Aiming
        UpdateMousePosition();
        CheckForMouseInput();
        UpdateCoordinates();
    }

    void Move()
    {
        accel = Gravity * mass;
        Velocity += accel;
        Position += Velocity;

        CollisionInfo firstCollision = null;
        firstCollision = CheckForBoundariesCollisions(firstCollision);
        if (firstCollision != null)
        {
            ResolveCollision(firstCollision);
        }
    }

    CollisionInfo CheckForBoundariesCollisions(CollisionInfo earliestCollision)
    {
        Level level = (Level)this.parent;

        //Check wall lines
        for (int i = 0; i < level.GetNumberOfWallLines(); i++)
        {
            LineSegment lineSegment = level.GetWallLine(i);

            //Check line caps
            for (int j = 0; j < 2; j++)
            {
                LineCap lineCap = j % 2 == 0 ? lineSegment.lineCapStart : lineSegment.lineCapEnd;
                if (lineCap == null)
                    continue;

                earliestCollision = CheckBallCollision(earliestCollision, lineCap);
            }

            //Check line segment
            earliestCollision = CheckLineSegmentCollision(earliestCollision, lineSegment);
        }

        //Check boundary lines - REMOVE when real levels are ready
        for (int i = 0; i < level.GetNumberOfLines(); i++)
        {
            LineSegment lineSegment = level.GetLine(i);

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
        float a = Vec2.Dot(oldPosition - lineSegment.start, lineNormal) - Radius;
        float b = Vec2.Dot(oldPosition - Position, lineNormal);
        if (b <= 0)
        {
            return earliestColl;
        }
        float toi;
        if (a >= 0)
        {
            toi = a / b;
        }
        else if (a >= -Radius)
        {
            toi = 0;
        }
        else
        {
            return earliestColl;
        }
        if (toi <= 1)
        {
            Vec2 poi = oldPosition + Velocity * toi;
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

    CollisionInfo CheckBallCollision(CollisionInfo earliestColl, LineCap ball)
    {
        Vec2 relativePosition = oldPosition - ball.position;
        float a = Mathf.Pow(Velocity.Magnitude(), 2);
        float b = 2 * Vec2.Dot(relativePosition, Velocity);
        float c = Mathf.Pow(relativePosition.Magnitude(), 2) - Mathf.Pow(Radius + 0, 2);
        if (c < 0)
        {
            if (b < 0)
            {
                Vec2 pNormal = relativePosition.Normalized() * (Radius + 0);
                earliestColl = new CollisionInfo(pNormal, ball, 0f);
            }
            return earliestColl;
        }
        if (a < 0.001f)
        {
            return earliestColl;
        }
        float D = Mathf.Pow(b, 2) - 4 * a * c;
        if (D < 0)
        {
            return earliestColl;
        }
        float toi = (-b - Mathf.Sqrt(D)) / (2 * a);
        if (toi < 1 && toi >= 0)
        {
            if (earliestColl == null || toi < earliestColl.timeOfImpact)
            {
                Vec2 poi = oldPosition + Velocity * toi;
                earliestColl = new CollisionInfo(poi - ball.position, ball, toi);
            }
        }

        return earliestColl;
    }

    void ResolveCollision(CollisionInfo coll)
    {
        if (coll.other is LineCap)
        {
            LineCap otherBall = (LineCap)coll.other;
            Position = otherBall.position + coll.normal;
            Velocity.Reflect(coll.normal.Normalized(), Bounciness);
        }
        else if (coll.other is LineSegment)
        {
            Position = oldPosition + Velocity * coll.timeOfImpact;
            Velocity.Reflect(coll.normal, Bounciness);
        }
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
        if (Input.GetMouseButtonDown(1))
        {
            SwitchElement();
        }

        if (isCharging)
        {
            Charge();
        }

        if (Input.GetKeyDown(Key.LEFT))
        {
            mass -= 0.1f;
        }
        if (Input.GetKeyDown(Key.RIGHT))
        {
            mass += 0.1f;
        }
    }

    void SwitchElement()
    {
        if(element == Element.Fire)
        {
            SetColor(0, 1, 1);
            element = Element.Ice;
        }
        else
        {
            SetColor(1, 0, 1);
            element = Element.Fire;
        }
    }

    void Charge()
    {
        Vec2 distanceVec = chargeMousePos - mousePosition;
        chargeDistance = distanceVec.Magnitude();

        chargeDistance = Mathf.Clamp(chargeDistance, 0f, chargeDistanceMax);
        chargeDistance = Mathf.Map(chargeDistance, 0, chargeDistanceMax, 0f, 20f);

        chargeIndicator.startPoint = Position;
        chargeIndicator.vector = chargeDistance * distanceVec.Normalized();
        chargeIndicator.lineWidth = (uint)Mathf.Map(chargeDistance, 0, chargeDistanceMax, 1f, 30f);
    }

    void Release()
    {
        //Launch
        Velocity = chargeDistance * (chargeMousePos - mousePosition).Normalized();

        chargeIndicator.visible = false;
    }

    void UpdateSize()
    {
        SetScaleXY(mass);
        Radius = width / 2;
    }

    void UpdateCoordinates()
    {
        x = Position.x;
        y = Position.y;
    }
}
