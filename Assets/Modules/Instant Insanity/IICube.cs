using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IICube : MonoBehaviour {

	public MeshRenderer[] cubeFaces;
	public Material[] possibleMaterials;
	int[] assignedCubeIdxes;

	public void AssignNewCubeFaceIdxes(IEnumerable<int> newIdxes)
    {
		assignedCubeIdxes = newIdxes.ToArray();
    }
	public int[] GetCubeFaceIdxes()
    {
		return assignedCubeIdxes;
    }

	public void UpdateCubeRenderers(int offsetIdxes = 0)
    {
		if (assignedCubeIdxes == null || assignedCubeIdxes.Length != 6)
		{
			Debug.LogWarning("Improper assignment for cube idxes. Skipping.");
			return;
		}
        for (var x = 0; x < 6; x++)
			cubeFaces[x].material = possibleMaterials[assignedCubeIdxes[x] + offsetIdxes];
    }

}
