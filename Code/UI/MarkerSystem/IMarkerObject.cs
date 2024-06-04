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
	/// Are we overriding the type here?
	/// </summary>
	Type MarkerPanelTypeOverride { get => null; }
}
