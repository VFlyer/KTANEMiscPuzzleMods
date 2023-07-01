using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class InstantInsanityModule : MonoBehaviour {
	public KMBombModule modSelf;
	public KMAudio mAudio;
	public KMColorblindMode colorblindMode;
	public IICube[] cubes, miscCubes;
	public Transform selectedCube;
	public MassSetRenderersScript[] checkerDisplays;
	public KMSelectable[] cubeRotateSelectables, cubeSelectables;
	public KMSelectable submitSelectable;
	int curCubeIdx = -1;
	static int modIDCnt;
	int moduleID;
	float speed = 10f;
	/*enum ActionType
	{
		None = 0,
		RotateCW,
		RotateCCW,
		TipUp,
		TipDown,
		TipRight,
		TipLeft,
		RevealCube,
		HideCube
	}
	List<ActionType> queuedActions;
	List<int> actionArgs;*/
	bool waiting = false, moduleSolved;
	// Use this for initialization
	Vector3[] storedCubesLocalPos, storedCubesLocalScale, storedMiscCubesLocalPos, storedMiscCubesLocalScale;
	Vector3 storedBigCubeLocalPos, storedBigCubeLocalScale;
	
	void QuickLog(string toLog, params object[] args)
	{
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
	}
	void Start() {
		moduleID = ++modIDCnt;
		GeneratePuzzle();
		//queuedActions = new List<ActionType>();
		//actionArgs = new List<int>();
		for (var x = 0; x < cubeSelectables.Length; x++)
		{
			var y = x;
			cubeSelectables[x].OnInteract += delegate {
				//mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, cubeSelectables[y].transform);
				if (!(waiting || moduleSolved) && cubeSelectables[y].gameObject.activeSelf)
				{
					waiting = true;
					HandleSelectCurCube(y, false);
				}
				return false;
			};
		}
		for (var x = 0; x < cubeRotateSelectables.Length; x++)
		{
			var y = x;
			cubeRotateSelectables[x].OnInteract += delegate {
				mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, cubeRotateSelectables[y].transform);
				cubeRotateSelectables[y].AddInteractionPunch(0.2f);
				if (!(waiting || moduleSolved))
				{
					waiting = true;
					HandleRotateCurCube(y, false);
				}
				return false;
			};
		}
		submitSelectable.OnInteract += delegate
		{
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitSelectable.transform);
			if (!(waiting || moduleSolved))
            {
				waiting = true;
				HandleSubmit();
            }
			return false;
		};

		storedBigCubeLocalPos = selectedCube.localPosition;
		storedBigCubeLocalScale = selectedCube.localScale;
		storedCubesLocalPos = cubes.Select(a => a.transform.localPosition).ToArray();
		storedCubesLocalScale = cubes.Select(a => a.transform.localScale).ToArray();
		storedMiscCubesLocalPos = miscCubes.Select(a => a.transform.localPosition).ToArray();
		storedMiscCubesLocalScale = miscCubes.Select(a => a.transform.localScale).ToArray();
		foreach (var groupedRenderers in checkerDisplays)
            foreach (MeshRenderer aRender in groupedRenderers.allRenderers)
                aRender.enabled = false;
		selectedCube.gameObject.SetActive(false);
	}
	void GeneratePuzzle()
	{
		var possibleSets = new[] {
				new[] { 0, 0 }, new[] { 0, 1 }, new[] { 0, 2 }, new[] { 0, 3 },
				new[] { 1, 1 }, new[] { 1, 2 }, new[] { 1, 3 },
				new[] { 2, 2 }, new[] { 2, 3 },
				new[] { 3, 3 }};
		var allowedCubes = new int[4][];
		for (var p = 0; p < 4; p++)
			allowedCubes[p] = new int[6];
		for (var x = 1; x <= 4; x++)
		{
			var randomIdxMatching = Enumerable.Range(0, 4).ToArray().Shuffle();
			for (var p = 0; p < 4; p++)
				allowedCubes[p][x] = randomIdxMatching[p];
		}
		var oppositePairings = new[] { new[] { 1, 3 }, new[] { 2, 4 } };
		for (var p = 0; p < 4; p++)
		{
			var curCubePairsOpposites = oppositePairings.Select(a => a.Select(b => allowedCubes[p][b]).ToArray());
			var filteredSets = possibleSets.Where(a => !curCubePairsOpposites.Any(b => b.SequenceEqual(a.Reverse()) || b.SequenceEqual(a)));
			var pickedSet = filteredSets.PickRandom();
			var flipPickedSet = Random.value < 0.5f;
			allowedCubes[p][0] = flipPickedSet ? pickedSet.Last() : pickedSet.First();
			allowedCubes[p][5] = flipPickedSet ? pickedSet.First() : pickedSet.Last();
		}
		QuickLog("Guaranteed Solution (Logged face order U, F, R, B, L, D):");
		foreach (var cube in allowedCubes)
			QuickLog(cube.Select(a => "RYGB"[a]).Join(", "));
		var randomIterationCount = Random.Range(15, 61);
		for (var x = 0; x < randomIterationCount; x++)
		{
			var pickedIdx = Random.Range(0, 4);
			/*
			 * 0	T
			 * 1234	FRBL
			 * 5	B
			 */
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
		QuickLog("Initial State (Logged face order U, F, R, B, L, D):");
		foreach (var cube in allowedCubes)
			QuickLog(cube.Select(a => "RYGB"[a]).Join(", "));
		for (var p = 0; p < cubes.Length; p++)
			cubes[p].AssignNewCubeFaceIdxes(allowedCubes[p], true);
	}
	void HandleSelectCurCube(int nxtCubeIdx, bool fastAction = false)
	{
		var oldCubeIdx = curCubeIdx;
		var curCubeEqualsArgCube = curCubeIdx == nxtCubeIdx;
		curCubeIdx = curCubeEqualsArgCube ? -1 : nxtCubeIdx;

		if (fastAction)
		{
			//selectedCube.AssignNewCubeFaceIdxes(cubes[nxtCubeIdx].GetCubeFaceIdxes(), true);
			for (var x = 0; x < cubes.Length; x++)
			{
				cubes[x].transform.localPosition = x == nxtCubeIdx ? storedBigCubeLocalPos : storedCubesLocalPos[x];
				cubes[x].transform.localScale = x == nxtCubeIdx ? storedBigCubeLocalScale : storedCubesLocalScale[x];
			}
			waiting = false;
		}
		else
		{
			if (curCubeEqualsArgCube)
				StartCoroutine(HandleCubeResizeAnim(nxtCubeIdx, true));
			else if (oldCubeIdx == -1)
				StartCoroutine(HandleCubeResizeAnim(nxtCubeIdx, false));
			else
				StartCoroutine(HandleCubeSwapAnim(oldCubeIdx, nxtCubeIdx));
		}
	}
	void HandleRotateCurCube(int rotationIdx, bool fastAction = false)
	{
		if (curCubeIdx < 0) { waiting = false; return; }
		var curCubeFaceColorIdxes = cubes[curCubeIdx].GetCubeFaceIdxes();
		var expectedRotation = Quaternion.Euler(Vector3.zero);
		/*
		 * 0	T
		 * 1234	FRBL
		 * 5	B
		 */
		switch (rotationIdx)
		{
			case 0: // Rotate CW
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 0, 2, 3, 4, 1, 5 }.Select(a => curCubeFaceColorIdxes[a]));
				expectedRotation = Quaternion.Euler(Vector3.down * 90);
				break;
			case 1: // Rotate CCW
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 0, 4, 1, 2, 3, 5 }.Select(a => curCubeFaceColorIdxes[a]));
				expectedRotation = Quaternion.Euler(Vector3.up * 90);
				break;
			case 2: // Tilt Up
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 1, 5, 2, 0, 4, 3 }.Select(a => curCubeFaceColorIdxes[a]));
				expectedRotation = Quaternion.Euler(Vector3.left * 90);
				break;
			case 3: // Tilt Down
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 3, 0, 2, 5, 4, 1 }.Select(a => curCubeFaceColorIdxes[a]));
				expectedRotation = Quaternion.Euler(Vector3.right * 90);
				break;
			case 4: // Tip Right
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 4, 1, 0, 3, 5, 2 }.Select(a => curCubeFaceColorIdxes[a]));
				expectedRotation = Quaternion.Euler(Vector3.forward * 90);
				break;
			case 5: // Tip Left
				cubes[curCubeIdx].AssignNewCubeFaceIdxes(new[] { 2, 1, 5, 3, 0, 4 }.Select(a => curCubeFaceColorIdxes[a]));
				expectedRotation = Quaternion.Euler(Vector3.back * 90);
				break;
			default:
				break;
		}
		if (fastAction)
		{
			cubes[curCubeIdx].UpdateCubeRenderers();
			//selectedCube.AssignNewCubeFaceIdxes(cubes[curCubeIdx].GetCubeFaceIdxes(), true);
			waiting = false;
		}
		else
			StartCoroutine(HandleCubeRotateAnim(expectedRotation, curCubeIdx));
	}
	IEnumerator HandleCubeRotateAnim(Quaternion offsetRotation, int curIdx = 0)
	{
		//selectedCube.AssignNewCubeFaceIdxes(cubes[curIdx].GetCubeFaceIdxes(), true);
		cubes[curIdx].UpdateCubeRenderers();
		for (float t = 0; t < 1f; t += Time.deltaTime * speed)
		{
			cubes[curIdx].transform.localRotation = Quaternion.LerpUnclamped(offsetRotation, Quaternion.Euler(Vector3.zero), t);
			yield return null;
		}
		cubes[curIdx].transform.localRotation = Quaternion.Euler(Vector3.zero);
		//for (var x = 0; x < cubes.Length; x++)
			
		waiting = false;
	}
	IEnumerator HandleCubeResizeAnim(int cubeIdx, bool restoring = false)
	{
		/*
		var localPosSelectedCube = cubes[cubeIdx].transform.localPosition;
		var localPosBigCube = selectedCube.transform.localPosition;
		var localScaleSelectedCube = cubes[cubeIdx].transform.localScale;
		var localScaleBigCube = selectedCube.transform.localScale;
		//selectedCube.AssignNewCubeFaceIdxes(cubes[cubeIdx].GetCubeFaceIdxes(), true);
		if (!restoring)
		{
			selectedCube.gameObject.SetActive(true);
			for (var x = 0; x < cubes.Length; x++)
				cubes[x].gameObject.SetActive(x != curCubeIdx);
		}*/
		mAudio.PlaySoundAtTransform(restoring ? "Whoosh" : "Whoop", transform);
		for (float t = 0; t < 1f; t += Time.deltaTime * speed)
        {
			cubes[cubeIdx].transform.localPosition = Vector3.LerpUnclamped(storedCubesLocalPos[cubeIdx], storedBigCubeLocalPos, restoring ? 1 - t : t);
			cubes[cubeIdx].transform.localScale = Vector3.LerpUnclamped(storedCubesLocalScale[cubeIdx], storedBigCubeLocalScale, restoring ? 1 - t : t);
			yield return null;
		}
		/*if (restoring)
		{
			selectedCube.gameObject.SetActive(false);
			for (var x = 0; x < cubes.Length; x++)
				cubes[x].gameObject.SetActive(x != curCubeIdx);
		}*/
		cubes[cubeIdx].transform.localPosition = restoring ? storedCubesLocalPos[cubeIdx] : storedBigCubeLocalPos;
		cubes[cubeIdx].transform.localScale = restoring ? storedCubesLocalScale[cubeIdx] : storedBigCubeLocalScale;
		waiting = false;
		yield break;
	}
	IEnumerator HandleCubeSwapAnim(int oldCubeIdx, int newCubeIdx)
	{
		/*var localPosOldCube = cubes[oldCubeIdx].transform.localPosition;
		var localPosNewCube = cubes[newCubeIdx].transform.localPosition;
		var localPosBigCube = selectedCube.transform.localPosition;
		var localScaleOldCube = cubes[oldCubeIdx].transform.localScale;
		var localScaleNewCube = cubes[newCubeIdx].transform.localScale;
		var localScaleBigCube = selectedCube.transform.localScale;
		selectedCube.AssignNewCubeFaceIdxes(cubes[oldCubeIdx].GetCubeFaceIdxes(), true);
		for (var x = 0; x < cubes.Length; x++)
			cubes[x].gameObject.SetActive(x != oldCubeIdx);*/
		mAudio.PlaySoundAtTransform("Whoosh", transform);
		for (float t = 0; t < 1f; t += Time.deltaTime * speed * 2f)
        {
			cubes[oldCubeIdx].transform.localPosition = Vector3.LerpUnclamped(storedBigCubeLocalPos, storedCubesLocalPos[oldCubeIdx], t);
			cubes[oldCubeIdx].transform.localScale = Vector3.LerpUnclamped(storedBigCubeLocalScale, storedCubesLocalScale[oldCubeIdx], t);
			yield return null;
		}
		cubes[oldCubeIdx].transform.localPosition = storedCubesLocalPos[oldCubeIdx];
		cubes[oldCubeIdx].transform.localScale = storedCubesLocalScale[oldCubeIdx];
		/*for (var x = 0; x < cubes.Length; x++)
			cubes[x].gameObject.SetActive(x != newCubeIdx);
		selectedCube.AssignNewCubeFaceIdxes(cubes[newCubeIdx].GetCubeFaceIdxes(), true);*/
		mAudio.PlaySoundAtTransform("Whoop", transform);
		for (float t = 0; t < 1f; t += Time.deltaTime * speed * 2f)
		{
			cubes[newCubeIdx].transform.localPosition = Vector3.LerpUnclamped(storedCubesLocalPos[newCubeIdx], storedBigCubeLocalPos, t);
			cubes[newCubeIdx].transform.localScale = Vector3.LerpUnclamped(storedCubesLocalScale[newCubeIdx], storedBigCubeLocalScale, t);
			yield return null;
		}
		cubes[newCubeIdx].transform.localPosition = storedBigCubeLocalPos;
		cubes[newCubeIdx].transform.localScale = storedBigCubeLocalScale;
		waiting = false;
		yield break;
	}
	void HandleSubmit(bool fastSubmit = false)
    {
		if (fastSubmit)
        {
			QuickLog("Submitted current state (Logged face order U, F, R, B, L, D):");
			foreach (var cube in cubes)
				QuickLog(cube.GetCubeFaceIdxes().Select(a => "RYGB"[a]).Join(", "));
			if (Enumerable.Range(1, 4).All(a => cubes.Select(b => b.GetCubeFaceIdxes()).Select(b => b[a]).Distinct().Count() == 4))
			{
				QuickLog("Submission valid.");
				modSelf.HandlePass();
				mAudio.PlaySoundAtTransform("XYRayThree", transform);
				moduleSolved = true;
				for (var p = 0; p < cubes.Length; p++)
					cubes[p].gameObject.SetActive(false);
				for (var p = 0; p < miscCubes.Length; p++)
				{
					miscCubes[p].gameObject.SetActive(true);
					miscCubes[p].AssignNewCubeFaceIdxes(cubes[p].GetCubeFaceIdxes(), true);
				}
				foreach (var groupedRenderers in checkerDisplays)
					foreach (MeshRenderer aRender in groupedRenderers.allRenderers)
                        aRender.enabled = true;
			}
			else
			{
				QuickLog("Submission invalid.");
				QuickLog("Following faces have duplicate colors: {0}", Enumerable.Range(1, 4).Where(a => cubes.Select(b => b.GetCubeFaceIdxes()).Select(b => b[a]).Distinct().Count() != 4).Select(a => "UFRBLD"[a]).Join(", "));
				modSelf.HandleStrike();
			}
			if (curCubeIdx != -1)
			{
				StartCoroutine(HandleCubeResizeAnim(curCubeIdx, true));
				curCubeIdx = -1;
			}
			else if (!moduleSolved)
				waiting = false;
		}
		else
			StartCoroutine(HandleSubmitAnim());
    }
	IEnumerator HandleSubmitAnim()
	{
		if (curCubeIdx != -1)
		{
			yield return HandleCubeResizeAnim(curCubeIdx, true);
			curCubeIdx = -1;
		}
		for (float t = 0; t < 1f; t += Time.deltaTime * speed)
		{
			for (var x = 0; x < cubes.Length; x++)
				cubes[x].transform.localScale = Vector3.Lerp(storedCubesLocalScale[x], Vector3.zero, Mathf.Clamp01(t));
			yield return null;
		}
		for (var x = 0; x < cubes.Length; x++)
			cubes[x].gameObject.SetActive(false);
		for (var x = 0; x < miscCubes.Length; x++)
		{
			var curCubeFaceIdxes = cubes[x].GetCubeFaceIdxes();
			miscCubes[x].gameObject.SetActive(true);
			miscCubes[x].AssignNewCubeFaceIdxes(curCubeFaceIdxes, true);
			for (float t = 0; t < 1f; t += Time.deltaTime * speed / 2f)
			{
				miscCubes[x].transform.localPosition = Vector3.Lerp(storedMiscCubesLocalPos[x] + Vector3.up * 3, storedMiscCubesLocalPos[x], Mathf.Clamp01(t));
				miscCubes[x].transform.localScale = Vector3.Lerp(Vector3.zero, storedMiscCubesLocalScale[x], Mathf.Clamp01(t));
				yield return null;
			}
			miscCubes[x].transform.localPosition = storedMiscCubesLocalPos[x];
			miscCubes[x].transform.localScale = storedMiscCubesLocalScale[x];
			mAudio.PlaySoundAtTransform(new[] { "sound2", "sound3", "sound5", "sound6" }[x], transform);
			for (var p = 0; p < 4; p++)
				checkerDisplays[p].allRenderers[curCubeFaceIdxes[p + 1] + 1].enabled = true;
		}
		yield return new WaitForSeconds(.1f);
		QuickLog("Submitted current state (Logged face order U, F, R, B, L, D):");
		foreach (var cube in cubes)
			QuickLog(cube.GetCubeFaceIdxes().Select(a => "RYGB"[a]).Join(", "));
		if (Enumerable.Range(1, 4).All(a => cubes.Select(b => b.GetCubeFaceIdxes()).Select(b => b[a]).Distinct().Count() == 4))
		{
			QuickLog("Submission valid.");
			modSelf.HandlePass();
			mAudio.PlaySoundAtTransform("XYRayThree", transform);
			moduleSolved = true;
			for (var x = 0; x < 5; x++)
			{
				for (var p = 0; p < 4; p++)
					checkerDisplays[p].allRenderers[0].enabled = x % 2 == 0;
				yield return new WaitForSeconds(0.1f);
			}
		}
		else
		{
			QuickLog("Submission invalid.");
			QuickLog("Following faces have duplicate colors: {0}", Enumerable.Range(1, 4).Where(a => cubes.Select(b => b.GetCubeFaceIdxes()).Select(b => b[a]).Distinct().Count() != 4).Select(a => "UFRBLD"[a]).Join(", "));
			modSelf.HandleStrike();
			for (var x = 0; x < 5; x++)
			{
				for (var p = 0; p < 4; p++)
					checkerDisplays[p].allRenderers[0].enabled = x % 2 == 0 && cubes.Select(b => b.GetCubeFaceIdxes()).Select(b => b[p + 1]).Distinct().Count() == 4;
				yield return new WaitForSeconds(0.1f);
			}
			yield return new WaitForSeconds(0.5f);
			foreach (var groupedRenderers in checkerDisplays)
				foreach (MeshRenderer aRender in groupedRenderers.allRenderers)
					aRender.enabled = false;
			for (var x = miscCubes.Length - 1; x >= 0; x--)
			{
				for (float t = 0; t < 1f; t += Time.deltaTime * speed / 2f)
				{
					miscCubes[x].transform.localPosition = Vector3.Lerp(storedMiscCubesLocalPos[x], storedMiscCubesLocalPos[x] + Vector3.up * 3, Mathf.Clamp01(t));
					miscCubes[x].transform.localScale = Vector3.Lerp(storedMiscCubesLocalScale[x], Vector3.zero, Mathf.Clamp01(t));
					yield return null;
				}
				miscCubes[x].transform.localPosition = storedMiscCubesLocalPos[x];
				miscCubes[x].transform.localScale = storedMiscCubesLocalScale[x];
				miscCubes[x].gameObject.SetActive(false);
			}


			for (var x = 0; x < cubes.Length; x++)
				cubes[x].gameObject.SetActive(true);
			for (float t = 0; t < 1f; t += Time.deltaTime * speed)
			{
				for (var x = 0; x < cubes.Length; x++)
					cubes[x].transform.localScale = Vector3.Lerp(Vector3.zero, storedCubesLocalScale[x], Mathf.Clamp01(t));
				yield return null;
			}
			for (var x = 0; x < cubes.Length; x++)
				cubes[x].transform.localScale = storedCubesLocalScale[x];
			waiting = false;
		}
    }
}
