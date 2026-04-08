using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class SpaceObjectManager : Node
{

    private SelfPropelledSpaceObject selectedObject = null;
    private UserInterfaceController UICon = null;

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
		Carrier1.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), SelfPropelledSpaceObject.NavMode.ORBITING);

        StartingPos = new Vector3(0, -50, 0);
        OrbitalCenter = new Vector3(-75, 0, 0);

        SelfPropelledSpaceObject Carrier2 = DevArtCarrier.Instantiate<SelfPropelledSpaceObject>();
        AddChild(Carrier2);
        Carrier2.Name = "Carrier 2";
        Carrier2.SetController(this);
        Carrier2.OverrideCurrentPos(StartingPos);
        Carrier2.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), SelfPropelledSpaceObject.NavMode.ORBITING);



        StartingPos = new Vector3(0, -100, 0);
        OrbitalCenter = new Vector3(0, 0, -75);

        SelfPropelledSpaceObject Fighter1 = DevArtFighter.Instantiate<SelfPropelledSpaceObject>();
        AddChild(Fighter1);
        Fighter1.Name = "Fighter1";
        Fighter1.SetController(this);
        Fighter1.OverrideCurrentPos(StartingPos);
        Fighter1.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), SelfPropelledSpaceObject.NavMode.ORBITING);

        StartingPos = new Vector3(0, 100, 0);
        OrbitalCenter = new Vector3(0, 0, 75);

        SelfPropelledSpaceObject Fighter2 = DevArtFighter.Instantiate<SelfPropelledSpaceObject>();
        AddChild(Fighter2);
        Fighter2.SetController(this);
        Fighter2.Name = "Fighter2";
        Fighter2.OverrideCurrentPos(StartingPos);
        Fighter2.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), SelfPropelledSpaceObject.NavMode.ORBITING);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void ChangeSelectedObject(SelfPropelledSpaceObject newSelection)
    {
        GD.Print("CSO");
        if (selectedObject != null)
        {
            selectedObject.Deselect();
        }
        selectedObject = newSelection;
        GD.Print(newSelection.Name, " Was Passed to SOM");
        EmitSignal(SignalName.NewObjectSelected, newSelection);
    }
    #region Signals
    
    [Signal]
    public delegate void NewObjectSelectedEventHandler(SelfPropelledSpaceObject newSelection);
    #endregion Signals
}
