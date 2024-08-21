namespace Facepunch;

public sealed class PartyPodium : Component
{
	[Property] public List<PartyMember> Podiums { get; set; }
	[Property] public Model EditorModel { get; set; }

	protected override void DrawGizmos()
	{
		Gizmo.Transform = new();

		foreach ( var podium in Podiums )
		{
			if ( !podium.Renderer.IsValid() )
			{
				var mdl = Gizmo.Draw.Model( EditorModel, podium.Transform.World );
				mdl.ColorTint = Color.White.WithAlpha( 0.75f );
			}

			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineCircle( podium.Transform.Position, Vector3.Up, 16 );
		}
	}

	int lastCount = 0;

	private void Default()
	{
		// How many players should show if we're not in a party room?
		int desiredCount = 1;

		if ( lastCount != desiredCount )
		{
			for ( int i = 0; i < desiredCount; i++ )
			{
				var podium = Podiums[i];
				podium.Renderer.Enabled = true;
				podium.Friend = new Friend( Game.SteamId );
			}

			lastCount = desiredCount;
		}
	}

	private void Party( PartyRoom room )
	{
		if ( lastCount != room.Members.Count() )
		{
			// Update
			lastCount = room.Members.Count();

			// Clear off any podiums
			foreach ( var podium in Podiums )
			{
				podium.Friend = null;
			}

			// Re-populate the podiums
			var i = 0;
			foreach ( var member in room.Members.Take( Podiums.Count ) )
			{
				var podium = Podiums[i];
				i++;

				podium.Friend = member;
			}
		}
		else
		{
			return;
		}
	}

	protected override void OnUpdate()
	{
		var party = PartyRoom.Current;

		if ( party is null )
		{
			Default();
		}
		else
		{
			Party( party );
		}
	}

	private static async void Create()
	{
		var party = await PartyRoom.Create( 4, "My Party", true );
	}

	[ConCmd( "party_create" )]
	public static void PartyCreate()
	{
		Create();
	}

	[ConCmd( "party_disband" )]
	public static void PartyDisband()
	{
		if ( PartyRoom.Current is not null )
		{
			PartyRoom.Current.Leave();
		}
	}
}
