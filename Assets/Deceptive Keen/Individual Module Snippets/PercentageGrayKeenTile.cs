using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PercentageGrayKeenTile : UniversalKeenTileModule {

	public KMSelectable moduleSelectable;
	public MeshRenderer moduleRenderer;
	public TextMesh percentageText;
	float timeHeld = 0f;
	bool isHolding = false;
	// Use this for initialization
	void Start () {
		moduleSelectable.OnInteract += delegate {
			isHolding = true;
			return false;
		};
		moduleSelectable.OnInteractEnded += delegate {
			isHolding = false;
			var timeHeldInt = Mathf.FloorToInt(timeHeld);
			if (timeHeldInt % 2 == 1 || timeHeldInt > 10)
				storedValue = -1;
			else
            {
				storedValue = timeHeldInt / 2 + 1;
            }
			timeHeld = 0;
		};
	}
	// Update is called once per frame
	void Update() {
		if (isHolding)
		{
			if (timeHeld < 10f)
                timeHeld += Time.deltaTime;
			else
				timeHeld = 10f;
			moduleRenderer.material.color = Color.white * (1f - (timeHeld / 10f));
			percentageText.text = (10f * timeHeld).ToString("0") + "%";
		}
	}
}
