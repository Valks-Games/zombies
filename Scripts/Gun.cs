namespace Zombies;

public partial class Gun : Node3D
{
	[Export] public Vector2 Spray { get; set; } // the guns spray offset
	[Export] public Vector3 Recoil { get; set; }
	[Export] public float ReturnSpeed { get; set; } = 1;
	[Export] public Vector3 RestPosition { get; set; }
	[Export] public Vector3 ADS_Position { get; set; }
	[Export] public float RestFOV_World { get; set; }
	[Export] public float ADS_FOV_World { get; set; }
	[Export] public float ADS_Acceleration { get; set; }
	[Export] public int ClipAmmo { get; set; }
	[Export] public int Clips { get; set; }
	[Export] public float FireRate { get; set; }

	private Player Player { get; set; }
	private AnimationPlayer AnimationPlayer { get; set; }
	private bool ADS_Toggle { get; set; }
	private int CurrentClipAmmo { get; set; }
	private int CurrentClips { get; set; }
	private Label3D ClipAmmoLabel { get; set; }
	private bool Reloading { get; set; }
	private Tween Tween { get; set; }
	private bool WeaponRecoilAnimationActive { get; set; }
	private Node3D MuzzleFlash { get; set; }

	public void Init(Player player)
	{
		Player = player;
	}

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		ClipAmmoLabel = GetNode<Label3D>("ClipAmmo");
		Position = RestPosition;
		CurrentClips = Clips;
		SetClipAmmo(ClipAmmo);
		MuzzleFlash = GetNode<Node3D>("MuzzleFlash");
	}

	public void Update(float delta)
	{
		Player.CameraOffset = Player.CameraOffset.Lerp(Vector3.Zero, delta * ReturnSpeed);

		Sway(delta);
		ADS(delta);

		if (Input.IsActionPressed("shoot"))
			Shoot(delta);

		if (Input.IsActionJustPressed("reload"))
			Reload();
	}

	private void ADS(float delta) 
	{
		if (Reloading)
			return;

		if (Input.IsActionPressed("ads"))
		{
			Position = Position.Lerp(ADS_Position, ADS_Acceleration * delta);
			Player.Camera.Fov = Mathf.Lerp(Player.Camera.Fov, ADS_FOV_World, ADS_Acceleration * delta);
		}
		else
		{
			Position = Position.Lerp(RestPosition, ADS_Acceleration * delta);
			Player.Camera.Fov = Mathf.Lerp(Player.Camera.Fov, RestFOV_World, ADS_Acceleration * delta);
		}
	}

	private void Sway(float delta)
	{
		Rotation = Rotation.Lerp(Vector3.Zero, delta * (Input.IsActionPressed("ads") ? 10 : 5));
		
		if (!Input.IsActionPressed("ads"))
		{
			RotateX(-Player.MouseInput.y * 0.001f);
			RotateY(-Player.MouseInput.x * 0.001f);

			var rot = Rotation;
			rot.x = Mathf.Clamp(Rotation.x, -0.3f, 0.3f);
			rot.y = Mathf.Clamp(Rotation.y, -0.3f, 0.3f);
			Rotation = rot;
		}
	}

	private void Shoot(float delta)
	{
		if (CurrentClipAmmo == 0 || WeaponRecoilAnimationActive || Reloading)
			return;

		SetClipAmmo(CurrentClipAmmo - 1);

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

		WeaponRecoilAnimationActive = true;

		var pos = Input.IsActionPressed("ads") ? ADS_Position : RestPosition;

		MuzzleFlash.Visible = true;

		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += () => MuzzleFlash.Visible = false;
			
		Tween = GetTree().CreateTween();
		Tween.TweenProperty(this, "position", pos + new Vector3(0, 0, 0.1f), 0.01f);
		Tween.TweenProperty(this, "position", pos, FireRate);
		Tween.Finished += () => WeaponRecoilAnimationActive = false;
		Tween.Play();
	}

	private void Reload()
	{
		if (Reloading)
			return;

		// do not reload if full clip
		if (CurrentClipAmmo == ClipAmmo)
			return;

		// can't reload, out of clips to insert
		if (CurrentClips == 0)
			return;

		CurrentClips -= 1;

		Reloading = true;
		AnimationPlayer.Play("reload");
	}

	private void SetClipAmmo(int v)
	{
		CurrentClipAmmo = v;
		ClipAmmoLabel.Text = $"{v}";
	}

	private void _on_animation_player_animation_finished(string animation)
	{
		switch (animation)
		{
			case "reload":
				SetClipAmmo(ClipAmmo);
				Reloading = false;
				break;
		}
	}
}
