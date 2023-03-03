using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CubeDottedRenderer : MonoBehaviour {

	public enum Axis{
		None,
		X,
		Y,
		Z
	}

	public Vector3Int countAxises;
	public MeshRenderer[] xLineRenderers, yLineRenderers, zLineRenderers, dotRenderers;
	Vector3Int curIdxPos;
	// Use this for initialization
	void Start () {
		StartCoroutine(TestRenders());
	}
	IEnumerator TestRenders()
    {
		var iterationCountCounts = countAxises.x * countAxises.y * countAxises.z;
		for (var x = 0; x < iterationCountCounts; x++)
		{
			ChangeIdxPos(x % countAxises.x, x / countAxises.x % countAxises.y, x / countAxises.x / countAxises.y);
			RenderByAxis(Axis.None);
			yield return new WaitForSeconds(1f);
			RenderByAxis(Axis.X);
			yield return new WaitForSeconds(1f);
			RenderByAxis(Axis.Y);
			yield return new WaitForSeconds(1f);
			RenderByAxis(Axis.Z);
			yield return new WaitForSeconds(1f);
		}
		yield break;
    }
	public void ChangeIdxPos(Vector3 nextVector3)
    {
		ChangeIdxPos((int)nextVector3.x, (int)nextVector3.y, (int)nextVector3.z);
    }
	public void ChangeIdxPos(int nextX, int nextY, int nextZ)
    {
		curIdxPos = new Vector3Int(nextX, nextY, nextZ);
    }

	public void RenderByAxis(Axis axis = Axis.None)
    {
		switch (axis)
        {
			case Axis.X:
				for (var x = 0; x < xLineRenderers.Length; x++)
					xLineRenderers[x].enabled = curIdxPos.x == x / (countAxises.y - 1) % countAxises.z || curIdxPos.x + 1 == x / (countAxises.y - 1) % countAxises.z;
				for (var x = 0; x < yLineRenderers.Length; x++)
					yLineRenderers[x].enabled = true;
				for (var x = 0; x < zLineRenderers.Length; x++)
					zLineRenderers[x].enabled = true;
				for (var x = 0; x < dotRenderers.Length; x++)
					dotRenderers[x].enabled = curIdxPos.x == x / countAxises.y % countAxises.z || curIdxPos.x + 1 == x / countAxises.y % countAxises.z;
				break;
			case Axis.None:
				for (var x = 0; x < xLineRenderers.Length; x++)
					xLineRenderers[x].enabled = true;
				for (var x = 0; x < yLineRenderers.Length; x++)
					yLineRenderers[x].enabled = true;
				for (var x = 0; x < zLineRenderers.Length; x++)
					zLineRenderers[x].enabled = true;
				for (var x = 0; x < dotRenderers.Length; x++)
					dotRenderers[x].enabled = true;
				break;
        }
    }
}
