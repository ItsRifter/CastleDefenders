using Sandbox;

public sealed class PathNode : Component
{
	[Property, Description( "The next target node for NPCs to follow" )] public PathNode NextNode { get; set; }
	[Property, Title("Starting node"), Description( "The starting node where npcs will spawn, ONLY ONE MUST EXIST" )] public bool IsStartNode { get; set; }
	[Property, Title("Final node"), Description("The final node in the list of paths, ONLY ONE MUST EXIST")] public bool IsEndNode { get; set; }
	[Property, Title("Can teleport"), Description( "Makes NPCs who enter this node teleport to the next" )] public bool IsTeleporter { get; set; }

	protected override void DrawGizmos()
	{
		Color baseColour = Color.White;
		Color lineColour = Color.White;

		if (IsStartNode && !IsEndNode)
			baseColour = Color.Green;

		if(IsEndNode && !IsStartNode)
			baseColour = Color.Red;

		if ( IsTeleporter )
		{
			baseColour = Color.Orange;
			lineColour = Color.Orange;
		}

		DebugOverlay.Box( BBox.FromPositionAndSize( WorldPosition + Vector3.Up * 4, 8.0f ), baseColour );

		if( NextNode != null )
		{
			Vector3 offset = Vector3.Up * 4;

			Vector3 pathA = GameObject.WorldPosition + offset;
			Vector3 pathB = NextNode.GameObject.WorldPosition + offset;

			DebugOverlay.Line( pathA, pathB, lineColour );
		}
	}
}
