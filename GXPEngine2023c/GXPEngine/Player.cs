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

    PlayerState playerState = PlayerState.None;

    Wall currentSlideWall;
    Element wallElement;
    float durationToStick = 2000;
    float durationToStickCounter = 0;

    public Player(string filename, int cols, int rows, TiledObject obj = null) : base(filename, cols, rows, -1, false, false)
    {
        SetOrigin(width / 2, height / 2);

        this.Position = new Vec2(obj.X + obj.Width / 2, obj.Y - obj.Height / 2);
        Radius = width / 2;

        element = obj.GetBoolProperty("Fire", true) ? Element.Fire : Element.Ice;

        if (element == Element.Fire)
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

    void Update()
    {
        //Camera slowly creeping up if the player is stationary
        if (playerState == PlayerState.None && Position.x == oldPosition.x && Position.y == oldPosition.y)
        {
            ((Level)parent).MoveLevel(0.5f);
        }

        oldPosition = Position;

        switch (playerState)
        {
            case PlayerState.Sticking:
                Stick();
                break;
            case PlayerState.Sliding:
                Slide();
                break;
            case PlayerState.None:
                Move();
                UpdateCoordinates();
                break;
            default:
                break;
        }

        //Aiming
        UpdateMousePosition();
        CheckForMouseInput();
        UpdateCoordinates();
    }

    void Move()
    {
        //If higher than certain amount, move the level instead
        if (Position.y <= game.height * 0.6f && Velocity.y < 0)
        {
            //Position.y = game.height * 0.75f;
            //oldPosition.y = Position.y + 1;

            accel = Gravity * mass;
            Velocity += accel;
            ((Level)parent).MoveLevel(-Velocity.y);

            Position.x += Velocity.x;
        }
        //Normal player movement
        else
        {
            accel = Gravity * mass;
            Velocity += accel;
            Position += Velocity;

            if (y > game.height)
            {
                ((Level)parent).ReloadLevel();
                return;
            }
        }

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
        for (int i = 0; i < level.GetWallCount(); i++)
        {
            Wall wall = level.GetWall(i);

            //Check line caps
            for (int j = 0; j < 2; j++)
            {
                LineCap lineCap = j % 2 == 0 ? wall.LineSegment.lineCapStart : wall.LineSegment.lineCapEnd;
                if (lineCap == null)
                    continue;

                earliestCollision = CheckBallCollision(earliestCollision, lineCap);
            }

            //Check line segment
            earliestCollision = CheckWallCollision(earliestCollision, wall);
        }

        //Check obstacles
        for (int i = 0; i < level.GetObstacleCount(); i++)
        {
            Obstacle obstacle = level.GetObstacle(i);

            /*//Check line caps
            for (int j = 0; j < obstacle.walls.Count; j++)
            {
                LineCap lineCap = obstacle.walls[j].lineCapStart;

                earliestCollision = CheckBallCollision(earliestCollision, lineCap);
            }*/

            earliestCollision = CheckObstacleCollision(earliestCollision, obstacle);
        }

        for (int i = 0; i < level.lines.Count; i++)
        {
            LineSegment line = level.lines[i];
            earliestCollision = CheckLineSegmentCollision(earliestCollision, line);
        }

        return earliestCollision;
    }

    CollisionInfo CheckWallCollision(CollisionInfo earliestColl, Wall wall)
    {
        Vec2 lineVector = wall.LineSegment.start - wall.LineSegment.end;
        Vec2 lineNormal = lineVector.Normal();
        float a = Vec2.Dot(oldPosition - wall.LineSegment.start, lineNormal) - Radius;
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
            float d = Vec2.Dot(wall.LineSegment.start - poi, lineVector.Normalized());
            if (d >= 0 && d <= lineVector.Magnitude())
            {
                if (earliestColl == null || toi < earliestColl.timeOfImpact)
                {
                    earliestColl = new CollisionInfo(lineNormal, wall, toi);
                }
            }
        }

        return earliestColl;
    }

    CollisionInfo CheckObstacleCollision(CollisionInfo earliestColl, Obstacle obstacle)
    {
        float minT = 1;

        for (int i = 0; i < 4; i++)
        {
            LineCap ball = null;
            if (i < 2)
            {
                ball = obstacle.topBottom[i].lineCapStart;
            }
            else
            {
                ball = obstacle.leftRight[i - 2].LineSegment.lineCapStart;
            }

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
                continue;
            }
            if (a < 0.001f)
            {
                continue;
            }
            float D = Mathf.Pow(b, 2) - 4 * a * c;
            if (D < 0)
            {
                continue;
            }
            float toi = (-b - Mathf.Sqrt(D)) / (2 * a);
            if (toi < minT && toi >= 0)
            {
                if (earliestColl == null || toi < earliestColl.timeOfImpact)
                {
                    Vec2 poi = oldPosition + Velocity * toi;
                    earliestColl = new CollisionInfo(poi - ball.position, ball, toi);
                    minT = toi;
                }
            }
        }

        for (int i = 0; i < obstacle.topBottom.Count; i++)
        {
            LineSegment currWall = obstacle.topBottom[i];

            Vec2 lineVector = currWall.start - currWall.end;
            Vec2 lineNormal = lineVector.Normal();
            float a = Vec2.Dot(oldPosition - currWall.start, lineNormal) - Radius;
            float b = Vec2.Dot(oldPosition - Position, lineNormal);
            if (b <= 0)
            {
                continue;
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
                continue;
            }
            if (toi <= minT)
            {
                Vec2 poi = oldPosition + Velocity * toi;
                float d = Vec2.Dot(currWall.start - poi, lineVector.Normalized());
                if (d >= 0 && d <= lineVector.Magnitude())
                {
                    if (earliestColl == null || toi < earliestColl.timeOfImpact)
                    {
                        earliestColl = new CollisionInfo(lineNormal, obstacle, toi);
                        minT = toi;
                    }
                }
            }

        }

        for (int i = 0; i < obstacle.leftRight.Count; i++)
        {
            LineSegment currWall = obstacle.leftRight[i].LineSegment;

            Vec2 lineVector = currWall.start - currWall.end;
            Vec2 lineNormal = lineVector.Normal();
            float a = Vec2.Dot(oldPosition - currWall.start, lineNormal) - Radius;
            float b = Vec2.Dot(oldPosition - Position, lineNormal);
            if (b <= 0)
            {
                continue;
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
                continue;
            }
            if (toi <= minT)
            {
                Vec2 poi = oldPosition + Velocity * toi;
                float d = Vec2.Dot(currWall.start - poi, lineVector.Normalized());
                if (d >= 0 && d <= lineVector.Magnitude())
                {
                    if (earliestColl == null || toi < earliestColl.timeOfImpact)
                    {
                        earliestColl = new CollisionInfo(lineNormal, obstacle, toi, obstacle.leftRight[i]);
                        minT = toi;
                    }
                }
            }

        }
        return earliestColl;
    }

    CollisionInfo CheckLineSegmentCollision(CollisionInfo earliestColl, LineSegment line)
    {
        Vec2 dif = new Vec2(oldPosition.x - line.start.x, oldPosition.y - line.start.y);
        Vec2 lineVector = line.end - line.start;
        Vec2 lineNormal = lineVector.Normal();
        float a = dif.Dot(lineNormal) - Radius;

        Vec2 totalMovement = Position - oldPosition;

        float b = -totalMovement.Dot(lineNormal);

        if (b <= 0) { return earliestColl; }

        float t;

        if (a >= 0)
        {
            t = a / b;
        }
        else if (a > -Radius)
        {
            t = 0;
        }
        else
        {
            return earliestColl;
        }

        if (t >= 0 && t <= 1)
        {
            Vec2 POI = oldPosition + Velocity * t;
            Vec2 dif2 = new Vec2(POI.x - line.start.x, POI.y - line.start.y);

            float d = dif2.Dot(lineVector.Normalized());

            if (d >= 0 && d <= lineVector.Magnitude())
            {
                earliestColl = new CollisionInfo(lineNormal, line, t);
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
        
        if (coll.other is Wall)
        {
            currentSlideWall = (Wall)coll.other;

            Position = oldPosition + Velocity * coll.timeOfImpact;
            Velocity = new Vec2();

            //Start timer
            durationToStickCounter = 0;
            playerState = PlayerState.Sticking;

            wallElement = Element.None;
            if (currentSlideWall is ElementWall)
            {
                wallElement = ((ElementWall)currentSlideWall).Element;
            }
        }
        else if (coll.other is Obstacle)
        {
            Obstacle curObstacle = (Obstacle)coll.other;

            if(!(curObstacle is ElementObstacle))
            {
                Position = oldPosition + Velocity * coll.timeOfImpact;
                Velocity = new Vec2();
                //Velocity.Reflect(coll.normal);
                return;
            }

            if (element != ((ElementObstacle)curObstacle).Element)
            {
                curObstacle.LateDestroy();
            }
            else
            {
                Position = oldPosition + Velocity * coll.timeOfImpact;
                Velocity = new Vec2();

                //if collision is top or bottom
                if (coll.otherReal == null) return;

                //if collision is left or right
                currentSlideWall = (Wall)coll.otherReal;
                playerState = PlayerState.Sticking;
                durationToStickCounter = durationToStick;

                wallElement = ((ElementObstacle)curObstacle).Element;

                if (mass > ((ElementObstacle)curObstacle).Mass)
                {
                    // stick
                    durationToStickCounter = 0;
                }
            }
        }
        else if (coll.other is LineSegment)
        {
            Position = oldPosition + Velocity * coll.timeOfImpact;
            Velocity = new Vec2();

            //Velocity.Reflect(coll.normal.Normalized(), Bounciness);
        }
        else if (coll.other is LineCap)
        {
            LineCap otherBall = (LineCap)coll.other;
            Position = otherBall.position + coll.normal;
            Velocity.Reflect(coll.normal.Normalized(), Bounciness);
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
    }

    void SwitchElement()
    {
        if (element == Element.Fire)
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

        if (playerState == PlayerState.Sticking || playerState == PlayerState.Sliding)
            playerState = PlayerState.None;
    }

    void Stick()
    {
        durationToStickCounter += Time.deltaTime;
        if (durationToStickCounter > durationToStick)
        {
            playerState = PlayerState.Sliding;//If the wall is a normal one, directly switch to the normal player state
            durationToStickCounter = 0;
            return;
        }

        if (wallElement != Element.None)
        {
            UpdateSize();
        }
    }

    void UpdateSize()
    {
        bool shouldGrow = element == wallElement;
        if (shouldGrow)
        {
            mass += 0.01f;
        }
        else
        {
            mass -= 0.01f;
        }

        mass = Mathf.Clamp(mass, 0.5f, 3f);

        SetScaleXY(mass);
        Radius = width / 2;

        CheckForScaleCorrection(shouldGrow);
    }

    void Slide()
    {
        Velocity = Gravity * 10 * mass;
        Position += Velocity;

        //Checks if the player has not reached the end of the given wall
        if ((currentSlideWall.IsLeft && Position.y >= currentSlideWall.LineSegment.end.y - 1)
            || (!currentSlideWall.IsLeft && Position.y >= currentSlideWall.LineSegment.start.y - 1))
        {
            playerState = PlayerState.None;
        }

        CollisionInfo firstCollision = null;
        firstCollision = CheckForBoundariesCollisions(firstCollision);
        if (firstCollision != null)
        {
            ResolveCollision(firstCollision);
        }
    }

    //Checks if the player is in or away from the wall and corrects it accordingly
    void CheckForScaleCorrection(bool shouldGrow)
    {
        float distance = 0f;
        if (shouldGrow && Mathf.Abs(Position.x - currentSlideWall.LineSegment.start.x) < Radius)
        {
            distance = Radius - Mathf.Abs(Position.x - currentSlideWall.LineSegment.start.x);
            Position.x = currentSlideWall.IsLeft ? Position.x + distance : Position.x - distance;
        }
        else if (Mathf.Abs(Position.x - currentSlideWall.LineSegment.start.x) > Radius)
        {
            distance = Mathf.Abs(Position.x - currentSlideWall.LineSegment.start.x) - Radius;
            Position.x = currentSlideWall.IsLeft ? Position.x - distance : Position.x + distance;
        }
    }

    void UpdateCoordinates()
    {
        x = Position.x;
        y = Position.y;
    }
}

enum PlayerState
{
    None, Sticking, Sliding
}