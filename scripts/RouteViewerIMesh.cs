using Godot;
using System;
using System.Runtime.CompilerServices;
using static ShipMotionTesting;

public partial class RouteViewerIMesh : MeshInstance3D
{
    private bool Enabled = true;
	private ShipMotionTesting Parent;
	private ImmediateMesh LineMesh;
    private CheckButton DisplayToggle;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Parent = GetParent<ShipMotionTesting>();

        DisplayToggle = GetNodeOrNull<CheckButton>("/root/World/UserInterface/DebugMenu/VBoxContainer/DisplayPathLines");
        if (DisplayToggle != null)
        {
            DisplayToggle.Toggled += OnPathToggled;
        }
        else
        {
            GD.PrintErr("ERROR: DisplayPathLines CheckButton not found!");
            GD.PrintErr("Expected path: Root/DebugMenu/VBoxContainer/DisplayPathLines");
        }
    }

    private void OnPathToggled(bool toggledOn)
    {
        Enabled = toggledOn;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        LineMesh = new ImmediateMesh();
        if (Enabled)
        {

            Vector3 CurrentPos = Parent.GlobalPosition;
            Vector3 TargetPos = Parent.TargetLocation;
            Vector3[] Route = Parent.Route;

            LineMesh.ClearSurfaces();
            LineMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
            LineMesh.SurfaceAddVertex(CurrentPos);
            LineMesh.SurfaceAddVertex(TargetPos - GlobalPosition);
            for (int i = Parent.TargetIndex; i < Route.Length - 1; i++)
            {
                LineMesh.SurfaceAddVertex(Route[i] - GlobalPosition);
                LineMesh.SurfaceAddVertex(Route[i + 1] - GlobalPosition);
            }
            LineMesh.SurfaceSetColor(Colors.Cyan);
            LineMesh.SurfaceEnd();

        }
        this.Mesh = LineMesh;
    }
}
