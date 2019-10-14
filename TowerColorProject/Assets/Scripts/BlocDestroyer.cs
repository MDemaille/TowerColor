using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocDestroyer : MonoBehaviour
{
	private void OnTriggerEnter(Collider other) {
		if(other.CompareTag("Bloc"))
			Destroy(other);
	}
}
