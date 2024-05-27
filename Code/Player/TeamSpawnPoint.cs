using Facepunch;

public sealed class TeamSpawnPoint : Component
{
	[Property]
	public Team Team { get; set; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		var spawnpointModel = Model.Load( "models/editor/spawnpoint.vmdl" );

		Gizmo.Hitbox.Model( spawnpointModel );
		Gizmo.Draw.Color = (Team switch
		{
			Team.Terrorist => Color.Red,
			Team.CounterTerrorist => Color.Blue,
			_ => Color.White
		}).WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.7f : 0.5f );
		var so = Gizmo.Draw.Model( spawnpointModel );
		if (so is not null)
		{
			so.Flags.CastShadows = true;
		}
	}
}
