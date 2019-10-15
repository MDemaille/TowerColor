using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
	public List<Renderer> _weaponRenderers;

	public void SetColorImage(int id, BlocColor color)
	{
		if (id < 0 || id >= _weaponRenderers.Count)
		{
			Debug.LogError(id + " out of range for this UI with only " + _weaponRenderers.Count + "color");
			return;	
		}

		_weaponRenderers[id].material = GameManager.Instance.GameData.GetBlocColorMaterial(color);
	}
}
