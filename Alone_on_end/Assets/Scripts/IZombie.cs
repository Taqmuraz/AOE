using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

[System.Serializable]
public class PartOfBody
{
	public GameObject rend;
	public Collider coll;

	public void CatchOff () {
		SkinnedMeshRenderer smr = rend.GetComponent<SkinnedMeshRenderer> ();
		GameObject catched = new GameObject ("PartOfBody(" + rend.name + ")");
		MeshRenderer mr = catched.AddComponent<MeshRenderer> ();
		mr.material = smr.material;
		MeshFilter mf = catched.AddComponent<MeshFilter> ();
		mf.mesh = smr.sharedMesh;
		Transform trans = catched.transform;
		Transform rTrans = rend.transform;
		Transform cTrans = coll.transform;
		trans.eulerAngles = cTrans.eulerAngles;
		trans.position = cTrans.position;
		trans.localScale = rTrans.localScale * rTrans.parent.lossyScale.y;
		//BoxCollider bc = 
		catched.AddComponent<BoxCollider> ();
		//Rigidbody rg = 
		catched.AddComponent<Rigidbody> ();
		//rg.mass = 5;
		MonoBehaviour.Destroy (rend);
		MonoBehaviour.Destroy (coll.GetComponent<CharacterJoint> ());
		MonoBehaviour.Destroy (coll.GetComponent<Rigidbody> ());
		MonoBehaviour.Destroy (coll);
		MonoBehaviour.Destroy (catched, 10);
	}
}

public class IZombie : Registrable {
	public ISavable savable;

	public NavMeshAgent agent;
	public Transform trans;
	public Animator anims;

	public Collider[] damageLevels;

	public PartOfBody[] partsOfZ;

	public Transform head;

	public static int dead = 0;

	public bool findMode = true;

	public float angleView = 30;

	public GameObject visiblePart;
	public Collider basic_coll;

	public void SetVisible (bool vis) {
		if (vis != vis_getter) {
			visiblePart.SetActive (vis);
			vis_getter = vis;
			basic_coll.enabled = !vis;
		}
	}
	private bool vis_getter = true;

	protected override void Awake () {
		agent = GetComponent<NavMeshAgent> ();
		trans = transform;
		anims = GetComponentInChildren<Animator> ();
		head = partsOfZ [4].coll.transform;
		count++;
		SetRagdoll (false);
		basic_coll = GetComponent<Collider> ();
		base.Awake ();
	}
	private void CheckVisible () {
		IHuman h = IHuman.GetPlayer ();
		if (h && IHuman.cameraMain && IHuman.cameraTransformMain) {
			float angle = IHuman.cameraMain.fieldOfView;
			Vector3 myDir = (head.position - h.trans.position).normalized;
			Vector3 hisDir = IHuman.cameraTransformMain.forward;
			bool headV = Vector3.Angle(myDir, hisDir) < angle;
			myDir = ((trans.position + (head.position - trans.position).normalized) - h.trans.position).normalized;
			bool middle = Vector3.Angle(myDir, hisDir) < angle;
			myDir = (trans.position - h.trans.position).normalized;
			bool foot = Vector3.Angle(myDir, hisDir) < angle;
			SetVisible (headV || middle || foot);
		}
	}
	private int updateFrameIndex = 0;
	private void Update () {
		Animate ();
		CheckVisible ();
		if (updateFrameIndex > IMainMenu.GetUpdateFramesPerSecond() / 10) {
			if (canDoSomething) {
				AI ();
			} else {
				if (attacking && who_under_attack) {
					Vector3 look = Vector0.Flat(who_under_attack.trans.position - trans.position);
					Quaternion n = Quaternion.LookRotation (look);
					trans.rotation = Quaternion.Slerp (trans.rotation, n, Time.deltaTime * 3);
				}
				Stop ();
			}
			if (savable.health < 1 || (hasNoArms && hasNoLegs) || hasNoHead) {
				Die ();
			}
			updateFrameIndex = 0;
		}
		updateFrameIndex++;
	}
	private void Stop () {
		agent.destination = trans.position;
	}
	private void AI () {
		if (humanNearby) {
			float distHear = 2;
			if (findMode) {
				distHear = 10000;
			}
			float angle = Vector3.Angle (trans.forward, (humanNearby.trans.position - trans.position).normalized);
			if (angle < angleView || (humanNearby.trans.position - trans.position).magnitude < distHear) {
				if ((humanNearby.trans.position - trans.position).magnitude < 2) {
					Attack (humanNearby);
				} else {
					MoveTo (humanNearby.trans.position);
				}
			}
		}
	}
	public void SetRagdoll (bool rag) {
		Rigidbody[] bs = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody b in bs) {
			b.isKinematic = !rag;
		}
		anims.enabled = !rag;
	}
	public static IZombie[] GetNearByAll (Vector3 pos, float maxDist) {
		IZombie[] humans = DataCenter.dataCenter.zombies;
		List<IZombie> finded = new List<IZombie> ();
		float dist = maxDist;

		for (int i = 0; i < humans.Length; i++) {
			float cur = (humans [i].trans.position - pos).magnitude;
			if (cur < dist) {
				finded.Add(humans [i]);
			}
		}
		return finded.ToArray ();
	}
	public Vector3 lastDamageDirection;
	public Rigidbody lastDamageColl;
	private void Die () {
		SetVisible (true);
		Destroy (basic_coll);
		SetRagdoll (true);
		Rigidbody r = lastDamageColl;
		if (r) {
			r.velocity = lastDamageDirection / 5;
		}
		anims.SetFloat ("Death", Random.Range(0, 2));
		anims.Play ("Death");
		Destroy (GetComponent<AudioSource>());
		Destroy (agent);
		IHuman.PlaySound ("Death_" + Random.Range(0, 2), trans.position + Vector3.up);
		dead++;
		count--;
		Destroy (gameObject, 15);
		Destroy (this);
	}
	private void MoveTo (Vector3 to) {
		agent.SetDestination (to);
	}
	float legs_anim = 0;
	private void Animate () {
		if (vis_getter) {
			anims.SetFloat ("Walk", agent.velocity.magnitude / agent.speed);
			int a = 0;
			if (attacking) {
				a = 1;
			}
			int p = -1;
			if (pain) {
				p = 0;
			}
			float speed = Time.deltaTime * 3;
			if (hasNoLegs) {
				legs_anim = Mathf.Lerp (legs_anim, 1, speed);
			} else {
				legs_anim = Mathf.Lerp (legs_anim, 0, speed);
			}
			anims.SetFloat ("Attack", a);
			anims.SetFloat ("Pain", p);
			anims.SetFloat ("Active", active);
			anims.SetFloat ("HasNoLegs", legs_anim);
		}
	}
	private float active
	{
		get {
			float a = 0;
			if (humanNearby) {
				a = Mathf.Clamp01(3 / (humanNearby.trans.position - trans.position).magnitude);
			}
			return a;
		}
	}
	public static IZombie ZombieNearbyPosition (Vector3 position, float far, bool noCast)
	{
		IZombie[] humans = DataCenter.dataCenter.zombies;
		IZombie finded = null;
		float dist = far;
		for (int i = 0; i < humans.Length; i++) {
			float cur = (humans [i].trans.position - position).magnitude;
			if (!noCast) {
				if (cur < dist) {
					finded = humans [i];
					dist = cur;
				}
			} else {
				if (cur < dist && !Physics.Linecast (position + Vector3.up * 1.8f,
					humans [i].trans.position + Vector3.up * 1.8f)) {
					finded = humans [i];
					dist = cur;
				}
			}
		}
		return finded;
	}
	public static int count = 0;
	private IHuman humanNearby
	{
		get {
			IHuman[] humans = DataCenter.dataCenter.humans;
			IHuman finded = null;
			float dist = 7;
			if (findMode) {
				dist = 10000;
			}
			for (int i = 0; i < humans.Length; i++) {
				float cur = (humans [i].trans.position - trans.position).magnitude;
				if (findMode) {
					if (cur < dist) {
						finded = humans[i];
						dist = cur;
					}
				} else {
					if (cur < dist && !Physics.Linecast(trans.position + Vector3.up * 1.8f,
						humans[i].trans.position + Vector3.up * 1.8f)) {
						finded = humans[i];
						dist = cur;
					}
				}
			}
			return finded;
		}
	}
	private bool attacking
	{
		get {
			return IsInvoking ("End_Attack");
		}
	}
	private bool pain
	{
		get {
			return IsInvoking ("PainEnd");
		}
	}
	private bool hasNoLegs
	{
		get {
			return !partsOfZ [0].rend || !partsOfZ [1].rend;
		}
	}
	private bool hasNoArms
	{
		get {
			return !partsOfZ [2].rend && !partsOfZ [3].rend;
		}
	}
	private bool hasNoHead
	{
		get {
			return !partsOfZ [4].rend;
		}
	}
	private bool canDoSomething
	{
		get {
			return !pain && !attacking;
		}
	}
	private void Attack (IHuman who) {
		if (canDoSomething) {
			who_under_attack = who;
			anims.SetFloat ("AttackK", Random.Range(0, 3));
			Invoke ("End_Attack", 1.5f);
		}
	}
	public void ApplyPain (int damage) {
		savable.ApplyDamage (damage);
		if (!hasNoLegs) {
			anims.Play ("Pain");
			Invoke ("PainEnd", 1);
		}
	}
	private void PainEnd () {
		return;
	}
	public int damage = 10;
	private void End_Attack () {
		if (Vector3.Angle(trans.forward, (who_under_attack.trans.position - trans.position).normalized) < angleView) {
			if ((who_under_attack.trans.position - trans.position).magnitude < 2) {
				who_under_attack.ApplyDamage (damage + Random.Range(0, damage));
			}
		}
	}
	private IHuman who_under_attack;
}
