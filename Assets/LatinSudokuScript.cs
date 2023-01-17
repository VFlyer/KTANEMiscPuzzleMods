using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class LatinSudokuScript : MonoBehaviour {

	[SerializeField]
	private KMAudio mAudio;
	[SerializeField]
	private KMBombModule modSelf;
	[SerializeField]
	private KMSelectable[] axisSelectables, inputSelectables;

	[SerializeField]
	private MeshRenderer[] gridRenderers, selectSolidRenderers;
	[SerializeField]
	private MeshRenderer[] ringRenderers, miscRingRenderers;
	[SerializeField]
	private MeshRenderer selectHLRenderer;
	[SerializeField]
	private TextMesh[] displayValues;
	[SerializeField]
	private Mesh[] meshes;
	[SerializeField]
	private Vector3[] meshScaleFactors;
	[SerializeField]
	private Material[] meshMats;

	int curXIdx, curZIdx, curYIdx, idxViewMode;
	private static readonly float[] xAxisVals = new[] { -0.0477f, -0.016f, 0.016f, 0.0477f },
		yAxisVals = new[] { -0.0442f, -0.0148f, 0.0172f, 0.047f },
		zAxisVals = new[] { 0.048f, 0.016f, -0.016f, -0.048f };


	static int modIDCnt;
	int moduleID;
	bool interactable = true, moduleSolved = false, holdingSolid = false;

	int[] finalGrid, currentGrid;
	bool[] lockPlacements;
	float timeHeld;

	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[Latin Sudoku #{0}] {1}", moduleID, string.Format(toLog, args));
    }
	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		//GenerateSolutionBoard();
		GenerateExampleBoard();

        for (var x = 0; x < inputSelectables.Length; x++)
        {
			var y = x + 1;
			inputSelectables[x].OnInteract += delegate {
				holdingSolid = true;
				timeHeld = 0f;
				if (interactable && !moduleSolved)
					HandleShapePress(y);
				return false;
			};
			inputSelectables[x].OnInteractEnded += delegate {
				holdingSolid = false;
				if (timeHeld > 1.5f)
					HandleResetBoard();			
			};
        }
        for (var x = 0; x < axisSelectables.Length; x++)
        {
			var y = x;
			axisSelectables[x].OnInteract += delegate {
				if (interactable && !moduleSolved)
					HandleMovementPress(y);
				return false;
			};
        }
		for (var x = 0; x < gridRenderers.Length; x++)
			StartCoroutine(SpinObject(Random.insideUnitSphere * 60, gridRenderers[x].transform));
		for (var x = 0; x < selectSolidRenderers.Length; x++)
			StartCoroutine(SpinObject(Random.insideUnitSphere * 60, selectSolidRenderers[x].transform));
	}
	IEnumerator SpinObject(Vector3 rotationSpeed, Transform affectedObject)
    {
		while (enabled)
        {
			affectedObject.localRotation *= Quaternion.Euler(rotationSpeed * Time.deltaTime);
			yield return null;
        }
		yield break;
    }

	/*
	void OnDestroy()
    {
		if (!moduleSolved)
        {
			QuickLog("Unsolved grid upon detonation/exiting:");
			LogGrid(currentGrid);
        }
    }
	*/
	void HandleResetBoard()
    {
		if (moduleSolved || !interactable) return;
		var countNoise = 0;
		for (var x = 0; x < currentGrid.Length; x++)
		{
			if (!lockPlacements[x] && currentGrid[x] != 0)
			{
				currentGrid[x] = 0;
				countNoise++;
			}
		}
		for (var x = 0; x < Mathf.Min(countNoise, 3); x++)
			mAudio.PlaySoundAtTransform("Place", transform);

		UpdateBoard();
	}
	void HandleMovementPress(int idx)
    {
		var expectedTransform = axisSelectables[idx].transform;
		switch (idx)
        {
			case 2:
				curXIdx = (curXIdx + 1) % 4;
				mAudio.PlaySoundAtTransform("Scroll" + curXIdx, expectedTransform);
				break;
			case 1:
				curYIdx = (curYIdx + 1) % 4;
				mAudio.PlaySoundAtTransform("Scroll" + curYIdx, expectedTransform);
				break;
			case 0:
				curZIdx = (curZIdx + 1) % 4;
				mAudio.PlaySoundAtTransform("Scroll" + curZIdx, expectedTransform);
				break;
			default:
				idxViewMode = (idxViewMode + 1) % 4;
				mAudio.PlaySoundAtTransform("Scroll" + idxViewMode, expectedTransform);
				break;
        }
		UpdateBoard();
    }

	void HandleShapePress(int value)
    {
		var idxToPlace = curZIdx + 4 * curYIdx + 16 * curXIdx;
		var actualTransform = inputSelectables[value - 1].transform;
		if (!lockPlacements[idxToPlace])
        {
			inputSelectables[value - 1].AddInteractionPunch(0.25f);
			mAudio.PlaySoundAtTransform("Place", actualTransform);
			currentGrid[idxToPlace] = currentGrid[idxToPlace] == value ? 0 : value;
			if (currentGrid.SequenceEqual(finalGrid))
            {
				mAudio.PlaySoundAtTransform("Solve", transform);
				interactable = false;
				moduleSolved = true;
				idxViewMode = 0;
				modSelf.HandlePass();
			}
        }
		UpdateBoard();
    }
	void UpdateBoard()
    {
		var idxCurPos = curZIdx + 4 * curYIdx + 16 * curXIdx;
		for (var x = 0; x < gridRenderers.Length; x++)
        {
			var curValue = currentGrid[x];
			var xCoordRend = x % 4;
			var yCoordRend = x / 4 % 4;
			var zCoordRend = x / 16;
			var adjustedIdx = zCoordRend + yCoordRend * 4 + xCoordRend * 16;
			if (curValue != 0 && (
				idxViewMode == 3 ? zCoordRend == curXIdx :
				idxViewMode == 2 ? yCoordRend == curYIdx :
				idxViewMode == 1 ? xCoordRend == curZIdx : idxViewMode == 0))
			{
				gridRenderers[adjustedIdx].enabled = true;
				gridRenderers[adjustedIdx].GetComponent<MeshFilter>().mesh = meshes[curValue - 1];
				gridRenderers[adjustedIdx].transform.localScale = meshScaleFactors[curValue - 1];
				gridRenderers[adjustedIdx].material = meshMats[curValue - 1];
			}
			else
				gridRenderers[adjustedIdx].enabled = false;
        }
		displayValues[2].text = moduleSolved ? "" : curXIdx.ToString();
		displayValues[1].text = moduleSolved ? "" : curYIdx.ToString();
		displayValues[0].text = moduleSolved ? "" : curZIdx.ToString();
		displayValues[3].text = moduleSolved ? "" : "-XYZ"[idxViewMode].ToString();

		selectHLRenderer.transform.localPosition = new Vector3(xAxisVals[curZIdx], yAxisVals[curYIdx], zAxisVals[curXIdx]);
		var curValPos = currentGrid[idxCurPos];
		selectHLRenderer.enabled = curValPos == 0;
        for (var x = 0; x < ringRenderers.Length; x++)
			ringRenderers[x].material.color = moduleSolved ? Color.yellow : curValPos == x + 1 ? lockPlacements[idxCurPos] ? Color.gray : Color.yellow : Color.black;
        for (var x = 0; moduleSolved && x < miscRingRenderers.Length; x++)
			miscRingRenderers[x].material.color = Color.yellow;
	}
	void LogGrid(int[] values)
    {
		var idxesGrouped = new[] { 0, 1, 2, 3, 16, 17, 18, 19, 32, 33, 34, 35, 48, 49, 50, 51 };
		for (var x = 0; x < 4; x++)
			QuickLog("{0}", Enumerable.Range(0, 4).Select(a => idxesGrouped.Skip(4 * a).Take(4).Select(b => "-1234"[values.ElementAtOrDefault(4 * x + b)]).Join(",")).Join(" "));
    }
	/* Grid layout for idxes:
	 * 
	 * X ->
	 * 00	01	02	03	Z	Y
	 * 04	05	06	07	|	|
	 * 08	09	10	11	V	|
	 * 12	13	14	15		V
	 * 
	 * 16	17	18	19
	 * 20	21	22	23
	 * 24	25	26	27
	 * 28	29	30	31
	 * 
	 * 32	33	34	35
	 * 36	37	38	39
	 * 40	41	42	43
	 * 44	45	46	47
	 * 
	 * 48	49	50	51
	 * 52	53	54	55
	 * 56	57	58	59
	 * 60	61	62	63
	 *
	 * Note that the module will generated it on Z,X,Y, rather than the specified.
	 */
	void GenerateExampleBoard()
    {
		// Taken from level 1 from the actual game, Actual 4x4x4 Sudoku. Link to actual version: https://aaron-f-bianchi.itch.io/actual-3d-sudoku
		finalGrid = new[] {
			1,2,4,3,
			3,4,1,2,
			4,3,2,1,
			2,1,3,4,
			
			4,3,2,1,
			2,1,3,4,
			1,2,4,3,
			3,4,1,2,
			
			2,1,3,4,
			4,3,2,1,
			3,4,1,2,
			1,2,4,3,
			
			3,4,1,2,
			1,2,4,3,
			2,1,3,4,
			4,3,2,1,
		};
		currentGrid = new[] {
			0,2,4,0,
			0,0,0,0,
			0,0,0,0,
			0,1,3,0,

			0,0,0,0,
			0,1,3,0,
			0,2,4,0,
			0,0,0,0,

			0,0,0,0,
			0,3,2,0,
			0,4,1,0,
			0,0,0,0,

			0,4,1,0,
			0,0,0,0,
			0,0,0,0,
			0,3,2,0,
		};
		var idxLockPlacements = new[] { 1, 2, 13, 14, 21, 22, 25, 26, 37, 38, 41, 42, 49, 50, 61, 62 };
		lockPlacements = Enumerable.Range(0, 64).Select(a => idxLockPlacements.Contains(a)).ToArray();
		UpdateBoard();
		QuickLog("Solution grid:");
		LogGrid(finalGrid);
		QuickLog("Initial grid:");
		LogGrid(currentGrid);
	}
	// Collapse until there is no other possible combinations left after placing the single option.
	IEnumerable<List<int>> CollapseGrid(IEnumerable<List<int>> combinationsCollapse)
    {
		if (combinationsCollapse.Count() != 64) return combinationsCollapse;
		var checkOffsetsX = new[] { 0, 1, 2, 3 }; // Offset checks for X.
		var checkOffsetsZ = new[] { 0, 4, 8, 12 }; // Offset checks for Y.
		var checkOffsetsY = new[] { 0, 16, 32, 48 }; // Offset checks for Z.
		var checkOffsetsXZBox = new[] { 0, 1, 4, 5 }; // Offset checks for XZ box.
		var checkOffsetsXYBox = new[] { 0, 1, 16, 17 }; // Offset checks for XY box.
		var checkOffsetsYZBox = new[] { 0, 4, 16, 20 }; // Offset checks for YZ box.

		var idxesCheckX = Enumerable.Range(0, 64).Where(a => a % 4 == 0);
		var idxesCheckZ = Enumerable.Range(0, 64).Where(a => a % 16 < 4);
		var idxesCheckY = Enumerable.Range(0, 16);
		var idxesCheckXZBox = Enumerable.Range(0, 64).Where(a => a % 2 == 0 && (a >> 2) % 2 == 0);
		var idxesCheckXYBox = Enumerable.Range(0, 64).Where(a => a % 2 == 0 && (a >> 4) % 2 == 0);
		var idxesCheckYZBox = Enumerable.Range(0, 64).Where(a => (a >> 2) % 2 == 0 && (a >> 4) % 2 == 0);
		//Debug.Log(idxesCheckX.Join());
		//Debug.Log(idxesCheckZ.Join());
		//Debug.Log(idxesCheckY.Join());
		//Debug.Log(idxesCheckXZBox.Join());
		//Debug.Log(idxesCheckXYBox.Join());
		//Debug.Log(idxesCheckYZBox.Join());

		var groupedIdxesX = idxesCheckX.Select(a => checkOffsetsX.Select(b => a + b).ToArray());
		var groupedIdxesY = idxesCheckY.Select(a => checkOffsetsY.Select(b => a + b).ToArray());
		var groupedIdxesZ = idxesCheckZ.Select(a => checkOffsetsZ.Select(b => a + b).ToArray());
		var groupedIdxesXZBox = idxesCheckXZBox.Select(a => checkOffsetsXZBox.Select(b => a + b).ToArray());
		var groupedIdxesXYBox = idxesCheckXYBox.Select(a => checkOffsetsXYBox.Select(b => a + b).ToArray());
		var groupedIdxesYZBox = idxesCheckYZBox.Select(a => checkOffsetsYZBox.Select(b => a + b).ToArray());

		//Debug.Log(groupedIdxesX.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesY.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesZ.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesXZBox.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesXYBox.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesYZBox.Select(a => a.Join(",")).Join());
		

		var newCombinationsLeft = combinationsCollapse.Select(a => a.ToList()).ToList();
		// Simple elimation. Basically find cells that have only 1 entry and candidates from other cells.
		var singleFilledCellIdxes = Enumerable.Range(0, 64).Where(a => combinationsCollapse.ElementAt(a).Count() == 1);
		foreach (var x in singleFilledCellIdxes)
        {
			var currentPossibilities = combinationsCollapse.ElementAt(x); // The current combination set for that index.

			var CurXGroup = groupedIdxesX.Single(a => a.Contains(x));
			var CurYGroup = groupedIdxesY.Single(a => a.Contains(x));
			var CurZGroup = groupedIdxesZ.Single(a => a.Contains(x));
			var CurXZBox = groupedIdxesXZBox.Single(a => a.Contains(x));
			var CurXYBox = groupedIdxesXYBox.Single(a => a.Contains(x));
			var CurYZBox = groupedIdxesYZBox.Single(a => a.Contains(x));
			// Grouped Idxes excluding the current idx in the particular location.
			var RemainingXGroup = CurXGroup.Where(a => a != x);
			var RemainingYGroup = CurYGroup.Where(a => a != x);
			var RemainingZGroup = CurZGroup.Where(a => a != x);
			var RemainingXZBox = CurXZBox.Where(a => a != x);
			var RemainingXYBox = CurXYBox.Where(a => a != x);
			var RemainingYZBox = CurYZBox.Where(a => a != x);

			var mergedCombinationIdxes = RemainingXGroup.Union(RemainingYGroup)
				.Union(RemainingZGroup).Union(RemainingXZBox)
				.Union(RemainingXYBox).Union(RemainingYZBox);
			// A merged index of all the mentioned items.
			foreach (int idx in mergedCombinationIdxes)
				newCombinationsLeft[idx].Remove(currentPossibilities.Single());
		}
		var multiFilledCellsIdxes = Enumerable.Range(0, 64).Where(a => combinationsCollapse.ElementAt(a).Count() > 1); // An idx list of all cells that have more than 1 combination.
		// Naked Singles.
		foreach (var x in multiFilledCellsIdxes)
		{
			var currentPossibilities = combinationsCollapse.ElementAt(x); // The current combination set for that index.

			var CurXGroup = groupedIdxesX.Single(a => a.Contains(x));
			var CurYGroup = groupedIdxesY.Single(a => a.Contains(x));
			var CurZGroup = groupedIdxesZ.Single(a => a.Contains(x));
			var CurXZBox = groupedIdxesXZBox.Single(a => a.Contains(x));
			var CurXYBox = groupedIdxesXYBox.Single(a => a.Contains(x));
			var CurYZBox = groupedIdxesYZBox.Single(a => a.Contains(x));
			// Grouped Idxes excluding the current idx in the particular location.
			var RemainingXGroup = CurXGroup.Where(a => a != x);
			var RemainingYGroup = CurYGroup.Where(a => a != x);
			var RemainingZGroup = CurZGroup.Where(a => a != x);
			var RemainingXZBox = CurXZBox.Where(a => a != x);
			var RemainingXYBox = CurXYBox.Where(a => a != x);
			var RemainingYZBox = CurYZBox.Where(a => a != x);

			var mergedCombinationIdxes = RemainingXGroup.Union(RemainingYGroup)
				.Union(RemainingZGroup).Union(RemainingXZBox)
				.Union(RemainingXYBox).Union(RemainingYZBox);
			// A merged index of all the mentioned items.
			foreach (var value in currentPossibilities)
			{
				if (!RemainingXGroup.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingYGroup.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingZGroup.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingXZBox.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingXYBox.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingYZBox.Any(a => combinationsCollapse.ElementAt(a).Contains(value))) // Basically, if there is no other combinations left within any of these regions for that value...
				{
					newCombinationsLeft[x].RemoveAll(a => a != value); // Remove other possibilities of this value...
					foreach (int idx in mergedCombinationIdxes)
						newCombinationsLeft[idx].Remove(currentPossibilities.Single());
					break;
				}
			}
		}
		return Enumerable.Range(0, newCombinationsLeft.Count).All(a => newCombinationsLeft[a].SequenceEqual(combinationsCollapse.ElementAt(a))) ? combinationsCollapse : CollapseGrid(newCombinationsLeft);
    }

	void GenerateSolutionBoard()
    {
		var possibleDigits = new List<int>[64];
		finalGrid = new int[64];
		currentGrid = new int[64];
		lockPlacements = new bool[64];

		var idxInputs = new List<int>();
		var digitsPlaced = new List<int>();
		var prioritizedIdxesCollapse = Enumerable.Range(0, 64).ToList();
	fullRetryGen:
		idxInputs.Clear();
		digitsPlaced.Clear();
		for (var x = 0; x < possibleDigits.Length; x++) // Start by filling every single option available on the module.
		{
			if (possibleDigits[x] == null)
				possibleDigits[x] = new List<int>();
			possibleDigits[x].Clear();
			possibleDigits[x].AddRange(Enumerable.Range(1, 4));
		}
		var iterCount = 0;
		prioritizedIdxesCollapse.Shuffle();
	iterate:
		iterCount++;
		
		//scan:
		// Pick a random cell that has the fewest amount of options, (not 1, not 0) and create a value from that.
		var possibleIdxes = Enumerable.Range(0, 64).Where(a => possibleDigits.Min(b => b.Count) >= possibleDigits[a].Count && possibleDigits[a].Count > 1);
		if (!possibleIdxes.Any())
			goto fullRetryGen;
		else
		{
			var pickedIdx = possibleIdxes.PickRandom();
			var pickedDigit = possibleDigits[pickedIdx].PickRandom();

			idxInputs.Add(pickedIdx);
			digitsPlaced.Add(pickedDigit);

			possibleDigits[pickedIdx].RemoveAll(a => a != pickedDigit);
		}

		



		if (possibleDigits.Any(a => !a.Any())) // If there are no other options for one of the cells, restart the entire generation.
		{
			//collapseFirst = true;
			goto fullRetryGen;
		}
		else if (!possibleDigits.All(a => a.Count() == 1) && iterCount < 128)
			goto iterate;
		QuickLog("Solution grid:");
		LogGrid(finalGrid);
		QuickLog("Initial grid:");
		LogGrid(currentGrid);
	}

	// Update is called once per frame
	void Update () {
		if (!interactable) return;
		if (holdingSolid && timeHeld < 3f)
			timeHeld += Time.deltaTime;
		var curEase = Easing.InSine(timeHeld, 0f, 2f, 1f);
		for (var x = 0; x < miscRingRenderers.Length; x++)
        {
			miscRingRenderers[x].material.color = holdingSolid ? Color.yellow * curEase + Color.black * (1f - curEase) : Color.black;
        }
	}
	//twitch plays
	private float _tpSpeed = 0.1f;
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} x/y/z/m [Presses the specified coordinate/planar view button(s)] | !{0} t/h/o/d [Presses the buttons representing tetrahedron, cube, octahedreon, dodecrahedron] | Previous mentioned commands may be chained, for example '!{0} xxymh dozyt' | !{0} setspeed 0.2 [Set a press speed between 0 and 1 seconds.]";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		var parameters = command.ToLowerInvariant().Split(' ');
		var m = Regex.Match(command, @"^\s*setspeed\s\d+(\.\d+)*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (m.Success)
		{
			if (parameters.Length != 2)
				yield break;
			float tempSpeed;
			if (!float.TryParse(parameters[1], out tempSpeed) || tempSpeed <= 0 || tempSpeed > 1)
			{
				yield return "sendtochaterror " + parameters[1] + " is not a valid speed! Press speed must be between 0 and 1 seconds.";
				yield break;
			}
			yield return null;
			_tpSpeed = tempSpeed;
			yield return "sendtochat Latin Hypercube's press speed has been set to " + parameters[1];
			yield break;
		}
		var chars = "xyzmthod ";
		var list = new List<int>();
		for (int i = 0; i < command.Length; i++)
		{
			int ix = chars.IndexOf(command[i]);
			if (ix == -1)
				yield break;
			if (ix == 8)
				continue;
			list.Add(ix);
		}
		var buttons = axisSelectables.Concat(inputSelectables);
		yield return null;
		for (int i = 0; i < list.Count; i++)
		{
			buttons.ElementAt(list[i]).OnInteract();
			yield return new WaitForSeconds(_tpSpeed);
		}
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		while (!currentGrid.SequenceEqual(finalGrid))
        {
			var shuffledIdxes = Enumerable.Range(0, 3).ToArray().Shuffle();
			for (var x = 0; x < 4; x++)
			{
				for (var y = 0; y < 4; y++)
				{
					for (var z = 0; z < 4; z++)
					{
						var idxCurPos = 16 * curZIdx + 4 * curYIdx + curXIdx;
						if (currentGrid[idxCurPos] != finalGrid[idxCurPos])
						{
							inputSelectables[finalGrid[idxCurPos] - 1].OnInteract();
							inputSelectables[finalGrid[idxCurPos] - 1].OnInteractEnded();
							yield return new WaitForSeconds(0.1f);
						}
						axisSelectables[shuffledIdxes[2]].OnInteract();
						yield return new WaitForSeconds(0.1f);
					}
					axisSelectables[shuffledIdxes[1]].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
				axisSelectables[shuffledIdxes[0]].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}


		yield break;
    }

}
