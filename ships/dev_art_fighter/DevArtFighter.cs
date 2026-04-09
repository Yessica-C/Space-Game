using Godot;
using SpaceGame.enums;
using System;

public partial class DevArtFighter : SelfPropelledSpaceObject
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        configureMotionParameters();
        setupPathLineToggle();
        SetSelectionBoxSize(8, 8, 8);
		SetObjectType(ObjectType.SHIP);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void configureMotionParameters()
    {
        MoveSpeed = 35.0f;
        NavContactRange = 8.0f;

        SelfRotationSpeed = 16f;
        TorqueMultiplier = 16f;
        AlignmentThreshold = 50.0f;

        AngularDamp = 0.98f;
        LinearDamp = 0.98f;
    }
}
