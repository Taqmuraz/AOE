using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IWater : MonoBehaviour {

	public Renderer rend;
	public float offsetSpeed = 1;

	private void FixedUpdate () {
		rend.material.mainTextureOffset += Vector2.up * Time.fixedDeltaTime * offsetSpeed;
	}
}
