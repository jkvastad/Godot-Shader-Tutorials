using Godot;
public partial class FragmentEdges : Node
{
    [Export]
    MeshInstance3D box2;
    public override void _Ready()
    {
        Tween boxMotion = GetTree().CreateTween();
        boxMotion.SetTrans(Tween.TransitionType.Sine).SetLoops();
        boxMotion.TweenInterval(1);
        boxMotion.TweenProperty(box2, "position:x", 1, 2.0);
        boxMotion.TweenInterval(1);
        boxMotion.TweenProperty(box2, "position:x", 0, 2.0);
        boxMotion.TweenInterval(1);
        boxMotion.TweenProperty(box2, "position:x", 1, 2.0);
        boxMotion.TweenInterval(1);
        boxMotion.TweenProperty(box2, "position:x", 2, 2.0);
    }
}
