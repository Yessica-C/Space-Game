using Godot;
using System;
using System.Runtime.InteropServices.JavaScript;

public partial class OrangeSelectionBox : Node3D
{

	[Export] public float SizeX = 5.0f;
    [Export] public float SizeY = 5.0f;
    [Export] public float SizeZ = 5.0f;
    private float DistanceX;
    private float DistanceY;
    private float DistanceZ;
    Node3D nnn;
    Node3D nnp;
    Node3D npn;
    Node3D npp;
    Node3D pnn;
    Node3D pnp;
    Node3D ppn;
    Node3D ppp;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        nnn = GetNode<Node3D>("---");
        nnp = GetNode<Node3D>("--+");
        npn = GetNode<Node3D>("-+-");
        npp = GetNode<Node3D>("-++");
        pnn = GetNode<Node3D>("+--");
        pnp = GetNode<Node3D>("+-+");
        ppn = GetNode<Node3D>("++-");
        ppp = GetNode<Node3D>("+++");

        if (nnn == null) GD.Print("OSB - nnn is null");
        if (nnp == null) GD.Print("OSB - nnp is null");
        if (npn == null) GD.Print("OSB - npn is null");
        if (npp == null) GD.Print("OSB - npp is null");
        if (pnn == null) GD.Print("OSB - pnn is null");
        if (pnp == null) GD.Print("OSB - pnp is null");
        if (ppn == null) GD.Print("OSB - ppn is null");
        if (ppp == null) GD.Print("OSB - ppp is null");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
    }

    public void SetSize(float SizeX, float SizeY, float SizeZ) 
    {
        DistanceX = ((SizeX / 2));
        DistanceY = ((SizeY / 2));
        DistanceZ = ((SizeZ / 2));
        nnn.Position += new Vector3(-DistanceX, -DistanceY, -DistanceZ);
        nnp.Position += new Vector3(-DistanceX, -DistanceY, DistanceZ);
        npn.Position += new Vector3(-DistanceX, DistanceY, -DistanceZ);
        npp.Position += new Vector3(-DistanceX, DistanceY, DistanceZ);
        pnn.Position += new Vector3(DistanceX, -DistanceY, -DistanceZ);
        pnp.Position += new Vector3(DistanceX, -DistanceY, DistanceZ);
        ppn.Position += new Vector3(DistanceX, DistanceY, -DistanceZ);
        ppp.Position += new Vector3(DistanceX, DistanceY, DistanceZ);
    }
}
