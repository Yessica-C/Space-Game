using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static ShipMotionTesting;

public partial class RouteViewerIMesh : MeshInstance3D
{
	private ShipMotionTesting Parent;
	private ImmediateMesh LineMesh;
    private CheckButton DisplayToggle;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Parent = GetParent<ShipMotionTesting>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        LineMesh = new ImmediateMesh();
        Vector3 CurrentPos = Parent.GlobalPosition;
        Vector3 TargetPos = Parent.TargetLocation;
        List<Vector3> Route = Parent.Route; // Changed from Vector3[] to List<Vector3>

        LineMesh.ClearSurfaces();
        LineMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        LineMesh.SurfaceSetColor(Colors.Cyan);

        //current position to next waypoint line
        LineMesh.SurfaceAddVertex(CurrentPos);
        LineMesh.SurfaceAddVertex(TargetPos - GlobalPosition);

        //linear remaining path
        if(Parent.NAV_MODE == NavMode.LINEAR)
        {
            for (int i = Parent.TargetIndex; i < Route.Count - 1; i++) // Changed from Route.Length to Route.Count
            {
                LineMesh.SurfaceAddVertex(Route[i] - GlobalPosition);
                LineMesh.SurfaceAddVertex(Route[i + 1] - GlobalPosition);
            }
        }
        if (Parent.NAV_MODE == NavMode.ORBITING)
        {
            for (int i = 0; i < Route.Count - 1; i++) // Changed from Route.Length to Route.Count
            {
                LineMesh.SurfaceAddVertex(Route[i] - GlobalPosition);
                LineMesh.SurfaceAddVertex(Route[i + 1] - GlobalPosition);
            }
            //finish loop
            LineMesh.SurfaceAddVertex(Route[Route.Count - 1] - GlobalPosition);
            LineMesh.SurfaceAddVertex(Route[0] - GlobalPosition);
        }

        LineMesh.SurfaceEnd();
        this.Mesh = LineMesh;
    }
}
