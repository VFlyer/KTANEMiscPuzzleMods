using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimGenericScript : MonoBehaviour {
	public ButtonPushAnim affectedObject;
	public KMSelectable btnSelectable;

	// Use this for initialization
	void Start () {
		btnSelectable.OnInteract += delegate {
			affectedObject.AnimatePush();
			affectedObject.SetRetractState(false);
			return false;
		};
		btnSelectable.OnInteractEnded += delegate { affectedObject.SetRetractState(true); };
	}
}
