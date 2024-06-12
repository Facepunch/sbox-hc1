using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

partial class ActionGraphHelpers
{
	[ActionGraphNode( "gamemode" )]
	public static GameMode GetGameMode => GameMode.Instance;
}

