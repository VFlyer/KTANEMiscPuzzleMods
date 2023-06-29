using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSetRenderersScript : MonoBehaviour {
	public MeshRenderer[] allRenderers;
	// Use this for initialization
	public void SetRenderersStateAll(params bool[] values) {
		for (var x = 0; x < allRenderers.Length; x++)
			allRenderers[x].enabled = values[x];
	}
}
