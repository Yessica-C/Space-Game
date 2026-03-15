using Godot;
using System;

public partial class DebugMenu : Container
{

	private bool Enabled = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//do nothing
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//do nothing
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventAction inputEvent && inputEvent.Action == "toggle_debug_menu")
        {
			GD.Print("Button");
            Enabled = !Enabled;
            this.Visible = Enabled;
        }
    }
}
