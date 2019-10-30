using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour {
	int screenshotcount = 0;
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F)) {
			screenshotcount++;
			string filename = "Screenshot" + screenshotcount + ".png";
			ScreenCapture.CaptureScreenshot (filename);
			Debug.Log (filename + " has been saved");
		}
	}
}
