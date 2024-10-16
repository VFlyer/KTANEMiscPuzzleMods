using OblivMazeEnums;
using System.Collections.Generic;

namespace OblivMazeEnums
{
	public enum ClueTypes
	{
		Empty,
		Any,
		PassU, PassR, PassL, PassD,
		WallU, WallR, WallL, WallD,
		WallCnt0, WallCnt1, WallCnt2, WallCnt3,
		WallComboT, // 3-way Passageways
		WallComboHV, // Horizontal/Vertical Passageways
		WallComboB, // Bending Passageways
		WallComboE, // Dead Ends
		WallComboEHV, // Dead Ends / Horizontal/Vertical Passageways
		WallComboBE, // Dead Ends / Bending Passageways
		WallComboTHV, // 3-way / Horizontal/Vertical Passageways
		WallComboTB, // 3-way / Bending Passageways
		WallComboLetter, // Lettered Passageways
	}
}
public class OblivMazePuzzle
{
	public List<ClueTypes>[] cluesUsed;
	public int[] solutionWallIdxes;
	public string[] clueTypeArgs;
}
