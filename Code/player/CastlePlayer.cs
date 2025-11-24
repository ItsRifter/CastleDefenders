using Sandbox;
using Sandbox.ModelEditor.Nodes;
using System;
using System.Threading;

public sealed class CastlePlayer : Component
{
	public int Money { get; private set; }

	GameObject previewTower;
	int currentSelection = -1;
	int lastSelection = -1;

	CameraComponent camera;

	PlayerController controller;

	bool topdownMode;

	protected override void OnStart()
	{
		//Delay a bit before getting the camera (while things are loading) so it isn't null
		CastleGame.AwaitAction( 0.1f, () => camera = Scene.Get<CameraComponent>() );

		controller = GetComponent<PlayerController>();

		Money = 50;
		topdownMode = false;
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

		//if(Input.Pressed("View"))
		//	ChangeCameraMode();

		//if(topdownMode)
		//	HandleTopDownControls();
	}

	void ChangeCameraMode()
	{
		topdownMode = !topdownMode;
		if ( topdownMode )
		{
			controller.Enabled = false;

			camera.WorldPosition = controller.WorldPosition + new Vector3( 0, 0, 450 );
			camera.WorldRotation = controller.WorldRotation * Rotation.FromPitch( 75 );

			Mouse.Visibility = MouseVisibility.Visible;
		}
		else
		{
			controller.Enabled = true;

			camera.WorldPosition = Vector3.Zero;
			camera.WorldRotation = Rotation.Identity;

			Mouse.Visibility = MouseVisibility.Auto;
		}
	}

	float moveTopdDownSpeed = 500.0f;

	void HandleTopDownControls()
	{
		Vector3 moveDir = Vector3.Zero;
		
		if ( Input.Down( "Forward" ) )
			moveDir += Vector3.Forward;
		
		if ( Input.Down( "Backward" ) )
			moveDir += Vector3.Backward;
		
		if ( Input.Down( "Left" ) )
			moveDir += Vector3.Left;
		
		if ( Input.Down( "Right" ) )
			moveDir += Vector3.Right;

		moveDir = camera.WorldRotation * moveDir;
		moveDir.z = 0;
		moveDir = moveDir.Normal * moveTopdDownSpeed * Time.Delta;
		camera.WorldPosition += moveDir;
	}

	void HandleSelections()
	{
		int lastSlot = GetSlotPressed();

		if ( lastSlot != -1 )
			currentSelection = lastSlot;

		if ( lastSelection != currentSelection )
		{
			lastSelection = currentSelection;

			if ( currentSelection != -1 && currentSelection != 0 )
			{
				Rotation lastRot = previewTower?.WorldRotation ?? Rotation.Identity;

				previewTower?.Destroy();
				previewTower = null;

				GameObject newTower = GetTower();
				if( newTower == null )
				{
					Log.Error( "Missing tower prefab for selection index: " + currentSelection );
					return;
				}

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

		if ( Input.Pressed( "Slot4" ) )
			return 4;

		if ( Input.Pressed( "Slot5" ) )
			return 5;

		if ( Input.Pressed( "Slot6" ) )
			return 6;

		if ( Input.Pressed( "Holster" ) )
			return 0;

		return -1;
	}

	float snapGrid = 4.0f;
	float snapCooldown = 0.05f;
    float snapTimer = 0.0f;
	float snapAngle = 45.0f;

	void HandlePreview()
    {
        if (previewTower == null) return;

		Vector3 endPos = Vector3.Zero;

		if ( !topdownMode )
			endPos = DoTrace( "player", "tower" ).EndPosition;
		else
			endPos = DoTraceMouse( "player", "tower" ).EndPosition;

		#region Position
		previewTower.WorldPosition = new Vector3(
			MathF.Round( endPos.x / snapGrid ) * snapGrid,
			MathF.Round( endPos.y / snapGrid ) * snapGrid,
			endPos.z
		);
		#endregion

		#region Rotation
		float scroll = Input.MouseWheel.y;
		
		if(scroll != 0.0f)
		{
			float rotationAmount = scroll * snapAngle;
			snapTimer -= Time.Delta;

			if ( snapTimer <= 0.0f )
			{
				var currentYaw = previewTower.WorldRotation.Yaw();
				var targetYaw = MathF.Round( (currentYaw + rotationAmount) / snapAngle ) * snapAngle;
				previewTower.WorldRotation = Rotation.FromYaw( targetYaw );
				snapTimer = snapCooldown;
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
		if(previewTower is null || !ValidPlacement()) return;

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

			case 4:
				return CastleGame.Instance.CannonPrefab;

			case 5:
				return CastleGame.Instance.ElectricPrefab;

			case 6:
				return CastleGame.Instance.RadarPrefab;

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

	SceneTraceResult DoTraceMouse(params string[] ignoreTags)
	{
		Vector3 camPos = camera.WorldPosition;
		Vector3 camForward = camPos + camera.WorldRotation.Forward * 999;
		Vector3 mouseWorld = camera.ScreenToWorld( Mouse.Position );

		camPos += (mouseWorld - camPos).Normal.WithZ(0) * 500.0f;

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
	}

	/// <summary>
	/// Check if the player can afford to this amount
	/// </summary>
	/// <param name="amt">The amount to check</param>
	/// <returns>Player has enough money to afford</returns>
	public bool CanAfford(int amt) => Money >= amt;
	#endregion
}
