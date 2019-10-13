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

    // Start is called before the first frame update
    void Awake()
    {
        RegisterToEvent(true);
    }

    void RegisterToEvent(bool register)
    {
		EventManager.SetEventListener(EventList.OnShotFired, UpdateNbShotText, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateNbShotText, register);
		EventManager.SetEventListener(EventList.OnDrawNewBall, UpdateSphereToShoot, register);
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
