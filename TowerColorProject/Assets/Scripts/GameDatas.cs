using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Datas", order = 1)]
public class GameDatas : ScriptableObject
{
	[Header("Prefab")]
	public GameObject BlocPrefab;

	[Header("Level parameters")]
	public List<LevelInfos> LevelInfos;

	[Header("Materials")]
	public Material MaterialBlue;
	public Material MaterialPurple;
	public Material MaterialPink;
	public Material MaterialGreen;
	public Material MaterialYellow;
	public Material MaterialRed;
	public Material MaterialBlack;

	//TODO : Color blind option
	public Material GetBlocColorMaterial(BlocColor color) {
		switch (color) {
			case (BlocColor.Blue):
				return MaterialBlue;
			case (BlocColor.Purple):
				return MaterialPurple;
			case (BlocColor.Pink):
				return MaterialPink;
			case (BlocColor.Green):
				return MaterialGreen;
			case (BlocColor.Yellow):
				return MaterialYellow;
			case (BlocColor.Red):
				return MaterialRed;
			default:
				return null;
		}
	}
}

[System.Serializable]
public class LevelInfos
{
	public int TowerHeight = 10;
	public int NbColors = 1;
	public int NbLinesEnabled = 5;
}
