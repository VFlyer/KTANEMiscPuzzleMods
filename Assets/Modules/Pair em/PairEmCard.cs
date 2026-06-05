using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairEmCard : MonoBehaviour {
	public MeshRenderer[] border;
	public MeshRenderer frontRender, backRender;
	public Transform affectedObject;
	public TextMesh cbTextMesh;
	public IEnumerator animHandler;
}
