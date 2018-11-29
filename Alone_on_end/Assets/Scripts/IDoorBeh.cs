using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDoorBeh : MonoBehaviour {
	private Transform trans;
	public Vector3 startEuler;
	public float addEuler = 0;
	public bool opened = false;


	private void Start () {
		trans = transform;
		startEuler = trans.localEulerAngles;
	}
	private void Update () {
		IHuman h = IHuman.HumanNearbyPosition (trans.position, false, 3);
		IZombie z = IZombie.ZombieNearbyPosition (trans.position, 3, false);
		opened = h || z;

		int r = 0;
		if (opened) {
			r = 1;
		}
		float speed = Time.deltaTime * 4f;
		addEuler = Mathf.Lerp (addEuler, r * 120, speed);
		trans.localEulerAngles = new Vector3 (startEuler.x, startEuler.y, startEuler.z + addEuler);
	}
}
