using Godot;
using System;

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
}
