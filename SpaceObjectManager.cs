using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class SpaceObjectManager : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PackedScene DevArtCarrier = GD.Load<PackedScene>("res://ships/dev_art_medium_ship/DemoCarrier.tscn");

		Vector3 StartingPos = new Vector3(0, 50, 0);
        Vector3 OrbitalCenter = new Vector3(75, 0, 0);

        ShipMotionTesting Carrier1 = DevArtCarrier.Instantiate<ShipMotionTesting>();
		AddChild(Carrier1);
		Carrier1.Name = "Carrier 1";
        Carrier1.OverrideCurrentPos(StartingPos);
		Carrier1.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), ShipMotionTesting.NavMode.ORBITING);

        StartingPos = new Vector3(0, -50, 0);
        OrbitalCenter = new Vector3(-75, 0, 0);

        ShipMotionTesting Carrier2 = DevArtCarrier.Instantiate<ShipMotionTesting>();
        AddChild(Carrier2);
        Carrier2.Name = "Carrier 2";
        Carrier2.OverrideCurrentPos(StartingPos);
        Carrier2.SetNewRoute(ShipMotionLib.GenerateOrbitalPoints(OrbitalCenter, 40f), ShipMotionTesting.NavMode.ORBITING);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
