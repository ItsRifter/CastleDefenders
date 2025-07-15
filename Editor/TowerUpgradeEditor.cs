using Editor;
using Editor.Assets;
using System;

[Inspector( typeof( TowerUpgrade ) )]
public class TowerUpgradeEditor : ControlObjectWidget
{
	// Whether or not this control supports multi-editing (if you have multiple GameObjects selected)
	public override bool SupportsMultiEdit => false;

	public TowerUpgradeEditor( SerializedProperty property ) : base( property, true )
	{
		Layout = Layout.Row();
		Layout.Spacing = 2;

		SerializedObject.TryGetProperty( nameof( TowerUpgrade.Number1 ), out var number1 );

		// Add some Controls to the Layout, both referencing their serialized properties
		Layout.Add( Create(number1) );
		Log.Info( "wtf" );
	}

	protected override void OnPaint()
	{
		// Overriding and doing nothing here will prevent the default background from being painted
	}
}
