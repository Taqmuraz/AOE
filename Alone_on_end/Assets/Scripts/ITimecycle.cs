using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITimecycle : MonoBehaviour {
	public static float time = 12f;
	public float timeSpeed = 1;
	public float showTime;
	public static bool night
	{
		get {
			return time > 18 || time < 8;
		}
	}
	public Transform directional;
	public Light lightD;

	private static Camera[] cameras = new Camera[0];

	private void OnDestroy () {
		cameras = new Camera[0];
	}

	public static void UpdateCameras () {
		if (IHuman.playerObj) {
			cameras = Resources.FindObjectsOfTypeAll<Camera> ();
		}
		Debug.Log ("UPDATED : " + cameras.Length);
	}

	private void Update () {
		showTime = time;
		directional.eulerAngles = Vector3.right * (-90 + time * 15);
		time += Time.deltaTime / 50 * timeSpeed;
		float c = (1 - Mathf.Abs ((time - 12) / 12f)) * 0.7f - 0.25f;
		lightD.intensity = c;
		Color cl = new Color (c, c, c, c);;
		RenderSettings.fogColor = cl;
		Camera[] cams = cameras;
		for (int i = 0; i < cams.Length; i++) {
			if (cams[i]) {
				cams [i].backgroundColor = cl;
			}
		}
		if (time > 24) {
			time = 0;
		}
	}
}
