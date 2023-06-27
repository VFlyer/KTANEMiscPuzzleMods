using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPushAnim : MonoBehaviour {

	[SerializeField]
	private Transform selectedObject;
	public Vector3 deltaModifier;

	[SerializeField]
	private float pushMulti = 4f;
	float curPushVal = 0f, expectedPushVal = 0f;
	bool canRetract = true;

	Vector3 initialLocalPos;

	// Use this for initialization
	void Start () {
		initialLocalPos = selectedObject.localPosition;
	}

	public void SetRetractState(bool newValue)
    {
		canRetract = newValue;
    }


	// Update is called once per frame
	void Update () {
		if (canRetract)
			expectedPushVal = Mathf.Max(0, expectedPushVal - Time.deltaTime);
		if (curPushVal < expectedPushVal)
        {
			curPushVal += Time.deltaTime * pushMulti;
			if (curPushVal >= expectedPushVal)
				curPushVal = expectedPushVal;
        }
		else if (curPushVal > expectedPushVal)
        {
			curPushVal -= Time.deltaTime * pushMulti;
			if (curPushVal < expectedPushVal)
				curPushVal = expectedPushVal;
		}
		selectedObject.localPosition = initialLocalPos + deltaModifier * curPushVal;
	}

	public void AnimatePush()
    {
		expectedPushVal = 1f;
    }

}
