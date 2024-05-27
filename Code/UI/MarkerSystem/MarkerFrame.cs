namespace Gunfight;

public record MarkerFrame
{
	public string DisplayText { get; set; }
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
}
