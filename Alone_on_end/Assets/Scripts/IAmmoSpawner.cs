using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AmmoSpawnSlot 
{
	public GameObject ammo;
	public Transform point;

	public AmmoSpawnSlot (Transform pointN) {
		int ammoType = Random.Range (0, WeaponChars.weapons.Length);
		GameObject ammoN = (GameObject)Resources.Load ("Prefabs/Ammo_" + ammoType);
		ammoN = IAmmoSpawner.Instantiate (ammoN, pointN);
		this.ammo = ammoN;
		this.point = pointN;
	}
}

public class IAmmoSpawner : MonoBehaviour {
	public static AmmoSpawnSlot[] slots;

	private void Start () {
		slots = new AmmoSpawnSlot[0];
		GameObject[] amPoints = GameObject.FindGameObjectsWithTag ("AmmoPoint");
		slots = new AmmoSpawnSlot[amPoints.Length];
		for (int i = 0; i < slots.Length; i++) {
			slots [i] = new AmmoSpawnSlot (amPoints[i].transform);
		}
		if (!IsInvoking("Refresh")) {
			Invoke ("Refresh", 0);
		}
	}

	public void Refresh () {
		for (int i = 0; i < slots.Length; i++) {
			if (!slots[i].ammo) {
				slots [i] = new AmmoSpawnSlot (slots[i].point);
			}
		}
		if (!IsInvoking("Refresh")) {
			Invoke ("Refresh", 25);
		}
	}
}
