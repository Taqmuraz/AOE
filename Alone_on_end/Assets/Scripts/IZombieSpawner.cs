using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IZombieSpawner : MonoBehaviour {
	public float interval = 5;
	public int maxCount = 10;
	public bool limited = false;
	public int limit = 10;
	public bool isFinders = true;
	public bool human = false;
	private void Start () {
		Invoke ("Spawn", Random.Range(0f, interval));
		if (!human) {
			maxCount = Settings.current.maxZombie;
		}
	}

	public void Spawn () {
		int l = 0;
		float max = maxCount;
		/*if (!ITimecycle.night) {
			max = maxCount / 2;
		}*/
		if (human) {
			l = DataCenter.dataCenter.humans.Length;
		} else {
			l = DataCenter.dataCenter.zombies.Length;
		}
		if ((l < max && limit > 0) || (!IHuman.playerObj && human)) {
			GameObject sp = (GameObject)Resources.Load ("Prefabs/ZombieRag_" + Random.Range(0, 1));
			if (!human) {
				sp = Instantiate (sp, transform.position + (Vector3)Vector0.Flat(Random.onUnitSphere), Quaternion.identity, transform);
				IZombie z = sp.GetComponent<IZombie> ();
				z.findMode = isFinders;
				z.savable.health = 100 + IZombie.dead * 10;
			} else {
				if (IHuman.playerObj) {
					sp = (GameObject)Resources.Load ("Prefabs/HumanRag");
					sp = Instantiate (sp, transform.position + (Vector3)Vector0.Flat(Random.onUnitSphere), Quaternion.identity, transform);
					IHuman h = sp.GetComponent<IHuman> ();
					h.savable.name = "BOT";
					Destroy (h.camObj);
				} else {
					sp = (GameObject)Resources.Load ("Prefabs/Human");
					sp = Instantiate (sp, transform.position + (Vector3)Vector0.Flat(Random.onUnitSphere), Quaternion.identity, transform);
					IHuman h = sp.GetComponent<IHuman> ();
					h.savable.name = "player";
					IHuman.GetPlayer ();
					ITimecycle.UpdateCameras ();
				}
			}
		}
		if (limited) {
			limit -= 1;
		}
		if (!IsInvoking("Spawn")) {
			Invoke ("Spawn", interval + Random.Range(0f, interval));
		}
	}
}
