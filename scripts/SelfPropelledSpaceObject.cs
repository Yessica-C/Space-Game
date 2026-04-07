using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;


public partial class SelfPropelledSpaceObject : RigidBody3D
{
    //user interaction parameters
    public bool Selected = false;
    private float SelectionSizeX = 0;
    private float SelectionSizeY = 0;
    private float SelectionSizeZ = 0;

    //auto thrust parameters
    [Export] public float MoveSpeed = 15.0f; //5.0 for carrier
	[Export] public float NavContactRange = 10.0f; // 10 for carrier

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
    PackedScene OrangeSelectionBox = GD.Load<PackedScene>("res://space_objects//orange_selection_box/orange_selection_box.tscn");

    public bool verbose = false;

    public enum NavMode
    { 
        STATIONARY,
        LINEAR,
        ORBITING
    }

    #region Engine Functions

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //do nothing, this is an abstract class
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
        
        if(verbose)
        {
            GD.Print("[", Name, "] Velocity: ", LinearVelocity.Length());
        }
    }
    public override void _Process(double _delta)
	{

    }
    #endregion Engine Functions
    public void setupPathLineToggle()
    {        
        //debug display route line button setup
        DisplayToggle = GetNodeOrNull<CheckButton>("/root/World/UserInterface/DebugMenu/VBoxContainer/DisplayPathLines");
        RouteLineScene = GD.Load<PackedScene>("res://ship_components/technical/path_line.tscn");
        if (DisplayToggle == null)
        {
            GD.PrintErr("ERROR: Expected Resource: Root/DebugMenu/VBoxContainer/DisplayPathLines NOT FOUND - SelfPropelledSpaceObject.cs");
            GD.PrintErr("Expected path: ");
        }
        DisplayToggle.Toggled += OnPathDisplayLineToggled;
        if (RouteLineScene == null)
        {
            GD.PrintErr("ERROR: Expected Resource: res://ship_components/technical/path_line.tscn NOT FOUND - SelfPropelledSpaceObject.cs");
        }
    }

    public void SetSelectionBoxSize(float x, float y, float z)
    {
        SelectionSizeX = x;
        SelectionSizeY = y;
        SelectionSizeZ = z;
    }

    //FOR INITIAL SPAWNING ONLY
    public void OverrideCurrentPos(Vector3 NewPos)
    {
        GlobalPosition = NewPos;
    }

    #region User Interaction    
    
    public void MouseHoverOn()
    {
        OrangeSelectionBox OSB = OrangeSelectionBox.Instantiate<OrangeSelectionBox>();
        AddChild(OSB);
        OSB.GlobalTransform = this.GlobalTransform;
        OSB.SetSize(SelectionSizeX, SelectionSizeY, SelectionSizeZ);
        OSB.Name = "OSB";
        Selected = true;
    }

    public void MouseHoverOff()
    {
        IEnumerable<OrangeSelectionBox> allSelectionBoxes = GetChildren().OfType<OrangeSelectionBox>();
        foreach (var child in allSelectionBoxes)
        {
            child.QueueFree();
        }
        Selected = false;
    }

    public void InputSelected()
    {
        GD.Print(Name, " Was Clicked!");
    }

    #endregion User Interaction
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
    #region Navigation
	private bool AlignedToCurrentTarget()
	{
		return ShipMotionLib.AlignmentDiffDegrees(this, TargetLocation) > AlignmentThreshold;
    }
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
	private bool WithinContactRangeOfTarget()
	{
		return GlobalPosition.DistanceTo(TargetLocation) < NavContactRange;
    }
    private void UpdateTargetPos()
    {
        //rotate between a few target positions to practice rotation
        if (Route.Count == 0)
        {
            if (verbose)
            { 
                GD.Print("[", Name, "] - Route Complete");
            }
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
            if (verbose)
            { 
                GD.Print("[", Name, "] - new Target: ", TargetLocation);
            }
        }
    }
    #endregion Navigation
}   
