using Godot;

public partial class Main : Node
{
    [Export]
    Camera3D Camera3D { get; set; }
    [Export]
    MeshInstance3D MeshInstance3D { get; set; }
    ShaderMaterial ShaderMaterial { get; set; }
    Vector3 GridPosition { get; set; }

    public override void _Ready()
    {
        ShaderMaterial = (ShaderMaterial)MeshInstance3D.GetActiveMaterial(0).NextPass.NextPass; //get the grid lines shader material
    }

    public override void _Process(double delta)
    {        
        //Update grid position in shader
        ShaderMaterial.SetShaderParameter("grid_point", GridPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
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