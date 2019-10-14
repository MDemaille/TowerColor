using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Datas", order = 1)]
public class GameDatas : ScriptableObject
{
	[Header("Prefab")]
	public GameObject BlocPrefab;
	public GameObject ShotPrefab;

	[Header("Level parameters")]
	public List<LevelStep> LevelSteps;

	[Header("Combo Parameters")]
	public List<ComboStep> ComboSteps;

	[Header("Materials")]
	public Material MaterialBlue;
	public Material MaterialPurple;
	public Material MaterialPink;
	public Material MaterialGreen;
	public Material MaterialYellow;
	public Material MaterialRed;
	public Material MaterialBlack;

	[Header("Color")]
	public Color ColorBlue;
	public Color ColorPurple;
	public Color ColorPink;
	public Color ColorGreen;
	public Color ColorYellow;
	public Color ColorRed;

	[Header("Parameters")]
	public float YDistanceToConsiderBlocDestroyed = 0.5f;
	public float YLowestCamera = 7f;
	public AnimationCurve BallTrajectory;
	public float TimeBetweenBlocDestruction = 0.1f;

	public float TimeToFailLevelWhenOutOfShots = 3f;

	public LevelStep GetParametersForLevel(int levelId) {
		LevelStep currentStep = LevelSteps[0];
		foreach (var gameDataLevelStep in LevelSteps) {
			if (levelId >= gameDataLevelStep.LevelToApplyStep)
				currentStep = gameDataLevelStep;
			else
				break;
		}

		return currentStep;
	}

	public ComboStep GetComboStep(int hit)
	{
		ComboStep currentStep = ComboSteps[0];
		foreach (var comboStep in ComboSteps) {
			if (hit >= comboStep.Count)
				currentStep = comboStep;
			else
				break;
		}

		return currentStep;
	}

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

	public Color GetBlocColorColorValue(BlocColor color) {
		switch (color) {
			case (BlocColor.Blue):
				return ColorBlue;
			case (BlocColor.Purple):
				return ColorPurple;
			case (BlocColor.Pink):
				return ColorPink;
			case (BlocColor.Green):
				return ColorGreen;
			case (BlocColor.Yellow):
				return ColorYellow;
			case (BlocColor.Red):
				return ColorRed;
			default:
				return Color.black;
		}
	}
}

[System.Serializable]
public class LevelStep
{
	public int LevelToApplyStep = 0;
	public int TowerHeight = 10;
	public int NbColors = 1;
	public int NbLinesEnabled = 5;
	public int NbShots = 10;
}

[System.Serializable]
public class ComboStep
{
	public int Count = 0;
	public string Label = "Cool !";
}