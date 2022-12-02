namespace Zombies;

public partial class Player : CharacterBody3D
{
	[Export] public float   MouseSensitivity         { get; set; } = 0.5f;
	[Export] public float   GravityForce             { get; set; } = 10;
	[Export] public float   JumpForce                { get; set; } = 150;
	[Export] public float   MoveSpeed                { get; set; } = 10;
	[Export] public float   MoveDampening            { get; set; } = 20; // the higher the value, the less the player will slide
	
	[Export] public Vector2 Spray { get; set; } // the guns spray offset
	[Export] public Vector3 Recoil { get; set; }
	[Export] public float ReturnSpeed { get; set; } = 1;
	
	private Camera3D        Camera                   { get; set; }
	private Camera3D        GunCam                   { get; set; }
	private RayCast3D       RayCast                  { get; set; }
	private Node3D          Gun                      { get; set; }
	private AnimationPlayer GunAnim                  { get; set; }
	private Vector3         GravityVec               { get; set; }

	private Vector3 CameraTarget { get; set; }
	private Vector3 CameraOffset { get; set; }

	public override void _Ready()
	{
		Camera  = GetNode<Camera3D>       ("Camera3D");
		GunCam  = GetNode<Camera3D>       ("Camera3D/SubViewportContainer/SubViewport/Camera3D");
		Gun     = GetNode<Node3D>         ("Camera3D/Gun");
		GunAnim = GetNode<AnimationPlayer>("Camera3D/Gun/AnimationPlayer");
		RayCast = GetNode<RayCast3D>      ("Camera3D/RayCast");
	}

	public override void _Process(double delta)
	{
		GunCam.GlobalTransform = Camera.GlobalTransform;
	}

	public override void _PhysicsProcess(double d)
	{
		var delta = (float)d;

		CameraOffset = CameraOffset.Lerp(Vector3.Zero, delta * ReturnSpeed);
		Camera.Rotation = CameraTarget + CameraOffset;

		if (Input.IsActionPressed("shoot"))
		{
			RayCast.Rotation = Vector3.Zero;
			RayCast.RotateX(Math.RandomRange(-Spray.x * delta, Spray.x * delta));
			RayCast.RotateY(Math.RandomRange(-Spray.y * delta, Spray.y * delta));

			var recoilY = Math.RandomRange(-Recoil.y * delta, Recoil.y * delta);
			var recoilZ = Math.RandomRange(-Recoil.z * delta, Recoil.z * delta);

			var recoilVec = new Vector3(Recoil.x * delta, recoilY, recoilZ);

			CameraOffset += recoilVec;
			CameraTarget += new Vector3(recoilVec.x / 2, 0, 0);

			if (RayCast.IsColliding())
			{
				// create a temporary sphere at the raycast collision point
				Geometry.CreateSphere(RayCast.GetCollisionPoint(), 0.2f, 2);
			}

			// play gun animation
			GunAnim.Play("fire");
		}

		//var h_rot = GlobalTransform.basis.GetEuler().y;
		var h_rot = Camera.Basis.GetEuler().y;

		var f_input = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
		var h_input = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");

		// normalized to prevent "fast strafing movement" by holding down 2 movement keys at the same time
		// rotated to horizontal rotation to always move in the correct direction
		var dir = new Vector3(h_input, 0, f_input).Rotated(Vector3.Up, h_rot).Normalized();

		if (IsOnFloor())
		{
			GravityVec = Vector3.Zero;

			if (Input.IsActionJustPressed("jump"))
			{
				GravityVec = Vector3.Up * JumpForce * delta;
			}
		}
		else
		{
			GravityVec += Vector3.Down * GravityForce * delta;
		}

		Velocity = Velocity.Lerp(dir * MoveSpeed, MoveDampening * delta);
		Velocity += GravityVec;

		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseMotion motion)
			return;

		if (Input.MouseMode != Input.MouseModeEnum.Captured)
			return;

		CameraTarget += new Vector3(-motion.Relative.y * MouseSensitivity, -motion.Relative.x * MouseSensitivity, 0);
	
		// prevent camera from looking too far up or down
		var rotDeg = CameraTarget;
		rotDeg.x = Mathf.Clamp(rotDeg.x, -89f.ToRadians(), 89f.ToRadians());
		CameraTarget = rotDeg;
	}
}
