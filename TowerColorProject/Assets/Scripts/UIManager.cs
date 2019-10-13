using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public GameObject SphereToShoot;
	public Renderer SphereToShootRenderer;

	public Text NbShotsText;

	[Header("Level and Score")]
	public Image BackgroundCurrentLevel;
	public Text TextCurrentLevel;
	public RectTransform ScoreJauge;
	public Image ScoreJaugeBackground;
	public Image ScoreJaugeFill;
	public Image BorderNextLevel;
	public Image BackgroundNextLevel;
	public Text TextNextLevel;

	public RectTransform ScorePanel;
	public Image JointScore;
	public Image BackgroundScore;
	public Text ScoreText;

	private float _xStartScore;
	private float _xEndScore;
  
    void Awake()
    {
        RegisterToEvent(true);

        _xStartScore = ScoreJauge.position.x - ScoreJauge.sizeDelta.x;
		_xEndScore = ScoreJauge.position.x + ScoreJauge.sizeDelta.x;
	}

    void RegisterToEvent(bool register)
    {
		EventManager.SetEventListener(EventList.OnShotFired, UpdateNbShotText, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateNbShotText, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateSphereToShoot, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateLevelAndScoreColor, register);
		EventManager.SetEventListener(EventList.OnGamePhaseChanged, SetLevelPanel, register);
		EventManager.SetEventListener(EventList.OnBlocDestroyed, UpdateScoreUI, register);
	}

    void SetLevelPanel(object obj)
    {
	    GamePhase gamePhase = (GamePhase) obj;
	    if (gamePhase == GamePhase.Play)
	    {
		    TextCurrentLevel.text = GameManager.Instance.CurrentLevel.ToString();
		    TextNextLevel.text = (GameManager.Instance.CurrentLevel + 1).ToString();
		    UpdateScoreUI(null);
		}
    }

    void UpdateLevelAndScoreColor(object obj) {

    }

    void UpdateScoreUI(object obj)
    {
	    float x = Mathf.Lerp(_xStartScore, _xEndScore, GameManager.Instance.Score);
		ScorePanel.position = new Vector2(x, ScorePanel.position.y);

		double prctScore = Math.Ceiling((double)GameManager.Instance.Score * 100);
		ScoreText.text = prctScore.ToString() + "%";

		ScoreJaugeFill.fillAmount = GameManager.Instance.Score;
    }

	void UpdateNbShotText(object obj)
	{
		NbShotsText.text = GameManager.Instance.NbShotsAvailable.ToString();
	}

	void UpdateSphereToShoot(object obj)
	{
		SphereToShootRenderer.material = GameManager.Instance.GameData.GetBlocColorMaterial(GameManager.Instance.CurrentShotColor);
	}

}
