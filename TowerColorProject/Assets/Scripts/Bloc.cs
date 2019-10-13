using System.Collections;
using System.Collections.Generic;
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

	public float StartY { get; private set; }
	private bool _startYRegistered = false;

	void Update()
	{
		if (_startYRegistered && !Destroyed && Mathf.Abs(StartY - transform.position.y) > GameManager.Instance.GameData.YDistanceToConsiderBlocDestroyed)
		{
			Destroy(false);
		}
	}

	public void RegisterStartY()
	{
		StartY = transform.position.y;
		_startYRegistered = true;
	}

	//Todo Color
	public void ApplyColor() {
		BlocRenderer.material = Destructible ? GameManager.Instance.GameData.GetBlocColorMaterial(Color) : GameManager.Instance.GameData.MaterialBlack;
	}

	public void SetDestructible(bool destructible)
	{
		Destructible = destructible;
		TogglePhysics(Destructible);
		ApplyColor();
	}

	public void TogglePhysics(bool toggle)
	{
		BlocRigidbody.isKinematic = !toggle;
	}

	public void Destroy(bool visualEffects = true)
	{
		Destroyed = true;

		if (visualEffects)
		{
			BlocRenderer.enabled = false;
			gameObject.SetActive(false);
		}

		EventManager.TriggerEvent(EventList.OnBlocDestroyed);
	}

}
