using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;

[System.Serializable]
public class ISavable
{
	public Vector0 position = new Vector0 ();
	public float health = 100;
	public void ApplyDamage (int damage) {
		health -= damage;
		if (health < 0) {
			health = 0;
		}
	}
	public string name = "unnamed";
}
[System.Serializable]
public class Vector0
{
	public float x = 0;
	public float y = 0;
	public float z = 0;

	public Vector0 () {
		this.x = 0;
		this.y = 0;
		this.z = 0;
	}
	public Vector0 (float new_x, float new_y) {
		this.x = new_x;
		this.y = new_y;
		this.z = 0;
	}
	public Vector0 (float new_x, float new_y, float new_z) {
		this.x = new_x;
		this.y = new_y;
		this.z = new_z;
	}

	public float magnitude
	{
		get {
			return 0;
		}
	}

	public static implicit operator Vector3 (Vector0 v0) {
		return new Vector3 (v0.x, v0.y, v0.z);
	}
	public static implicit operator Vector0 (Vector3 v3) {
		return new Vector0 (v3.x, v3.y, v3.z);
	}
	public static Vector0 operator / (Vector0 a, float d)
	{
		return new Vector0 (a.x / d, a.y / d, a.z / d);
	}
	public static Vector0 operator * (Vector0 a, float d)
	{
		return new Vector0 (a.x * d, a.y * d, a.z * d);
	}
	public static Vector0 operator - (Vector0 a, Vector0 b) {
		return new Vector0 (a.x - b.x, a.y - b.y, a.z - b.z);
	}
	public static Vector0 operator + (Vector0 a, Vector0 b) {
		return new Vector0 (a.x + b.x, a.y + b.y, a.z + b.z);
	}
	public static Vector0 operator - (Vector0 a) {
		return new Vector0 (-a.x, -a.y, -a.z);
	}
	public static Vector0 Flat (Vector0 f) {
		f = new Vector3 (f.x, 0, f.z).normalized;
		return f;
	}
}
[System.Serializable]
public class ActionFrame
{
	public enum ActionType
	{
		Wait,
		Strike,
		OnlyMove,
		OnlyLook,
		MoveOnTime,
		MoveAndLook,
		StrikeOnTime,
		MoveAndStrike
	}
	public ActionType typeOfAction = ActionType.Wait;
	public Vector0 destination = new Vector0();
	public Vector0 lookPoint = new Vector0();
	public Vector0 addLook = new Vector0 ();
	public Transform body;
	public int strikesCount = 0;
	public float timeLength = 1f;
	public float timeStart = 0;
	public bool hasCompleted
	{
		get
		{
			bool has = false;
			if (typeOfAction == ActionType.Wait) {
				if (haveToTime < 0) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.Strike) {
				if (strikesCount < 1) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.StrikeOnTime) {
				if (strikesCount < 1 || haveToTime < 0) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.OnlyMove) {
				if (haveToGoMeters < 0.2f) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.OnlyLook) {
				if (haveToLookAngle) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.MoveOnTime) {
				if (haveToGoMeters < 0.2f || haveToTime < 0) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.MoveAndStrike) {
				if (strikesCount < 1) {
					has = true;
				}
			}
			if (typeOfAction == ActionType.MoveAndLook) {
				if (haveToGoMeters < 0.2f && haveToLookAngle) {
					has = true;
				}
			}
			return has;
		}
	}
	public float haveToTime
	{
		get
		{
			return timeLength - (Time.time - timeStart);
		}
	}
	public float haveToGoMeters
	{
		get
		{
			return (new Vector3(destination.x, body.position.y, destination.z) - body.position).magnitude;
		}
	}
	public bool haveToLookAngle
	{
		get {
			Vector3 curDir = body.forward;
			Vector3 needDir = Vector0.Flat((Vector3)lookPoint - body.position);
			return Vector3.Angle (curDir, needDir) < 10;
		}
	}
	/// <summary>
	/// Only go to point, end only if destinetion has completed
	/// </summary>
	/// <param name="dest">Destination.</param>
	public ActionFrame (Transform trans,Vector3 dest) {
		this.destination = dest;
		this.lookPoint = dest + Vector3.up * IHuman.headHeight;
		this.strikesCount = 0;
		this.timeLength = 1;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.OnlyMove;
		this.body = trans;
	}
	/// <summary>
	/// Only look at point. Completed, if look delta angle less that 10.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="look">Look.</param>
	public ActionFrame (Transform trans, Vector0 look) {
		this.destination = trans.position;
		this.lookPoint = look;
		this.strikesCount = 0;
		this.timeLength = 1;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.OnlyLook;
		this.body = trans;
	}
	/// <summary>
	/// If you want to strike at position. Completed, when losted strikes.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="look">Look.</param>
	/// <param name="strikes">Strikes.</param>
	public ActionFrame (Transform trans, Vector0 look, int strikes) {
		this.destination = trans.position;
		this.lookPoint = look;
		this.strikesCount = strikes;
		this.timeLength = 1;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.Strike;
		this.body = trans;
	}
	/// <summary>
	/// Will moves and strike at point. Will be completed, when will be losted strikes,
	/// but if body will be in place of dest, completed too.
	/// </summary>
	/// <param name="dest">Destination.</param>
	/// <param name="look">Look.</param>
	/// <param name="strikes">Strikes.</param>
	public ActionFrame (Transform trans,Vector0 dest, Vector0 look, int strikes) {
		this.destination = dest;
		this.lookPoint = look;
		this.strikesCount = strikes;
		this.timeLength = 1;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.MoveAndStrike;
		this.body = trans;
	}/// <summary>
	/// Move and look. Completed, when moved and looked.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="dest">Destination.</param>
	/// <param name="look">Look.</param>
	public ActionFrame (Transform trans, Vector0 dest, Vector0 look) {
		this.destination = dest;
		this.lookPoint = look;
		this.strikesCount = 0;
		this.timeLength = 1;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.MoveAndLook;
		this.body = trans;
	}
	public ActionFrame (Transform trans,Vector0 look, int strikes, float time) {
		this.destination = trans.position;
		this.lookPoint = look;
		this.strikesCount = strikes;
		this.timeLength = time;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.StrikeOnTime;
		this.body = trans;
	}
	/// <summary>
	/// Only wait.
	/// </summary>
	/// <param name="trans">Trans.</param>
	/// <param name="look">Look.</param>
	/// <param name="strikes">Strikes.</param>
	/// <param name="time">Time.</param>
	public ActionFrame (Transform trans, float time) {
		this.destination = trans.position;
		this.lookPoint = trans.forward + trans.position + Vector3.up * IHuman.headHeight;
		this.strikesCount = 0;
		this.timeLength = time;
		this.timeStart = Time.time;
		this.typeOfAction = ActionType.StrikeOnTime;
		this.body = trans;
	}
}
[System.Serializable]
public class IStatus : ISavable
{
	public int[] patrones_= {24, 90};
	public int[] patrones_in = {8, 30};
	public int currentWeapon = 0;

	public int[] items;

	public float infection = 0;
	public float hunger = 0;
}
[System.Serializable]
public class WeaponChars
{
	public int damage = 15;
	public int shotsPerSecond = 5;
	public int patrontage = 15;

	public WeaponChars (int dmg, int sps, int ptr) {
		this.damage = dmg;
		this.shotsPerSecond = sps;
		this.patrontage = ptr;
	}

	public static WeaponChars[] weapons
	{
		get {
			WeaponChars w_0_sig = new WeaponChars (8, 2, 8);
			WeaponChars w_1_ak = new WeaponChars (15, 10, 30);
			WeaponChars w_2_toz = new WeaponChars (50, 2, 8);
			WeaponChars w_3_sniper = new WeaponChars (100, 1, 1);
			WeaponChars w_4_m16 = new WeaponChars (10, 30, 300);

			WeaponChars[] w = {w_0_sig, w_1_ak, w_2_toz, w_3_sniper, w_4_m16};

			return w;
		}
	}
}

public class IHuman : Registrable {
	public IStatus savable;
	public Animator anims;
	public Transform trans;
	public ActionFrame currentFrame;
	private static Camera cameraGetter;
	public GameObject[] weapons;
	private NavMeshAgent agent;
	private Transform agentTrans;
	public float walkSpeed = 2;
	[Header("For player")]
	public float camEulerY = 45;
	public float camEulerX = 5;
	public Collider[] damageLevels;
	public Rigidbody body;
	public AudioSource heart;
	public Vector3 path;
	private Light flashlight;

	private void OnCollisionEnter (Collision other) {
		if (other.relativeVelocity.magnitude > 11) {
			ApplyDamage ((int)other.relativeVelocity.magnitude * 7);
		}
	}


	public static IHuman GetPlayer () {
		IHuman finded = null;
		if (!playerGetter) {
			IHuman[] humans = DataCenter.dataCenter.humans;
			for (int i = 0; i < humans.Length; i++) {
				if (humans [i].savable.name == "player") {
					finded = humans [i];
					playerObj = finded.gameObject;
					playerGetter = finded;
					break;
				}
			}
		} else {
			finded = playerGetter;
		}
		return finded;
	}
	private static IHuman playerGetter;
	public static GameObject playerObj;

	private void PathControl () {
		if ((agent.destination - (Vector3)currentFrame.destination).magnitude > 0.5f) {
			agent.SetDestination (currentFrame.destination);
		}
		path = agentTrans.position;
		if ((path - trans.position).magnitude > 0.5f) {
			MoveAt (path - trans.position);
		}
	}


	public void SinhroWeapon () {
		for (int i = 0; i < weapons.Length; i++) {
			weapons [i].SetActive (i == savable.currentWeapon);
		}
	}

	public void SetWeapon (int index) {
		if (canFire) {
			if (index == 4) {
				if (savable.name != "player") {
					index = 1;
					savable.patrones_ [3] = 0;
					savable.patrones_in [3] = 0;
				}
			}
			savable.currentWeapon = index - 1;
			SinhroWeapon ();
		}
	}

	public static Camera cameraMain
	{
		get {
			if (!cameraGetter) {
				cameraGetter = Camera.main;
			}
			return cameraGetter;
		}
	}
	private static Transform cameraTransGetter;
	public static Transform cameraTransformMain
	{
		get {
			if (!cameraTransGetter) {
				if (Camera.main) {
					cameraTransGetter = Camera.main.transform;
				}
			}
			return cameraTransGetter;
		}
	}
	private static Transform cameraParentGetter;
	public static Transform cameraMainParent
	{
		get {
			if (!cameraParentGetter) {
				cameraParentGetter = Camera.main.transform.parent;
			}
			return cameraParentGetter;
		}
	}

	public static void PlaySound (string name, Vector3 pos) {
		AudioClip clip = (AudioClip)Resources.Load ("Sounds/" + name);
		GameObject obj = new GameObject ();
		obj.transform.position = pos;
		AudioSource s = obj.AddComponent <AudioSource>();
		s.volume = 0.2f;
		s.clip = clip;
		s.loop = false;
		s.maxDistance = 300;
		s.spatialBlend = 1;
		s.Play ();
		if (clip) {
			Destroy (obj, clip.length);
		} else {
			Destroy (obj);
		}
	}
	public static void PlaySoundInParent (string name, Transform parent, Vector3 pos) {
		AudioClip clip = (AudioClip)Resources.Load ("Sounds/" + name);
		GameObject obj = new GameObject ();
		obj.transform.parent = parent;
		obj.transform.localPosition = pos;
		AudioSource s = obj.AddComponent <AudioSource>();
		s.volume = 0.2f;
		s.clip = clip;
		s.maxDistance = 20;
		s.loop = false;
		s.spatialBlend = 1;
		s.Play ();
		if (clip) {
			Destroy (obj, clip.length);
		} else {
			Destroy (obj);
		}
	}

	public Transform firePlace
	{
		get {
			if (firePlaceObjectGetter && firePlaceObjectGetter.activeInHierarchy && firePlaceGetter) {
				return firePlaceGetter;
			} else {
				Transform mine = null;
				GameObject[] places = GameObject.FindGameObjectsWithTag ("FirePlace");

				for (int i = 0; i < places.Length; i++) {
					Transform cur = places [i].transform;
					if (cur.IsChildOf (trans)) {
						mine = cur;
						break;
					}
				}
				firePlaceGetter = mine;
				if (mine) {
					firePlaceObjectGetter = mine.gameObject;
				}
				return mine;
			}
		}
	}
	public Transform firePlaceGetter;
	private GameObject firePlaceObjectGetter;

	public static Vector3 FromCamera (Vector3 origin) {
		Vector3 f = cameraTransformMain.forward;
		Vector3 r = cameraTransformMain.right;
		Vector3 u = cameraTransformMain.up;
		return f * origin.z + r * origin.x + u * origin.y;
	}
	public static Vector3 FromCameraFlat (Vector3 origin) {
		Vector3 f = cameraTransformMain.forward;
		Vector3 r = cameraTransformMain.right;
		f = Vector0.Flat (f);
		r = Vector0.Flat (r);
		return f * origin.z + r * origin.x;
	}
	float animWalkX;
	float animWalkY;
	float animLook;
	float animLookY;
	float animCrouch;
	float animWeapon;
	public static float headHeight = 1.6f;
	public Vector3 GetLookDirection () {
		return ((Vector3)currentFrame.lookPoint - (trans.position + Vector3.up * headHeight)).normalized + trans.TransformDirection(currentFrame.addLook) * 0.01f * WeaponChars.weapons[savable.currentWeapon].damage / 2f;
	}
	private void Animate () {
		
		Vector2 legs = GetVectorForLegs ();

		float speed = Time.deltaTime * 4;

		Vector3 lookR = GetLookDirection ();

		float l = Mathf.Asin(lookR.y) * Mathf.Rad2Deg;

		float la = l / 90f;

		float lY = (camEulerY - falledEulerY) / 90f;

		if (!falled) {
			lY = 0;
		}

		animLookY = Mathf.Lerp (animLookY, lY, speed * 2);

		animWalkX = Mathf.Lerp (animWalkX, legs.x, speed);
		animWalkY = Mathf.Lerp (animWalkY, legs.y, speed);
		if (!float.IsNaN(la)) {
			animLook = Mathf.Lerp (animLook, la, speed * 2);
		}
		animWeapon = Mathf.Lerp (animWeapon, savable.currentWeapon, speed);
		animCrouch = Mathf.Lerp (animCrouch, crouch, speed);

		anims.SetFloat ("WalkLR", animWalkX);
		anims.SetFloat ("WalkBF", animWalkY);
		anims.SetFloat ("Crouch", animCrouch);
		anims.SetFloat ("Weapon", animWeapon);
		anims.SetBool ("Reload", reloading);

		anims.SetFloat ("Look", animLook);
		anims.SetFloat ("LookY", animLookY);
	}

	public void ApplyDamage (int damage) {
		savable.ApplyDamage (damage);
		IBullet.DropBlood (trans.position + Vector3.up * 1.4f, trans.rotation);
	}
	Collider[] myColls;
	public bool IsMyColl (Collider c)
	{
		bool s = false;
		for (int i = 0; i < myColls.Length; i++) {
			if (myColls[i] == c) {
				s = true;
				break;
			}
		}
		return s;
	}
	private bool onGround {
		get {
			List<Collider> c = new List<Collider> ();
			int count = 0;
			c.AddRange(Physics.OverlapSphere (trans.position + Vector3.up * 0.15f, 0.25f));
			for (int i = 0; i < c.Count; i++) {
				if (!IsMyColl(c[i])) {
					count++;
				}
			}
			return count > 0;
		}
	}

	public void Jump () {
		if (onGround && !falled) {
			body.AddRelativeForce (Vector3.up * 250 * body.mass);
		}
	}

	private float crouch
	{
		get {

			float c = 0;

			if (onGround) {
				if (savable.name == "player") {
					c = Input.GetAxis ("Crouch");
					if (!(c > 0)) {
						if (IScreen.crouch) {
							c = 1;
						}
					}
				}
			} else {
				c = -1;
			}

			return c;
		}
	}

	private void Fire_End () {
		return;
	}

	private bool canFire
	{
		get {
			return !IsInvoking ("Fire_End") && !reloading;
		}
	}

	public Vector2 GetVectorForLegs () {
		Vector3 vel = Vector0.Flat(body.velocity);

		Vector3 dir = trans.InverseTransformDirection (vel);

		if (savable.name == "player") {
			dir = Joystick.move_input;
			dir = new Vector3 (dir.x, 0, dir.y);
		}

		Vector2 tr = new Vector2 (dir.x, dir.z);

		return tr;
	}

	public GameObject camObj;

	protected override void Awake () {
		body = GetComponent<Rigidbody> ();
		agent = GetComponentInChildren<NavMeshAgent> ();
		agentTrans = agent.transform;
		anims = GetComponent<Animator> ();
		trans = transform;
		SetWeapon (savable.currentWeapon + 1);
		flashlight = GetComponentInChildren<Light> ();
		heart = GetComponentInChildren<AudioSource> ();
		if (savable.name != "player") {
			RagdollOff ();
		}
		Collider[] c = GetComponentsInChildren<Collider> ();
		myColls = new Collider[c.Length + 1];
		for (int i = 0; i < c.Length; i++) {
			myColls [i] = c [i];
		}
		myColls [c.Length] = GetComponent<Collider> ();
		base.Awake ();
	}
	private void Update () {
		if (camObj) {
			camObj.SetActive (savable.name == "player");
		}
		Animate ();
		HumanUpdate ();
	}
	public void LightTurnOffOn () {
		this.flashlight.enabled = !flashlight.enabled;
	}
	private Transform cameraFlag
	{
		get {
			if (!cameraFlagGetter) {
				GameObject g = new GameObject ();
				g.name = "Flag";
				g.transform.position = Vector3.zero;
				cameraFlagGetter = g.transform;
			}
			return cameraFlagGetter;
		}
	}
	//private Rigidbody[] anchors;
	private void RagdollOff () {
		Rigidbody[] rgs = GetComponentsInChildren<Rigidbody> ();
		//CharacterJoint[] js = GetComponentsInChildren<CharacterJoint> ();
		//anchors = new Rigidbody[js.Length];
		//for (int i = 0; i < js.Length; i++) {
		//	anchors [i] = js [i].connectedBody;
		//	js [i].connectedBody = null;
		//}

		foreach (Rigidbody rg in rgs) {
			if (rg != body) {
				rg.constraints = RigidbodyConstraints.FreezeRotation;
				rg.useGravity = false;
				rg.GetComponent<Collider> ().enabled = false;
			}
		}
	}
	private void RagdollOn () {
		Rigidbody[] rgs = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rg in rgs) {
			if (rg != body) {
				rg.constraints = RigidbodyConstraints.None;
				rg.useGravity = true;
				rg.GetComponent<Collider> ().enabled = true;
			//	CharacterJoint j = rg.GetComponent<CharacterJoint> ();
			//	Rigidbody t = rg.GetComponentInParent<Rigidbody>();
			//	if (j) {
			//		j.connectedBody = t;
			//	}
			}
		}
		Destroy (body);
		Destroy (GetComponent<Collider>());
	}
	private Transform cameraFlagGetter;
	public float cameraDist = 2;
	public float cameraDistRight = 0.5f;
	private void FixedUpdate () {
		if (savable.name == "player") {
			//Vector3 pos = trans.position + trans.right * cameraDistRight + Vector3.up * 1.6f;
			//Ray ray_n = new Ray(pos, -cameraMainParent.forward);
			//RaycastHit hit;
			//float h = cameraDist;
			//if (Physics.Raycast(ray_n, out hit, h)) {
			//	h = hit.distance - 0.25f;
			//}
			//cameraMainParent.position = Vector3.Slerp(cameraMainParent.position, pos, 0.1f); //Vector3.Slerp(cameraTransformMain.position, ray_n.GetPoint(h), Time.fixedDeltaTime * 8);
			//cameraTransformMain.localPosition = -Vector3.forward * h;
			float x = 0;
			float y = 0;
			Vector2 trn = Joystick.turn_input + Joystick.additiveInput * 4;
			x = trn.x;
			y = trn.y;
			camEulerY += x * Time.deltaTime * 70 * Settings.current.sens;
			camEulerX -= y * Time.deltaTime * 70 * Settings.current.sens;
			float maxX = 70;
			float minX = -70;
			if (falled) {
				maxX = 85;
				minX = -85;
				camEulerY = Mathf.Clamp (camEulerY, falledEulerY - 85, falledEulerY + 85);
			}
			camEulerX = Mathf.Clamp (camEulerX, minX, maxX);
			cameraFlag.eulerAngles = new Vector3 (camEulerX, camEulerY, 0);
		}
	}
	private bool isHumanOnLine
	{
		get {
			Ray ray = new Ray (trans.position + Vector3.up * 1.4f, trans.forward);
			bool cast = false;
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				if (hit.collider.GetComponentInParent<IHuman>()) {
					cast = true;
				}
			}
			return cast;
		}
	}
	private float walk_speed_k = 1;
	private void HumanUpdate () {
		savable.health += Time.deltaTime / 2;
		savable.health = Mathf.Clamp (savable.health, 0, 100);
		float speed = Time.deltaTime * 4;
		currentFrame.addLook = Vector3.Slerp (currentFrame.addLook, Vector3.zero, speed);
		if (heart) {
			if (savable.name == "player") {
				float dist = 10000;
				IZombie z = IZombie.ZombieNearbyPosition(trans.position, 10000, false);
				if (z) {
					dist = (trans.position - z.trans.position).magnitude;
				}
				float vol = 1 / dist;
				heart.volume = vol;
			} else {
				heart.volume = 0;
			}
		}
		if (agentTrans.localPosition.magnitude > 4f) {
			agentTrans.localPosition = agentTrans.localPosition.normalized * 4f;
		}
		walk_speed_k = walkSpeed / 2 + walkSpeed / 2 * (1 - crouch);
		if (savable.name == "player") {
			Control ();
		} else {
			AI ();
		}
		LookAt ();
		if (savable.health < 1) {
			Die ();
		}
		if (savable.health < 25 && !falled) {
			Fall ();
		}
	}
	private void Control () {
		Quaternion r = Quaternion.LookRotation (trans.forward);
		if (firePlace) {
			r = firePlace.rotation;
		}
		cameraMainParent.rotation = Quaternion.Slerp (cameraMainParent.rotation, r, Time.deltaTime * 12);
		Vector3 input = Vector3.zero;
		Vector2 pt = Joystick.move_input;
		input = new Vector3 (pt.x, 0, pt.y);
		if (!Application.isMobilePlatform) {
			if (Input.GetMouseButton(0)) {
				Fire ();
			}
		}
		Vector3 dir = FromCameraFlat(input);
		MoveAt (dir);
		/*Ray ray = cameraMain.ScreenPointToRay (Input.mousePosition);
		float sin = Mathf.Abs((ray.GetPoint (1) - ray.origin).y);
		float height = Mathf.Abs((cameraTransformMain.position - trans.position).y) - 1.6f;

*/

		currentFrame.lookPoint = trans.position + Vector3.up * headHeight + cameraFlag.forward * 100;


		for (int i = 0; i < 6; i++) {
			if (Input.GetKeyDown("" + i)) {
				SetWeapon (i);
			}
		}
		if (Input.GetKeyDown("r")) {
			Reload ();
		}
		if (Input.GetKeyDown("l")) {
			LightTurnOffOn ();
		}
		if (Input.GetKeyDown("z")) {
			if (falled && savable.health > 25) {
				OutFall ();
			} else {
				Fall ();
			}
		}
		if (Input.GetButtonDown("Jump")) {
			Jump ();
		}
	}
	private void ReloadOnTime (float time) {
		Invoke ("EndReload", time);
	}
	public void Reload () {
		if (!reloading && savable.patrones_[savable.currentWeapon] > 0 && Mathf.Abs(savable.patrones_in[savable.currentWeapon] - WeaponChars.weapons[savable.currentWeapon].patrontage) > 0) {
			if (!reloading) {
				AudioClip cl = (AudioClip)Resources.Load ("Sounds/Reload_" + savable.currentWeapon);
				PlaySoundInParent ("Reload_" + savable.currentWeapon, trans, Vector3.up * 6);
				Invoke ("EndReload", cl.length);
			}
		}
	}
	private void EndReload () {
		if (savable.currentWeapon != 2) {
			int toAdd = savable.patrones_ [savable.currentWeapon];
			int ptr = WeaponChars.weapons [savable.currentWeapon].patrontage;
			if (toAdd < ptr) {
				ptr = toAdd;
			}
			int ito = (WeaponChars.weapons [savable.currentWeapon].patrontage - savable.patrones_in [savable.currentWeapon]);


			ito = Mathf.Clamp (ito, 0, toAdd);

			if (ito > 0) {
				toAdd -= ito;

				savable.patrones_ [savable.currentWeapon] = toAdd;
				savable.patrones_in [savable.currentWeapon] += ito;
			}
		} else {
			int toAdd = 1;
			if (savable.patrones_[savable.currentWeapon] > 0 && savable.patrones_in[savable.currentWeapon] < WeaponChars.weapons[savable.currentWeapon].patrontage) {
				savable.patrones_ [savable.currentWeapon] -= toAdd;
				savable.patrones_in [savable.currentWeapon] += toAdd;
			}
		}
	}
	private void LookAt () {
		if (currentFrame.typeOfAction == ActionFrame.ActionType.MoveAndLook ||
		    currentFrame.typeOfAction == ActionFrame.ActionType.MoveAndStrike ||
		    currentFrame.typeOfAction == ActionFrame.ActionType.MoveAndLook ||
		    currentFrame.typeOfAction == ActionFrame.ActionType.OnlyLook ||
		    currentFrame.typeOfAction == ActionFrame.ActionType.Strike ||
		    currentFrame.typeOfAction == ActionFrame.ActionType.StrikeOnTime ||
		    savable.name == "player") {

			if (!falled) {
				Vector3 look = Vector0.Flat (GetLookDirection ());
				trans.rotation = Quaternion.Slerp (trans.rotation, Quaternion.LookRotation (look), Time.deltaTime * 12);
			}
		}
	}
	private bool reloading
	{
		get {
			return IsInvoking ("EndReload");
		}
	}
	public void Fire () {
		if (canFire) {
			if (savable.patrones_in [savable.currentWeapon] > 0) {
				Transform mine = firePlace;
				if (mine) {
					GameObject bullet = (GameObject)Resources.Load ("Prefabs/Bullet");
					GameObject bt = Instantiate (bullet, mine.position, Quaternion.LookRotation (mine.forward));
					IBullet ibu = bt.GetComponent<IBullet> ();
					ibu.damage = WeaponChars.weapons [savable.currentWeapon].damage;
					ibu.Start ();
					if (savable.currentWeapon == 2) {
						//ibu.BurstCartech ();
					}
					if (savable.currentWeapon == 3) {
						AudioClip cl = (AudioClip)Resources.Load ("Sounds/FireWeapon_3");
						ReloadOnTime (cl.length);
					}
					GameObject splash = (GameObject)Resources.Load ("Prefabs/FireObj");
					GameObject sp = Instantiate (splash, mine.position, Quaternion.LookRotation (mine.forward));
					Destroy (sp, 0.1f);
					currentFrame.addLook = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f), 0);
					string name = "FireWeapon_" + savable.currentWeapon;

					PlaySound (name, mine.position);
					savable.patrones_in [savable.currentWeapon] -= 1;
				}
			} else {
				string name = "NoFire";

				PlaySound (name, trans.position + Vector3.up * 1.6f);
			}
			currentFrame.strikesCount -= 1;
			Invoke ("Fire_End", (1f / WeaponChars.weapons[savable.currentWeapon].shotsPerSecond) * Random.Range(0.5f, 1.5f));
		}
	}
	public static IHuman[] GetNearByAll (Vector3 pos, float maxDist) {
		IHuman[] humans = DataCenter.dataCenter.humans;
		float dist = maxDist;
		return humans.Where ((IHuman hum) => (hum.trans.position - pos).magnitude < dist).ToArray();

	}
	public static IHuman HumanNearbyPosition (Vector3 position, bool noCast, float maxDist)
	{
		IHuman[] humans = DataCenter.dataCenter.humans;
		IHuman finded = null;
		float dist = maxDist;

		for (int i = 0; i < humans.Length; i++) {
			float cur = (humans [i].trans.position - position).magnitude;
			if (!noCast) {
				if (cur < dist) {
					finded = humans [i];
					dist = cur;
				}
			} else {
				if (cur < dist && !Physics.Linecast (position + Vector3.up * headHeight,
					humans [i].trans.position + Vector3.up * headHeight)) {
					finded = humans [i];
					dist = cur;
				}
			}
		}
		return finded;
	}
	private bool hasAnyAmmo
	{
		get {
			int a = 0;
			for (int i = 0; i < savable.patrones_.Length; i++) {
				a += savable.patrones_ [i];
			}
			showsAnyAmmo = a;
			return a > 8;
		}
	}
	public int showsAnyAmmo;
	private void AI () {
		if (currentFrame.hasCompleted) {

			ActionFrame n = new ActionFrame (trans, 1);

			Vector3 dest = trans.position;

			if (!hasAnyAmmo) {
				dest = ammoNear.position;
				n = new ActionFrame (trans, dest);
			} else {
				if (zombieNearby) {
					if ((zombieNearby.trans.position - trans.position).magnitude < 5) {
						dest = trans.position - trans.forward * 5;
					}
					n = new ActionFrame (trans, dest, zombieNearby.head.position, Random.Range (1, 6));
				}
			}

			currentFrame = n;
		}
		if (currentFrame.typeOfAction == ActionFrame.ActionType.MoveAndLook ||
			currentFrame.typeOfAction == ActionFrame.ActionType.MoveAndStrike ||
			currentFrame.typeOfAction == ActionFrame.ActionType.MoveOnTime ||
			currentFrame.typeOfAction == ActionFrame.ActionType.OnlyMove) {
			PathControl ();
		}
		if (currentFrame.strikesCount > 0 && savable.patrones_in[savable.currentWeapon] > 0) {
			if (!isHumanOnLine) {
				Fire ();
			}
		}
		if (savable.patrones_ [savable.currentWeapon] < 1) {
			int more = savable.currentWeapon;
			for (int i = 0; i < savable.patrones_.Length; i++) {
				if (savable.patrones_ [i] > 0) {
					more = i;
					break;
				}
			}
			SetWeapon (more + 1);
		}
		if (!hasAnyAmmo) {
			currentFrame.strikesCount = 0;
		}
		if (!reloading) {
			if (savable.patrones_in [savable.currentWeapon] < 1) {
				Reload ();
			}
		}
		flashlight.enabled = ITimecycle.night;
	}
	private Transform ammoNear
	{
		get {
			AmmoSpawnSlot[] slots = IAmmoSpawner.slots;

			float dist = 10000;
			Transform ammo = null;
			for (int i = 0; i < slots.Length; i++) {
				float cur = (trans.position - slots [i].point.position).magnitude;
				if (slots[i].ammo && cur < dist) {
					ammo = slots [i].point;
					dist = cur;
				}
			}
			return ammo;
		}
	}
	public IZombie zombieNearby {
		get {
			float dist = 25;
			if (ITimecycle.night) {
				dist = 15;
			}
			IZombie[] all = DataCenter.dataCenter.zombies;
			IZombie finded = null;
			for (int i = 0; i < all.Length; i++) {
				float cur = (all[i].trans.position - trans.position).magnitude;
				if (cur < dist) {
					if (!Physics.Linecast(all[i].trans.position + Vector3.up * headHeight, trans.position + Vector3.up * headHeight)) {
						finded = all [i];
						dist = cur;
					}
				}
			}
			return finded;
		}
	}
	private void MoveAt (Vector3 direction) {
		if (onGround && !falled) {
			direction = Vector0.Flat (direction);
			Vector3 vel = direction * walkSpeed * walk_speed_k;
			/*if (body.velocity.magnitude < walkSpeed * walk_speed_k) {
			body.AddForce (vel * walkForce * Time.deltaTime);
		}*/
			body.velocity = new Vector3 (vel.x, body.velocity.y, vel.z);
		}
	}
	public float walkForce = 1000;
	public bool falled = false;
	public float falledEulerY;
	public void Fall () {
		if (savable.name == "player") {
			anims.Play ("Death");
			falled = true;
			IZombie z = zombieNearby;
			if (z) {
				trans.rotation = Quaternion.LookRotation (Vector0.Flat(z.trans.position - trans.position));
			}
			falledEulerY = trans.eulerAngles.y;
		}
	}
	public void OutFall () {
		if (savable.health > 25) {
			anims.Play ("Main");
			falled = false;
		}
	}
	private void Die () {
		if (!falled) {
			anims.Play ("Death");
		}
		if (savable.name != "player") {
			Destroy (anims);
			RagdollOn ();
		}
		//anims.SetFloat ("Look", 0);
		float time = 15;
		if (savable.name == "player") {
			time = 7;
		}
		Destroy (gameObject, time);
		Destroy (this);
	}
}






















