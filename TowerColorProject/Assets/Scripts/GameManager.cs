using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum GamePhase
{
	Init,
	Show,
	Play,
	LevelEnd
}

public enum DisplayMode
{
	Normal,
	ColorBlind
}

public class GameManager : Singleton<GameManager>
{
	public GameDatas GameData;
	public Tower Tower;

	[Header("Scene References")]
	public Transform CameraTransform;
	public Transform CameraTargetTransform;
	public Transform CameraShowPhasePosition;
	public Transform CameraPlayPhasePosition;

	public bool ColorBlindOption = false;

	//Variables to handle game loop
	private GamePhase _currentGamePhase = GamePhase.Init;
	private int _currentLevel = 0;
	private string _playerPrefLevelData = "LevelSave";

	[Header("Camera Movement")]
	public float CameraSpeed = 50f;
	private float _yCameraTarget = 0f;
	private Vector3 velocity = Vector3.zero;

	public void Awake()
	{
		_currentLevel = PlayerPrefs.GetInt(_playerPrefLevelData);

		//TODO : Fade to black

		InitLevel(GameData.LevelInfos[_currentLevel], _currentLevel);
	}

	public void InitLevel(LevelInfos infos, int levelId)
	{
		_currentLevel = levelId;
		PlayerPrefs.SetInt(_playerPrefLevelData, _currentLevel);
		PlayerPrefs.Save();

		Tower.InitTower(infos.NbLinesEnabled, infos.TowerHeight, infos.NbColors);
		//ShowLevel();
		_currentGamePhase = GamePhase.Play;

	}

	public void ShowLevel()
	{
		_currentGamePhase = GamePhase.Show;

		StartCoroutine(ShowLevelCoroutine());
	}

	public IEnumerator ShowLevelCoroutine()
	{
		//Camera placement
		//Fade from black
		//Tutorial display
		//Wait for touch
		//Camera blend 
		//Tower Lock section
		//Begin play
		yield return null;
	}

	public void Update()
	{
		if (_currentGamePhase.Equals(GamePhase.Play))
		{
			GetShootInput();
			UpdateCamera();
		}

		//TODO : If score reached call the level end function
	}

	public void LevelEnd()
	{
		_currentGamePhase = GamePhase.LevelEnd;

		//TODO Go to next Level
	}

	public void GetShootInput()
	{
		if (Input.touchCount > 0)
		{
			Touch currentTouch = Input.GetTouch(0);
			//If the touch is short we raycast and we destroy stuff
		}
	}

	public void SetPlayCameraY(float value)
	{
		_yCameraTarget = value;
	}

	public void UpdateCamera()
	{
		//X : Determine by input
		if (Input.touchCount > 0)
		{
			Touch currentTouch = Input.GetTouch(0);
			if (currentTouch.phase == TouchPhase.Moved)
			{
				float normalizedXDelta = currentTouch.deltaPosition.x / Screen.width;
				CameraTargetTransform.RotateAround(new Vector3(0, CameraTargetTransform.position.y, 0), Vector3.up, CameraSpeed * normalizedXDelta * Time.deltaTime);
			}
		}

		//The transform the camera will follow
		CameraTargetTransform.position = new Vector3(CameraTargetTransform.position.x, _yCameraTarget, CameraTargetTransform.position.z);

		Vector3 centerToCameraTarget = CameraTargetTransform.position - new Vector3(0, CameraTargetTransform.position.y, 0);

		Vector3 smoothPosition = Vector3.SmoothDamp(CameraTransform.position, CameraTargetTransform.position, ref velocity, 0.2f);
		Vector3 finalPosition = (smoothPosition - new Vector3(0, smoothPosition.y, 0)).normalized * centerToCameraTarget.magnitude;

		finalPosition += new Vector3(0, _yCameraTarget, 0);

		CameraTransform.position = finalPosition;//CameraTargetTransform.position;//Vector3.Lerp(CameraTransform.position, CameraTargetTransform.position, Time.smoothDeltaTime);//Vector3.SmoothDamp(CameraTransform.position, CameraTargetTransform.position, ref velocity, 0.2f);
		CameraTransform.LookAt( new Vector3(0, CameraTransform.position.y, 0));
	}
}
