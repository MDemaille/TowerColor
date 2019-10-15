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

	[Header("Weapons")]
	public List<WeaponUI> WeaponsUi;
	public Text WeaponLabelText;

    void Awake()
    {
        RegisterToEvent(true);
	}

    void RegisterToEvent(bool register)
    {
		EventManager.SetEventListener(EventList.OnShotFired, UpdateNbShotText, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateNbShotText, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateWeaponColors, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateLevelAndScoreColor, register);
		EventManager.SetEventListener(EventList.OnGamePhaseChanged, SetLevelPanel, register);
		EventManager.SetEventListener(EventList.OnBlocDestroyed, UpdateScoreUI, register);
		EventManager.SetEventListener(EventList.OnComboCountUpdated, UpdateComboUI, register);
		EventManager.SetEventListener(EventList.OnFailTimerUpdate, UpdateFailTimerUI, register);
		EventManager.SetEventListener(EventList.OnShotFired, ResetComboMedals, register);
		EventManager.SetEventListener(EventList.OnMaxComboCountShow, ShowMaxComboUI, register);
	}

    void SetLevelPanel(object obj)
    {
	    GamePhase gamePhase = (GamePhase) obj;

	    if (gamePhase == GamePhase.Init)
	    {
			//SphereToShoot.transform.localScale = Vector3.zero;
			HideWeaponUI();
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

			WeaponLabelText.transform.localScale = Vector3.zero;

	    }

	    if (gamePhase == GamePhase.Show)
	    {
		    TextCurrentLevel.text = (GameManager.Instance.CurrentLevel + 1).ToString();
		    TextNextLevel.text = (GameManager.Instance.CurrentLevel + 2).ToString();
		    UpdateScoreUI(null);
		}

	    if (gamePhase == GamePhase.Play)
	    {
		    //SphereToShoot.transform.localScale = Vector3.one * 0.07f;
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
	    Color newColor = GameManager.Instance.GameData.GetBlocColorColorValue(GameManager.Instance.CurrentWeaponColors[0]);
	    BackgroundCurrentLevel.color = newColor;
	    ScoreJaugeBackground.color = newColor;
	    BorderNextLevel.color = newColor;
	    ScoreJaugeBackground.color = newColor;
	    ScoreJaugeFill.color = newColor;
	    TextNextLevel.color = newColor;

	    BackgroundNextLevel.color = Color.white;
    }

    void UpdateScoreUI(object obj)
    {
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
		//Quickfix
		if (GameManager.Instance.CurrentGamePhase == GamePhase.LevelEnd)
			return;

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

    void ShowMaxComboUI(object obj)
    {
	    int maxCombo = (int) obj;
	    ComboCountText.text = maxCombo + " HITS";
		ComboStep step = GameManager.instance.GameData.GetComboStep(maxCombo);
	    ComboLabelText.text = step.Label;
	    ComboCountText.color =
		    GameManager.Instance.GameData.GetBlocColorColorValue(GameManager.instance.LastDestroyedColor);

	    int indexCombo = GameManager.instance.GameData.ComboSteps.IndexOf(step);
	    for (int i = 0; i <= indexCombo; i++)
	    {
		    if (i >= 0 && i < ComboMedals.Count)
		    {
			    GameObject medal = ComboMedals[i];
			    medal.SetActive(true);
			    medal.transform.localScale = Vector3.zero;
			    medal.transform.DOScale(2f, 0.25f).OnComplete(() => { medal.transform.DOScale(1f, 0.25f); });
		    }
	    }
	}

    void ResetComboMedals(object obj)
    {
	    foreach (var medal in ComboMedals) {
		    medal.SetActive(false);
	    }
	}

	void UpdateNbShotText(object obj)
	{
		NbShotsText.text = GameManager.Instance.NbShotsAvailable.ToString();
	}

	void UpdateWeaponColors(object obj)
	{
		//SphereToShootRenderer.material = GameManager.Instance.GameData.GetBlocColorMaterial(GameManager.Instance.CurrentWeaponColors[0]);
		int weaponColorCount = GameManager.Instance.CurrentWeaponColors.Count;

		int weaponId = GameManager.Instance.CurrentWeaponColors.Count - 1;
		for (int i = 0; i < WeaponsUi.Count; i++)
		{
			WeaponsUi[i].gameObject.SetActive(i == weaponId);
		}

		for (int i = 0; i < weaponColorCount; i++)
		{
			WeaponsUi[weaponId].SetColorImage(i,GameManager.Instance.CurrentWeaponColors[i]);
		}

		Weapon currentWeapon = GameManager.instance.GameData.GetWeapon(weaponColorCount);
		WeaponLabelText.text = currentWeapon.Label;
		WeaponLabelText.transform.DOScale(2f, 0.4f).OnComplete(() => { WeaponLabelText.transform.DOScale(0f, 0.4f); });
	}

	void HideWeaponUI()
	{
		for (int i = 0; i < WeaponsUi.Count; i++) {
			WeaponsUi[i].gameObject.SetActive(false);
		}
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
