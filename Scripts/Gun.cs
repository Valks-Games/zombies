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
    private bool ADS_Toggle { get; set; }
    private int CurrentClipAmmo { get; set; }
    private int CurrentClips { get; set; }
    private Label3D ClipAmmoLabel { get; set; }
    private bool Reloading { get; set; }
    private Tween TweenKickback { get; set; }
    private Tween TweenReload { get; set; }
    private bool WeaponKickbackAnimationActive { get; set; }
    private Node3D MuzzleFlash { get; set; }
    private bool NearCollider { get; set; }
    private Godot.Timer TimerSwayClampDisabled { get; set; }

    public void Init(Player player)
    {
        Player = player;
    }

    public override void _Ready()
    {
        ClipAmmoLabel = GetNode<Label3D>("ClipAmmo");
        Position = RestPosition;
        CurrentClips = Clips;
        SetClipAmmo(ClipAmmo);
        MuzzleFlash = GetNode<Node3D>("MuzzleFlash");
        TimerSwayClampDisabled = new Godot.Timer();
        TimerSwayClampDisabled.WaitTime = 1; // 1 second
        AddChild(TimerSwayClampDisabled);
    }

    public void Update(float delta)
    {
        Player.CameraOffset = Player.CameraOffset.Lerp(Vector3.Zero, delta * ReturnSpeed);

        Sway(delta);
        ADS(delta);
        LiftWepNearCollider(delta);

        if (Input.IsActionPressed("shoot"))
            Shoot(delta);

        if (Input.IsActionJustPressed("reload"))
            Reload();
    }

    private void LiftWepNearCollider(float delta)
    {
        if (Player.RayCast.IsColliding() &&
            Player.RayCast.GetCollisionPoint().DistanceTo(Player.Position) < 3)
        {
            NearCollider = true;
            TimerSwayClampDisabled.Start();
            Rotation = Rotation.Lerp(new Vector3(1.2f, Rotation.Y, Rotation.Z), delta * 4);
        }
        else
        {
            NearCollider = false;
        }
    }

    private void ADS(float delta)
    {
        if (Reloading)
            return;

        if (Input.IsActionPressed("ads") && !NearCollider)
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
        if (Reloading || NearCollider)
            return;

        Rotation = Rotation.Lerp(Vector3.Zero, delta * (Input.IsActionPressed("ads") ? 10 : 5));

        // Weapon sway in ADS is very distracting so disable it in ADS
        if (!Input.IsActionPressed("ads"))
        {
            RotateX(-Player.MouseInput.Y * 0.001f);
            RotateY(-Player.MouseInput.X * 0.001f);

            if (TimerSwayClampDisabled.TimeLeft == 0)
            {
                var rot = Rotation;
                rot.X = Mathf.Clamp(Rotation.X, -0.3f, 0.3f);
                rot.Y = Mathf.Clamp(Rotation.Y, -0.3f, 0.3f);
                Rotation = rot;
            }
        }
    }

    private void Shoot(float delta)
    {
        if (CurrentClipAmmo == 0 || WeaponKickbackAnimationActive || Reloading || NearCollider)
            return;

        SetClipAmmo(CurrentClipAmmo - 1);
        WepSpray(delta);
        WepRecoil(delta);
        ActivateMuzzleFlash();
        Kickback();

        if (Player.RayCast.IsColliding())
        {
            // create a temporary sphere at the raycast collision point
            Geometry.CreateSphere(Player.RayCast.GetCollisionPoint(), 0.2f, 2);
        }
    }

    private void WepSpray(float delta)
    {
        Player.RayCast.Rotation = Vector3.Zero;
        Player.RayCast.RotateX(Math.RandomRange(-Spray.X * delta, Spray.X * delta));
        Player.RayCast.RotateY(Math.RandomRange(-Spray.Y * delta, Spray.Y * delta));
    }

    private void WepRecoil(float delta)
    {
        var recoilY = Math.RandomRange(-Recoil.Y * delta, Recoil.Y * delta);
        var recoilZ = Math.RandomRange(-Recoil.Z * delta, Recoil.Z * delta);

        var recoilVec = new Vector3(Recoil.X * delta, recoilY, recoilZ);

        Player.CameraOffset += recoilVec;
        //Player.CameraTarget += new Vector3(recoilVec.x / 2, 0, 0); // Only return halfway
    }

    private void Kickback()
    {
        WeaponKickbackAnimationActive = true;

        var pos = Input.IsActionPressed("ads") ? ADS_Position : RestPosition;

        TweenKickback = GetTree().CreateTween();
        TweenKickback.TweenProperty(this, "position", pos + new Vector3(0, 0, 0.1f), 0.01f);
        TweenKickback.TweenProperty(this, "position", pos, FireRate);
        TweenKickback.Finished += () => WeaponKickbackAnimationActive = false;
        TweenKickback.Play();
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

        TweenReload = GetTree().CreateTween();
        TweenReload.TweenProperty(this, "rotation:x", Mathf.Pi * 2, 0.6f);
        TweenReload.Finished += () =>
        {
            Rotation = new Vector3(0, Rotation.Y, Rotation.Z); // reset rotation
            SetClipAmmo(ClipAmmo);
            Reloading = false;
        };
    }

    private void ActivateMuzzleFlash()
    {
        MuzzleFlash.Visible = true;

        var timer = GetTree().CreateTimer(0.1f);
        timer.Timeout += () => MuzzleFlash.Visible = false;
    }

    private void SetClipAmmo(int v)
    {
        CurrentClipAmmo = v;
        ClipAmmoLabel.Text = $"{v}";
    }
}
