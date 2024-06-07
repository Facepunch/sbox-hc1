namespace Facepunch;

/// <summary>
/// One in a scene, used by <see cref="PlayerVoiceComponent"/> to dictate rules around who can broadcast their voices to who.
/// </summary>
public interface IVoiceFilter
{
	/// <summary>
	/// Should we exclude this potential listener from hearing our voice?
	/// </summary>
	/// <returns></returns>
	IEnumerable<Connection> GetExcludeFilter();
}
