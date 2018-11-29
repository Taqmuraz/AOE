using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPanel : MonoBehaviour, IDragHandler
{
	public void OnDrag (PointerEventData data) {
		Joystick.turn_input = data.delta;
	}
}
