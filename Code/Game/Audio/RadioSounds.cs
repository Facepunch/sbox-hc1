using Sandbox.Audio;

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
	/// <summary>
	/// When someone dies, and there's 2 people left.
	/// </summary>
	TwoEnemiesLeft,
	/// <summary>
	/// When someone dies, and there's 1 person left.
	/// </summary>
	OneEnemyLeft,
	/// <summary>
	/// When a teammate dies.
	/// </summary>
	TeammateDies
}

public class TeamRadioCategory
{
	[KeyProperty] public string Category { get; set; }
	[Property] public bool IsHidden { get; set; }

	public List<TeamRadioEntry> Entries { get; set; }

	public class TeamRadioEntry
	{
		[KeyProperty] public string Name { get; set; }
		[KeyProperty] public SoundEvent Sound { get; set; }
		[Property] public bool IsHidden { get; set; }
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

	/// <summary>
	/// Locally controlled, spam control.
	/// </summary>
	private static TimeSince TimeSinceRadio;

	/// <summary>
	/// How frequently can we spam the radio?
	/// </summary>
	private const float RadioSpamDelay = 2f;
	
	/// <summary>
	/// Send a radio sound to all players on the specified team
	/// </summary>
	/// <param name="team"></param>
	/// <param name="category"></param>
	/// <param name="sound"></param>
	public static void Play( Team team, string category, string sound )
	{
		if ( GameUtils.LocalPlayer.HealthComponent.State != LifeState.Alive )
			return;

		// Only send this message to members of the specified team
		using ( Rpc.FilterInclude( GameUtils.GetPlayers( team ).Select( x => x.Network.OwnerConnection ) ) )
		{
			Chat.Instance?.AddText( sound, Chat.ChatModes.Team, "radio" );
			RpcPlay( team, category, sound );
		}
	}

	[ConCmd( "radio_sound" )]
	public static void CmdPlay( string category, string sound )
	{
		if ( TimeSinceRadio < RadioSpamDelay && TimeSinceRadio > 0f )
			return;
		
		TimeSinceRadio = 0f;

		var team = GameUtils.LocalPlayer.GameObject.GetTeam();
		Play( team, category, sound );
	}

	[Broadcast]
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

		// Play the sound
		var snd = Sound.Play( entry.Sound );
		snd.TargetMixer = Mixer.FindMixerByName( "Radio" );
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
			var s = Sound.Play( sndEvent );
			s.TargetMixer = Mixer.FindMixerByName( "Radio" );
		}
		else
		{
			Log.Warning( $"No sound found for {snd}" );
		}
	}

	protected override void PostLoad()
	{
		if ( All.Add( this ) ) return;
		Log.Warning( "Tried to add two of the same radio sounds (?)" );
	}
}
