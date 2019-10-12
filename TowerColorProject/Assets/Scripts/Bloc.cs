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

	public Renderer renderer;
	public Rigidbody rigidbody;

	//Todo Color
	public void ApplyColor() {
		renderer.material = Destructible ? GameManager.Instance.GameData.GetBlocColorMaterial(Color) : GameManager.Instance.GameData.MaterialBlack;
	}

	public void SetDestructible(bool destructible)
	{
		Destructible = destructible;
		TogglePhysics(Destructible);
		ApplyColor();
	}

	public void TogglePhysics(bool toggle)
	{
		rigidbody.isKinematic = !toggle;
	}

	public void Destroy()
	{
		Destroyed = true;
		renderer.enabled = false;
		gameObject.SetActive(false);
	}

}
