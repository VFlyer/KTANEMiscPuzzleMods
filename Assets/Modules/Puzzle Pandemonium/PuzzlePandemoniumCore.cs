using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PuzzlePandemoniumCore : MonoBehaviour {

	public KMBombModule modSelf;
	[SerializeField]
	private PuzzleGeneric[] allPuzzles;
	public KMSelectable[] gridSelectables;

	enum PuzzleType
    {
		None = -1,
		Plumbing,
		Sudoku,
		Lights_Out,
		Kakurasu
    }

	static readonly PuzzleType[] allPossiblePuzzleTypes = new[] {
		PuzzleType.Plumbing,
		PuzzleType.Sudoku,
		PuzzleType.Lights_Out,
		PuzzleType.Kakurasu
	};

	PuzzleType[] alterationInteractionChain;
	static int modIDCnt;
	int moduleID;
	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}",modSelf.ModuleDisplayName, moduleID, string.Format(toLog,args));
    }

	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		alterationInteractionChain = allPossiblePuzzleTypes.ToArray().Shuffle();
		for (var x = 0; x < alterationInteractionChain.Length; x++)
			QuickLog("Interacting the tiles in {0} will also affect the tiles in {1}.", alterationInteractionChain[x].ToString().Replace('_',' '), alterationInteractionChain[(x + 1) % 4].ToString().Replace('_',' '));

		foreach (var puzzle in allPuzzles)
		{
			puzzle.GenerateBoard();
			puzzle.ShuffleCurrentBoard();
			//puzzle.DisplayCurrentBoard();
		}
	}
}
