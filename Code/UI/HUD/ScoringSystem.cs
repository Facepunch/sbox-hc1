namespace Facepunch.UI;

public partial class ScoringSystem : Panel
{
	public void OnScore( int amount, string reason )
	{
		AddChild( new ScoringEntry()
		{
			Score = amount,
			Reason = reason
		} );
	}
}
