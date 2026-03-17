using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;


public partial class ShipMotionTesting : RigidBody3D
{
	//auto thrust parameters
	[Export] public float MoveSpeed = 15.0f; //5.0 for carrier
	[Export] public float NavContactRange = 10.0f; // 10 for carrier
	[Export] public float MinAlignment = 0.5f; // % alligned to target point to begin thrusting

	//auto rotation parameters
	[Export] public float SelfRotationSpeed = 4f; //How quickly the body rotates toward the target direction
	[Export] public float TorqueMultiplier = 4f; //1.0 for carrier
	[Export] public float AlignmentThreshold = 75.0f; //Angle threshold in degrees to be "aligned"
	

	//navigation parameters
    public NavMode NAV_MODE = NavMode.STATIONARY;
    public List<Vector3> Route = new List<Vector3>();
    public Vector3 TargetLocation;
	public int TargetIndex = -1;

    //debug path view parameters
    private CheckButton DisplayToggle;
    private bool PathLineEnabled = true;
    PackedScene RouteLineScene;

    public enum NavMode
    { 
        STATIONARY,
        LINEAR,
        ORBITING
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

		//config damping TODO read this from a ship-specific json on load - maybe a data child?
        AngularDamp = 0.5f;
        LinearDamp = 0.5f;

        //debug display route line button setup
        DisplayToggle = GetNodeOrNull<CheckButton>("/root/World/UserInterface/DebugMenu/VBoxContainer/DisplayPathLines");
        RouteLineScene = GD.Load<PackedScene>("res://ship_components/path_line.tscn");
        if (DisplayToggle == null)
        {
            GD.PrintErr("ERROR: Expected Resource: Root/DebugMenu/VBoxContainer/DisplayPathLines NOT FOUND - ShipMotionTesting.cs");
            GD.PrintErr("Expected path: ");
        }
        DisplayToggle.Toggled += OnPathDisplayLineToggled;
        if (RouteLineScene == null)
        {
            GD.PrintErr("ERROR: Expected Resource: res://ship_components/path_line.tscn NOT FOUND - ShipMotionTesting.cs");
        }
    }
	
    public override void _PhysicsProcess(double delta)
	{
		if (NAV_MODE != NavMode.STATIONARY)
        {
			ShipMotionLib.AlignTowardsTarget(this, TargetLocation, TorqueMultiplier);
            ShipMotionLib.AlignmentAdjustedAccToTarget(this, TargetLocation, MoveSpeed, NavContactRange);
            
            if (WithinContactRangeOfTarget())//if within contact range, of target, move to next target
            {
                UpdateTargetPos();
            }
        }
    }

    public override void _Process(double delta)
	{

    }

    //FOR INITIAL SETUP
    public void OverrideCurrentPos(Vector3 NewPos)
    {
        GlobalPosition = NewPos;
    }
    //For giving commands
    public void SetNewRoute(List<Vector3> NewRoute, NavMode NewMode)
    {
        Route = NewRoute;
        NAV_MODE = NewMode;

        if (NewMode == NavMode.ORBITING)
        {
            Vector3 ClosestRoutePoint;
            int ClosestRouteIndex;
            ShipMotionLib.GetClosestPointInRoute(this, Route, out ClosestRoutePoint, out ClosestRouteIndex);
            TargetLocation = ClosestRoutePoint;
            TargetIndex = ClosestRouteIndex;
        }
        else
        { 
            UpdateTargetPos();
        }
    }

    private void OnPathDisplayLineToggled(bool toggledOn)
    {
        if (toggledOn)
        {
            // Spawn the node
            Node PathLineNode = RouteLineScene.Instantiate();
            AddChild(PathLineNode);
            PathLineNode.Name = "Path Line";
        }
        else
        {
            // Remove the node
            Node PathLineNode = GetNodeOrNull("Path Line");
            if (PathLineNode != null)
            {
                PathLineNode.QueueFree();
            }
        }
    }

    private void PrintTransform()
    {
        GD.Print("---------------------------------");
        GD.Print("Global Position:\t", GlobalPosition);
        GD.Print("Global Rotation:\t", GlobalRotation);
        GD.Print("Local Quaternion:\t", Quaternion);
    }

	private bool AlignedToCurrentTarget()
	{
		return ShipMotionLib.AlignmentDiffDegrees(this, TargetLocation) > AlignmentThreshold;
    }

	private bool WithinContactRangeOfTarget()
	{
		return GlobalPosition.DistanceTo(TargetLocation) < NavContactRange;
    }

    private void UpdateTargetPos()
    {
        //rotate between a few target positions to practice rotation
        if (Route.Count == 0)
        {
            GD.Print("[", Name, "] - Route Complete");
            NAV_MODE = NavMode.STATIONARY;
        }
        else 
        {
            if (NAV_MODE == NavMode.LINEAR)
            {
                TargetLocation = Route[0];
                Route.RemoveAt(0);
            }
            if (NAV_MODE == NavMode.ORBITING)
            {
                TargetIndex++;

                // Check if we've reached the last item in the route
                if (TargetIndex >= Route.Count)
                {
                    TargetIndex = 0; // Loop back to the first item
                }

                TargetLocation = Route[TargetIndex];
            }
            GD.Print("[", Name, "] - new Target: ", TargetLocation);
        }
    }
}
