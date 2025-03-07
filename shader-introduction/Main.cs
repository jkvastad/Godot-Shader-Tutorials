using Godot;

public partial class Main : Node
{
    public override void _Ready()
    {
        //PrimitiveMesh myMesh = GD.Load<PrimitiveMesh>("res://my_mesh.tres"); //load a specific mesh...
        PrimitiveMesh myMesh = new BoxMesh(); //... or just inspect a default mesh

        //Number of decimals to display, due to floating point errors values are rarely exactly 0.
        float precision = 0.001f; 
        Vector3 decimals = new(precision, precision, precision);

        //MeshDataTool initialisation
        ArrayMesh arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, myMesh.GetMeshArrays());
        MeshDataTool mdt = new MeshDataTool();
        mdt.CreateFromSurface(arrayMesh, 0);

        //Run main scene to print data related to a mesh
        for (var i = 0; i < mdt.GetVertexCount(); i++)
        {
            GD.Print($"{mdt.GetVertex(i).Snapped(decimals),-20} - {mdt.GetVertexNormal(i).Snapped(decimals)}");
        }
    }
}