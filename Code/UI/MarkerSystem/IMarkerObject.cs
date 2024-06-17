namespace Facepunch;

public interface IMarkerObject : IValid
{
	/// <summary>
	/// Raw access to the marker object's <see cref="GameObject"/>
	/// </summary>
	GameObject GameObject { get; }

	/// <summary>
	/// Where is this marker?
	/// </summary>
	public Vector3 MarkerPosition { get; }

	/// <summary>
	/// Are we overriding the type here?
	/// </summary>
	Type MarkerPanelTypeOverride => null;

	/// <summary>
	/// What icon does this marker have?
	/// </summary>
	string MarkerIcon => null;

	/// <summary>
	/// What styles should we apply to the marker?
	/// </summary>
	string MarkerStyles => null;

	/// <summary>
	/// What text should we show?
	/// </summary>
	string DisplayText => null;

	/// <summary>
	/// How far can we see this marker?
	/// </summary>
	float MarkerMaxDistance => 0;

	/// <summary>
	/// How big's the marker?
	/// </summary>
	int IconSize => 32;

	/// <summary>
	/// Should we show a chevron when we're off-screen?
	/// </summary>
	bool ShowChevron => true;

	/// <summary>
	/// Should we even show this marker?
	/// </summary>
	/// <returns></returns>
	bool ShouldShow() => true;

	/// <summary>
	/// Input hint?
	/// </summary>
	string InputHint => null;

	/// <summary>
	/// Should we dim the marker when looking at it?
	/// </summary>
	bool LookOpacity => true;
}
