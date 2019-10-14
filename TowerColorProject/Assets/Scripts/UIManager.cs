using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	public GameObject SphereToShoot;
	public Renderer SphereToShootRenderer;

	public Text NbShotsText;

	public Image FadeImage;

	public Text TouchToStartText;

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

	public Text TextWin;
  
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

	    if (gamePhase == GamePhase.Init)
	    {
			SphereToShoot.transform.localScale = Vector3.zero;
			NbShotsText.gameObject.SetActive(false);
	    }

	    if (gamePhase == GamePhase.Show)
	    {
		    TextCurrentLevel.text = GameManager.Instance.CurrentLevel.ToString();
		    TextNextLevel.text = (GameManager.Instance.CurrentLevel + 1).ToString();
		    UpdateScoreUI(null);
		}

	    if (gamePhase == GamePhase.Play)
	    {
		    SphereToShoot.transform.localScale = Vector3.one * 0.1f;
		    NbShotsText.gameObject.SetActive(true);
		}

	    if (gamePhase == GamePhase.LevelEnd)
	    {
		    BackgroundNextLevel.color = BackgroundCurrentLevel.color;
		    TextNextLevel.color = Color.white;
	    }
    }

    void UpdateLevelAndScoreColor(object obj)
    {
	    Color newColor = GameManager.Instance.GameData.GetBlocColorColorValue(GameManager.Instance.CurrentShotColor);
	    BackgroundCurrentLevel.color = newColor;
	    ScoreJaugeBackground.color = newColor;
	    BorderNextLevel.color = newColor;
	    ScoreJaugeBackground.color = newColor;
	    ScoreJaugeFill.color = newColor;
	    TextNextLevel.color = newColor;

	    BackgroundNextLevel.color = Color.white;

	    JointScore.color = newColor;
	    BackgroundScore.color = newColor;
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

	public IEnumerator FadeScreen(Color colorTarget, float time)
	{
		float timer = 0f;
		float progression = 0f;
		Color starColor = FadeImage.color;
		while (timer < time) {
			timer += Time.deltaTime;
			progression = timer / time;

			FadeImage.color = Color.Lerp(starColor, colorTarget, progression);

			yield return new WaitForSeconds(Time.deltaTime);
		}

		FadeImage.color = colorTarget;

	}

}
