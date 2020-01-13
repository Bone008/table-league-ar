using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordAttacks : MonoBehaviour {

	public Text effectName;
	public GameObject spawnPoint;
	public List<GameObject> VFX = new List<GameObject> ();

	private int count = 0;
	private Animator anim;
	private GameObject effectToSpawn;

	void Start () {
		anim = GetComponent<Animator> ();
		effectToSpawn = VFX [0];
		if (effectName != null) effectName.text = effectToSpawn.name;
	}

	 void Update () {
		if(Input.GetKeyDown (KeyCode.Alpha1)){
			anim.SetTrigger ("Attack01");
			SpawnVFX ();
		}
		if(Input.GetKeyDown (KeyCode.Alpha2)){
			anim.SetTrigger ("Attack02");
			SpawnVFX ();
		}

		if (Input.GetKeyDown (KeyCode.D))
			Next ();
		
		if (Input.GetKeyDown (KeyCode.A)) 
			Previous ();	
	}

	public void SpawnVFX () {
		GameObject vfx;

		vfx = Instantiate (effectToSpawn, spawnPoint.transform.position, Quaternion.identity);
		vfx.transform.SetParent (transform);

		var ps = vfx.GetComponent<ParticleSystem> ();
		if (ps != null) {
			Destroy (vfx, ps.main.duration + ps.main.startLifetime.constantMax);
		} else {
			var psChild = vfx.transform.GetChild(0).GetComponent<ParticleSystem> ();
			if (psChild != null)
				Destroy (vfx, psChild.main.duration + psChild.main.startLifetime.constantMax);
		}
	}

	public void Next () {
		count++;

		if (count > VFX.Count)
			count = 0;

		for(int i = 0; i < VFX.Count; i++){
			if (count == i)	effectToSpawn = VFX [i];
			if (effectName != null)	effectName.text = effectToSpawn.name;
		}
	}

	public void Previous () {
		count--;

		if (count < 0)
			count = VFX.Count;

		for (int i = 0; i < VFX.Count; i++) {
			if (count == i) effectToSpawn = VFX [i];
			if (effectName != null)	effectName.text = effectToSpawn.name;
		}
	}
}
