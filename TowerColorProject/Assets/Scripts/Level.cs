using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
	private List<Bloc> _tower = new List<Bloc>();
	private int _nbBlocPerLine;
	private int _height;
	private int _nbColor;

	private const float Y_START = 1.5f;
	private const float BLOC_HEIGHT = 2;
	private const int NB_BLOC_PER_LINE = 15;
	private const float RADIUS = 2.5f;

    // Start is called before the first frame update
    void Start()
    {
		GenerateTower(NB_BLOC_PER_LINE, 40, 6);
    }

	// Update is called once per frame
	void GenerateTower(int nbBlocPerLine, int height, int nbColor) {
		_nbBlocPerLine = nbBlocPerLine;
		_height = height;
		_nbColor = nbColor;

		//STEP 1 : CREATE THE TOWER DATA
		_tower.Clear();

		for (int y = 0; y < _height; y++) {
			for (int x = 0; x < _nbBlocPerLine ; x++) {

				/*float value = (float)x / (float)_nbBlocPerLine;

				float teta = (Mathf.PI * 2) * value;
				float phi = -Mathf.PI / 2;

				float pX =  (float)(Mathf.Sin(phi) * Mathf.Cos(teta));
				float pZ =  (float)(Mathf.Sin(phi) * Mathf.Sin((teta)));

				Vector3 position = new Vector3(pX, Y_START + y * BLOC_HEIGHT,pZ);*/

				
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

				_tower.Add(currentBloc);
			}
		}
	}

	public Bloc GetBloc(int index) {
		if (index > 0 && index < _tower.Count) {
			return _tower[index];
		}

		return null;
	}

	public Bloc GetBloc(int x, int y) {

		x = x % _tower.Count; 

		if (y * _nbBlocPerLine + x > 0 && y * _nbBlocPerLine + x < _tower.Count) {
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

		while (blocsToCheck.Count > 0) {
			Bloc blocNode = blocsToCheck[0];
			blocsToCheck.RemoveAt(0);

			Bloc UpBloc = GetBloc(blocNode.X, blocNode.Y + 1);
			if (UpBloc != null && UpBloc.Destructible && UpBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(UpBloc)) {
				blocsToDestroy.Add(UpBloc);
				blocsToCheck.Add(UpBloc);
			}

			Bloc DownBloc = GetBloc(blocNode.X, blocNode.Y - 1);
			if (DownBloc != null && DownBloc.Destructible && DownBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(DownBloc)) {
				blocsToDestroy.Add(DownBloc);
				blocsToCheck.Add(DownBloc);
			}

			Bloc LeftBloc = GetBloc(blocNode.X - 1, blocNode.Y);
			if (LeftBloc != null && LeftBloc.Destructible && LeftBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(LeftBloc)) {
				blocsToDestroy.Add(LeftBloc);
				blocsToCheck.Add(LeftBloc);
			}

			Bloc RightBloc = GetBloc(blocNode.X + 1, blocNode.Y);
			if (RightBloc != null && RightBloc.Destructible && RightBloc.Color.Equals(blocHit.Color) && !blocsToDestroy.Contains(RightBloc)) {
				blocsToDestroy.Add(RightBloc);
				blocsToCheck.Add(RightBloc);
			}
		}

		return blocsToDestroy;
	}



}
