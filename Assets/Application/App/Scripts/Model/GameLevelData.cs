using System.Collections.Generic;
using UnityEngine;
using yourvrexperience.Utils;

namespace companyX.codingtest
{
	[System.Serializable]
	public class SerializedHighscoresData
	{
		public HighscoreData[] Highscores;
	}

    [CreateAssetMenu(menuName = "Game/GameLevelData")]
	public class GameLevelData : ScriptableObject
    {
        public const string EventGameLevelDataScoreUpdated = "EventGameLevelDataScoreUpdated";
        public const string EventGameLevelDataTimeUpdated = "EventGameLevelDataTimeUpdated";

        public enum GameLevelStates { Initialization = 0, Presentation, InGame, GameOver }

        public const string HighscoresDataKey = "HighscoresDataKey";

	    private static GameLevelData _instance;
        public static GameLevelData Instance
        {
            get { return _instance; }
        }

        [Tooltip("Name of the layer where the player is standing")]
		[SerializeField] private string layerFloorName = "Floor";
        [Tooltip("Name of the layer that will limit the game area")]
        [SerializeField] private string layerGameArea = "GameArea";
        [Tooltip("Name of the layer of the UI")]
        [SerializeField] private string layerUI = "UI";
        [Tooltip("Name of the layer of the Gun")]
        [SerializeField] private string layerGun = "Gun";
        [Tooltip("Speed of movement of the desktop client")]
		[SerializeField] private float playerDesktopSpeed = 50;
        [Tooltip("Speed of movement of the VR client")]
        [SerializeField] private float playerVRSpeed = 20;
        [Tooltip("Sensitivity of the rotation of the camera in desktop mode")]
        [SerializeField] private float sensitivityCamera = 7;
        [Tooltip("Maximum number of cubes generated")]
        [SerializeField] private int maxNumberCubes = 40;
        [Tooltip("Minimum number of cubes generated")]
        [SerializeField] private int minNumberCubes = 10;
        [Tooltip("Maximum size of the cubes generated")]
        [SerializeField] private float maxSizeCubes = 3;
        [Tooltip("Minimum size of the cubes generated")]
        [SerializeField] private float minSizeCubes = 0.5f;
        [Tooltip("Total time of the animation of the cubes to appear in the game level")]
        [SerializeField] private float timeCubesAnimation = 6;
        [Tooltip("Total size of the pool of bullets")]
        [SerializeField] private int bulletPoolSize = 10;
        [Tooltip("Impulse force applied to the bullet")]
        [SerializeField] private float bulletSpeed = 10;
        [Tooltip("Total number of available highscores")]
        [SerializeField] private int maxNumberHighscores = 10;

        private int _layerFloor;
        private int _layerUI;
        private int _layerGun;
		private GameLevelStates _gameLevelState = GameLevelStates.Initialization;
        private float _timerLevel = 0;
        private int _currentScore = 0;
        private int _currentTime = 0;
        private List<HighscoreData> _highscores = new List<HighscoreData>();

        public int LayerFloor
        {
            get { return _layerFloor; }
        }
        public int LayerUI
        {
            get { return _layerUI; }
        }
        public int LayerGun
        {
            get { return _layerGun; }
        }        
        public string LayerGameArea
        {
            get { return layerGameArea; }
        }
        public float PlayersDesktopSpeed
        {
            get { return playerDesktopSpeed; }
        }
        public float PlayerVRSpeed
        {
            get { return playerVRSpeed; }
        }
        public float SensitivityCamera
        {
            get { return sensitivityCamera; }
        }
        public GameLevelStates GameLevelState
        {
            get { return _gameLevelState; }
        }
        public float TimerLevel
        {
            get { return _timerLevel; }
        }
        public int MaxNumberCubes
        {
            get { return maxNumberCubes; }
        }
        public int MinNumberCubes
        {
            get { return minNumberCubes; }
        }
        public float MaxSizeCubes
        {
            get { return maxSizeCubes; }
        }
        public float MinSizeCubes
        {
            get { return minSizeCubes; }
        }
        public float TimeCubesAnimation
        {
            get { return timeCubesAnimation; }
        }
        public int BulletPoolSize
        {
            get { return bulletPoolSize; }
        }
        public float BulletSpeed
        {
            get { return bulletSpeed; }
        }
        public int CurrentScore
        {
            get { return _currentScore; }
            set { _currentScore = value; 
                SystemEventController.Instance.DispatchSystemEvent(EventGameLevelDataScoreUpdated, _currentScore);
            }
        }
        public int CurrentTime
        {
            get { return _currentTime; }
            set { _currentTime = value; 
                SystemEventController.Instance.DispatchSystemEvent(EventGameLevelDataTimeUpdated, _currentScore);
            }
        }
        public List<HighscoreData> Highscores
        {
            get { return _highscores; }
        }

        public void Initialize()
        {
            _instance = this;
            _layerFloor = LayerMask.GetMask(layerFloorName);
            _layerUI = LayerMask.GetMask(layerUI);
            _layerGun = LayerMask.GetMask(layerGun);
            LoadHighscores();
        }

        public void ResetGameLevelData()
        {
		    _gameLevelState = GameLevelStates.Initialization;
            _timerLevel = 0;
        }

        public void SaveGameLevelState(GameLevelStates gameLevelState, float timerLevel)
        {
		    _gameLevelState = gameLevelState;
            _timerLevel = timerLevel;
        }

        public void RegisterNewHigscore(int score, int time)
        {
            int indexToInsert = 0;
            for (int i = _highscores.Count - 1; i >= 0; i--)
            {
                HighscoreData highscore = _highscores[i];
                if (highscore.Score > score)
                {
                    indexToInsert = i + 1;
                    break;
                }
                else
                {
                    if (highscore.Score == score)
                    {
                        if (highscore.Time < time)
                        {
                            indexToInsert = i + 1;
                            break;
                        }
                    }
                }
            }
            _highscores.Insert(indexToInsert, new HighscoreData(score, time));

            while (_highscores.Count > maxNumberHighscores)
            {
                _highscores.RemoveAt(_highscores.Count - 1);
            }
            SaveHighscores();
        }

        private void SaveHighscores()
        {
            if (_highscores.Count > 0)
            {
                SerializedHighscoresData serializedHighscoresData = new SerializedHighscoresData();
                serializedHighscoresData.Highscores = _highscores.ToArray();
                string jsonData = JsonUtility.ToJson(serializedHighscoresData, true);				
                PlayerPrefs.SetString(HighscoresDataKey, jsonData);
            }
        }

        private void LoadHighscores()
        {
            string jsonData = PlayerPrefs.GetString(HighscoresDataKey, "");
            if (jsonData.Length > 0)
            {
                SerializedHighscoresData serializedHighscoresData = JsonUtility.FromJson<SerializedHighscoresData>(jsonData);
                
                _highscores = new List<HighscoreData>();
                for (int i = 0; i < serializedHighscoresData.Highscores.Length; i++)
                {
                    _highscores.Add(new HighscoreData(serializedHighscoresData.Highscores[i]));
                }
            }
        }
	}
}