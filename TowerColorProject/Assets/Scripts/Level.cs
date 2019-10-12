using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
	private List<Bloc> _tower = new List<Bloc>();
	private int _nbBlocPerLine;
	private int _height;
	private int _nbColor;


    // Start is called before the first frame update
    void Start()
    {
        
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
				Bloc currentBloc = new Bloc();
				currentBloc.Id = y * _nbBlocPerLine + x;
				currentBloc.Y = y;
				currentBloc.X = x;
				currentBloc.Color = (BlocColor)Random.Range(0, nbColor);

				_tower.Add(currentBloc);
			}
		}

		//STEP2 : INSTANTIATE THE TOWER (IMPROVEMENT : POOL OBJECTS)

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
