using Sandbox;
using System;

public sealed class CastlePlayer : Component
{
	public int Money { get; private set; } = 0;

	GameObject previewTower;
	int currentSelection = -1;
	int lastSelection = -1;

	CameraComponent camera;

	PlayerController controller;

	protected override void OnStart()
	{
		//Delay a bit before getting the camera (while things are loading) so it isn't null
		CastleGame.AwaitAction( 0.1f, () => camera = Scene.Get<CameraComponent>() );

		controller = GetComponent<PlayerController>();
	}

	protected override void OnUpdate()
	{
		HandleInputs();
		HandlePreview();
	}

	void HandleInputs()
	{
		if( GetSlotPressed() != -1 )
			currentSelection = GetSlotPressed();

		if ( lastSelection != currentSelection )
		{
			lastSelection = currentSelection;
			
			if ( currentSelection != -1 && currentSelection != 0 )
			{
				previewTower?.Destroy();
				previewTower = null;

				GameObject newTower = GetTower();
				previewTower = newTower.Clone();

			}
			else if ( currentSelection == 0 )
			{
				previewTower?.Destroy();
				previewTower = null;
			}
			else
				previewTower = null;
		}
	}

	int GetSlotPressed()
	{
		if ( Input.Pressed( "Slot1" ) )
			return 1;

		if ( Input.Pressed( "Slot2" ) )
			return 2;

		if ( Input.Pressed( "Slot3" ) )
			return 3;

		if ( Input.Pressed( "Holster" ) )
			return 0;

		return -1;
	}

	float previewDist = 160.0f;

	float snapCooldown = 0.05f;
    float snapTimer = 0.0f;

    void HandlePreview()
    {
        if (previewTower == null) return;

        Vector3 camPos = camera.WorldPosition;
        Vector3 camForward = camPos + camera.WorldRotation.Forward * previewDist;

        var trace = Scene.Trace.Ray(camPos, camForward)

            .UseHitboxes()
            .WithoutTags("Player", "tower")
            .Run();

        previewTower.WorldPosition = trace.EndPosition;

        bool isRotating = Input.Down("SecMouse");
        controller.UseLookControls = !isRotating;

        // Tower Rotation
        if (isRotating)
        {
            float rotationSpeed = 90.0f;
            float delta = Time.Delta;
            float rotateAmount = 0.0f;

            float mouseX = Input.MouseDelta.x;
            rotateAmount = mouseX * rotationSpeed * delta;

            // Snapping rotation, kept to 15 degree increments
            if (Input.Down("SnapRotate"))
            {
                snapTimer -= delta;

                if (snapTimer <= 0.0f && MathF.Abs(mouseX) > 0.001f)
                {
                    // Accumulate rotation and snap
                    var currentYaw = previewTower.WorldRotation.Yaw();
                    var targetYaw = MathF.Round((currentYaw + rotateAmount) / 15.0f) * 15.0f;
                    previewTower.WorldRotation = Rotation.FromYaw(targetYaw);

                    snapTimer = snapCooldown;
                }
            }
            else
            {
                snapTimer = 0.0f;
                previewTower.WorldRotation *= Rotation.FromYaw(rotateAmount);
            }
        }
        else
            snapTimer = 0.0f;
    }

	void HandlePlacement()
	{

	}

	GameObject GetTower()
	{
		switch(currentSelection)
		{
			case 1:
				return CastleGame.Instance.PistolPrefab;

			case 2:
				return CastleGame.Instance.SmgPrefab;

			case 3:
				return CastleGame.Instance.ShotgunPrefab;

			default: return null;
		}
	}

	/// <summary>
	/// Adds money to the player
	/// </summary>
	/// <param name="amt">How much to add</param>
	public void AddMoney(int amt) => Money += amt;


	/// <summary>
	/// Takes money from the player
	/// </summary>
	/// <param name="amt">How much to take</param>
	public void TakeMoney(int amt)
	{
		amt = Math.Clamp(amt, 0, Money);

		//Taking nothing (player is poor)
		if ( amt == 0 ) return;

		Money -= amt;
	}

	/// <summary>
	/// Check if the player can afford to this amount
	/// </summary>
	/// <param name="amt">The amount to check</param>
	/// <returns>Player has enough money to afford</returns>
	public bool CanAfford(int amt) => Money >= amt;
}
