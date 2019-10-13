using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Tower : MonoBehaviour
{
	private List<Bloc> _tower = new List<Bloc>();
	private int _nbBlocPerLine;
	private int _height;
	private int _nbColorInTower;
	private int _nbLineEnabled;

	private const float Y_START = 1.25f;
	private const float BLOC_HEIGHT = 1.5f;
	private const int NB_BLOC_PER_LINE = 15;
	private const float RADIUS = 2.5f;

	public void Awake()
	{
		RegisterToEvents(true);
	}

	public void RegisterToEvents(bool register)
	{
		EventManager.SetEventListener(EventList.OnBlocDestroyed, UpdateDestructibleBlocs, register);
	}

    public void InitTower(int nbLineEnabled, int height, int nbColor)
    {
	    _nbLineEnabled = nbLineEnabled;
		GenerateTower(NB_BLOC_PER_LINE, height, nbColor);
    }

	void GenerateTower(int nbBlocPerLine, int height, int nbColor) {
		_nbBlocPerLine = nbBlocPerLine;
		_height = height;
		_nbColorInTower = nbColor;

		//STEP 1 : CREATE THE TOWER DATA
		foreach (var bloc in _tower)
		{
			Destroy(bloc.gameObject);
		}
		_tower.Clear();

		for (int y = 0; y < _height; y++) {
			for (int x = 0; x < _nbBlocPerLine ; x++) {
				
				float angle = 360f / (float)nbBlocPerLine;
				float angleOffset = (y % 2 == 0) ? 0 : 360 / (nbBlocPerLine * 2);

				Vector3 position = new Vector3(RADIUS * Mathf.Sin(((angle*x) + angleOffset) * Mathf.Deg2Rad),
												Y_START + y*BLOC_HEIGHT,
												RADIUS * Mathf.Cos(((angle*x) + angleOffset) * Mathf.Deg2Rad));

				GameObject goBloc = Instantiate(GameManager.Instance.GameData.BlocPrefab, position, Quaternion.identity);

				Bloc currentBloc = goBloc.GetComponent<Bloc>();
				currentBloc.Id = y * _nbBlocPerLine + x;
				currentBloc.Y = y;
				currentBloc.X = x;
				currentBloc.Color = (BlocColor)Random.Range(0, nbColor);
				currentBloc.ApplyColor();
				currentBloc.TogglePhysics(false);
				currentBloc.RegisterStartY();

				_tower.Add(currentBloc);
			}
		}

		UpdateDestructibleBlocs();
	}

	//return special ratio updated to fit score
	public float GetDestroyedBlocRatioForScore()
	{
		int cptDestroyedBlocs = 0;
		foreach (var bloc in _tower)
		{
			cptDestroyedBlocs += bloc.Destroyed ? 1 : 0;
		}

		return (float)cptDestroyedBlocs /(float) (_tower.Count - NB_BLOC_PER_LINE );
	}

	public List<BlocColor> GetColorsAvailableInTower()
	{
		List<BlocColor> colors = new List<BlocColor>();

		for (int i = 0; i < _tower.Count; i++)
		{
			if(!colors.Contains(_tower[i].Color))
				colors.Add(_tower[i].Color);

			if (colors.Count >= _nbColorInTower)
				break;
		}

		return colors;
	}

	//Called when the play phase starts and each time a bloc is considered destroyed
	public void UpdateDestructibleBlocs(object obj = null)
	{
		int indexLastDestructibleLine = 0;
		for (int y = _height - 1; y >= 0; y--)
		{
			if (!isLineDestroyed(y))
			{
				indexLastDestructibleLine = y - _nbLineEnabled;
				break;
			}
		}

		for (int y = _height - 1; y >= 0; y--)
		{
			SetLineDestructible(y, y>=indexLastDestructibleLine);
		}

		//Update camera Y
		Bloc MiddleBloc = GetBloc(0, indexLastDestructibleLine + (_nbLineEnabled / 2));
		if(MiddleBloc != null)
			GameManager.Instance.SetPlayCameraY(MiddleBloc.StartY);

	}

	public void SetLineDestructible(int y, bool destructible) {
		for (int x = 0; x < _nbBlocPerLine; x++) {
			Bloc blocToCheck = GetBloc(x, y);
			if (blocToCheck != null && !blocToCheck.Destroyed)
				blocToCheck.SetDestructible(destructible);
			else
			{
				//GetBloc(x, y);
				//Debug.Log("Hello");
			}
		}
	}

	public bool isLineDestroyed(int y)
	{
		for (int x = 0; x < _nbBlocPerLine; x++)
		{
			Bloc blocToCheck = GetBloc(x, y);
			if (blocToCheck != null && !blocToCheck.Destroyed)
				return false;
		}

		return true;
	}

	public Bloc GetBloc(int index) {
		if (index > 0 && index < _tower.Count) {
			return _tower[index];
		}

		return null;
	}

	public Bloc GetBloc(int x, int y) {

		x = x % _nbBlocPerLine; 

		if (y * _nbBlocPerLine + x >= 0 && y * _nbBlocPerLine + x < _tower.Count) {
			return _tower[y * _nbBlocPerLine + x];
		}

		return null;
	}

	//Return the blocs to destroy if a bloc has been hit
	public List<Bloc> GetBlocsToDestroy(Bloc blocHit) {

		List<Bloc> blocsToDestroy = new List<Bloc>();
		blocsToDestroy.Add(blocHit);

		List<Bloc> blocsToCheck = new List<Bloc>();
		blocsToCheck.Add(blocHit);

		//List<Bloc> blocsChecked = new List<Bloc>();
		//blocsChecked.Add(blocHit);

		while (blocsToCheck.Count > 0) {
			Bloc blocNode = blocsToCheck[0];
			blocsToCheck.RemoveAt(0);

			Bloc UpBloc = GetBloc(blocNode.X, blocNode.Y + 1);
			if (UpBloc != null && !UpBloc.Destroyed && UpBloc.Destructible && UpBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(UpBloc)) {
				blocsToDestroy.Add(UpBloc);
				blocsToCheck.Add(UpBloc);
			}

			Bloc DownBloc = GetBloc(blocNode.X, blocNode.Y - 1);
			if (DownBloc != null && !DownBloc.Destroyed && DownBloc.Destructible && DownBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(DownBloc)) {
				blocsToDestroy.Add(DownBloc);
				blocsToCheck.Add(DownBloc);
			}

			Bloc LeftBloc = GetBloc(blocNode.X - 1, blocNode.Y);
			if (LeftBloc != null && !LeftBloc.Destroyed && LeftBloc.Destructible && LeftBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(LeftBloc)) {
				blocsToDestroy.Add(LeftBloc);
				blocsToCheck.Add(LeftBloc);
			}

			Bloc RightBloc = GetBloc(blocNode.X + 1, blocNode.Y);
			if (RightBloc != null && !RightBloc.Destroyed && RightBloc.Destructible && RightBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(RightBloc)) {
				blocsToDestroy.Add(RightBloc);
				blocsToCheck.Add(RightBloc);
			}
		}

		return blocsToDestroy;
	}



}
