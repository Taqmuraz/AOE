using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DataCenter
{
	public IHuman[] humans { get; private set; }
	public IZombie[] zombies { get; private set; }

	public void Add (Registrable obj) {
		if (obj is IHuman) {
			List<IHuman> hs = humans.ToList<IHuman> ();
			hs.Add ((IHuman)obj);
			humans = hs.ToArray ();
		}
		if (obj is IZombie) {
			List<IZombie> zs = zombies.ToList<IZombie> ();
			zs.Add ((IZombie)obj);
			zombies = zs.ToArray ();
		}
		Debug.Log ("Added : " + obj.name);
	}
	public void Remove (Registrable obj) {
		if (obj is IHuman) {
			List<IHuman> hs = humans.ToList<IHuman> ();
			hs.Remove ((IHuman)obj);
			humans = hs.ToArray ();
		}
		if (obj is IZombie) {
			List<IZombie> zs = zombies.ToList<IZombie> ();
			zs.Remove ((IZombie)obj);
			zombies = zs.ToArray ();
		}
		Debug.Log ("Removed : " + obj.name);
	}

	public DataCenter () {
		humans = new IHuman[0];
		zombies = new IZombie[0];
	}

	public static DataCenter dataCenter = new DataCenter ();
}

public class Registrable : MonoBehaviour
{
	protected virtual void Awake () {
		
		if (this is IZombie) {
			DataCenter.dataCenter.Add ((IZombie)this);
		}
		if (this is IHuman) {
			DataCenter.dataCenter.Add ((IHuman)this);
		}
	}

	protected virtual void OnDestroy () {
		if (this is IZombie) {
			DataCenter.dataCenter.Remove ((IZombie)this);
		}
		if (this is IHuman) {
			DataCenter.dataCenter.Remove ((IHuman)this);
		}
	}
}

