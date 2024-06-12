using Facepunch.UI;

namespace Facepunch.GameRules;

public sealed class ShowToast : Component
{
	[Property]
	public string Message { get; set; }
}
