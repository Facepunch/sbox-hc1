namespace Facepunch;

public enum RadioSound
{
	/// <summary>
	/// When the round starts.
	/// </summary>
	RoundStarted,
	/// <summary>
	/// When the team has won.
	/// </summary>
	RoundWon,
	/// <summary>
	/// When the team has lost.
	/// </summary>
	RoundLost,
	/// <summary>
	/// When the bomb has been planted.
	/// </summary>
	BombPlanted,
	/// <summary>
	/// When the bomb has been defused.
	/// </summary>
	BombDefused,
	/// <summary>
	/// When someone has thrown a grenade.
	/// </summary>
	ThrownGrenade,
}

public class TeamRadioCategory
{
	[KeyProperty] public string Category { get; set; }

	public List<TeamRadioEntry> Entries { get; set; }

	public class TeamRadioEntry
	{
		[KeyProperty] public string Name { get; set; }
		[KeyProperty] public SoundEvent Sound { get; set; }
	}
}

[GameResource( "Radio sounds", "radio", "" )]
public class RadioSounds : GameResource
{
	public static HashSet<RadioSounds> All { get; set; } = new();

	[Property, Group( "Setup" )] public Team Team { get; set; }

	[Property, Group( "Data" )] public Dictionary<RadioSound, SoundEvent> Sounds { get; set; }

	[Property, Group( "Data" )] public List<TeamRadioCategory> TeamSounds { get; set; }

	/// <summary>
	/// Called on the host to broadcast radio sounds to people
	/// </summary>
	/// <param name="team"></param>
	/// <param name="snd"></param>
	public static void Play( Team team, RadioSound snd )
	{
		if ( !Networking.IsHost ) return;

		// Only send this message to members of the specified team
		using ( Rpc.FilterInclude( GameUtils.GetPlayers( team ).Select( x => x.Network.OwnerConnection ) ) )
		{
			RpcPlay( team, snd );
		}
	}

	[Broadcast]
	public static void Play( Team team, string category, string sound )
	{
		Chat.Instance?.AddText( sound, Chat.ChatModes.Team, "radio" );

		// Only send this message to members of the specified team
		using ( Rpc.FilterInclude( GameUtils.GetPlayers( team ).Select( x => x.Network.OwnerConnection ) ) )
		{
			RpcPlay( team, category, sound );
		}
	}

	[ConCmd( "radio_sound" )]
	public static void CmdPlay( string category, string sound )
	{
		var team = GameUtils.LocalPlayer.GameObject.GetTeam();
		Play( team, category, sound );
	}

	private static void RpcPlay( Team team, string category, string sound )
	{
		var localPlayerTeam = GameUtils.LocalPlayer.GameObject.GetTeam();

		var soundList = All.FirstOrDefault( x => x.Team == localPlayerTeam );
		if ( soundList is null )
			return;

		// Multi-client testing
		if ( team != localPlayerTeam )
			return;

		var categorySounds = soundList.TeamSounds.FirstOrDefault( x => x.Category == category );
		if ( categorySounds is null )
			return;

		var entry = categorySounds.Entries.FirstOrDefault( x => x.Name == sound );
		if ( entry is null )
			return;

		// play the sound
		Sound.Play( entry.Sound );
	}

	[Broadcast]
	private static void RpcPlay( Team team, RadioSound snd )
	{
		var localPlayerTeam = GameUtils.LocalPlayer.GameObject.GetTeam();

		var soundList = All.FirstOrDefault( x => x.Team == localPlayerTeam );
		if ( soundList is null )
			return;

		// Multi-client testing
		if ( team != localPlayerTeam )
			return;

		if ( soundList.Sounds.TryGetValue( snd, out var sndEvent ) )
		{
			// play the sound
			Sound.Play( sndEvent );
		}
		else
		{
			Log.Warning( $"No sound found for {snd}" );
		}
	}

	protected override void PostLoad()
	{
		if ( All.Contains( this ) )
		{
			Log.Warning( "Tried to add two of the same radio sounds (?)" );
			return;
		}

		All.Add( this );
	}
}
