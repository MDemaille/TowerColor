using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Datas", order = 1)]
public class GameDatas : ScriptableObject
{
	[Header("Prefab")]
	public GameObject BlocPrefab;

	[Header("Materials")]
	public Material MaterialBlue;
	public Material MaterialPurple;
	public Material MaterialPink;
	public Material MaterialGreen;
	public Material MaterialYellow;
	public Material MaterialRed;
	public Material MaterialBlack;
}
