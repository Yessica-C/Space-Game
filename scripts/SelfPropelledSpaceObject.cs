using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using SpaceGame.enums;
using System.Reflection.Metadata;
using System.Security.Cryptography;


public partial class SelfPropelledSpaceObject : RigidBody3D
{
    //user interaction parameters
    public bool Selected = false;
    private float SelectionSizeX = 5;
    private float SelectionSizeY = 5;
    private float SelectionSizeZ = 5;
    private SpaceObjectManager Controller = null;

    //auto thrust parameters
    [Export] public float MoveSpeed = 0; //5.0 for carrier
	[Export] public float NavContactRange = 0f; // 10 for carrier

	//auto rotation parameters
	[Export] public float SelfRotationSpeed = 0f; //How quickly the body rotates toward the target direction
	[Export] public float TorqueMultiplier = 0f; //1.0 for carrier
	[Export] public float AlignmentThreshold = 0f; //Angle threshold in degrees to be "aligned"

	//navigation parameters
    public NavMode NAV_MODE = NavMode.STATIONARY;
    public List<Vector3> Route = new List<Vector3>();
    public Vector3 TargetLocation;
	private int TargetIndex = -1;
    private double TimeSinceNavUpdate = 0;
    SelfPropelledSpaceObject NavTarget = null;
    float OrbitalInclination = 0;
    Vector3 OrbitalCenter;

    //debug path view parameters
    private CheckButton DisplayToggle;
    private bool PathLineEnabled = true;
    PackedScene RouteLineScene; 
    PackedScene OrangeSelectionBox = GD.Load<PackedScene>("res://space_objects//orange_selection_box/orange_selection_box.tscn");

    //object component parameters
    private ObjectType OBJType;
    public bool verbose = false;



    #region Engine Functions

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetSelectionBoxSize(10, 10, 10);
    }

    
    public override void _PhysicsProcess(double delta)
	{
        HandleNavigation(delta);
    }
    public override void _Process(double _delta)
	{

    }
    #endregion
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


    #region Setters & Getters

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

    public void SetController(SpaceObjectManager controller)
    {
        this.Controller = controller;
    }

    public ObjectType GetObjectType()
    {
        return this.OBJType;
    }
    public void SetObjectType(ObjectType Type)
    {
        this.OBJType = Type;
    }
    public void SetNavTarget(SelfPropelledSpaceObject Target)
    {
        NavTarget = Target;
        OrbitalCenter = Target.GlobalPosition;
    }
    public void SetOrbitalInclination(float Inclination)
    {
        OrbitalInclination = Inclination;
    }
    public float GetOrbitalInclination()
    {
        return OrbitalInclination;
    }
    public int GetTargetIndex()
    {
        return TargetIndex;
    }
    #endregion Setters & Getters
    #region User Interaction    
    
    public void MouseHoverOn()
    {
        OrangeSelectionBox OSB = OrangeSelectionBox.Instantiate<OrangeSelectionBox>();
        AddChild(OSB);
        OSB.GlobalTransform = this.GlobalTransform;
        OSB.SetSize(SelectionSizeX, SelectionSizeY, SelectionSizeZ);
        OSB.Name = "OSB";
    }

    public void MouseHoverOff()
    {
        if(!Selected)
        {
            IEnumerable<OrangeSelectionBox> allSelectionBoxes = GetChildren().OfType<OrangeSelectionBox>();
            foreach (var child in allSelectionBoxes)
            {
                child.QueueFree();
            }
        }
    }

    public void InputSelected()
    {
        GD.Print(Name, " Was Clicked!");
        Controller = GetParent<SpaceObjectManager>();
        Controller.ObjectSelectionTrigger(this);
        Selected = true;
        MouseHoverOn();
    }
    public void Deselect()
    {
        Selected = false;
        MouseHoverOff();
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
	private void HandleNavigation(double delta)
    {
        switch(NAV_MODE)
        {
            case NavMode.STATIONARY:
                //do nothing
                break;
            case NavMode.LINEAR:
                HandleLinearMotion(delta);
                break;
            case NavMode.ORBITING_STATIONARY:
                HandleStationaryOrbitMotion(delta);
                break;
            case NavMode.ORBITING_MOVING:
                HandleMovingOrbitMotion(delta);
                break;
        }
    }

    private void HandleLinearMotion(double delta)
    {
        ShipMotionLib.AlignTowardsTarget(this, TargetLocation, TorqueMultiplier);
        ShipMotionLib.AlignmentAdjustedAccToTarget(this, TargetLocation, MoveSpeed, NavContactRange);
        
        if (WithinContactRangeOfTarget())//if within contact range, of target, move to next target
        {
            UpdateTargetPos();
        }
    }
    private void HandleStationaryOrbitMotion(double delta)
    {
        HandleLinearMotion(delta);
    }
    private void HandleMovingOrbitMotion(double delta)
    {
        TimeSinceNavUpdate += delta;
        if (TimeSinceNavUpdate > 0.5)
        {
            TimeSinceNavUpdate = 0;
            Vector3 TargetPosDiff = NavTarget.GlobalPosition - OrbitalCenter;
            OrbitalCenter = NavTarget.GlobalPosition;
            //for each point in route, update with change in target position
            for (int i = 0; i < Route.Count; i++)
            {
                Route[i] += TargetPosDiff;
            }
            TargetLocation = Route[TargetIndex];
            //SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(NavTarget.GlobalPosition, 40f, false, GetOrbitalInclination()), NAV_MODE);
        }
        HandleLinearMotion(delta);
    }
    private bool AlignedToCurrentTarget()
	{
		return ShipMotionLib.AlignmentDiffDegrees(this, TargetLocation) > AlignmentThreshold;
    }
    public void SetNewRoute(List<Vector3> NewRoute, NavMode NewMode)
    {
        Route = NewRoute;
        NAV_MODE = NewMode;

        if (NewMode == NavMode.ORBITING_STATIONARY)
        {
            Vector3 ClosestRoutePoint;
            int ClosestRouteIndex;
            ShipMotionLib.GetClosestPointInRoute(this, Route, out ClosestRoutePoint, out ClosestRouteIndex);
            TargetLocation = ClosestRoutePoint;
            TargetIndex = ClosestRouteIndex;
        }
        if(NewMode == NavMode.ORBITING_MOVING)
        {
            Vector3 ClosestRoutePoint;
            int ClosestRouteIndex;
            ShipMotionLib.GetClosestPointInRoute(this, Route, out ClosestRoutePoint, out ClosestRouteIndex);
            TargetIndex = ClosestRouteIndex;
            UpdateTargetPos();
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
            if (NAV_MODE == NavMode.ORBITING_STATIONARY || NAV_MODE == NavMode.ORBITING_MOVING)
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
