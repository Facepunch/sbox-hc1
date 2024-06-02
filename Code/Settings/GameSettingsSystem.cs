using Sandbox.Audio;

namespace Facepunch;

public class GameSettings
{
	[Title( "Field Of View" ), Description( "Effects the camera's vision." ), Group( "Game" ), Icon( "grid_view" ), Range( 65, 110, 1 )]
	public float FieldOfView { get; set; } = 85;

	[Title( "Master" ), Description( "The overall volume" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float MasterVolume { get; set; } = 100;

	[Title( "Music" ), Description( "How loud any music will play" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float MusicVolume { get; set; } = 100;

	[Title( "SFX" ), Description( "Most effects in the game" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float SFXVolume { get; set; } = 100;

	[Title( "UI" ), Description( "interface sounds" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float UIVolume { get; set; } = 100;

	[Title( "Radio" ), Description( "" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float RadioVolume { get; set; } = 100;
}

public partial class GameSettingsSystem
{
	private static GameSettings current { get; set; }
	public static GameSettings Current
	{
		get
		{
			if ( current is null ) Load();
			return current;
		}
		set
		{
			current = value;
		}
	}

	public static string FilePath => "gamesettings.json";

	public static void Save()
	{
		Mixer.Master.Volume = Current.MasterVolume / 100;
		var channel = Mixer.Master.GetChildren();
		channel[0].Volume = Current.MusicVolume / 100;
		channel[1].Volume = Current.SFXVolume / 100;
		channel[2].Volume = Current.UIVolume / 100;
		channel[3].Volume = Current.RadioVolume / 100;

		FileSystem.Data.WriteJson<GameSettings>( FilePath, Current );
	}

	public static void Load()
	{
		Current = FileSystem.Data.ReadJson<GameSettings>( FilePath, new() );
	}
}
