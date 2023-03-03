using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class AdjustedCubeDottedRenderer : MonoBehaviour {

	public enum Axis{
		None,
		X,
		Y,
		Z
	}

	public Vector3Int countAxises;
	public Vector3 individualCubeSize, offsetStartOrigin, outlineSize;
	public MeshRenderer[] xLineRenderers, yLineRenderers, zLineRenderers, dotRenderers;
	Vector3Int curIdxPos;
	// Use this for initialization
	void Start () {
		StartCoroutine(TestRenders());
	}
	IEnumerator TestRenders(float delay = 0.2f)
    {
		var iterationCombinedCounts = countAxises.x * countAxises.y * countAxises.z;
		for (var x = 0; enabled; x = (x + 1) % iterationCombinedCounts)
		{
			ChangeIdxPos(x % countAxises.x, x / countAxises.x % countAxises.y, x / countAxises.x / countAxises.y);
			RenderByAxis(Axis.None);
			yield return new WaitForSeconds(delay);
			RenderByAxis(Axis.X);
			yield return new WaitForSeconds(delay);
			RenderByAxis(Axis.Y);
			yield return new WaitForSeconds(delay);
			RenderByAxis(Axis.Z);
			yield return new WaitForSeconds(delay);
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
				{
					xLineRenderers[x].enabled = true;
					xLineRenderers[x].transform.localScale = new Vector3(individualCubeSize.x, outlineSize.y, outlineSize.z);
					xLineRenderers[x].transform.localPosition = new Vector3(individualCubeSize.x * curIdxPos.x + individualCubeSize.x / 2 + offsetStartOrigin.x, xLineRenderers[x].transform.localPosition.y, xLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < yLineRenderers.Length; x++)
				{
					yLineRenderers[x].enabled = x % (countAxises.x + 1) == curIdxPos.x || x % (countAxises.x + 1) == curIdxPos.x + 1;
					yLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, individualCubeSize.y * countAxises.y, outlineSize.z);
					yLineRenderers[x].transform.localPosition = new Vector3(yLineRenderers[x].transform.localPosition.x, individualCubeSize.y * countAxises.y / 2 + offsetStartOrigin.y, yLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < zLineRenderers.Length; x++)
				{
					zLineRenderers[x].enabled = x % (countAxises.x + 1) == curIdxPos.x || x % (countAxises.x + 1) == curIdxPos.x + 1;
					zLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, outlineSize.y, individualCubeSize.z * countAxises.z);
					zLineRenderers[x].transform.localPosition = new Vector3(zLineRenderers[x].transform.localPosition.x, zLineRenderers[x].transform.localPosition.y, individualCubeSize.z * countAxises.z / 2 + offsetStartOrigin.z);
				}
				for (var x = 0; x < dotRenderers.Length; x++)
					dotRenderers[x].enabled = curIdxPos.x == x % (countAxises.x + 1) || curIdxPos.x + 1 == x % (countAxises.x + 1);
				break;
			case Axis.Y:
				for (var x = 0; x < xLineRenderers.Length; x++)
				{
					xLineRenderers[x].enabled = x % (1 + countAxises.y) == curIdxPos.y || x % (1 + countAxises.y) == curIdxPos.y + 1;
					xLineRenderers[x].transform.localScale = new Vector3(individualCubeSize.x * countAxises.x, outlineSize.y, outlineSize.z);
					xLineRenderers[x].transform.localPosition = new Vector3(individualCubeSize.x * countAxises.x / 2 + offsetStartOrigin.x, xLineRenderers[x].transform.localPosition.y, xLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < yLineRenderers.Length; x++)
				{
					yLineRenderers[x].enabled = true;
					yLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, individualCubeSize.y, outlineSize.z);
					yLineRenderers[x].transform.localPosition = new Vector3(yLineRenderers[x].transform.localPosition.x, individualCubeSize.y * curIdxPos.y + individualCubeSize.y / 2 + offsetStartOrigin.y , yLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < zLineRenderers.Length; x++)
				{
					zLineRenderers[x].enabled = x / (countAxises.x + 1) == curIdxPos.y || x / (countAxises.x + 1) == curIdxPos.y + 1;
					zLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, outlineSize.y, individualCubeSize.z * countAxises.z);
					zLineRenderers[x].transform.localPosition = new Vector3(zLineRenderers[x].transform.localPosition.x, zLineRenderers[x].transform.localPosition.y, individualCubeSize.z * countAxises.z / 2 + offsetStartOrigin.z);
				}
				for (var x = 0; x < dotRenderers.Length; x++)
					dotRenderers[x].enabled = curIdxPos.y == x / (countAxises.x + 1) % (countAxises.y + 1) || curIdxPos.y + 1 == x / (countAxises.x + 1) % (countAxises.y + 1);
				break;
			case Axis.Z:
				for (var x = 0; x < xLineRenderers.Length; x++)
				{
					xLineRenderers[x].enabled = x / (1 + countAxises.y) == curIdxPos.z || x / (1 + countAxises.y) == curIdxPos.z + 1;
					xLineRenderers[x].transform.localScale = new Vector3(individualCubeSize.x * countAxises.x, outlineSize.y, outlineSize.z);
					xLineRenderers[x].transform.localPosition = new Vector3(individualCubeSize.x * countAxises.x / 2 + offsetStartOrigin.x, xLineRenderers[x].transform.localPosition.y, xLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < yLineRenderers.Length; x++)
				{
					yLineRenderers[x].enabled = x / (countAxises.x + 1) == curIdxPos.z || x / (countAxises.x + 1) == curIdxPos.z + 1;
					yLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, individualCubeSize.y * countAxises.y, outlineSize.z);
					yLineRenderers[x].transform.localPosition = new Vector3(yLineRenderers[x].transform.localPosition.x, individualCubeSize.y * countAxises.y / 2 + offsetStartOrigin.y, yLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < zLineRenderers.Length; x++)
				{
					zLineRenderers[x].enabled = true;
					zLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, outlineSize.y, individualCubeSize.z);
					zLineRenderers[x].transform.localPosition = new Vector3(zLineRenderers[x].transform.localPosition.x, zLineRenderers[x].transform.localPosition.y, individualCubeSize.z * curIdxPos.z + individualCubeSize.z / 2 + offsetStartOrigin.z);
				}
				for (var x = 0; x < dotRenderers.Length; x++)
					dotRenderers[x].enabled = curIdxPos.z == x / (countAxises.x + 1) / (countAxises.y + 1) || curIdxPos.z + 1 == x / (countAxises.x + 1) / (countAxises.y + 1);
				break;
			case Axis.None:
				for (var x = 0; x < xLineRenderers.Length; x++)
				{
					xLineRenderers[x].enabled = true;
					xLineRenderers[x].transform.localScale = new Vector3(individualCubeSize.x * countAxises.x, outlineSize.y, outlineSize.z);
					xLineRenderers[x].transform.localPosition = new Vector3(individualCubeSize.x * countAxises.x / 2 + offsetStartOrigin.x, xLineRenderers[x].transform.localPosition.y, xLineRenderers[x].transform.localPosition.z);
                }
				for (var x = 0; x < yLineRenderers.Length; x++)
				{
					yLineRenderers[x].enabled = true;
					yLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, individualCubeSize.y * countAxises.y, outlineSize.z);
					yLineRenderers[x].transform.localPosition = new Vector3(yLineRenderers[x].transform.localPosition.x, individualCubeSize.y * countAxises.y / 2 + offsetStartOrigin.y, yLineRenderers[x].transform.localPosition.z);
				}
				for (var x = 0; x < zLineRenderers.Length; x++)
				{
					zLineRenderers[x].enabled = true;
					zLineRenderers[x].transform.localScale = new Vector3(outlineSize.x, outlineSize.y, individualCubeSize.z * countAxises.z);
					zLineRenderers[x].transform.localPosition = new Vector3(zLineRenderers[x].transform.localPosition.x, zLineRenderers[x].transform.localPosition.y, individualCubeSize.z * countAxises.z / 2 + offsetStartOrigin.z);
				}
				for (var x = 0; x < dotRenderers.Length; x++)
					dotRenderers[x].enabled = true;
				break;
        }
    }
}
