using Facepunch.UI;

namespace Facepunch;

public interface IMarkerObject : IValid
{
	/// <summary>
	/// Raw access
	/// </summary>
	GameObject GameObject { get; }
	
	/// <summary>
	/// Data in the frame
	/// </summary>
	MarkerFrame MarkerFrame { get; }

	/// <summary>
	/// Should we show the marker?
	/// </summary>
	bool ShouldShowMarker { get; }

	/// <summary>
	/// Are we overriding the type here?
	/// </summary>
	Type MarkerPanelTypeOverride { get => null; }

	public void Register()
	{
		MarkerSystem.Register( this );
	}

	public void UnRegister()
	{
		MarkerSystem.UnRegister( this );
	}
}
