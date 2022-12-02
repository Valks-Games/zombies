namespace Zombies;

public partial class Gun : Node3D
{
	[Export] public Vector2 Spray { get; set; } // the guns spray offset
	[Export] public Vector3 Recoil { get; set; }
	[Export] public float ReturnSpeed { get; set; } = 1;
	[Export] public Vector3 RestPosition { get; set; }
	[Export] public Vector3 ADS_Position { get; set; }
	[Export] public float RestFOV { get; set; }
	[Export] public float ADS_FOV { get; set; }
	[Export] public float ADS_Acceleration { get; set; }

	private Player Player { get; set; }
	private AnimationPlayer AnimationPlayer { get; set; }
	private bool ADS_Toggle { get; set; }

	public void Init(Player player)
	{
		Player = player;
	}

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		Position = RestPosition;
	}

	public void Update(float delta)
	{
		Player.CameraOffset = Player.CameraOffset.Lerp(Vector3.Zero, delta * ReturnSpeed);

		if (Input.IsActionPressed("shoot"))
			Shoot(delta);

		if (Input.IsActionPressed("ads"))
		{
			Position = Position.Lerp(ADS_Position, ADS_Acceleration * delta);
			Player.Camera.Fov = Mathf.Lerp(Player.Camera.Fov, ADS_FOV, ADS_Acceleration * delta);
		}
		else
		{
			Position = Position.Lerp(RestPosition, ADS_Acceleration * delta);
			Player.Camera.Fov = Mathf.Lerp(Player.Camera.Fov, RestFOV, ADS_Acceleration * delta);
		}
	}

	private void Shoot(float delta)
	{
		Player.RayCast.Rotation = Vector3.Zero;
		Player.RayCast.RotateX(Math.RandomRange(-Spray.x * delta, Spray.x * delta));
		Player.RayCast.RotateY(Math.RandomRange(-Spray.y * delta, Spray.y * delta));

		var recoilY = Math.RandomRange(-Recoil.y * delta, Recoil.y * delta);
		var recoilZ = Math.RandomRange(-Recoil.z * delta, Recoil.z * delta);

		var recoilVec = new Vector3(Recoil.x * delta, recoilY, recoilZ);

		Player.CameraOffset += recoilVec;
		//Player.CameraTarget += new Vector3(recoilVec.x / 2, 0, 0); // Only return halfway

		if (Player.RayCast.IsColliding())
		{
			// create a temporary sphere at the raycast collision point
			Geometry.CreateSphere(Player.RayCast.GetCollisionPoint(), 0.2f, 2);
		}

		// play gun animation
		AnimationPlayer.Play("fire");
	}
}
