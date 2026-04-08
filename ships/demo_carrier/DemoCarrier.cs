using Godot;
using System;

public partial class DemoCarrier : SelfPropelledSpaceObject
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		configureMotionParameters();
        setupPathLineToggle();
        SetSelectionBoxSize(25, 20, 55);
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void configureMotionParameters()
	{
		MoveSpeed = 10.0f;
        NavContactRange = 10.0f;

        SelfRotationSpeed = 3.0f;
        TorqueMultiplier = 3.0f;
        AlignmentThreshold = 75.0f;

        AngularDamp = 0.6f;
        LinearDamp = 0.6f;
    }
}
