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
	private bool Maneuvering = false;
	private float ManeuverDistance = 0.0f;

	//auto rotation parameters
	[Export] public float SelfRotationSpeed = 4f; //How quickly the body rotates toward the target direction
	[Export] public float TorqueMultiplier = 4f; //1.0 for carrier
	[Export] public float AlignmentThreshold = 75.0f; //Angle threshold in degrees to be "aligned"
	

	//navigation parameters
    public NavMode NAV_MODE = NavMode.STATIONARY;
    public List<Vector3> Route = new List<Vector3>();
    public Vector3 TargetLocation;
	public int TargetIndex = -1;
	private bool HasDesto = true;

    //debug path view parameters
    private CheckButton DisplayToggle;
    private bool PathLineEnabled = true;
    PackedScene PathLine;

    public enum NavMode
    { 
        STATIONARY,
        LINEAR,
        ORBITING
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Vector3 OrbitalCenter = new Vector3(75, 0, 0);
        //Route = ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f);
        Route.Add(new Vector3(0, 20, -50));
        Route.Add(new Vector3(-50, 20, -75));
        Route.Add(new Vector3(0, 20, -100));
        Route.Add(new Vector3(50, 0, 10));
        Route.Add(new Vector3(50, 50, 150));
        
        NAV_MODE = NavMode.ORBITING;
		HasDesto = true;
		UpdateTargetPos();

		//config damping TODO read this from a ship-specific json on load - maybe a data child?
        AngularDamp = 0.5f;
        LinearDamp = 0.5f;

        //debug display path line button setup
        DisplayToggle = GetNodeOrNull<CheckButton>("/root/World/UserInterface/DebugMenu/VBoxContainer/DisplayPathLines");
        PathLine = GD.Load<PackedScene>("res://ship_components/path_line.tscn");
        if (DisplayToggle == null)
        {
            GD.PrintErr("ERROR: Expected Resource: Root/DebugMenu/VBoxContainer/DisplayPathLines NOT FOUND - ShipMotionTesting.cs");
            GD.PrintErr("Expected path: ");
        }
        DisplayToggle.Toggled += OnPathDisplayLineToggled;
        if (PathLine == null)
        {
            GD.PrintErr("ERROR: Expected Resource: res://ship_components/path_line.tscn NOT FOUND - ShipMotionTesting.cs");
        }
    }
	
    public override void _PhysicsProcess(double delta)
	{
		if (HasDesto)
        {
			ShipMotionLib.AlignTowardsTarget(this, TargetLocation, TorqueMultiplier);

            if (!AlignedToCurrentTarget() && !WithinContactRangeOfTarget())
            {
                Maneuvering = true;
            }
            if (Maneuvering)
            {
				ShipMotionLib.AlignmentAdjustedAccToTarget(this, TargetLocation, MoveSpeed, NavContactRange);
            }
            if (WithinContactRangeOfTarget())//if within contact range, 
            {
                Maneuvering = false;
                UpdateTargetPos();
            }
        }
    }

    public override void _Process(double delta)
	{

    }

    private void OnPathDisplayLineToggled(bool toggledOn)
    {
        if (toggledOn)
        {
            // Spawn the node
            Node PathLineNode = PathLine.Instantiate();
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

    private void _PrintTransform()
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
            GD.Print("Route Complete");
            NAV_MODE = NavMode.STATIONARY;
            HasDesto = false;
        }
        else 
        {
            if (NAV_MODE == NavMode.LINEAR)
            {
                TargetLocation = Route[0];
                Route.RemoveAt(0);
                GD.Print("new Target: ", TargetLocation);
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
                GD.Print("new Target: ", TargetLocation);
            }
        }
    }
}
