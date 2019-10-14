
using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform ShakeTransform;

	// How long the object should shake for.
	public float ShakeDuration = 0.25f;

	private float _shakeTimer = 0f;
	private bool _isShaking = false;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float ShakeAmount = 0.7f;
	public float DecreaseFactor = 1.0f;

	Vector3 _originalPos;

	void Awake()
	{
		EventManager.SetEventListener(EventList.OnBlocDestroyedFromHit, Shake, true);
		EventManager.SetEventListener(EventList.OnGamePhaseChanged, ShakeOnLevelEnd, true);
	}

	void ShakeOnLevelEnd(object obj)
	{
		GamePhase gamePhase = (GamePhase) obj;

		if (gamePhase == GamePhase.LevelEnd)
		{
			Shake();
		}
	}

	void Shake(object obj = null)
	{
		if (_isShaking)
			return;

		_shakeTimer = ShakeDuration;
		_originalPos = ShakeTransform.localPosition;
		_isShaking = true;
	}

	void Update() {
		if (_shakeTimer > 0) {
			ShakeTransform.localPosition = _originalPos + Random.insideUnitSphere * ShakeAmount;

			_shakeTimer -= Time.deltaTime * DecreaseFactor;
		} else {
			_shakeTimer = 0f;
			ShakeTransform.localPosition = _originalPos;
			_isShaking = false;
		}
	}
}