using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControler : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void LateUpdate () {
        ScreenGrab();
	}
    void ScreenGrab()
    {
        var directory = new System.IO.DirectoryInfo(Application.dataPath);
        var path = System.IO.Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd-H-mm-ss")));
        Debug.Log("Taking screenshot to " + path);
        ScreenCapture.CaptureScreenshot(path + "screenshot");

    }
}
