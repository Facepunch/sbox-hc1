using Sandbox.Network;

namespace Facepunch;

/// <summary>
/// This purely exists as a wrappe for a real lobby, so we can display fake lobbies in the UI.
/// </summary>
public partial struct Lobby
{
	/// <summary>
	/// A reference to our internal lobby type.
	/// </summary>
	public LobbyInformation? _lobby;

	public ulong? FakeId = null;

	public string Name => _lobby?.Name ?? "Tony's game";
	public int Members => _lobby?.Members ?? 8;
	public int MaxMembers => _lobby?.MaxMembers ?? 8;
	public ulong OwnerId => _lobby?.OwnerId ?? 0;
	public ulong LobbyId => FakeId.HasValue ? FakeId.Value : _lobby?.LobbyId ?? 0;
	public bool IsFull => _lobby?.IsFull ?? (Members >= MaxMembers);
	public bool IsEditorLobby => _lobby?.IsEditorLobby() ?? false;

	public string Map
	{
		get
		{
			if ( _lobby?.Data?.TryGetValue( "hc1-map", out string map ) ?? false )
			{
				return map;
			}

			return _lobby?.Map ?? "Unknown";
		}
	}

	public string GameMode
	{
		get
		{
			if ( _lobby?.Data?.TryGetValue( "hc1-gm", out string gm ) ?? false )
			{
				return gm;
			}

			return "Unknown";
		}
	}

	public Lobby( LobbyInformation lobby )
	{
		_lobby = lobby;
	}

	public Lobby() { }
}


public static class LobbyInformationExtensions
{
	public static bool IsEditorLobby( this LobbyInformation lobby )
	{
		if ( lobby.Data?.TryGetValue( "dev", out string dev ) ?? false )
		{
			return Convert.ToInt16( dev ) == 1;
		}

		return false;
	}
}
