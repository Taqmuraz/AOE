using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAmmo : MonoBehaviour {

	public Transform child;
	public int type = 0;
	public int count = 8;

	private void Update () {
		child.localEulerAngles += Vector3.up * Time.deltaTime * 90f;
	}

	private void OnTriggerEnter (Collider other) {
		IHuman h = other.GetComponentInParent<IHuman> ();
		if (h) {
			h.savable.patrones_ [type] += count;
			Destroy (gameObject);
		}
	}
}
