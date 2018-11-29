using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IScreen : MonoBehaviour {

	public RawImage patronesType;
	public Text patrones;
	private IHuman player;
	public Text health;
	public RectTransform radar;
	public Text killed;
	public RectTransform crossHair;
	private float rangeView = 60;
	public RawImage weapon;

	public GameObject mobileInput;

	private void Awake () {
		mobileInput.SetActive (Application.isMobilePlatform);
	}

	private void Start () {
		IHuman[] humans = DataCenter.dataCenter.humans;
		for (int i = 0; i < humans.Length; i++) {
			if (humans[i].savable.name == "player") {
				player = humans [i];
			}
		}
	}

	private void Update () {
		if (player) {
			Sinhro ();
		} else {
			player = IHuman.GetPlayer ();
		}
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	private bool fire = false;
	public void ToFire () {
		fire = true;
	}
	public void OutFire () {
		fire = false;
	}

	public void Fire () {
		if (player) {
			player.Fire ();
		}
	}

	public void ToOrOutCrouch () {
		crouch = !crouch;
	}
	public void Cross () {
		cross = !cross;
	}
	public void Jump () {
		if (player) {
			player.Jump ();
		}
	}
	public void WakeUp () {
		if (player) {
			if (!player.falled) {
				player.Fall ();
			} else {
				player.OutFall ();
			}
		}
	}
	public void Reload () {
		if (player) {
			player.Reload ();
		}
	}
	public void FlashLight () {
		if (player) {
			player.LightTurnOffOn ();
		}
	}
	public void WeaponPlus () {
		if (player) {
			player.savable.currentWeapon++;
			if (!(player.savable.currentWeapon < WeaponChars.weapons.Length)) {
				player.savable.currentWeapon = 0;
			}
			player.SetWeapon (player.savable.currentWeapon + 1);
		}
	}
	public void WeaponMinus () {
		if (player) {
			player.savable.currentWeapon--;
			if (player.savable.currentWeapon < 0) {
				player.savable.currentWeapon = WeaponChars.weapons.Length - 1;
			}
			player.SetWeapon (player.savable.currentWeapon + 1);
		}
	}

	private static bool _crouch = false;
	public static bool crouch
	{
		get {
			return !Application.isMobilePlatform ? Input.GetKey (KeyCode.LeftControl) : _crouch;
		}
		set {
			_crouch = value;
		}
	}

	public static bool _cross = false;
	public static bool cross
	{
		get {
			return !Application.isMobilePlatform ? Input.GetMouseButton(1) : _cross;
		}
		set {
			_cross = value;
		}
	}

	private void SinhroCross () {
		RaycastHit hit;
		Ray ray = new Ray (player.firePlace.position, player.firePlace.forward);
		Vector3 point = IHuman.cameraTransformMain.position + IHuman.cameraTransformMain.forward * 100;
		if (Physics.Raycast(ray, out hit, 4)) {
			point = hit.point;
			if (Physics.Raycast (ray, out hit, hit.distance)) {
				point = hit.point;
			}
		}
		Vector3 pos = IHuman.cameraMain.WorldToScreenPoint (point);
		crossHair.position = pos;
	}

	private void Sinhro () {
		if (Input.GetButtonDown("Cancel")) {
			IMainMenu.ToMenu ();
		}
		weapon.texture = (Texture)Resources.Load ("Sprites/Weapon_" + player.savable.currentWeapon);
		if (fire) {
			Fire ();
		}
		//SinhroCross ();
		Transform cm = IHuman.cameraMainParent;
		Transform fp = player.firePlace;
		float speed = Time.deltaTime * 8;
		if (cross) {
			rangeView = 30;
			Vector3 v = fp.position - fp.forward + fp.up * 0.02f;
			Debug.DrawRay (v, fp.forward);
			cm.position = Vector3.Slerp (cm.position, v, speed);
		} else {
			rangeView = 60;
			cm.localPosition = Vector3.Slerp(cm.localPosition, new Vector3(0, 0.0014f, 0.0013f), speed);
		}
		IHuman.cameraMain.fieldOfView = Mathf.Lerp (IHuman.cameraMain.fieldOfView, rangeView, 0.25f);
		health.text = "" + ((int)player.savable.health) + '\n' + "Зомби сейчас : " + DataCenter.dataCenter.zombies.Length;
		float t = ITimecycle.time;
		int sec = (int)((t - (int)t) * 60);
		string s = "" + sec;
		if (sec < 10) {
			s = "0" + sec;
		}
		killed.text = "Время : " + (int)t + ":" + s;
		IZombie z = IZombie.ZombieNearbyPosition (player.trans.position, 5, false);
		if (z) {
			Vector3 v = Vector0.Flat (z.trans.position - player.trans.position);
			v = player.trans.InverseTransformDirection (v);
			Quaternion q = Quaternion.LookRotation (v);
			zAngle = -135 - q.eulerAngles.y;
		} else {
			zAngle -= Time.deltaTime * 180;
		}
		radar.localEulerAngles = Vector3.forward * (zAngle);
		patronesType.texture = (Texture)Resources.Load ("Sprites/Patrones_" + player.savable.currentWeapon);
		patrones.text = "" + player.savable.patrones_in [player.savable.currentWeapon] + "/" + player.savable.patrones_ [player.savable.currentWeapon];
	}
	private float zAngle;
	private void OnGUI () {
		if (player) {
			//DrawRadar ();
		}
	}
	private void DrawRadar () {
		IHuman[] humansIn = IHuman.GetNearByAll (player.trans.position, 50);
		Rect r = new Rect (0, 0, 100, 100);
		GUI.color = Color.gray;
		GUI.Box (r, "");
		for (int i = 0; i < humansIn.Length; i++) {
			Vector3 p = player.trans.InverseTransformPoint(humansIn [i].trans.position - player.trans.position) + new Vector3(50, 0, 50);
			r = new Rect (p.x - 5, p.z - 5, 10, 10);
			GUI.color = Color.blue;
			GUI.Box (r, "");
		}
		IZombie[] zIn = IZombie.GetNearByAll (player.trans.position, 50);
		for (int i = 0; i < zIn.Length; i++) {
			Vector3 p = player.trans.InverseTransformPoint(zIn [i].trans.position - player.trans.position) + new Vector3(50, 0, 50);
			r = new Rect (p.x - 10, p.z - 10, 20, 20);
			GUI.color = Color.red;
			GUI.Box (r, "");
		}
	}
}
