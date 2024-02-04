using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P03PuzzleScript : MonoBehaviour {

	public KMSelectable[] gridsSelectable, componentSelectables;
	public KMSelectable submitBtn, resetBtn;

	int[][] initialPowersEach;
	int?[][] idxComponentsPlaced;

	const int length = 6, boards = 2;

	// Use this for initialization
	void Start () {
		initialPowersEach = new int[boards][];
		idxComponentsPlaced = new int?[boards][];
	}
}
