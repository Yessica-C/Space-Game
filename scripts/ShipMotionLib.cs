using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Godot.HttpRequest;

public static class ShipMotionLib
{
    //TODO add integer return codes if some arg does not exist
    public static int AlignmentAdjustedAccToTarget(RigidBody3D Body, Vector3 TargetLocation, float MoveSpeed, float DistanceThreshold)
    {
        float minAlignment = 0.5f;
        float alignmentFactor = 1.0f;
        Vector3 currentPosition = Body.Position;
        Vector3 forwardDirection = Body.Transform.Basis.Z;
        Vector3 directionToTarget = (TargetLocation - currentPosition).Normalized();

        float alignment = Math.Abs(forwardDirection.Dot(directionToTarget));
        // Only accelerate if aligned enough
        if (alignment >= minAlignment)
        {
            alignment = Mathf.Clamp(alignment, 0.0f, 1.0f);
            float accelerationMagnitude = MoveSpeed * alignment * alignmentFactor;

            // Calculate distance to target
            float distanceToTarget = (TargetLocation - currentPosition).Length();

            // Reduce acceleration based on distance to minimize overshoot
            float distanceFactor = Mathf.Clamp(distanceToTarget / (2.5f * DistanceThreshold), 0.1f, 1.0f);
            accelerationMagnitude *= distanceFactor;

            Vector3 acceleration = directionToTarget * accelerationMagnitude;
            Body.ApplyCentralForce(acceleration);
            return 1;
        }
        return 0;
    }

    public static float AlignmentDiffDegrees(RigidBody3D Body, Vector3 TargetPos)
    {
        Vector3 DirectionTotarget = (TargetPos - Body.GlobalTransform.Origin).Normalized();
        Vector3 ForwardDirection = Body.GlobalTransform.Basis.Z.Normalized(); //forward direction of self
        float Angle = (float)Math.Acos(Mathf.Clamp(ForwardDirection.Dot(DirectionTotarget), -1.0f, 1.0f)); //Calculate the angle between the directions
        float AngleDeg = Math.Abs(float.RadiansToDegrees(Angle) - 180); //in degrees
        return AngleDeg;
    }

    public static Quaternion GetQuatToRotateTo(RigidBody3D Body, Vector3 TargetOrientation)
    {
        Vector3 Forward = TargetOrientation.Normalized(); //forward direction of self
        Vector3 Up = new Vector3(0, -1, 0); //y is up
        if (Math.Abs(Forward.Dot(Up)) > 0.99)//case for up and forward are parallel
        {
            Up = new Vector3(1, 0, 0);
        }
        Vector3 Right = Forward.Cross(Up).Normalized();
        Up = Right.Cross(Forward).Normalized();

        Basis b = new Basis(Right, Up, Forward);
        return b.GetRotationQuaternion();
    }

    //TODO add integer return codes if some arg does not exist
    public static int AlignTowardsTarget(RigidBody3D Body, Vector3 TargetPos, float TorqueMultiplier)
    {
        /*
		 "[Quaternions], though beautifully ingenious, have been an unmixed evil to those who have touched them in any way"
		 Lord Kelvin - 1892
         */
        Vector3 DirectionToTarget = (TargetPos - Body.GlobalPosition).Normalized();

        // Calculate the rotation difference using quaternions
        Quaternion CurrentQuat = Basis.FromEuler(Body.GlobalRotation).GetRotationQuaternion();
        Quaternion TargetQuat = GetQuatToRotateTo(Body, DirectionToTarget);

        // Ensure we take the shortest path by checking the dot product
        float dotProduct = CurrentQuat.Dot(TargetQuat);

        // If dot product is negative, flip the target quaternion to take the shorter path
        if (dotProduct < 0.0f)
        {
            TargetQuat = -TargetQuat;
        }

        // Calculate the rotation difference
        Quaternion RotationDifference = TargetQuat * CurrentQuat.Inverse();

        // Apply torque (simplified approach)
        Vector3 AxisAngleTorque = RotationDifference.GetAxis() * RotationDifference.GetAngle() * TorqueMultiplier;

        // Apply the torque
        Body.ApplyTorque(AxisAngleTorque);
        return 0;
    }

    public static List<Vector3> GenerateOrbitalPoints(Vector3 center, float radius)
    {
        // Ensure normal is normalized
        Vector3 normal = new Vector3(0, 1, 0);
        normal = normal.Normalized();

        // Create orthogonal vectors for the circle plane
        Vector3 tangent1, tangent2;
        CreateOrthogonalBasis(normal, out tangent1, out tangent2);

        List<Vector3> points = new List<Vector3>();

        // Generate 10 points around the circle
        for (int i = 0; i < 10; i++)
        {
            float angle = (float)i / 10.0f * 2.0f * Mathf.Pi;

            // Calculate point using parametric circle equation
            Vector3 point = center +
                           radius * (tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle));

            points.Add(point);
        }
        Random random = new Random();
        double chance = random.NextDouble();

        // 50% chance to reverse the orbit (CCW is normal, CW is reversed)
        if (chance < 0.5)
        {
            points.Reverse();
        }
        return points;
    }

    //used for orbit point generation
    private static void CreateOrthogonalBasis(Vector3 normal, out Vector3 tangent1, out Vector3 tangent2)
    {
        // Find a vector that's not parallel to the normal
        Vector3 reference = Vector3.Right;
        if (Mathf.Abs(normal.Dot(Vector3.Right)) > 0.99f)
        {
            reference = Vector3.Forward;
        }

        // Create two orthogonal vectors using cross products
        tangent1 = normal.Cross(reference).Normalized();
        tangent2 = normal.Cross(tangent1).Normalized();
    }

    public static void GetClosestPointInRoute(RigidBody3D Body, List<Vector3> Route, out Vector3 ClosestPoint, out int RouteIndex)
    {
        Vector3 Closest = Route[0];
        int BestIndex = -1;
        float ClosestDistance = float.MaxValue;

        int i = -1;
        foreach (Vector3 Point in Route)
        {
            i++;
            float distance = Body.GlobalTransform.Origin.DistanceTo(Point);
            if (distance < ClosestDistance)
            {
                ClosestDistance = distance;
                Closest = Point;
                BestIndex = i;
            }
        }
        ClosestPoint = Closest;
        RouteIndex = BestIndex;
    }

}
