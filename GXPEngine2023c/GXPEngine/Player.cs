using System;
using System.Drawing;
using System.Linq.Expressions;
using GXPEngine;
using static GXPEngine.GlobalVariables;
using TiledMapParser;

public class Player : AnimationSprite
{
    public readonly float Bounciness = 0f;

    public Vec2 Position;
    public float Radius;

    public Vec2 Velocity;
    public Vec2 Gravity = new Vec2(0, 0.34f);

    public float Mass => mass;
    public bool CanJump => canJump;
    public bool CanSwitchElement => canSwitchElement;

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
    float startMass;

    ElementObstacle currentElementObstacle;

    bool canJump = true;
    bool canSwitchElement = true;

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
            ((Level)parent).MoveLevel(.5f);
        }

        oldPosition = Position;

        switch (playerState)
        {
            case PlayerState.StickWall:
                StickWall();
                break;
            case PlayerState.StickObstacle:
                StickObstacle();
                break;
            case PlayerState.Slide:
                SlideWall();
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
            oldPosition.y = Position.y - Velocity.y;

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
            earliestCollision = CheckLineSegmentCollision(earliestCollision, wall.LineSegment, wall);
        }

        //Check obstacles
        for (int i = 0; i < level.GetObstacleCount(); i++)
        {
            Obstacle obstacle = level.GetObstacle(i);

            //Check line caps
            for (int j = 0; j < 4; j++)
            {
                LineCap lineCap = null;
                switch (j)
                {
                    case 0:
                        lineCap = obstacle.leftRight[0].lineCapStart;
                        break;
                    case 1:
                        lineCap = obstacle.leftRight[0].lineCapEnd;
                        break;
                    case 2:
                        lineCap = obstacle.leftRight[1].lineCapStart;
                        break;
                    case 3:
                        lineCap = obstacle.leftRight[1].lineCapEnd;
                        break;
                    default:
                        Console.WriteLine("Smth went wrong in the line caps traversal in obstacles");
                        break;
                }
                if (lineCap == null)
                    continue;

                earliestCollision = CheckBallCollision(earliestCollision, lineCap, obstacle);
            }

            //Check line segment

            foreach (var line in obstacle.leftRight)
            {
                earliestCollision = CheckLineSegmentCollision(earliestCollision, line, obstacle);
            }
            foreach (var line in obstacle.topBottom)
            {
                earliestCollision = CheckLineSegmentCollision(earliestCollision, line, obstacle);
            }
        }

        return earliestCollision;
    }

    CollisionInfo CheckLineSegmentCollision(CollisionInfo earliestColl, LineSegment lineSegment, GameObject other = null)
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
                    earliestColl = new CollisionInfo(lineNormal, other == null ? lineSegment : other, toi, lineSegment);
                }
            }
        }

        return earliestColl;
    }

    CollisionInfo CheckBallCollision(CollisionInfo earliestColl, LineCap ball, GameObject other = null)
    {
        Vec2 relativePosition = oldPosition - ball.position;
        float a = Mathf.Pow(Velocity.Magnitude(), 2);
        float b = 2 * Vec2.Dot(relativePosition, Velocity);
        float c = Mathf.Pow(relativePosition.Magnitude(), 2) - Mathf.Pow(Radius + 0, 2);
        if (c < 0)
        {
            if (b < 0)
            {
                Vec2 pNormal = relativePosition.Normalized();
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
                earliestColl = new CollisionInfo((poi - ball.position).Normalized(), other == null ? ball : other, toi, ball);
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
            startMass = mass;
            playerState = PlayerState.StickWall;

            wallElement = Element.None;
            if (currentSlideWall is ElementWall)
            {
                wallElement = ((ElementWall)currentSlideWall).Element;
                if (startMass == 1f && element != wallElement)
                {
                    ((Level)parent).ReloadLevel();
                    return;
                }
                else if (startMass == 3f && element == wallElement)
                {
                    playerState = PlayerState.Slide;
                    return;
                }

                canSwitchElement = false;
            }
        }
        else if (coll.other is ElementObstacle)
        {
            currentElementObstacle = (ElementObstacle)coll.other;

            if (element == currentElementObstacle.Element)
            {
                if (coll.otherReal is LineCap)
                {
                    Position = oldPosition + Velocity * coll.timeOfImpact;
                    Velocity.Reflect(coll.normal, Bounciness);
                }
                //Collide from the side
                else if (coll.normal.y == 0f)
                {
                    Position = oldPosition + Velocity * coll.timeOfImpact;
                    Velocity = new Vec2();

                    currentSlideWall = new Wall(coll.normal.x == -1 ? false : true, (LineSegment)coll.otherReal);

                    if (mass < currentElementObstacle.Mass)
                    {
                        startMass = mass;
                        playerState = PlayerState.StickObstacle;
                        wallElement = currentElementObstacle.Element;
                    }
                    else
                    {
                        playerState = PlayerState.Slide;
                    }
                }
                //Collide from top or bottom
                else if (coll.normal.y != 0f)
                {
                    if (coll.normal.y == -1f)
                    {
                        //Restart abilities
                        canJump = true;
                        canSwitchElement = true;
                    }

                    Position = oldPosition + Velocity * coll.timeOfImpact;
                    Velocity.Reflect(coll.normal, Bounciness);
                }
            }
            else
            {
                if (mass <= currentElementObstacle.Mass)
                {
                    ((Level)parent).ReloadLevel();
                    return;
                }
                else
                {
                    mass -= currentElementObstacle.Mass;
                    SetScaleXY(mass);
                    Radius = width / 2;

                    Position = oldPosition + Velocity * coll.timeOfImpact;
                    Velocity *= 0.6f;

                    ((Level)parent).RemoveObstacle(currentElementObstacle);
                }
            }
        }
        else if (coll.other is Obstacle)
        {
            if (coll.normal.y == -1f)
            {
                //Restart abilities
                canJump = true;
                canSwitchElement = true;
            }

            Position = oldPosition + Velocity * coll.timeOfImpact;
            Velocity.Reflect(coll.normal, Bounciness);
        }
        else if (coll.other is LineCap)
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
        if (!isCharging && Input.GetMouseButtonDown(0) && canJump)
        {
            isCharging = true;
            chargeMousePos = mousePosition;

            chargeIndicator.visible = true;
        }
        if (Input.GetMouseButtonUp(0) && canJump)
        {
            isCharging = false;
            Release();
        }
        if (Input.GetMouseButtonDown(1) && canSwitchElement)
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

        canSwitchElement = false;
    }

    void Charge()
    {
        Vec2 distanceVec = chargeMousePos - mousePosition;
        chargeDistance = distanceVec.Magnitude();

        chargeDistance = Mathf.Clamp(chargeDistance, 0f, chargeDistanceMax);
        chargeDistance = Mathf.Map(chargeDistance, 0, chargeDistanceMax, 0f, 20f);


        //chargeIndicator.startPoint = new Vec2(x, game.height/2);
        chargeIndicator.startPoint = Position;
        chargeIndicator.vector = chargeDistance * distanceVec.Normalized();
        chargeIndicator.lineWidth = (uint)Mathf.Map(chargeDistance, 0, chargeDistanceMax, 1f, 30f);
    }

    void Release()
    {
        //Launch
        Velocity = chargeDistance * (chargeMousePos - mousePosition).Normalized();

        chargeIndicator.visible = false;

        if (playerState == PlayerState.Slide)
            playerState = PlayerState.None;

        canJump = false;
    }

    void StickWall()
    {
        bool shouldGrow = element == wallElement;
        if (shouldGrow)
        {
            mass += 0.01f;
            mass = Mathf.Clamp(mass, startMass, startMass + 1);
            if (mass == startMass + 1)
            {
                SetStateToSlide();
            }
        }
        else
        {
            mass -= 0.01f;
            mass = Mathf.Clamp(mass, startMass - 1, startMass);
            if (mass == startMass - 1)
            {
                SetStateToSlide();
            }
        }

        SetScaleXY(mass);
        Radius = width / 2;

        CheckForScaleCorrection(shouldGrow);
    }

    void StickObstacle()
    {
        mass += 0.01f;
        mass = Mathf.Clamp(mass, startMass, currentElementObstacle.Mass);
        if (mass == currentElementObstacle.Mass)
        {
            SetStateToSlide();
        }

        SetScaleXY(mass);
        Radius = width / 2;

        CheckForScaleCorrection(true);
    }

    void SetStateToSlide()
    {
        playerState = PlayerState.Slide;
        //Restart abilities
        canJump = true;
        canSwitchElement = true;
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

        SetScaleXY(mass);
        Radius = width / 2;

        CheckForScaleCorrection(shouldGrow);
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

    void SlideWall()
    {
        Velocity = Gravity * 10 * mass;
        Position += Velocity;

        //Checks if the player has not reached the end of the given wall
        if ((currentSlideWall.IsLeft && Position.y >= currentSlideWall.LineSegment.end.y)
            || (!currentSlideWall.IsLeft && Position.y >= currentSlideWall.LineSegment.start.y))
        {
            playerState = PlayerState.None;
            return;
        }

        CollisionInfo firstCollision = null;
        firstCollision = CheckForBoundariesCollisions(firstCollision);
        if (firstCollision != null)
        {
            ResolveCollision(firstCollision);
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
    None, StickWall, StickObstacle, Slide
}