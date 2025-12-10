using Godot;

public partial class Main : Node
{
    [Export]
    Camera3D Camera3D { get; set; }
    [Export]
    MeshInstance3D MeshInstance3D { get; set; }
    ShaderMaterial ShaderMaterial { get; set; }
    Vector3 GridPosition { get; set; }

    public float MoveSpeed { get; set; } = 6.0f;
    public float MouseSensitivity { get; set; } = 0.0035f;

    // internal state for camera rotation
    float _yaw = 0f;
    float _pitch = 0f;

    public override void _Ready()
    {
        //get the grid lines shader material
        ShaderMaterial = (ShaderMaterial)MeshInstance3D.GetActiveMaterial(0).NextPass.NextPass;

        // initialize yaw/pitch from camera current rotation
        var camRot = Camera3D.Rotation;
        _pitch = camRot.X;
        _yaw = camRot.Y;

        // start with visible cursor; right click will capture (like the Godot editor)
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _Input(InputEvent @event)
    {
        // Handle right mouse press/release to capture/release the cursor (as in editor)
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right)
        {
            if (mb.Pressed)
                Input.MouseMode = Input.MouseModeEnum.Captured;
            else
                // Release cursor when right mouse button is released
                Input.MouseMode = Input.MouseModeEnum.Visible;

        }

        // Mouse look when captured
        if (@event is InputEventMouseMotion mm && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // mm.Relative gives mouse movement since last event
            var rel = mm.Relative;
            _yaw -= rel.X * MouseSensitivity;
            _pitch -= rel.Y * MouseSensitivity;

            // clamp pitch to avoid flipping
            _pitch = Mathf.Clamp(_pitch, -Mathf.Pi / 2f + 0.01f, Mathf.Pi / 2f - 0.01f);

            Camera3D.Rotation = new Vector3(_pitch, _yaw, 0f);
        }
    }

    public override void _Process(double delta)
    {
        //Update grid position in shader
        ShaderMaterial.SetShaderParameter("grid_point", GridPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Movement (W/A/S/D for horizontal movement, Q/E for down/up)        
        var basis = Camera3D.GlobalTransform.Basis;
        Vector3 dir = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W)) dir += -basis.Z; // forward
        if (Input.IsKeyPressed(Key.S)) dir += basis.Z;  // back
        if (Input.IsKeyPressed(Key.A)) dir += -basis.X; // left
        if (Input.IsKeyPressed(Key.D)) dir += basis.X;  // right
        if (Input.IsKeyPressed(Key.Q)) dir += -basis.Y; // down
        if (Input.IsKeyPressed(Key.E)) dir += basis.Y;  // up

        if (dir != Vector3.Zero)
        {
            dir = dir.Normalized();
            float speed = MoveSpeed;

            Camera3D.GlobalPosition += dir * speed * (float)delta;
        }


        // Raycast for highlight if mouse not captured
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            GridPosition = Vector3I.Zero;
            return;
        }

        // Get screen mouse position
        var mousePosition = GetViewport().GetMousePosition();

        // Cast ray from mouse position to camera far plane as described in docs:
        // https://docs.godotengine.org/en/4.0/tutorials/physics/ray-casting.html#d-ray-casting-from-screen        
        var from = Camera3D.ProjectRayOrigin(mousePosition);
        var to = from + Camera3D.ProjectRayNormal(mousePosition) * Camera3D.Far;

        // Hit an area in 3D space
        // - we only have one plane so no need to perform multiple ray casts and filter out occluding hits
        var spaceState = Camera3D.GetWorld3D().DirectSpaceState;
        var query = new PhysicsRayQueryParameters3D() { From = from, To = to, CollideWithAreas = true };
        var result = spaceState.IntersectRay(query);

        // note that "from" is the camera position in 3D, while "to" is a point on the far plane
        GD.Print($"{mousePosition},{from},{to}");

        if (result.ContainsKey("position"))
        {
            GD.Print($"{result["position"]}");
            // Per IntersectRay docs, "position" is the intersection point in 3D space
            GridPosition = (Vector3)result["position"];
        }
    }
}