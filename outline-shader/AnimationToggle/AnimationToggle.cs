using Godot;
using System;

public partial class AnimationToggle : Control
{
    [Export]
    public HBoxContainer ButtonContainer { get; set; }
    override public void _Ready()
    {
        InputBindings inputBindings = new(); //Ensure input bindings are setup
        AddWrappedArea();
        AddWrappedArea();
    }

    private void AddWrappedArea()
    {
        CustomArea2D area2D = new();
        area2D.Setup();

        Wrapper2D wrapper2D = GD.Load<PackedScene>(Wrapper2D.PATH_TO_SCENE).Instantiate<Wrapper2D>();
        wrapper2D.Setup(area2D);
        ButtonContainer.AddChild(wrapper2D, true);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(InputBindings.LEFT_MOUSE_BUTTON))
        {
            Vector2 clickedPoint = GetViewport().GetMousePosition();

            var query2D = new PhysicsPointQueryParameters2D()
            {
                CollideWithAreas = true,
                Position = clickedPoint
            };

            var physicsSpace = GetViewport().World2D.DirectSpaceState;
            var result2D = physicsSpace.IntersectPoint(query2D);

            if (result2D.Count > 0)
            {
                //Try for click on button
                if (result2D[0]["collider"].Obj is CustomArea2D area2D)
                {
                    area2D.Press();
                    return;
                }
            }
        }
    }
}

/// <summary>
/// Manually construct custom area 2d, allows easy subclassing
/// </summary>
public partial class CustomArea2D : Area2D
{
    public MeshInstance2D MeshInstance2D { get; private set; }
    public CollisionShape2D CollisionShape2D { get; private set; }
    public event Action Pressed;
    /// <summary>
    /// Constructor is used by godot internals but a Setup pattern will do nicely
    /// </summary>
    public void Setup()
    {
        Pressed += CustomArea2D_Pressed;

        MeshInstance2D = new MeshInstance2D();
        MeshInstance2D.Mesh = new QuadMesh() { Size = Vector2.One };
        MeshInstance2D.Texture = GD.Load<Texture2D>("res://icon.svg");
        AddChild(MeshInstance2D);

        CollisionShape2D = new CollisionShape2D();
        CollisionShape2D.Shape = new RectangleShape2D() { Size = Vector2.One };
        AddChild(CollisionShape2D);
    }

    public void Press()
    {
        Pressed?.Invoke();
    }

    private void CustomArea2D_Pressed()
    {
        if (MeshInstance2D.Material == null)
        {
            ShaderMaterial outline_2D_shader_material = GD.Load<ShaderMaterial>("res://OutlineArea2D/2D_outline_shader_material.tres");
            MeshInstance2D.Material = outline_2D_shader_material;
        }
        else
        {
            MeshInstance2D.Material = null;
        }
    }
}

/// <summary>
/// Input bindings for the game.
/// </summary>
public class InputBindings
{
    public const string LEFT_MOUSE_BUTTON = "left_mouse_pressed";
    public const string ESCAPE_BUTTON = "escape_button_pressed";    

    static InputBindings()
    {
        InputMap.AddAction(LEFT_MOUSE_BUTTON);
        InputMap.ActionAddEvent(LEFT_MOUSE_BUTTON, new InputEventMouseButton() { ButtonIndex = MouseButton.Left });

        InputMap.AddAction(ESCAPE_BUTTON);
        InputMap.ActionAddEvent(ESCAPE_BUTTON, new InputEventKey() { Keycode = Key.Escape });        
    }
}