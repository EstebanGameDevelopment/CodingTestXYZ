using System.Collections.Generic;
using yourvrexperience.Utils;
using UnityEngine;
using System;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	[System.Serializable]
	public class SerializedGameCubeData
	{
		public GameCubeData[] Cubes;
	
		public int Total;
	}

	[System.Serializable]
	public class GameCubeData
	{
		public int Id = 0;
		public int Color = 0;
		public Vector3 Position;
		public float  Size;
		public float Rotation;

		public GameCubeData(int id, int color, Vector3 position, float size, float rotation)
		{
			Id = id;
			Color = color;
			Position = position;
			Size = size;
			Rotation = rotation;
		}
	}

	public class CubesController : MonoBehaviour
	{
		public const string EventCubesControllerAllDestroyed = "EventCubesControllerAllDestroyed";
		public const string EventCubesControllerNetworkInitialization = "EventCubesControllerNetworkInitialization";

        private static CubesController _instance;

        public static CubesController Instance
        {
            get
            {				
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(CubesController)) as CubesController;
                }
                return _instance;
            }
        }

		[SerializeField] private GameObject gameCube;

		private List<CubeView> _cubes = new List<CubeView>();
		private List<GameObject> _borders;

		private Vector3 _playerPosition;
		private Vector3 _minArea;
		private Vector3 _maxArea;
		private Vector3 _centerGameArea;
		private Vector3 _sizeGameArea;

		public void Initialize(Vector3 playerPosition, Vector3 minArea, Vector3 maxArea)
		{						
			SystemEventController.Instance.Event += OnSystemEvent;
			_playerPosition = playerPosition;
			_minArea = minArea;
			_maxArea = maxArea;

			CreateBorders();
#if ENABLE_NETWORKING
			NetworkController.Instance.NetworkEvent += OnNetworkEvent;
			if (MainController.Instance.NumberClients == 1)
			{
				CreateGameCubes();
			}
			else
			{
				if (NetworkController.Instance.IsServer)
				{
					CreateGameCubes();
				}
			}
#else
			CreateGameCubes();
#endif		
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
#if ENABLE_NETWORKING
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
#endif			
		}

        private void CreateBorders()
		{
			_centerGameArea =  (_maxArea + _minArea) / 2;
			_sizeGameArea = _maxArea - _minArea;
			_borders = new List<GameObject>();
			
			CreateSingleBorder(new Vector3(_centerGameArea.x, _centerGameArea.y + _sizeGameArea.y, _centerGameArea.z), new Vector3(_sizeGameArea.x * 2, _sizeGameArea.y, _sizeGameArea.z * 2));
			CreateSingleBorder(new Vector3(_centerGameArea.x, _centerGameArea.y - _sizeGameArea.y, _centerGameArea.z), new Vector3(_sizeGameArea.x * 2, _sizeGameArea.y, _sizeGameArea.z * 2));
			CreateSingleBorder(new Vector3(_centerGameArea.x + _sizeGameArea.x, _centerGameArea.y, _centerGameArea.z), new Vector3(_sizeGameArea.x, _sizeGameArea.y * 2, _sizeGameArea.z * 2));
			CreateSingleBorder(new Vector3(_centerGameArea.x - _sizeGameArea.x, _centerGameArea.y, _centerGameArea.z), new Vector3(_sizeGameArea.x, _sizeGameArea.y * 2, _sizeGameArea.z * 2));
			CreateSingleBorder(new Vector3(_centerGameArea.x, _centerGameArea.y, _centerGameArea.z + _sizeGameArea.z), new Vector3(_sizeGameArea.x * 2, _sizeGameArea.y * 2, _sizeGameArea.z));
			CreateSingleBorder(new Vector3(_centerGameArea.x, _centerGameArea.y, _centerGameArea.z - _sizeGameArea.z), new Vector3(_sizeGameArea.x * 2, _sizeGameArea.y * 2, _sizeGameArea.z));
		}

		private void CreateSingleBorder(Vector3 position, Vector3 sizeGameArea)
		{
			GameObject borderCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			borderCube.transform.localScale = sizeGameArea;
			borderCube.transform.position = position;
			borderCube.layer = LayerMask.NameToLayer(GameLevelData.Instance.LayerGameArea);
			borderCube.transform.GetComponent<Renderer>().enabled = false;
			borderCube.name = "Border";
			borderCube.transform.parent = this.transform;
			_borders.Add(borderCube);
		}

		public CubeView GetCubeViewById(int cubeID)
		{
			foreach (CubeView cube in _cubes)
			{
				if (cube.Id == cubeID) return cube;
			}
			return null;
		}

		private void CreateGameCubes()
		{
			List<GameCubeData> cubes = new List<GameCubeData>();
			int numberCubeToCreate = UnityEngine.Random.Range(GameLevelData.Instance.MinNumberCubes, GameLevelData.Instance.MaxNumberCubes);
			for (int i = 0; i < numberCubeToCreate; i++)
			{
				Vector3 cubePosition = Vector3.zero;
				do 
				{
					cubePosition.x = UnityEngine.Random.Range(_minArea.x, _maxArea.x);
					cubePosition.y = UnityEngine.Random.Range(_minArea.y, _maxArea.y);
					cubePosition.z = UnityEngine.Random.Range(_minArea.z, _maxArea.z);

				} while (Vector3.Distance(_playerPosition, cubePosition) < GameLevelData.Instance.MaxSizeCubes * 2);

				float cubeRotation = UnityEngine.Random.Range(0, 360);
				float cubeSize = UnityEngine.Random.Range(GameLevelData.Instance.MinSizeCubes, GameLevelData.Instance.MaxSizeCubes);
				int cubeColor = (int)UnityEngine.Random.Range(0, 7);
				cubes.Add(new GameCubeData(i, cubeColor, cubePosition, cubeSize, cubeRotation));
			}

			SerializedGameCubeData serializedGameCubeData = new SerializedGameCubeData();
			serializedGameCubeData.Cubes = cubes.ToArray();
			serializedGameCubeData.Total = cubes.Count;

#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients == 1)
			{
				ParseJsonCubes(serializedGameCubeData);
			}
			else
			{
				string jsonData = JsonUtility.ToJson(serializedGameCubeData, true);
				NetworkController.Instance.DispatchNetworkEvent(EventCubesControllerNetworkInitialization, -1, -1, jsonData);
			}
#else
			ParseJsonCubes(serializedGameCubeData);			
#endif			
		}

		private void ParseJsonCubes(SerializedGameCubeData data)
		{
			_cubes = new List<CubeView>();
			foreach(GameCubeData cube in data.Cubes)
			{
				CubeView cubeView = (Instantiate(gameCube) as GameObject).GetComponent<CubeView>();
				Color cubeColor = Color.white;
				switch (cube.Color)
				{
					case 0:
						cubeColor = Color.red;
						break;

					case 1:
						cubeColor = Color.green;
						break;

					case 2:
						cubeColor = Color.blue;
						break;

					case 3:
						cubeColor = Color.cyan;
						break;

					case 4:
						cubeColor = Color.magenta;
						break;

					case 5:
						cubeColor = Color.yellow;
						break;

					case 6:
						cubeColor = Color.grey;
						break;					

					case 7:
						cubeColor = Color.white;
						break;																																				
				}

				cubeView.Initialize(cube.Id, cube.Position, cube.Rotation, cube.Size, cubeColor);
				float finalTimeAnimation = UnityEngine.Random.Range(1, GameLevelData.Instance.TimeCubesAnimation);
				float initialDelay = GameLevelData.Instance.TimeCubesAnimation - finalTimeAnimation;
				cubeView.Animate(initialDelay, finalTimeAnimation);
				cubeView.transform.parent = this.transform;
				_cubes.Add(cubeView);
			}
		}

#if ENABLE_NETWORKING
        private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
        {
            if (nameEvent.Equals(EventCubesControllerNetworkInitialization))
			{
				if (_cubes.Count == 0)
				{
					string dataJson = (string)parameters[0];
					SerializedGameCubeData serializedGameCubeData = JsonUtility.FromJson<SerializedGameCubeData>(dataJson);
					ParseJsonCubes(serializedGameCubeData);
				}
			}
			if (nameEvent.Equals(CubeView.EventCubeViewNetworkDestruction))
			{
				int cubeViewID = (int)parameters[0];
				int bulletID = (int)parameters[1];
				Vector3 positionCube = (Vector3)parameters[2];
				int playerID = (int)parameters[3];
				CubeView cubeToDestroy = GetCubeViewById(cubeViewID);
				if (cubeToDestroy != null)
				{
					cubeToDestroy.DestroyCube(bulletID, positionCube, playerID);
				}
			}
        }
#endif		

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(CubeView.EventCubeViewDestroyed))
			{
				CubeView targetCube = GetCubeViewById((int)parameters[0]);
				if (targetCube != null)
				{
					if (_cubes.Remove(targetCube))
					{
						if (_cubes.Count == 0)
						{
							SystemEventController.Instance.DispatchSystemEvent(EventCubesControllerAllDestroyed);
						}
					}
				}
			}
            if (nameEvent.Equals(MainController.EventMainControllerReleaseGameResources))
			{
				if (_instance != null)
				{
					_instance = null;
					foreach (CubeView cube in _cubes)
					{
						GameObject.Destroy(cube.gameObject);
					}
					foreach (GameObject border in _borders)
					{
						GameObject.Destroy(border);
					}					
					_cubes.Clear();

					GameObject.Destroy(this.gameObject);
				}
			}
        }
	}
}