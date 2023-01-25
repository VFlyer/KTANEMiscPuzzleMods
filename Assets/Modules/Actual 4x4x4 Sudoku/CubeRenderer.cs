using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CubeRenderer : MonoBehaviour {

	public enum Axis{
		None,
		X,
		Y,
		Z
	}

	public Vector3 countAxises;
	public MeshRenderer[] xLineRenderers, yLineRenderers, zLineRenderers, dotRenderers;
	Vector3 curIdxPos;
	// Use this for initialization
	void Start () {
	}
	public void RenderByAxis(Axis axis = Axis.None)
    {
		switch (axis)
        {
			case Axis.None:
				for (var x = 0; x < xLineRenderers.Length; x++)
					xLineRenderers[x].enabled = true;
				for (var x = 0; x < yLineRenderers.Length; x++)
					xLineRenderers[x].enabled = true;
				for (var x = 0; x < zLineRenderers.Length; x++)
					xLineRenderers[x].enabled = true;
				for (var x = 0; x < dotRenderers.Length; x++)
					xLineRenderers[x].enabled = true;
				break;
        }			
    }
	// Update is called once per frame
	void Update () {

	}
}
