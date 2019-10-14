using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BlocColor {
	Blue,
	Yellow,
	Purple,
	Green,
	Pink,
	Red
}

public class Bloc : MonoBehaviour
{
	[HideInInspector]
	public int Id;
	[HideInInspector]
	public int X;
	[HideInInspector]
	public int Y;
	[HideInInspector]
	public BlocColor Color;
	[HideInInspector]
	public bool Destructible = true;
	[HideInInspector]
	public bool Destroyed = false;

	public Renderer BlocRenderer;
	public Rigidbody BlocRigidbody;
	public Collider BlocCollider;
	public ParticleSystem DestructionParticleSystem;
	public ParticleSystemRenderer DestructionParticleRenderer;

	public float StartY { get; private set; }
	private bool _startYRegistered = false;

	private List<Bloc> _neighborsBlocs = new List<Bloc>();

	void Update()
	{
		if (_startYRegistered && !Destroyed && Mathf.Abs(StartY - transform.position.y) > GameManager.Instance.GameData.YDistanceToConsiderBlocDestroyed)
		{
			DestroyBloc(false);
		}
	}

	public void AddNeighbor(Bloc bloc)
	{
		if(!_neighborsBlocs.Contains(bloc))
			_neighborsBlocs.Add(bloc);
	}

	public void RemoveNeighbor(Bloc bloc)
	{
		_neighborsBlocs.Remove(bloc);
	}

	public void RegisterStartY()
	{
		StartY = transform.position.y;
		_startYRegistered = true;
	}

	//Todo Color
	public void ApplyColor() {
		BlocRenderer.material = Destructible ? GameManager.Instance.GameData.GetBlocColorMaterial(Color) : GameManager.Instance.GameData.MaterialBlack;
		DestructionParticleRenderer.material = GameManager.Instance.GameData.GetBlocColorMaterial(Color);
	}

	public void SetDestructible(bool destructible)
	{
		Destructible = destructible;

		if(destructible)
			StartCoroutine(EnablePhysicsAfterTime());
		else 
			TogglePhysics(false);

		ApplyColor();
	}

	IEnumerator EnablePhysicsAfterTime()
	{
		yield return new WaitForSeconds(1f);
		TogglePhysics(true);
	}

	public void TogglePhysics(bool toggle)
	{
		if(BlocRigidbody)
			BlocRigidbody.isKinematic = !toggle;
	}

	public void DestroyBloc(bool destroyFromHit = true)
	{
		Destroyed = true;

		if (destroyFromHit)
		{
			BlocRenderer.enabled = false;
			BlocCollider.enabled = false;
			Destroy(BlocRigidbody);

			DestructionParticleSystem.gameObject.SetActive(true);

			StartCoroutine(DestroyNeighbors(GameManager.Instance.GameData.TimeBetweenBlocDestruction));

			//gameObject.SetActive(false);
		}

		EventManager.TriggerEvent(EventList.OnBlocDestroyed);
	}

	public IEnumerator DestroyNeighbors(float timeBetweenDestruction)
	{
		List<Bloc> tempNeighbors = new List<Bloc>();
		tempNeighbors.AddRange(_neighborsBlocs);

		foreach (var neighbor in tempNeighbors) {
			if (neighbor.Destructible && !neighbor.Destroyed && neighbor.Color.Equals(Color))
			{
				yield return new WaitForSeconds(timeBetweenDestruction);
				neighbor.DestroyBloc(true);
			}
		}
	}

}
