using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour {

	const float SEC_IN_DAY = 86400f;
	const float ratio = 3600f;

	static float currentTimeOfDay = 0;

	// Use this for initialization
	void Start () {
	}

	//Steps daytime by exactly one second. Called once per second.
	void stepTime() {
		//currentTimeOfDay %= SEC_IN_DAY;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log (Time.time);

		//Debug.Log ("Current time: " + currentTimeOfDay + "/" + SEC_IN_DAY);

		Vector3 oldRotation = this.gameObject.transform.rotation.eulerAngles;

		currentTimeOfDay = (Time.time * ratio);
		float xRotation = 360f * currentTimeOfDay / SEC_IN_DAY;
		Debug.Log (xRotation);
		Vector3 newRotation = new Vector3 (xRotation, oldRotation.y, oldRotation.z);
		this.gameObject.transform.rotation = Quaternion.Euler (newRotation);
	}
}
