using Godot;

/// <summary>
/// Wrapper class for resizing area2d to fit control
/// Godot default input handling has problems with overlaying 2D and 3D as these are raytraced simultaneously
/// A common usecase is to occlude 3D world with 2D overlays e.g. any RTS 
/// wrapper class lets us use the nice control layouts such as HBoxContainer while still having control over raycasting order and behavior in physics_process
/// </summary>
public partial class Wrapper2D : Control
{
    public const string PATH_TO_SCENE = "res://AnimationToggle/Wrapper2D/wrapper_2d.tscn";
    public Area2D Area2D { get; private set; }

    public void Setup(Area2D area2D)
    {
        Area2D = area2D;
        AddChild(Area2D);
    }
    override public void _Ready()
    {
        Resized += Wrapper2D_Resized; ;
    }

    private void Wrapper2D_Resized()
    {        
        Area2D.Position = new(Size.X / 2, Size.Y / 2); //Node2D is centered on origin
        Area2D.Scale = new(Size.X, Size.Y); // Assumes CollisionShape2D and MeshInstance2D are of size 1x1
    }
}
