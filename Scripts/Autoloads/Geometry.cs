namespace Zombies;

public partial class Geometry : Node
{
    private static SceneTree SceneTree { get; set; }

    public override void _Ready()
    {
        SceneTree = GetTree();
    }

    public static void CreateSphere(Vector3 pos, float radius = 0.2f, float removeDelay = -1)
    {
        MeshInstance3D sphere = new()
        {
            Mesh = new SphereMesh()
        };
        SphereMesh sphereMesh = (SphereMesh)sphere.Mesh;

        sphereMesh.Radius = radius;
        sphereMesh.Height = sphereMesh.Radius * 2;

        sphere.Position = pos;

        if (removeDelay != -1)
        {
            SceneTreeTimer timer = SceneTree.CreateTimer(removeDelay);
            timer.Timeout += sphere.QueueFree;
        }

        SceneTree.Root.AddChild(sphere);
    }
}
