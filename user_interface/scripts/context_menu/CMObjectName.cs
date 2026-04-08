using Godot;

public partial class CMObjectName : Label
{
    public void _on_space_object_manager_new_object_selected(SelfPropelledSpaceObject newSelection)
    {
        this.Text = newSelection.Name;
    }
}