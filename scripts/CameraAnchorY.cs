using Godot;
using System;
using SpaceGame.enums;

public partial class CameraAnchorY : Node3D
{
    [Export] public float MousePanningSensitivity = 0.1f;
    [Export] public float KeyboardPanningSensitivity = 100f;
    [Export] public float MouseRotationSpeed = 0.01f;
    [Export] public float KeyboardRotationSpeed = 5f;
    [Export] public float MaxVerticalAngle = 1.5f; // in radians

    private bool isRotating = false;
    private Vector2 lastMousePos;
    private Node3D camera;
    private CameraMode Mode = CameraMode.FREE;
    private Node3D TrackingTarget = null;
    private SpaceObjectManager SOM = null;

    public override void _Ready()
    {
        camera = GetNode<Node3D>("CameraAnchorX/Camera3D");
        SOM = GetNode<SpaceObjectManager>("/root/World/SpaceObjectManager");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (Input.IsActionPressed("cam_rotate"))
            {
                isRotating = mouseButton.Pressed;
                if (isRotating)
                {
                    lastMousePos = mouseButton.Position;
                }
                else
                {
                    Input.SetMouseMode(Input.MouseModeEnum.Visible);
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && isRotating)
        {
            Vector2 delta = mouseMotion.Relative;
            RotateY(-delta.X * MouseRotationSpeed);
            Rotation = new Vector3(0, Rotation.Y, 0); // Reset X and Z rotation
        }

        // Panning
        if ((Input.IsActionPressed("cam_pan") || 
             (Input.IsActionPressed("cam_rotate") && Input.IsActionPressed("cam_pan_modif"))) && 
            @event is InputEventMouseMotion motion)
        {
            //free camera from tracking if user pans
            FreeTracking();
            // Create movement vector in local space
            Vector3 movement = new Vector3(
                -motion.Relative.X * MousePanningSensitivity,  // Left/right (local x)
                0,  // No vertical movement
                -motion.Relative.Y * MousePanningSensitivity   // Forward/backward (local z)
            );

            // Transform local movement to global space
            Vector3 globalMovement = GlobalTransform.Basis * movement;

            // Apply movement to global position
            GlobalPosition += globalMovement;
        }
    }

    public override void _Process(double delta)
    {
        HandleUserCameraMotion(delta);
        if(Mode == CameraMode.TRACKING)
        {
            HandleTrackingMotion();
        }
    }

    private void HandleUserCameraMotion(double delta)
    {
        float zoomFactor = (GlobalPosition.DistanceTo(camera.GlobalPosition)) / 50;

        // Keyboard camera panning
        Vector3 movement = Vector3.Zero;
        if (Input.IsActionPressed("move_left"))
        {
            movement += new Vector3(-KeyboardPanningSensitivity, 0, 0);
            FreeTracking();
        }
        if (Input.IsActionPressed("move_right"))
        {
            movement += new Vector3(KeyboardPanningSensitivity, 0, 0);
            FreeTracking();
        }
        if (Input.IsActionPressed("move_forward"))
        {   
            movement += new Vector3(0, 0, -KeyboardPanningSensitivity);
            FreeTracking();
        }
        if (Input.IsActionPressed("move_backward"))
        {
            movement += new Vector3(0, 0, KeyboardPanningSensitivity);
            FreeTracking();
        }
        Vector3 globalMovement = GlobalTransform.Basis * movement * (float)delta * zoomFactor;
        GlobalPosition += globalMovement;

        // Keyboard camera rotation
        if (Input.IsActionPressed("rotate_left"))
            RotateY(KeyboardRotationSpeed * (float)delta);
        if (Input.IsActionPressed("rotate_right"))
            RotateY(-KeyboardRotationSpeed * (float)delta);

        Rotation = new Vector3(0, Rotation.Y, 0); // Reset X and Z rotation

        // Rotation check
        if (Input.IsActionPressed("cam_rotate"))
        {
            if (!isRotating)
                isRotating = true;
        }
        else
        {
            if (isRotating)
                isRotating = false;
        }
    }

    private void HandleTrackingMotion()
    {
        Vector3 newPos = new Vector3(TrackingTarget.GlobalPosition.X, 0, TrackingTarget.GlobalPosition.Z);
        GlobalPosition = newPos;
    }

    private void FreeTracking()
    {
        if(Mode == CameraMode.TRACKING)
        {
            Mode = CameraMode.FREE;
        }
    }
    public void _on_camera_track_button_pressed()
    {
        
        Mode = CameraMode.TRACKING;
        TrackingTarget = SOM.GetSelectedObject();
    }
}

