using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBullet : MonoBehaviour {
	public float force = 100;
	public float damage = 25;
	public float time;
	private LineRenderer rend;
	private Transform trans;
	private List<Vector3> poss = new List<Vector3> ();
	public void Start () {
		if (!started) {
			trans = transform;
			Rigidbody body = GetComponent<Rigidbody>();
			body.AddRelativeForce (Vector3.forward * force);
			rend = GetComponentInChildren<LineRenderer> ();
			time = Time.time;
			poss.Add (trans.position);
			started = true;
		}
	}
	private bool started = false;
	private void Update () {
		if ((Time.time - time) > 5) {
			Destroy (gameObject);
		}
		if (rend) {
			if ((trans.position - poss[poss.Count - 1]).magnitude > 2) {
				AddPos ();
			}
			List<Vector3> p = poss;
			p.Add (trans.position);
			rend.SetPositions (p.ToArray());
		}
	}
	private void AddPos () {
		poss.Add (trans.position);
		if (poss.Count > 4) {
			poss.RemoveAt (0);
		}
	}
	public void BurstCartech () {
		int r = Random.Range (5, 12);
		GameObject bullet = (GameObject)Resources.Load ("Prefabs/Bullet");
		for (int i = 0; i < r; i++) {
			GameObject bt = Instantiate (bullet, trans.position, Quaternion.LookRotation (trans.forward));
			IBullet ibu = bt.GetComponent<IBullet> ();
			ibu.damage = damage;
			ibu.Start ();
		}
	}
	public void OnCollisionEnter (Collision other) {

		IZombie zombie = other.collider.GetComponentInParent<IZombie> ();
		Rigidbody a = other.collider.attachedRigidbody;
		if (zombie) {
			float x = 1;
			for (int i = 0; i < zombie.damageLevels.Length; i++) {
				if (other.collider == zombie.damageLevels[i]) {
					x = Mathf.Pow(i + 1, 2);
				}
			}
			zombie.ApplyPain ((int)(damage * x + Random.Range(0, damage * x)));
			DropBlood (trans.position, trans.rotation);

			for (int i = 0; i < zombie.partsOfZ.Length; i++) {
				if (zombie.partsOfZ[i].coll == other.collider) {
					zombie.partsOfZ [i].CatchOff ();
				}
			}
			if (a) {
				zombie.lastDamageColl = a;
				zombie.lastDamageDirection = -other.relativeVelocity * damage / 50;
			}
		}
		if (a) {
			a.velocity = -other.relativeVelocity * damage / 200f;
		}

		/*IHuman human = other.collider.GetComponentInParent<IHuman> ();

		if (human) {
			if (human.savable.name != "player") {
				float x = 1;
				for (int i = 0; i < human.damageLevels.Length; i++) {
					if (other.collider == human.damageLevels[i]) {
						x = Mathf.Pow(i + 1, 2);
					}
				}
				human.savable.ApplyDamage ((int)(damage * x + Random.Range(0, damage * x)));
				DropBlood (trans.position, trans.rotation);
			}
		}*/

		if (!other.collider.GetComponent<IBullet>()) {
			float r = Random.Range (0f, 100f);
			if (r < 90) {
				Destroy (gameObject);
			}
		}
	}
	public static void DropBlood (Vector3 pos, Quaternion rot) {

		GameObject blood = (GameObject)Resources.Load ("Prefabs/BloodObj");
		GameObject b = Instantiate (blood, pos, Quaternion.Euler(rot.eulerAngles + Vector3.forward * Random.Range(0, 360)));

		Ray ray = new Ray (pos, Vector3.down);

		RaycastHit[] hits = Physics.RaycastAll (ray, 5);

		RaycastHit hit = new RaycastHit ();
		bool hitten = false;

		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].collider is MeshCollider) {
				hit = hits [i];
				hitten = true;
				break;
			}
		}
		if (hitten) {
			Vector3 n = hit.point + hit.normal * 0.1f;
			Quaternion r = Quaternion.Euler(Quaternion.LookRotation (hit.normal).eulerAngles + Vector3.forward * Random.Range(0, 360));
			GameObject blood_trail = Instantiate (blood, n, r);
			Destroy (blood_trail, 15);
		}

		Destroy (b, 0.1f);
	}
}
