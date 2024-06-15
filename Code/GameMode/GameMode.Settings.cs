
namespace Facepunch;

partial class GameMode
{
	[Property, HostSync] public bool UnlimitedMoney { get; set; }
	[Property, HostSync] public int MaxBalance { get; set; } = 16000;
}
