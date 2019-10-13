using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighborDetector : MonoBehaviour
{
	public Bloc CurrentBloc;

	void OnTriggerEnter(Collider collider)
	{
		Bloc bloc = collider.GetComponent<Bloc>();
		if (bloc != null)
		{
			CurrentBloc.AddNeighbor(bloc);
		}
	}

	void OnTriggerExit(Collider collider) {
		Bloc bloc = collider.GetComponent<Bloc>();
		if (bloc != null) {
			CurrentBloc.RemoveNeighbor(bloc);
		}
	}
}
