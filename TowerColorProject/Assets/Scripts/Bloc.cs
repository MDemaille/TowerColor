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
	public GameObject DestructionParticleSystem;

	public float StartY { get; private set; }
	private bool _startYRegistered = false;

	public List<Bloc> NeighborsBlocs { get; private set; }

	void Awake()
	{
		NeighborsBlocs = new List<Bloc>();
		BlocRigidbody.centerOfMass = new Vector3(0, -0.6f,0);
	}

	void Update()
	{
		if (_startYRegistered && !Destroyed && Mathf.Abs(StartY - transform.position.y) > GameManager.Instance.GameData.YDistanceToConsiderBlocDestroyed)
		{
			DestroyBloc(false);
		}
	}

	public void AddNeighbor(Bloc bloc)
	{
		if(!NeighborsBlocs.Contains(bloc))
			NeighborsBlocs.Add(bloc);
	}

	public void RemoveNeighbor(Bloc bloc)
	{
		NeighborsBlocs.Remove(bloc);
	}

	public void RegisterStartY()
	{
		StartY = transform.position.y;
		_startYRegistered = true;
	}

	public void SetVisible(bool visible)
	{
		BlocRenderer.enabled = visible;
	}

	//Todo Color
	public void ApplyColor(bool setBlackIfNotDestructible = true) {

		if (setBlackIfNotDestructible)
		{
			BlocRenderer.material = Destructible
				? GameManager.Instance.GameData.GetBlocColorMaterial(Color)
				: GameManager.Instance.GameData.MaterialBlack;
		}
		else
		{
			BlocRenderer.material = GameManager.Instance.GameData.GetBlocColorMaterial(Color);
		}

		//DestructionParticleRenderer.material = GameManager.Instance.GameData.GetBlocColorMaterial(Color);
	}

	public void SetDestructible(bool destructible)
	{
		Destructible = destructible;
		TogglePhysics(destructible);
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

		if (BlocRigidbody)
			BlocRigidbody.mass = 0.1f;

		EventManager.TriggerEvent(EventList.OnBlocDestroyed);

		if (destroyFromHit)
		{
			//BlocRenderer.enabled = false;
			//BlocCollider.enabled = false;

			EventManager.TriggerEvent(EventList.OnBlocDestroyedFromHit);

			GameObject particleSystem = Instantiate(DestructionParticleSystem, transform.position, Quaternion.identity);
			ParticleSystemRenderer destructionParticleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
			destructionParticleRenderer.material = GameManager.Instance.GameData.GetBlocColorMaterial(Color);

			Destroy(gameObject);

			//gameObject.SetActive(false);
		}

		
	}

	

}
