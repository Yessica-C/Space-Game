using Godot;
using System;
using SpaceGame.enums;

public partial class BlueEnderStar : SelfPropelledSpaceObject
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        SetSelectionBoxSize(10, 10, 10);
		SetObjectType(ObjectType.ASTEROID);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
