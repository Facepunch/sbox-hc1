namespace Facepunch;

partial class GameMode
{
	[Property, Sync( SyncFlags.FromHost )] public bool UnlimitedMoney { get; set; }
	[Property, Sync( SyncFlags.FromHost )] public int MaxBalance { get; set; } = 16000;
}
