using Godot;
using System;
using System.Runtime.CompilerServices;
using SpaceGame.enums;

public partial class SpaceObjectManager : Node
{

    private SelfPropelledSpaceObject selectedObject = null;
    private UserInterfaceController UICon = null;
    private SelectionBehavior NextBehavior = SelectionBehavior.SELECT;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        //get user interface
        UICon = GetNode<UserInterfaceController>("/root/World/UserInterface");
        if (UICon == null)
        {
            GD.Print("UIC NULL");
        }

		PackedScene DevArtCarrier = GD.Load<PackedScene>("res://ships/demo_carrier/DemoCarrier.tscn");
		PackedScene DevArtFighter = GD.Load<PackedScene>("res://ships/dev_art_fighter/DemoFighter.tscn");

		Vector3 StartingPos = new Vector3(0, 50, 0);
        Vector3 OrbitalCenter = new Vector3(75, 0, 0);

        SelfPropelledSpaceObject Carrier1 = DevArtCarrier.Instantiate<SelfPropelledSpaceObject>();
		AddChild(Carrier1);
		Carrier1.Name = "Carrier 1";
        Carrier1.SetController(this);
        Carrier1.OverrideCurrentPos(StartingPos);
		Carrier1.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), NavMode.ORBITING_STATIONARY);

        StartingPos = new Vector3(0, -50, 0);
        OrbitalCenter = new Vector3(-75, 0, 0);

        SelfPropelledSpaceObject Carrier2 = DevArtCarrier.Instantiate<SelfPropelledSpaceObject>();
        AddChild(Carrier2);
        Carrier2.Name = "Carrier 2";
        Carrier2.SetController(this);
        Carrier2.OverrideCurrentPos(StartingPos);
        Carrier2.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), NavMode.ORBITING_STATIONARY);



        StartingPos = new Vector3(0, -100, 0);
        OrbitalCenter = new Vector3(0, 0, -75);

        SelfPropelledSpaceObject Fighter1 = DevArtFighter.Instantiate<SelfPropelledSpaceObject>();
        AddChild(Fighter1);
        Fighter1.Name = "Fighter1";
        Fighter1.SetController(this);
        Fighter1.OverrideCurrentPos(StartingPos);
        Fighter1.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), NavMode.ORBITING_STATIONARY);

        StartingPos = new Vector3(0, 100, 0);
        OrbitalCenter = new Vector3(0, 0, 75);

        SelfPropelledSpaceObject Fighter2 = DevArtFighter.Instantiate<SelfPropelledSpaceObject>();
        AddChild(Fighter2);
        Fighter2.SetController(this);
        Fighter2.Name = "Fighter2";
        Fighter2.OverrideCurrentPos(StartingPos);
        Fighter2.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), NavMode.ORBITING_STATIONARY);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public SelfPropelledSpaceObject GetSelectedObject()
    {
        return selectedObject;
    }

    public void ObjectSelectionTrigger(SelfPropelledSpaceObject newSelection)
    {
        switch (NextBehavior)
        {
            case SelectionBehavior.SELECT:
                SetSelectedObject(newSelection);
                break;
            case SelectionBehavior.ORBITAL_TARGET:
                SetNewOrbitTarget(selectedObject, newSelection);
                break;
        }
    }

    public void SetSelectedObject(SelfPropelledSpaceObject newSelection)
    {
        if (selectedObject != null)
        {
            selectedObject.Deselect();
        }
        selectedObject = newSelection;
        EmitSignal(SignalName.NewObjectSelected, newSelection);
    }

    private void SetNewOrbitTarget(SelfPropelledSpaceObject Body, SelfPropelledSpaceObject Target)
    {
        //TODO sometimes objects selected to orbit are not properly deselected
        //TODO automatically select between ORBITING_STATIONARY and ORBITING_MOVING based on target object type
        GD.Print("Commanding ", Body.Name, " To Orbit ", Target.Name);
        Vector3 OrbitalCenter = Target.GlobalPosition;
        float OrbitalRadius = 40f;
        Body.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, OrbitalRadius), NavMode.ORBITING_STATIONARY);
        Target.Deselect();
        NextBehavior = SelectionBehavior.SELECT;
    }

    #region Signals
    
    [Signal]
    public delegate void NewObjectSelectedEventHandler(SelfPropelledSpaceObject newSelection);

    #endregion Signals
    #region Signal Handlers
    public void _SelectedObjectRequestedNewOrbitTarget()
    {
        GD.Print("Select Object To Orbit");
        NextBehavior = SelectionBehavior.ORBITAL_TARGET;
    }
    #endregion Signal Handlers
}
