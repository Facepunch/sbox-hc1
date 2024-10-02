namespace Facepunch.Gunsmith;

public partial class GunsmithPart
{
	/// <summary>
	/// A list of every single gunsmith part.
	/// </summary>
	public static HashSet<GunsmithPart> All { get; set; } = new();

	/// <summary>
	/// A list of every single gunsmith part, mapped by type.
	/// </summary>
	public static Dictionary<GunsmithPartType, HashSet<GunsmithPart>> Categories { get; set; } = new();

	protected override void PostLoad()
	{
		if ( !All.Contains( this ) )
		{
			All.Add( this );
		}

		if ( Categories.TryGetValue( Category, out var list ) )
		{
			list.Add( this );
		}
		else
		{
			Categories.TryAdd( this.Category, new()
			{
				this
			} );
		}		
	}
}
