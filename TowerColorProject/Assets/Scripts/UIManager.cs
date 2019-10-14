using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

	[Header("Texts to show steps")]
	public Text TextWin;
	public Text TextFail;

	[Header("Fail Timer")]
	public GameObject FailTimer;
	public Image FailTimerJauge;

	[Header("Combo UI")]
	public Text ComboCountText;
	public Text ComboLabelText;

	public List<GameObject> ComboMedals;

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
		EventManager.SetEventListener(EventList.OnComboCountUpdated, UpdateComboUI, register);
		EventManager.SetEventListener(EventList.OnFailTimerUpdate, UpdateFailTimerUI, register);
	}

    void SetLevelPanel(object obj)
    {
	    GamePhase gamePhase = (GamePhase) obj;

	    if (gamePhase == GamePhase.Init)
	    {
			SphereToShoot.transform.localScale = Vector3.zero;
			NbShotsText.gameObject.SetActive(false);

			BackgroundNextLevel.color = Color.white;
			TextNextLevel.color = BackgroundCurrentLevel.color;

			foreach (var medal in ComboMedals)
			{
				medal.SetActive(false);
			}

			FailTimer.SetActive(false);
			TextFail.gameObject.SetActive(false);
			FailTimerJauge.fillAmount = 0f;
	    }

	    if (gamePhase == GamePhase.Show)
	    {
		    TextCurrentLevel.text = (GameManager.Instance.CurrentLevel + 1).ToString();
		    TextNextLevel.text = (GameManager.Instance.CurrentLevel + 2).ToString();
		    UpdateScoreUI(null);
		}

	    if (gamePhase == GamePhase.Play)
	    {
		    SphereToShoot.transform.localScale = Vector3.one * 0.07f;
		    NbShotsText.gameObject.SetActive(true);
		}

	    if (gamePhase == GamePhase.LevelEnd)
	    {
		    ScoreJaugeFill.fillAmount = 1f;
		    BackgroundNextLevel.color = BackgroundCurrentLevel.color;
		    TextNextLevel.color = Color.white;
			FailTimer.SetActive(false);
	    }

	    if (gamePhase == GamePhase.LevelFail) {
			TextFail.gameObject.SetActive(true);
		    FailTimer.SetActive(false);
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

    void UpdateFailTimerUI(object obj)
    {
	    float progression = (float) obj;
		FailTimer.gameObject.SetActive(true);

		FailTimerJauge.fillAmount = progression;
    }

    void UpdateComboUI(object obj)
    {
	    int comboCount = GameManager.Instance.ComboCount;

	    if (comboCount > 0)
	    {
		    ComboCountText.gameObject.SetActive(true);
		    ComboLabelText.gameObject.SetActive(true);
		    ComboCountText.text = comboCount + " HITS";
		    ComboStep step = GameManager.instance.GameData.GetComboStep(comboCount);
		    ComboLabelText.text = step.Label;
		    ComboCountText.color =
			    GameManager.Instance.GameData.GetBlocColorColorValue(GameManager.instance.LastDestroyedColor);

		    int indexCombo = GameManager.instance.GameData.ComboSteps.IndexOf(step);
		    if (indexCombo > 0 && indexCombo < ComboMedals.Count)
		    {
			    GameObject medal = ComboMedals[indexCombo - 1];
			    if (!medal.activeSelf)
			    {
				    medal.SetActive(true);
				    medal.transform.localScale = Vector3.zero;
				    medal.transform.DOScale(2f, 0.25f).OnComplete(() => { medal.transform.DOScale(1f, 0.25f); });
				    ComboLabelText.transform.DOScale(2f, 0.25f).OnComplete(() =>
				    {
					    ComboLabelText.transform.DOScale(1f, 0.25f);
				    });
			    }
		    }
	    }
	    else
	    {
			ComboCountText.gameObject.SetActive(false);
			ComboLabelText.gameObject.SetActive(false);
	    }
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
