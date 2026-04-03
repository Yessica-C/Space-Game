using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class MouseSelectionBox : Area3D
{
    Node Parent;
    bool Selected = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Parent = GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _MouseEnter()
    {
        Selected = true;
        // Send a signal to the parent node
        if (Parent != null)
        {
            Parent.Call("MouseHoverOn");
        }
    }
    public override void _MouseExit()
    {
        Selected = false;
        // Send a signal to the parent node
        if (Parent != null)
        {
            Parent.Call("MouseHoverOff");
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("select") && Selected)
        {
            Parent.Call("InputSelected");
        }
    }
}
