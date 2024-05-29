using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HalfTimeTeamSwap : Component, IRoundEndListener
{
	[RequireComponent] public RoundCounter RoundCounter { get; private set; }
	[RequireComponent] public RoundLimit RoundLimit { get; private set; }
}

