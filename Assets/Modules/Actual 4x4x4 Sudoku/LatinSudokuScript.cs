using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class LatinSudokuScript : MonoBehaviour {

	public KMAudio mAudio;
	public KMBombModule modSelf;
	public KMSelectable[] axisSelectables, inputSelectables;
	public AdjustedCubeDottedRenderer cubeDotRenderer;
	public MeshRenderer[] gridRenderers, selectSolidRenderers;
	public MeshRenderer[] ringRenderers, miscRingRenderers;
	public MeshRenderer selectHLRenderer;
	public TextMesh[] displayValues;
	public Mesh[] meshes;
	public Vector3[] meshScaleFactors;
	public Material[] meshMats;
	public Transform floatingCubeBase;

	int curXIdx, curZIdx, curYIdx, idxViewMode;
	private static readonly float[] xAxisVals = new[] { -0.9f, -0.3f, 0.3f, 0.9f },
		yAxisVals = new[] { -0.9f, -0.3f, 0.3f, 0.9f },
		zAxisVals = new[] { 0.9f, 0.3f, -0.3f, -0.9f };


	static int modIDCnt;
	int moduleID;
	bool interactable = true, moduleSolved = false, holdingSolid = false, autoRotate = false;
	[SerializeField]
	bool generateExample = false;

	int[] finalGrid, currentGrid;
	bool[] lockPlacements;
	float timeHeld;

	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }
	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		if (generateExample)
			GenerateExampleBoard();
		else
			GenerateSolutionBoard();


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
			axisSelectables[x].OnInteractEnded += delegate {
			};
        }
		for (var x = 0; x < gridRenderers.Length; x++)
			StartCoroutine(SpinObject(Random.insideUnitSphere * 60, gridRenderers[x].transform));
		for (var x = 0; x < selectSolidRenderers.Length; x++)
			StartCoroutine(SpinObject(Random.insideUnitSphere * 60, selectSolidRenderers[x].transform));
		StartCoroutine(EaseTransformModifierInfinitely(floatingCubeBase, Vector3.up * 0.001f));
	}
	IEnumerator EaseTransformModifierInfinitely(Transform affectedObject, Vector3 offset)
    {
		var storedLocalPos = affectedObject.localPosition;
		while (enabled)
        {
			for (float t = 0; t < 2f; t += Time.deltaTime)
            {
				yield return null;
				var curEase = Easing.InOutSine(t, 0f, 2f, 1f);
				affectedObject.localPosition = Vector3.LerpUnclamped(storedLocalPos, storedLocalPos + offset, curEase);
            }
        }
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

	
	void OnDestroy()
    {
		StopAllCoroutines();
		if (!moduleSolved)
        {
			QuickLog("Unsolved grid upon detonation/abandoning:");
			LogGrid(currentGrid);
        }
    }
	
	void HandleResetBoard()
    {
		if (moduleSolved || !interactable) return;

		if (Enumerable.Range(0, 64).Where(a => !lockPlacements[a]).Any(a => a != 0))
		{
			QuickLog("Non-initial grid before reset:");
			LogGrid(currentGrid);
		}
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
		axisSelectables[idx].AddInteractionPunch(0.1f);
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
			miscRingRenderers[x].material.color = Color.black;
		cubeDotRenderer.ChangeIdxPos(curZIdx, curYIdx, 3 - curXIdx);
		var allPossibleModes = new[] { AdjustedCubeDottedRenderer.Axis.None, AdjustedCubeDottedRenderer.Axis.X, AdjustedCubeDottedRenderer.Axis.Y, AdjustedCubeDottedRenderer.Axis.Z, };
		cubeDotRenderer.RenderByAxis(allPossibleModes[idxViewMode]);
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
		// Last possible value within given region, known as Naked Singles.
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
					break;
				}
			}
		}
		
		// Naked Pairs.
		var allGroups = groupedIdxesX.Concat(groupedIdxesY).Concat(groupedIdxesZ).Concat(groupedIdxesYZBox).Concat(groupedIdxesXYBox).Concat(groupedIdxesXZBox);
		foreach (var grouping in allGroups)
        {
			for (var idx1 = 0; idx1 < 3; idx1++)
            {
				var possibleDigits1 = combinationsCollapse.ElementAt(grouping[idx1]);
				for (var idx2 = idx1 + 1; idx2 < 4; idx2++)
                {
					var exemptIdxValues = Enumerable.Range(0, 4).Where(a => idx1 != a && a != idx2).ToArray();
					var possibleDigits2 = combinationsCollapse.ElementAt(grouping[idx2]);
					var unionedSets = possibleDigits1.Union(possibleDigits2);
					// There are a couple ways to approach this, one way is to check if the union of the 2 sets have 2 numbers, and the sets have 2 possibilities.
					// Another way is to check if those 2 cells have exactly those combinations left.
					if (possibleDigits1.OrderBy(a => a).SequenceEqual(possibleDigits2.OrderBy(b => b)) && possibleDigits1.Count == 2 && possibleDigits2.Count == 2)
                    {
						// If it does... Remove the other possibilities from the other tiles within that group.
						foreach (var idxVoid in exemptIdxValues)
							newCombinationsLeft[grouping[idxVoid]].RemoveAll(a => unionedSets.Contains(a));
                    }

				}
			}
        }
		
		return Enumerable.Range(0, 64).All(a => newCombinationsLeft[a].SequenceEqual(combinationsCollapse.ElementAt(a))) ? combinationsCollapse : CollapseGrid(newCombinationsLeft);
		// If all 64 cells have the same possibility after applying the combinations, stop here.
    }

	void GenerateSolutionBoard()
    {
		var possibleDigitsPrecollapse = new List<int>[64];
		finalGrid = new int[64];
		currentGrid = new int[64];
		lockPlacements = new bool[64];

		var idxInputs = new List<int>();
		var digitsPlaced = new List<int>();
		var prioritizedIdxesCollapse = Enumerable.Range(0, 64).ToList();
		var retryCount = 0;
	fullRetryGen:
		idxInputs.Clear();
		digitsPlaced.Clear();
		for (var x = 0; x < possibleDigitsPrecollapse.Length; x++) // Start by filling every single option available on the module.
		{
			if (possibleDigitsPrecollapse[x] == null)
				possibleDigitsPrecollapse[x] = new List<int>();
			possibleDigitsPrecollapse[x].Clear();
			possibleDigitsPrecollapse[x].AddRange(Enumerable.Range(1, 4));
		}
		var iterCount = 0;
		prioritizedIdxesCollapse.Shuffle();
		var possibleDigitsPostCollapse = possibleDigitsPrecollapse.Select(a => a.ToList()).ToArray();
	iterate:
		iterCount++;

		var filteredIdxesToCollapse = prioritizedIdxesCollapse.Where(a => possibleDigitsPostCollapse[a].Count > 1);
		//scan:
		if (!filteredIdxesToCollapse.Any())
		{
			retryCount++;
			goto fullRetryGen;
		}
		else
		{
			var pickedIdx = filteredIdxesToCollapse.First();
			var pickedDigit = possibleDigitsPostCollapse[pickedIdx].PickRandom();

			idxInputs.Add(pickedIdx);
			digitsPlaced.Add(pickedDigit);

			possibleDigitsPostCollapse[pickedIdx].RemoveAll(a => a != pickedDigit);
		}
		possibleDigitsPostCollapse = CollapseGrid(possibleDigitsPostCollapse).ToArray();




		if (possibleDigitsPostCollapse.Any(a => !a.Any()))
		{
			retryCount++;
			goto fullRetryGen;
		}
		// If there are no other options for one of the cells, restart the entire generation.
		else if (!possibleDigitsPostCollapse.All(a => a.Count() == 1) && iterCount < 128)
			goto iterate;
		// If there isn't 1 option left, keep selecting until one of two things happen: An unsolvable board is created, or all of the cells have 1 possibility.
		finalGrid = possibleDigitsPostCollapse.Select(a => a.Single()).ToArray();
        for (var x = 0; x < idxInputs.Count; x++)
        {
            lockPlacements[idxInputs[x]] = true;
            currentGrid[idxInputs[x]] = digitsPlaced[x];
        }
		UpdateBoard();
		QuickLog("Solution grid after {0} iteration(s) and {1} failed attempt(s):", iterCount, retryCount);
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
	IEnumerator HandleAutoRotate()
    {
		var lastStoredValue = 0;
		while (autoRotate)
        {
			yield return null;
			if (lastStoredValue != idxViewMode)
            {
				lastStoredValue = idxViewMode;
				yield return RotateNextSide(lastStoredValue);
            }
        }
		if (floatingCubeBase.localEulerAngles != Vector3.zero)
			yield return RotateNextSide(0);
		yield break;
    }
	IEnumerator RotateNextSide(int idx)
    {
		var possibleRotations = new[] { Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), Quaternion.Euler(90, 0, 0) };
		var lastRotation = floatingCubeBase.localRotation;
		var nextRotation = possibleRotations.ElementAt(idx);
		for (float t = 0;t < 1f;t += Time.deltaTime * 2)
        {
			yield return null;
			floatingCubeBase.localRotation = Quaternion.LerpUnclamped(lastRotation, nextRotation, t);
        }
		floatingCubeBase.localRotation = nextRotation;
		yield break;
    }


	private float _tpSpeed = 0.1f;
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "\"!{0} x/y/z/m\" [Presses the specified coordinate/planar view button(s)] | \"!{0}\" t/h/o/d [Presses the buttons representing tetrahedron, cube, octahedreon, dodecrahedron] | Previous mentioned commands may be chained, for example \"!{0} xxymh dozyt\" | \"!{0} setspeed 0.2\" [Set a press speed between 0 and 1 seconds.] | \"!{0} reset/clear\" [Clears all, excluding initial cells.] | \"!{0} autorotate\" [Toggles automatic rotation of the cube to view specific faces.]";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		var parameters = command.ToLowerInvariant().Split(' ');
		var regexSpeed = Regex.Match(command, @"^\s*setspeed\s\d+(\.\d+)*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var regexReset = Regex.Match(command, @"^reset$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var regexAutoRotate = Regex.Match(command, @"^autorotate$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (regexAutoRotate.Success)
        {
			yield return null;
			autoRotate ^= true;
			if (autoRotate)
				StartCoroutine(HandleAutoRotate());
			yield return string.Format("sendtochat I have now {0} automatic rotation on the module.{1}", autoRotate ? "activated" : "deactivated", autoRotate ? " Be aware of your perception when using this." : "");
			yield break;
        }
		else if (regexSpeed.Success)
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
			yield return "sendtochat Actual 4x4x4 Sudoku's press speed has been set to " + parameters[1];
			yield break;
		}
		else if (regexReset.Success)
        {
			var idxCurPos = curZIdx + 4 * curYIdx + 16 * curXIdx;
			var selectedInputIdx = Enumerable.Range(0, 4).Where(a => a != finalGrid[idxCurPos]).PickRandom();
			yield return null;
			inputSelectables[selectedInputIdx].OnInteract();
			yield return new WaitUntil(delegate { return timeHeld > 1.75f; });
			inputSelectables[selectedInputIdx].OnInteractEnded();
			yield break;
		}
		var allowedChars = "xyzmthod ";
		var list = new List<int>();
		foreach (char chrPress in command.ToLowerInvariant())
		{
			int ix = allowedChars.IndexOf(chrPress);
			if (ix == -1)
				yield break;
			if (ix == 8 || char.IsWhiteSpace(chrPress))
				continue;
			list.Add(ix);
		}
		var buttons = axisSelectables.Concat(inputSelectables);
		yield return null;
		for (int i = 0; i < list.Count; i++)
		{
			buttons.ElementAt(list[i]).OnInteract();
			buttons.ElementAt(list[i]).OnInteractEnded();
			yield return new WaitForSeconds(_tpSpeed);
		}
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		while (!currentGrid.SequenceEqual(finalGrid))
        {
			var shuffledIdxes = Enumerable.Range(0, 3).ToArray().Shuffle();
			for (var x = 0; x < 4 && !moduleSolved; x++)
			{
				for (var y = 0; y < 4 && !moduleSolved; y++)
				{
					for (var z = 0; z < 4 && !moduleSolved; z++)
					{
						var idxCurPos = curZIdx + 4 * curYIdx + 16 * curXIdx;
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
