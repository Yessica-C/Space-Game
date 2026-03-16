using Godot;
using Godot.NativeInterop;
using System;
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
	

	//temp target location

	[Export]
	public Vector3[] Route = new Vector3[5];
	public Vector3 TargetLocation;
	public int TargetIndex = -1;
	private bool HasDesto = true;

    //target path toggle
    private CheckButton DisplayToggle;
    private bool PathLineEnabled = true;
    PackedScene PathLine;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		Route[0] = new Vector3(0, 20, -50);
        Route[1] = new Vector3(-50, 20, -75);
        Route[2] = new Vector3(0, 20, -100);
        Route[3] = new Vector3(50, 0, 10);
        Route[4] = new Vector3(50, 50, 150);

		HasDesto = true;
		UpdateTargetPos();
		//config damping TODO read this from a class-specific json
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
        DisplayToggle.Toggled += OnPathToggled;
        if (PathLine == null)
        {
            GD.PrintErr("ERROR: Expected Resource: res://ship_components/path_line.tscn NOT FOUND - ShipMotionTesting.cs");
        }
    }

    private void OnPathToggled(bool toggledOn)
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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

    }

    private void _PrintTransform()
    {
        GD.Print("---------------------------------");
        GD.Print("Global Position:\t", GlobalPosition);
        GD.Print("Global Rotation:\t", GlobalRotation);
        GD.Print("Local Quaternion:\t", Quaternion);
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
		if (TargetIndex == Route.Length)
		{
			GD.Print("Route Complete");
			HasDesto = false;
		}
		else
		{
			TargetIndex++;
			TargetLocation = Route[TargetIndex];
			GD.Print("new Target: ", TargetLocation);
		}
    }

}
