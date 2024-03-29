﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
	Init,
	Show,
	Play,
	LevelEnd,
	LevelFail
}

//Class that handles core gameplay, controls and camera
public class GameManager : Singleton<GameManager>
{
	public GameDatas GameData;
	public Tower Tower;

	[Header("Scene References")]
	public Camera MainCamera;
	public Transform CameraTargetTransform;
	public Transform CameraStartPosition;
	public Transform ShotSpawn;

	public GameObject VictoryParticleSystem;

	[Header("Camera Movement")]
	public float CameraSpeed = 50f;
	private float _yCameraTarget = 0f;
	private Vector3 velocity = Vector3.zero;
	private float YVelocity = 0f;

	//Variables to handle game loop
	public GamePhase CurrentGamePhase { get; private set; }
	public int CurrentLevel { get; private set; }
	private string _playerPrefLevelData = "LevelSave";

	//Shots
	public int NbShotsAvailable { get; private set; }
	public List<BlocColor> CurrentWeaponColors { get; private set; }

	public float MaximumTimeTouchToShoot = 0.15f;
	private float _touchTimer = 0f;

	public float Score { get; private set; }

	//Combo
	public int ComboCount = 0;
	public BlocColor LastDestroyedColor { get; private set; }
	private int _maxComboCount = 0;

	//Level Fail
	private bool _failTimerEnabled = false;




	public void Awake()
	{
		CurrentLevel = PlayerPrefs.GetInt(_playerPrefLevelData);

		RegisterToEvents(true);
		
		InitLevel(CurrentLevel);
	}

	public void RegisterToEvents(bool register)
	{
		EventManager.SetEventListener(EventList.OnBlocDestroyed, OnBlocDestroyed, register);
	}

	public void Update()
	{
		if (CurrentGamePhase.Equals(GamePhase.Play))
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

	#region Game Phases and Coroutine

	public void SetGamePhase(GamePhase gamePhase) {
		CurrentGamePhase = gamePhase;
		EventManager.TriggerEvent(EventList.OnGamePhaseChanged, CurrentGamePhase);
	}

	public void InitLevel(int levelId) {
		SetGamePhase(GamePhase.Init);

		CurrentLevel = levelId;
		PlayerPrefs.SetInt(_playerPrefLevelData, CurrentLevel);
		PlayerPrefs.Save();

		LevelStep stepForcurrentLevel = GameData.GetParametersForLevel(CurrentLevel);

		Tower.InitTower(stepForcurrentLevel.NbLinesEnabled, stepForcurrentLevel.TowerHeight, stepForcurrentLevel.NbColors);
		NbShotsAvailable = stepForcurrentLevel.NbShots;

		UIManager.Instance.FadeImage.color = Color.white;

		SetScore(0);
		SetComboCount(0);

		_failTimerEnabled = false;
		_maxComboCount = 0;

		ShowLevel();
	}

	public void ShowLevel() {
		SetGamePhase(GamePhase.Show);
		StartCoroutine(ShowLevelCoroutine());
	}

	public IEnumerator ShowLevelCoroutine() {
		Tower.SetTowerVisible(false);

		MainCamera.transform.position = CameraStartPosition.position;
		MainCamera.transform.LookAt(new Vector3(0, MainCamera.transform.position.y, 0));

		CameraTargetTransform.position = MainCamera.transform.position;

		yield return UIManager.Instance.FadeScreen(Color.clear, 1f);

		StartCoroutine(Tower.ShowBuildTowerAnimation());
		yield return new WaitForSeconds(1f);

		UpdateCameraTargetHeight();

		yield return BlendCameraPosition(MainCamera.transform.position, CameraTargetTransform.position, 1f);

		DrawNewWeapon();
		SetGamePhase(GamePhase.Play);
	}

	public void LevelEnd() {
		SetGamePhase(GamePhase.LevelEnd);

		CurrentLevel++;
		StartCoroutine(LevelEndCoroutine());
	}

	public IEnumerator LevelEndCoroutine() {
		VictoryParticleSystem.gameObject.SetActive(true);

		yield return BlendCameraPosition(MainCamera.transform.position,
			MainCamera.transform.position + (-1 * MainCamera.transform.forward + MainCamera.transform.up), 0.1f);

		UIManager.Instance.TextWin.gameObject.SetActive(true);

		EventManager.TriggerEvent(EventList.OnMaxComboCountShow, _maxComboCount);

		yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0);

		yield return UIManager.instance.FadeScreen(Color.white, 0.5f);

		VictoryParticleSystem.gameObject.SetActive(false);
		UIManager.Instance.TextWin.gameObject.SetActive(false);
		InitLevel(CurrentLevel);
	}

	public IEnumerator TimerBeforeLevelFail() {
		_failTimerEnabled = true;
		int level = CurrentLevel;
		float timer = 0f;
		while (timer < GameData.TimeToFailLevelWhenOutOfShots) {
			if (CurrentGamePhase == GamePhase.LevelEnd)
				yield break;

			yield return new WaitForSeconds(Time.deltaTime);
			timer += Time.deltaTime;
			EventManager.TriggerEvent(EventList.OnFailTimerUpdate, timer / GameData.TimeToFailLevelWhenOutOfShots);
		}

		if (!IsVictoryScoreReached() && level == CurrentLevel)
			LevelFail();
	}

	public void LevelFail() {
		StartCoroutine(LevelFailCoroutine());
	}

	public IEnumerator LevelFailCoroutine() {
		SetGamePhase(GamePhase.LevelFail);
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0);
		InitLevel(CurrentLevel);
	}

	#endregion

	#region Score

	public void SetScore(float score) {
		Score = score;
		EventManager.TriggerEvent(EventList.OnScoreUpdated);
	}

	public void UpdateScore()
	{
		SetScore(Tower.GetDestroyedBlocRatioForScore());
	}

	public bool IsVictoryScoreReached()
	{
		return Score >= 1f;
	}

	#endregion

	#region Shoot

	public void DrawNewWeapon() {
		float randomFloat = Random.Range(0f, 1f);
		Weapon currentWeapon = GameData.GetWeapon(1);

		foreach (var weapon in GameData.Weapons)
		{
			if (randomFloat <= weapon.ChanceOfBeingPicked && weapon.NbColorAffected < Tower.NbColorInTower)
			{
				currentWeapon = weapon;
				break;
			}
		}

		CurrentWeaponColors = Tower.GetRandomColorsAvailableInTower(currentWeapon.NbColorAffected);
		ComboCount = 0;
		EventManager.TriggerEvent(EventList.OnDrawNewBall);
	}

	public void OnBlocDestroyed(object obj)
	{
		LastDestroyedColor = (BlocColor) obj;
		UpdateScore();
		SetComboCount(ComboCount+1);
	}

	public void SetComboCount(int count)
	{
		ComboCount = count;
		if (ComboCount > _maxComboCount)
			_maxComboCount = ComboCount;

		EventManager.TriggerEvent(EventList.OnComboCountUpdated);
	}

	public void GetShootInput() {

		if (NbShotsAvailable <= 0)
			return;

#if UNITY_EDITOR

		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfos;

			if (Physics.Raycast(ray, out hitInfos)) {
				Bloc hitBloc = hitInfos.transform.GetComponent<Bloc>();
				if (hitBloc != null && hitBloc.Destructible)
					Shoot(hitBloc);
			}
		}

#endif

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
	}

	public void Shoot(Bloc blocToShoot)
	{
		NbShotsAvailable--;
		StartCoroutine(ShootBallCoroutine(0.25f, blocToShoot, CurrentWeaponColors));

		EventManager.TriggerEvent(EventList.OnShotFired, NbShotsAvailable);
		DrawNewWeapon();
	}

	public IEnumerator ShootBallCoroutine(float timeToReachTarget, Bloc blocToShoot, List<BlocColor> weaponColors )
	{
		GameObject ball = Instantiate(Instance.GameData.ShotPrefab, ShotSpawn.position, Quaternion.identity);
		Renderer ballRenderer = ball.GetComponent<Renderer>();

		ballRenderer.material = GameData.GetBlocColorMaterial(CurrentWeaponColors[0]);

		float timer = 0f;
		float progression = 0f;
		Vector3 startPosition = ShotSpawn.position;
		while (timer < timeToReachTarget)
		{
			if (blocToShoot == null)
				break;

			timer += Time.deltaTime;
			progression = timer / timeToReachTarget;

			ball.transform.position = Vector3.Lerp(startPosition, blocToShoot.transform.position, progression);

			float yVariation = GameData.BallTrajectory.Evaluate(progression);
			ball.transform.position += Vector3.up * yVariation;

			yield return new WaitForSeconds(Time.deltaTime);
		}

		Destroy(ball);
		if (blocToShoot.Destructible && weaponColors.Contains(blocToShoot.Color)) {
			List<Bloc> blocsToDestroy = Tower.GetBlocsToDestroy(blocToShoot,weaponColors);
			StartCoroutine(DestroyBlocsWithIntervalTime(GameData.TimeBetweenBlocDestruction, blocsToDestroy));
		}
	}

	public IEnumerator DestroyBlocsWithIntervalTime(float timeBetweenDestruction, List<Bloc> blocs) {
		List<Bloc> tempBlocs = new List<Bloc>();
		tempBlocs.AddRange(blocs);

		foreach (var bloc in tempBlocs) {
			yield return new WaitForSeconds(timeBetweenDestruction);
			if(bloc != null)
				bloc.DestroyBloc(true);
		}
	}

	#endregion


	#region Camera

	public void SetPlayCameraY(float value) {
		_yCameraTarget = Mathf.Max(value, GameData.YLowestCamera);
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

		UpdateCameraTargetHeight();
		Vector3 smoothPosition = Vector3.SmoothDamp(MainCamera.transform.position, CameraTargetTransform.position, ref velocity, 0.2f);

		//Because the smooth position does not move according to a circle, we apply a translation to keep it at the right distance
		Vector3 centerToCameraTarget = CameraTargetTransform.position - new Vector3(0, CameraTargetTransform.position.y, 0);
		Vector3 finalPosition = (smoothPosition - new Vector3(0, smoothPosition.y, 0)).normalized * centerToCameraTarget.magnitude;

		float smoothY = Mathf.SmoothDamp(MainCamera.transform.position.y, CameraTargetTransform.transform.position.y,
			ref YVelocity, 0.2f);

		finalPosition += new Vector3(0, smoothY, 0);

		MainCamera.transform.position = finalPosition;//CameraTargetTransform.position;//Vector3.Lerp(CameraTransform.position, CameraTargetTransform.position, Time.smoothDeltaTime);//Vector3.SmoothDamp(CameraTransform.position, CameraTargetTransform.position, ref velocity, 0.2f);
		MainCamera.transform.LookAt(new Vector3(0, MainCamera.transform.position.y, 0));
	}

	public void UpdateCameraTargetHeight()
	{
		CameraTargetTransform.position = new Vector3(CameraTargetTransform.position.x, _yCameraTarget, CameraTargetTransform.position.z);
	}

	public IEnumerator BlendCameraPosition(Vector3 startPosition, Vector3 endPosition, float time)
	{
		float timer = 0f;
		float progression = 0f;
	
		while (timer < time) {
			timer += Time.deltaTime;
			progression = timer / time;

			MainCamera.transform.position = Vector3.Lerp(startPosition, endPosition, progression);

			yield return new WaitForSeconds(Time.deltaTime);
		}

		MainCamera.transform.position = endPosition;
	}

	#endregion

}
