using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTesterScript : MonoBehaviour {

	public PuzzleGeneric testingPuzzle;
	public KMSelectable[] gridSelectables;
	public KMBombModule modSelf;
	[SerializeField]
	public bool logPuzzle;

	// Use this for initialization
	void Start () {
		if (testingPuzzle != null)
        {
			testingPuzzle.GenerateBoard();
			do
			{
				testingPuzzle.ShuffleCurrentBoard();
				testingPuzzle.CheckCurrentBoard();
			}
			while (testingPuzzle.IsPuzzleSolved());
			if (logPuzzle)
			{
				Debug.LogFormat("Solve: {0}", testingPuzzle.GetSolutionBoard().Join());
				Debug.LogFormat("Current: {0}", testingPuzzle.GetCurrentBoard().Join());
			}
			testingPuzzle.DisplayCurrentBoard();
			for (var x = 0; x < gridSelectables.Length; x++)
            {
				var y = x;
				gridSelectables[x].OnInteract += delegate {
					testingPuzzle.HandleIdxPress(y);
					testingPuzzle.DisplayCurrentBoard();
					if (testingPuzzle.CheckCurrentBoard())
						modSelf.HandlePass();
					return false;
				};
            }

        }
	}
}
