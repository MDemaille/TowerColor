using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlocColor {
	Blue,
	Purple,
	Pink,
	Yellow,
	Red
}

public class Bloc : MonoBehaviour
{
	public int Id;

	public int X;
	public int Y;

	public BlocColor Color;
	public bool Destructible = true;
}
