using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum GamePhase
{
	Init,
	Show,
	Play,
	LevelEnd,
	LevelFail
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
	public Camera MainCamera;
	public Transform CameraTargetTransform;
	public Transform CameraShowPhasePosition;
	public Transform CameraPlayPhasePosition;
	public Transform ShotSpawn;

	public bool ColorBlindOption = false;

	//Variables to handle game loop
	private GamePhase _currentGamePhase = GamePhase.Init;
	private int _currentLevel = 0;
	private string _playerPrefLevelData = "LevelSave";

	[Header("Camera Movement")]
	public float CameraSpeed = 50f;
	private float _yCameraTarget = 0f;
	private Vector3 velocity = Vector3.zero;

	//Shots
	public int NbShotsAvailable { get; private set; }
	public BlocColor CurrentShotColor { get; private set; }

	public float MaximumTimeTouchToShoot = 0.15f;
	private float _touchTimer = 0f;

	private float _score = 0f;

	private bool _failTimerEnabled = false;

	public void Awake()
	{
		_currentLevel = PlayerPrefs.GetInt(_playerPrefLevelData);

		//TODO : Fade to black
		RegisterToEvents(true);
		

		InitLevel(GameData.LevelInfos[_currentLevel], _currentLevel);
	}

	public void RegisterToEvents(bool register)
	{
		EventManager.SetEventListener(EventList.OnBlocDestroyed, UpdateScore, register);
	}

	public void SetGamePhase(GamePhase gamePhase)
	{
		_currentGamePhase = gamePhase;
		EventManager.TriggerEvent(EventList.OnGamePhaseChanged, _currentGamePhase);
	}

	public void InitLevel(LevelInfos infos, int levelId)
	{
		_currentLevel = levelId;
		PlayerPrefs.SetInt(_playerPrefLevelData, _currentLevel);
		PlayerPrefs.Save();

		Tower.InitTower(infos.NbLinesEnabled, infos.TowerHeight, infos.NbColors);
		NbShotsAvailable = infos.NbShots;
		_score = 0;
		_failTimerEnabled = false;

		//ShowLevel();
		SetGamePhase(GamePhase.Play);
		DrawNewBall();

	}

	public void ShowLevel()
	{
		SetGamePhase(GamePhase.Show);

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
			UpdateCamera();
			GetShootInput();
			if (IsVictoryScoreReached()) {
				LevelEnd();
			}

			if (!_failTimerEnabled && NbShotsAvailable <= 0)
			{
				StartCoroutine(TimerBeforeLevelFail());
			}
		}
	}

	#region Score

	public void UpdateScore(object obj)
	{
		_score = Tower.GetDestroyedBlocRatioForScore();
		Debug.Log(_score);
	}

	public bool IsVictoryScoreReached()
	{
		return _score >= 1f;
	}

	public IEnumerator TimerBeforeLevelFail()
	{
		_failTimerEnabled = true;
		yield return new WaitForSeconds(GameData.TimeToFailLevelWhenOutOfShots);

		if(!IsVictoryScoreReached())
			LevelFail();
	}

	public void LevelEnd()
	{
		SetGamePhase(GamePhase.LevelEnd);

		_currentLevel++;
		InitLevel(GameData.LevelInfos[_currentLevel], _currentLevel);
	}

	public void LevelFail() {
		SetGamePhase(GamePhase.LevelFail);

		InitLevel(GameData.LevelInfos[_currentLevel], _currentLevel);
	}

	#endregion

	#region Shoot

	public void DrawNewBall() {
		//Get a list of still available colors in tower and pick a random one
		List<BlocColor> availableBlocColors = Tower.GetColorsAvailableInTower();
		if (availableBlocColors.Count == 0)
		{
			Debug.LogError("No color available in tower, this should not happen");
		}
		else
		{
			CurrentShotColor = availableBlocColors[Random.Range(0, availableBlocColors.Count)];
		}

		EventManager.TriggerEvent(EventList.OnDrawNewBall);
	}

	public bool GetShootInput() {
		if (Input.touchCount > 0) {
			Touch currentTouch = Input.GetTouch(0);
			if (currentTouch.phase == TouchPhase.Began || currentTouch.phase == TouchPhase.Canceled)
			{
				_touchTimer = 0f;
			}
			else if(currentTouch.phase != TouchPhase.Ended)
			{
				_touchTimer += Time.deltaTime;
			}

			if (currentTouch.phase == TouchPhase.Ended && _touchTimer < MaximumTimeTouchToShoot)
			{
				Ray ray = MainCamera.ScreenPointToRay(currentTouch.position);
				RaycastHit hitInfos;
				if (Physics.Raycast(ray, out hitInfos))
				{
					Bloc hitBloc = hitInfos.transform.GetComponent<Bloc>();
					if(hitBloc != null && hitBloc.Destructible)
						Shoot(hitBloc);
				}

			}
		}

		return false;
	}

	public void Shoot(Bloc blocToShoot)
	{
		NbShotsAvailable--;
		StartCoroutine(ShootBallCoroutine(0.25f, blocToShoot, CurrentShotColor));

		EventManager.TriggerEvent(EventList.OnShotFired, NbShotsAvailable);
		DrawNewBall();
	}

	public IEnumerator ShootBallCoroutine(float timeToReachTarget, Bloc blocToShoot, BlocColor colorShot )
	{
		GameObject ball = Instantiate(Instance.GameData.ShotPrefab, ShotSpawn.position, Quaternion.identity);
		Renderer ballRenderer = ball.GetComponent<Renderer>();

		ballRenderer.material = GameData.GetBlocColorMaterial(CurrentShotColor);

		float timer = 0f;
		float progression = 0f;
		Vector3 startPosition = ShotSpawn.position;
		while (timer < timeToReachTarget)
		{
			timer += Time.deltaTime;
			progression = timer / timeToReachTarget;

			ball.transform.position = Vector3.Lerp(startPosition, blocToShoot.transform.position, progression);

			float yVariation = GameData.BallTrajectory.Evaluate(progression);
			ball.transform.position += Vector3.up * yVariation;

			yield return new WaitForSeconds(Time.deltaTime);
		}

		Destroy(ball);
		if (blocToShoot.Destructible && blocToShoot.Color.Equals(colorShot)) {
			List<Bloc> blocsToDestroy = Tower.GetBlocsToDestroy(blocToShoot);
			foreach (var bloc in blocsToDestroy) {
				bloc.Destroy();
			}
		}
	}

	#endregion


	#region Camera

	public void SetPlayCameraY(float value) {
		_yCameraTarget = value;
	}

	public void UpdateCamera() {
		//X : Determine by input
		if (Input.touchCount > 0) {
			Touch currentTouch = Input.GetTouch(0);
			if (currentTouch.phase == TouchPhase.Moved) {
				float normalizedXDelta = currentTouch.deltaPosition.x / Screen.width;
				CameraTargetTransform.RotateAround(new Vector3(0, CameraTargetTransform.position.y, 0), Vector3.up, CameraSpeed * normalizedXDelta * Time.deltaTime);
			}
		}


		Vector3 smoothPosition = Vector3.SmoothDamp(MainCamera.transform.position, CameraTargetTransform.position, ref velocity, 0.2f);

		//Because the smooth position does not move according to a circle, we apply a translation to keep it at the right distance
		CameraTargetTransform.position = new Vector3(CameraTargetTransform.position.x, _yCameraTarget, CameraTargetTransform.position.z);
		Vector3 centerToCameraTarget = CameraTargetTransform.position - new Vector3(0, CameraTargetTransform.position.y, 0);

		Vector3 finalPosition = (smoothPosition - new Vector3(0, smoothPosition.y, 0)).normalized * centerToCameraTarget.magnitude;

		finalPosition += new Vector3(0, _yCameraTarget, 0);

		MainCamera.transform.position = finalPosition;//CameraTargetTransform.position;//Vector3.Lerp(CameraTransform.position, CameraTargetTransform.position, Time.smoothDeltaTime);//Vector3.SmoothDamp(CameraTransform.position, CameraTargetTransform.position, ref velocity, 0.2f);
		MainCamera.transform.LookAt(new Vector3(0, MainCamera.transform.position.y, 0));
	}

	#endregion

}
