using Sandbox;
using System;

public sealed class CastlePlayer : Component
{
	public int Money { get; private set; } = 0;

	GameObject previewTower;
	int currentSelection = -1;
	int lastSelection = -1;

	protected override void OnUpdate()
	{
		HandleInputs();
		HandlePreview();
	}

	void HandleInputs()
	{
		if( GetSlotPressed() != -1 )
			currentSelection = GetSlotPressed();

		if( lastSelection != currentSelection )
		{
			lastSelection = currentSelection;
			
			if ( currentSelection != -1 )
			{
				previewTower = GetTower();
			}
			else
				previewTower = null;
		}

		if ( Input.Down( "SecMouse" ) && previewTower != null )
		{
			
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

		return -1;
	}

	void HandlePreview()
	{

	}

	void HandlePlacement()
	{

	}

	GameObject GetTower()
	{
		switch(currentSelection)
		{
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
