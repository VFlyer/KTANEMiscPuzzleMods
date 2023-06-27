using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class InstantInsanityModule : MonoBehaviour {
	public KMBombModule modSelf;
	public KMColorblindMode colorblindMode;
	public IICube[] cubes;
	public KMSelectable[] cubeRotateSelectables;
	public KMSelectable submitSelectable;
	int curCubeIdx = -1;
	// Use this for initialization
	void Start () {
		var allowedCubes = new int[4][];
		for (var p = 0; p < 4; p++)
			allowedCubes[p] = new int[6];
		for (var x = 1; x <= 4; x++)
		{
			var randomIdxMatching = Enumerable.Range(0, 4).ToArray().Shuffle();
			for (var p = 0; p < 4; p++)
				allowedCubes[p][x] = randomIdxMatching[p];
		}
		for (var p = 0; p < 4; p++)
		{
			/*
			var possibleSets = new[] {
				new[] { 0, 0 }, new[] { 0, 1 }, new[] { 0, 2 }, new[] { 0, 3 },
				new[] { 1, 1 }, new[] { 1, 2 }, new[] { 1, 3 },
				new[] { 2, 2 }, new[] { 2, 3 },
				new[] { 3, 3 }};
			*/
			allowedCubes[p][0] = Random.Range(0, 4);
			allowedCubes[p][5] = Random.Range(0, 4);
		}
		var randomIterationCount = Random.Range(15, 40);
		for (var x = 0; x < randomIterationCount; x++)
        {
			var pickedIdx = Random.Range(0, 4);
			
			switch (Random.Range(0, 6))
			{
				case 0: // Rotate CW
					allowedCubes[pickedIdx] = new[] { 0, 2, 3, 4, 1, 5 }.Select(a => allowedCubes[pickedIdx][a]).ToArray();
					break;
				case 1: // Rotate CCW
					allowedCubes[pickedIdx] = new[] { 0, 4, 1, 2, 3, 5 }.Select(a => allowedCubes[pickedIdx][a]).ToArray();
					break;
				case 2: // Tilt Up
					allowedCubes[pickedIdx] = new[] { 1, 5, 2, 0, 4, 3 }.Select(a => allowedCubes[pickedIdx][a]).ToArray();
					break;
				case 3: // Tilt Down
					allowedCubes[pickedIdx] = new[] { 3, 0, 2, 5, 4, 1 }.Select(a => allowedCubes[pickedIdx][a]).ToArray();
					break;
				case 4: // Tip Right
					allowedCubes[pickedIdx] = new[] { 4, 1, 0, 3, 5, 2 }.Select(a => allowedCubes[pickedIdx][a]).ToArray();
					break;
				case 5: // Tip Left
					allowedCubes[pickedIdx] = new[] { 2, 1, 5, 3, 0, 4 }.Select(a => allowedCubes[pickedIdx][a]).ToArray();
					break;
				default:
					break;
			}
		}
		for (var p = 0; p < cubes.Length; p++)
		{
			cubes[p].AssignNewCubeFaceIdxes(allowedCubes[p]);
			cubes[p].UpdateCubeRenderers();
		}
	}
	void HandleRotateCurCube(int rotationIdx, bool fastRotation = false)
    {
		if (curCubeIdx < 0) return;
		var curCubeFaceColorIdxes = cubes[curCubeIdx].GetCubeFaceIdxes();
		var expectedRotation = Quaternion.Euler(Vector3.zero);
		/*
		 * 0	T
		 * 1234	FRBL
		 * 5	B
		 */
		switch(rotationIdx)
        {
			case 0: // Rotate CW
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 0, 2, 3, 4, 1, 5 }.Select(a => curCubeFaceColorIdxes[a]));
				break;
			case 1: // Rotate CCW
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 0, 4, 1, 2, 3, 5 }.Select(a => curCubeFaceColorIdxes[a]));
				break;
			case 2: // Tilt Up
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 1, 5, 2, 0, 4, 3 }.Select(a => curCubeFaceColorIdxes[a]));
				break;
			case 3: // Tilt Down
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 3, 0, 2, 5, 4, 1 }.Select(a => curCubeFaceColorIdxes[a]));
				break;
			case 4: // Tip Right
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 4, 1, 0, 3, 5, 2 }.Select(a => curCubeFaceColorIdxes[a]));
				break;
			case 5: // Tip Left
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 2, 1, 5, 3, 0, 4 }.Select(a => curCubeFaceColorIdxes[a]));
				break;
			default:
				break;
        }
		if (!fastRotation)
        {

        }
    }
}
