using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	public GameObject joyRoot;
	public GameObject joyPoint;
	RectTransform root;
	RectTransform point;
	public bool additiveCameraInput = false;

	static Vector2 _move;
	public static Vector2 move_input
	{
		get {
			if (Application.isMobilePlatform) {
				return _move;
			} else {
				Vector2 pc = new Vector2 (Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
				return pc.magnitude > _move.magnitude ? pc : _move;
			}
		}
		set {
			_move = value;
		}
	}
	public static Vector2 turn_input
	{
		get {
			if (!Application.isMobilePlatform) {
				return new Vector2 (Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 5;
			} else {
				Vector2 t = turn_getter;
				turn_getter = Vector2.zero;
				return t;
			}
		}
		set {
			turn_getter = value;
		}
	}
	public static Vector2 additiveInput;
	private static Vector2 turn_getter;

	private void Update () {
	}
	private void Start () {
		root = joyRoot.GetComponent<RectTransform> ();
		point = joyPoint.GetComponent<RectTransform> ();
		isDrag = false;
	}
	private Vector2 root_position
	{
		get {
			return root_pos_getter;
		}
		set {
			root_pos_getter = value;
			if (!additiveCameraInput) {
				root.position = root_pos_getter;
			}
		}
	}
	private Vector2 root_pos_getter;

	private Vector2 point_position
	{
		get {
			return point_pos_getter;
		}
		set {
			point_pos_getter = value;
			if (point_pos_getter.magnitude > root.sizeDelta.y / 2) {
				point_pos_getter = point_pos_getter.normalized * root.sizeDelta.y / 2;
			}
			point.anchoredPosition = point_pos_getter;
			if (!additiveCameraInput) {
				move_input = point_pos_getter / (root.sizeDelta.y / 2);
			} else {
				additiveInput = point_pos_getter / (root.sizeDelta.y / 2);
			}
		}
	}
	private Vector2 point_pos_getter;

	public bool isDrag
	{
		get {
			return isDragGetter;
		}
		set {
			isDragGetter = value;
			if (!additiveCameraInput) {
				joyRoot.SetActive (isDragGetter);
				joyPoint.SetActive (isDragGetter);
			}
		}
	}
	private bool isDragGetter = false;

	public void OnBeginDrag (PointerEventData data) {
		isDrag = true;
		root_position = data.position;
		point_position = Vector2.zero;
	}
	public void OnDrag (PointerEventData data) {
		point_position = data.position - root_position;
	}
	public void OnEndDrag (PointerEventData data) {
		isDrag = false;
		point_position = Vector2.zero;
	}
}
