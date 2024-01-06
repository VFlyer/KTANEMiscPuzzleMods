using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontTestScript : MonoBehaviour {

	public Font[] usedFonts;
	public Vector3 offsetsAll, initialOffset, initialRotationsAll;
	public string[] textTests;
	public float charSizeRepeat;
	public int fontSizeRepeat;
	// Use this for initialization
	void Start () {
		for (var x = 0; x < usedFonts.Length; x++)
		{
			var newObject = new GameObject("Font Test");
			newObject.transform.parent = transform;
			var renderer = newObject.AddComponent<MeshRenderer>();
			var txtMeshNew = newObject.AddComponent<TextMesh>();
			txtMeshNew.text = textTests.Join("\n");
			txtMeshNew.fontSize = fontSizeRepeat;
			txtMeshNew.characterSize = charSizeRepeat;
			txtMeshNew.font = usedFonts[x];
			txtMeshNew.anchor = TextAnchor.MiddleCenter;
			renderer.material = usedFonts[x].material;
			newObject.transform.localPosition = initialOffset + offsetsAll * x;
			newObject.transform.localRotation = Quaternion.Euler(initialRotationsAll);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
