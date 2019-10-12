using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlocColor {
	Blue,
	Green,
	Purple,
	Yellow,
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
}
