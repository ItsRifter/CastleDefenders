using Sandbox;
using Sandbox.ModelEditor.Nodes;
using System;

public sealed class CastlePlayer : Component
{
	public int Money { get; private set; }

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

		Money = 50;
	}

	protected override void OnUpdate()
	{
		HandleInputs();
		HandlePreview();
	}

	void HandleInputs()
	{
		//Tower selection
		HandleSelections();

		//Tower actions
		if ( Input.Pressed( "PrimMouse" ) )
			TryPlacement();

		if (Input.Pressed("Sell"))
			TrySell();
	}

	void HandleSelections()
	{
		if ( GetSlotPressed() != -1 )
			currentSelection = GetSlotPressed();

		if ( lastSelection != currentSelection )
		{
			lastSelection = currentSelection;

			if ( currentSelection != -1 && currentSelection != 0 )
			{
				Rotation lastRot = previewTower?.WorldRotation ?? Rotation.Identity;

				previewTower?.Destroy();
				previewTower = null;

				GameObject newTower = GetTower();
				previewTower = newTower.Clone();

				previewTower.GetComponent<CastleTower>().Enabled = false;
				previewTower.WorldRotation = lastRot;

				previewTower.Tags.Add( "Preview" );
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

	void TrySell()
	{
		if ( currentSelection != 0 ) return;

		var trace = DoTrace( "player" );

		if ( trace.Hit && trace.GameObject.GetComponent<CastleTower>() != null )
		{
			var tower = trace.GameObject.GetComponent<CastleTower>();

			if ( tower.Owner != null && tower.Owner != this ) return;

			int sellPrice = (int)(tower.Statistics.Cost / 1.75f * tower.Level);

			if ( tower.Owner == null )
				AddMoney( sellPrice );
			else
				tower.Owner.AddMoney( sellPrice );

			tower.GameObject.Destroy();
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

	float snapCooldown = 0.05f;
    float snapTimer = 0.0f;
	float snapAngle = 15.0f;

    void HandlePreview()
    {
        if (previewTower == null) return;

		var trace = DoTrace( "player", "tower" );

		previewTower.WorldPosition = trace.EndPosition;

		#region Rotation
		bool isRotating = Input.Down("SecMouse");
        controller.UseLookControls = !isRotating;

        // Tower Rotation
        if (isRotating)
        {
            float rotationSpeed = 250.0f;
            float delta = Time.Delta;
            float rotateAmount = 0.0f;

            float mouseX = Input.MouseDelta.x;
            rotateAmount = mouseX * rotationSpeed * delta;

            // Snapping rotation, kept to 15 degree increments
            if (Input.Down("SnapRotate"))
            {
                snapTimer -= delta;

                if (snapTimer <= 0.0f)
                {
                    var currentYaw = previewTower.WorldRotation.Yaw();
                    var targetYaw = MathF.Round( (currentYaw + rotateAmount) / snapAngle ) * snapAngle;
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
		#endregion

		#region Valid Placements
		Color validColor = ValidPlacement() ? Color.Green : Color.Red;
		validColor = validColor.WithAlpha(0.5f);

		previewTower.GetComponent<ModelRenderer>().Tint = validColor;
		#endregion
	}

	void TryPlacement()
	{
		if(previewTower == null || !ValidPlacement()) return;

		int cost = previewTower.GetComponent<TowerStats>().Cost;

		if ( !CanAfford( cost ) ) return;

		var tower = GetTower().Clone();

		tower.WorldPosition = previewTower.WorldPosition;
		tower.WorldRotation = previewTower.WorldRotation;

		tower.GetComponent<CastleTower>().SetOwner( this );

		TakeMoney( cost );
	}

	bool ValidPlacement()
	{
		if ( previewTower == null ) return false;

		var trace = DoTrace( "player", "tower" );

		//Non-flat surface
		if ( trace.Normal != Vector3.Up ) return false;

		if (trace.Hit)
		{
			GameObject hitObject = trace.GameObject;

			if ( hitObject.Tags.Has( "noPlace" ) ) return false;

			var sphereTrace = Scene.Trace.Sphere( 16.0f, trace.EndPosition, trace.EndPosition )
				.WithoutTags( "Player", "Preview" )
				.RunAll();

			//At least one other tower is too close for placement
			if ( sphereTrace.Count() >= 2 ) return false;
		}

		return true;
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

	float previewDist = 128.0f;

	SceneTraceResult DoTrace( params string[] ignoreTags )
	{
		Vector3 camPos = camera.WorldPosition;
		Vector3 camForward = camPos + camera.WorldRotation.Forward * previewDist;

		var trace = Scene.Trace.Ray( camPos, camForward )
			.UseHitboxes()
			.WithoutTags( ignoreTags )
			.Run();

		return trace;
	}

	#region Money
	/// <summary>
	/// Adds money to the player
	/// </summary>
	/// <param name="amt">How much to add</param>
	public void AddMoney(int amt)
	{
		Money += amt;
		Log.Info( Money );
	}

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
		Log.Info( Money );
	}

	/// <summary>
	/// Check if the player can afford to this amount
	/// </summary>
	/// <param name="amt">The amount to check</param>
	/// <returns>Player has enough money to afford</returns>
	public bool CanAfford(int amt) => Money >= amt;
	#endregion
}
