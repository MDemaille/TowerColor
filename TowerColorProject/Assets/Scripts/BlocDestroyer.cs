using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocDestroyer : MonoBehaviour
{
	private void OnCollisionEnter(Collision other)
	{
		Bloc bloc = other.gameObject.GetComponent<Bloc>();
		if(bloc.Y > 0)
			bloc.DestroyBlocByGround();
	}
}
