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

	public string Name => _lobby?.Name ?? "My cool lobby";
	public int Members => _lobby?.Members ?? 8;
	public int MaxMembers => _lobby?.MaxMembers ?? 8;
	public string Map => _lobby?.Map;
	public ulong OwnerId => _lobby?.OwnerId ?? 0;
	public ulong LobbyId => _lobby?.LobbyId ?? 0;

	public Lobby( LobbyInformation lobby )
	{
		_lobby = lobby;
	}

	public Lobby() { }
}
