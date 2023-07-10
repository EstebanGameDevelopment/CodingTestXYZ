using yourvrexperience.Utils;
using UnityEngine;
using yourvrexperience.VR;
using System.Collections.Generic;
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
using UnityEngine.XR;
#endif
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	public class MainController : MonoBehaviour
	{
		public enum StatesGame { None = 0, MainMenu, Connecting, Loading, Run, Pause, ReleaseMemory }

		public const string EventMainControllerReleaseGameResources = "EventMainControllerReleaseGameResources";
		public const string EventMainControllerGameReadyToStart = "EventMainControllerGameReadyToStart";
		public const string EventMainControllerChangeState = "EventMainControllerChangeState";
		public const string EventMainControllerLocalPlayerViewAssigned = "EventMainControllerLocalPlayerViewAssigned";
		public const string EventMainControllerAllPlayerViewReadyToStartGame = "EventMainControllerAllPlayerViewReadyToStartGame";
		public const string EventMainControllerReportPlayerScore = "EventMainControllerReportPlayerScore";
		public const string EventMainControllerAllPlayersScoresReported = "EventMainControllerAllPlayersScoresReported";

        private static MainController _instance;

        public static MainController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(MainController)) as MainController;
                }
                return _instance;
            }
        }

		[SerializeField] private GameLevelData gameLevelData;
		[SerializeField] private GameObject desktopPlayer;
		[SerializeField] private GameObject VRPlayer;
		[SerializeField] private GameObject PlayerViewHandLeftPrefab;
		[SerializeField] private GameObject PlayerViewHandRightPrefab;
		[SerializeField] private GameObject MenuLevel;
		[SerializeField] private GameObject GameLevel;
		[SerializeField] private GameObject CameraFade;
		[SerializeField] private GameObject CubesController;
		[SerializeField] private GameObject BulletsController;
		[SerializeField] private GameObject FXsController;
		[SerializeField] private Material SkyBoxMenu;
		[SerializeField] private Material SkyBoxGame;

		private IGameState _gameState;
		private IInputController _inputController;
		private PlayerView _playerView;
		private LevelView _levelView;
		private StatesGame _state;
		private StatesGame _previousState;
		private CameraFader _cameraFader;
		private Dictionary<PlayerView, int> _registeredPlayers = new Dictionary<PlayerView, int>();
		private Dictionary<int, int> _scorePlayers = new Dictionary<int, int>();

		private bool _inputInited = false;
		private bool _screenInited = false;
		private bool _requestCreation = false;
		private int _numberClients = 2;

		public IInputController GameInputController
		{
			get { return _inputController; }
		}
		public PlayerView PlayerView
		{
			get { return _playerView; }
		}
		public LevelView LevelView
		{
			get { return _levelView; }
		}
		public StatesGame State
		{
			get { return _state; }
		}
		public StatesGame PreviousState
		{
			get { return _previousState; }
		}
		public Dictionary<int, int> ScorePlayers
		{
			get { return _scorePlayers; }
		}
		public int NumberClients
		{
			get { 
#if ENABLE_NETWORKING
				return _numberClients; 
#else
				return 1; 
#endif
			}
			set { _numberClients = value; }
		}

		void Awake()
		{
			gameLevelData.Initialize();
			SystemEventController.Instance.Event += OnSystemEvent;
		}

		void Start()
		{									
			RenderSettings.skybox = SkyBoxMenu;

#if ENABLE_NETWORKING
			NetworkController.Instance.Initialize();
			NetworkController.Instance.NetworkEvent += OnNetworkEvent;
#endif

#if !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
			InitializeSystem(true);
#endif
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
#if ENABLE_NETWORKING
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
#endif
		}

		public void FadeInCamera()
		{
			if (_cameraFader != null)
			{
				_cameraFader.FadeIn();
			}		
		}

		public void FadeOutCamera()
		{
			if (_cameraFader != null)
			{
				_cameraFader.FadeOut();
			}		
		}

		public void CreateMenuLevelView()
		{
			if (_levelView == null)		
			{
				_levelView = (Instantiate(MenuLevel) as GameObject).GetComponent<LevelView>();
			}
			_levelView.transform.position = Vector3.zero;
			_levelView.ReportInited();

			RenderSettings.skybox = SkyBoxMenu;
		}

		public void CreateGameElementsView()
		{
			if (_requestCreation) return;
			_requestCreation = true;

			GameLevelData.Instance.CurrentScore = 0;
			GameLevelData.Instance.CurrentTime = 0;

			if (_playerView == null)
			{			
#if ENABLE_NETWORKING
				if (MainController.Instance.NumberClients == 1)
				{
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
					Instantiate(VRPlayer);
#else
					Instantiate(desktopPlayer);
#endif				
				}
				else
				{
#if ENABLE_OCULUS || ENABLE_OPENXR
					NetworkController.Instance.CreateNetworkPrefab(false, VRPlayer.name, VRPlayer.gameObject, "GameElements\\Player\\" + VRPlayer.name, Vector3.zero, Quaternion.identity, 0);
#else
					NetworkController.Instance.CreateNetworkPrefab(false, desktopPlayer.name, desktopPlayer.gameObject, "GameElements\\Player\\" + desktopPlayer.name, Vector3.zero, Quaternion.identity, 0);
#endif					
				}
#else
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
				Instantiate(VRPlayer);
#else
				Instantiate(desktopPlayer);
#endif				
#endif				
			}

			// Cubes Controller
			Instantiate(CubesController);

			// Bullets Controller
			Instantiate(BulletsController);

			// FX Controller
			Instantiate(FXsController);

			RenderSettings.skybox = SkyBoxGame;
		}

		public void CreateCameraFader()
		{
			if (_cameraFader == null)
			{
				_cameraFader = (Instantiate(CameraFade) as GameObject).GetComponent<CameraFader>();
			}
			if (_inputController != null)
			{
				_cameraFader.transform.parent = _inputController.Camera.gameObject.transform;
			}
			else
			{
				_cameraFader.transform.parent = Camera.main.transform;
			}
			_cameraFader.transform.localPosition = Vector3.zero;
		}

		public void ChangeGameState(StatesGame newGameState)
		{
#if ENABLE_NETWORKING
			if (_numberClients == 1)
			{
				ChangeLocalGameState(newGameState);
			}
			else
			{
				switch (newGameState)
				{
					case StatesGame.MainMenu:
					case StatesGame.Loading:
					case StatesGame.Connecting:
						ChangeLocalGameState(newGameState);
						break;

					default:
						ChangeRemoteGameState((int)newGameState);
						break;
				}
			}
#else
			ChangeLocalGameState(newGameState);
#endif			
		}

		private void ChangeLocalGameState(StatesGame newGameState)
		{
			if (_state == newGameState)
			{
				return;
			}
			if (_gameState != null)
			{
				_gameState.Destroy();
			}
			_gameState = null;
			_previousState = _state;
			_state = newGameState;
			switch (_state)
			{
				case StatesGame.MainMenu:
					_gameState = new GameStateMenu();
					break;

				case StatesGame.Connecting:
#if ENABLE_NETWORKING				
					_gameState = new GameStateConnecting();
#endif					
					break;

				case StatesGame.Loading:
					_gameState = new GameStateLoad();
					break;

				case StatesGame.Run:
					_gameState = new GameStateRun();
					break;

				case StatesGame.Pause:
					_gameState = new GameStatePause();
					break;

				case StatesGame.ReleaseMemory:
					_gameState = new GameStateReleaseMemory();
					break;		
			}
			if (_gameState != null)
			{
				_gameState.Initialize();
			}					
		}

		private void InitializeSystem(bool force)
		{
			if (((_state == StatesGame.None) && (_inputInited) && (_screenInited)) || force)
			{
				CreateCameraFader();
				ChangeGameState(StatesGame.MainMenu);	
			}
		}

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(InputController.EventInputControllerHasStarted))
			{
				_inputController = ((GameObject)parameters[0]).GetComponent<IInputController>();
				_inputController.Initialize();
				_inputInited = true;
				InitializeSystem(false);
			}
			if (nameEvent.Equals(ScreenController.EventScreenControllerStarted))
			{
				_screenInited = true;
				InitializeSystem(false);
			}			
			if (nameEvent.Equals(GameStateMenu.EventGameStateMenuQuitGame))
			{
				Application.Quit();
				Debug.LogError("QUIT APPLICATION");
			}
			if (nameEvent.Equals(EventMainControllerReleaseGameResources))
			{
				_levelView = null;
				_playerView = null;
				_requestCreation = false;
#if ENABLE_NETWORKING
				_hasStartedSession = false;
				_registeredPlayers.Clear();				
				_scorePlayers.Clear();
#endif
			}
			if (nameEvent.Equals(PlayerView.EventPlayerAppHasStarted))
			{
				PlayerView player = (PlayerView)parameters[0];
				if (_playerView == null)
				{					
#if ENABLE_NETWORKING
					if (MainController.Instance.NumberClients == 1)
					{
						_playerView = player;
					}
					else
					{
						if (player.NetworkGameIDView.AmOwner())
						{
							_playerView = player;
						}
					}
#else
					_playerView = player;
#endif
					if (_playerView != null)
					{
						_playerView.Initialize();

						// Game Level View
						if (_levelView == null)
						{
							_levelView = (Instantiate(GameLevel) as GameObject).GetComponent<LevelView>();
						}
						_levelView.transform.position = Vector3.zero;
						_levelView.ReportInited();
						SystemEventController.Instance.DispatchSystemEvent(EventMainControllerLocalPlayerViewAssigned);
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR) && ENABLE_NETWORKING
						NetworkController.Instance.CreateNetworkPrefab(false, PlayerViewHandLeftPrefab.name, PlayerViewHandLeftPrefab.gameObject, "GameElements\\Player\\" + PlayerViewHandLeftPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0);
						NetworkController.Instance.CreateNetworkPrefab(false, PlayerViewHandRightPrefab.name, PlayerViewHandRightPrefab.gameObject, "GameElements\\Player\\" + PlayerViewHandRightPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0);
#endif					
					}
				}
#if ENABLE_NETWORKING					
				if (MainController.Instance.NumberClients > 1)
				{
					if (!_registeredPlayers.ContainsKey(player))
					{
						_registeredPlayers.Add(player, 0);
					}
					if (NetworkController.Instance.IsServer)
					{
						if (_registeredPlayers.Count == _numberClients)
						{
							NetworkController.Instance.DelayNetworkEvent(EventMainControllerAllPlayerViewReadyToStartGame, 0.5f, -1, -1);
						}
					}
				}
#endif				
			}
#if ENABLE_NETWORKING								
			if (nameEvent.Equals(PlayerHandView.EventPlayerViewHandHasStarted))
			{
				PlayerHandView playerAppView = (PlayerHandView)parameters[0];
				if (playerAppView.NetworkGameIDView.AmOwner())
				{
					playerAppView.Player = _playerView;				
				}
			}
#endif							
			if (nameEvent.Equals(LevelView.EventLevelViewStarted))
			{
				if (_playerView == null)
				{
					Vector3 position = (Vector3)parameters[0];
					Quaternion orientation = (Quaternion)parameters[1];
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
#if ENABLE_OCULUS 
                    VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerResetToInitial, position, orientation);
#elif ENABLE_OPENXR
					VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerResetToInitial, position + new Vector3(0, -1, 0), orientation);
#else
					VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerResetToInitial, position, orientation);
#endif					
#else				
					if (_inputController != null)	
					{
						_inputController.Camera.transform.position = position;
						_inputController.Camera.transform.rotation = orientation;
					}
					else
					{
						Camera.main.transform.position = position;
						Camera.main.transform.rotation = orientation;
					}
#endif					
					SystemEventController.Instance.DelaySystemEvent(PlayerView.EventPlayerViewPositionUpdated, 0.2f);
				}				
			}
        }

#if ENABLE_NETWORKING
		private bool _changeStateRequested = false;
		private bool _hasStartedSession = false;
		private bool _isHost = false;
		private string _roomName = "RoomName";
		private PlayerView _localPlayer;
		private List<PlayerView> _players = new List<PlayerView>();

		protected virtual void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
		{
			if (nameEvent.Equals(NetworkController.EventNetworkControllerListRoomsConfirmedUpdated))
			{
				if (!_hasStartedSession)
				{
					_hasStartedSession = true;
#if ENABLE_MIRROR
					NetworkController.Instance.JoinRoom(_roomName);
#else
					if (NetworkController.Instance.RoomsLobby.Count == 0) 
					{
						NetworkController.Instance.CreateRoom(_roomName, _numberClients);
					}
					else 
					{
						NetworkController.Instance.JoinRoom(_roomName);
					}
#endif					
				}
			}
			if (nameEvent.Equals(NetworkController.EventNetworkControllerConfirmationConnectionWithRoom))
			{
				ChangeGameState(StatesGame.Loading);
				Utilities.DebugLogColor("JOINED ROOM WITH ID["+(int)parameters[0]+"] OF A TOTAL OF CONNECTIONS[" + NetworkController.Instance.Connections.Count + "]", Color.red);
				if (NetworkController.Instance.IsServer)
				{
					if (NetworkController.Instance.Connections.Count == _numberClients)
					{
						NetworkController.Instance.DelayNetworkEvent(MainController.EventMainControllerGameReadyToStart, 0.2f, -1, -1);
					}
				}
			}
			if (nameEvent.Equals(NetworkController.EventNetworkControllerNewPlayerJoinedRoom))
			{
				Utilities.DebugLogColor("NEW PLAYER["+(int)parameters[0]+"] JOINED TO THE ROOM", Color.red);
			}
			if (nameEvent.Equals(NetworkController.EventNetworkControllerPlayerDisconnected))
			{
				int netIDDisconnected = -1;
				if (parameters != null)
				{
					if (parameters.Length > 0)
					{
						netIDDisconnected = (int)parameters[0];
					}
				}
				for (int i = 0; i < _players.Count; i++)
				{
					PlayerView playerToDelete = _players[i];
					if (playerToDelete != null)
					{
						if (playerToDelete.NetworkGameIDView.GetOwnerID() == netIDDisconnected)
						{
							_players.RemoveAt(i);
							GameObject.Destroy(playerToDelete.gameObject);
							Utilities.DebugLogColor("PLAYER["+netIDDisconnected+"] SUCCESSFULLY DESTROYED", Color.red);
						}
					}
				}				
			}		
			if (nameEvent.Equals(NetworkController.EventNetworkControllerDisconnected))
			{
				DestroyNetworkLevelObjects();
			}
			if (nameEvent.Equals(EventMainControllerChangeState))
			{
				int newState = (int)parameters[0];
				_changeStateRequested = false;
				ChangeLocalGameState((StatesGame)newState);
			}
			if (nameEvent.Equals(EventMainControllerReportPlayerScore))
			{
				int playerID = (int)parameters[0];
				int scorePlayer = (int)parameters[1];
				if (!_scorePlayers.ContainsKey(playerID))
				{
					_scorePlayers.Add(playerID, scorePlayer);
					if (_scorePlayers.Count == _registeredPlayers.Count)
					{
						SystemEventController.Instance.DispatchSystemEvent(EventMainControllerAllPlayersScoresReported);
					}
				}
			}
		}

		private void DestroyNetworkLevelObjects()
		{
			NetworkObjectID[] networkObjects = GameObject.FindObjectsOfType<NetworkObjectID>();
			foreach (NetworkObjectID netObjectID in networkObjects)
			{
				if (netObjectID != null)
				{
					string nameToDestroy = netObjectID.name;
					netObjectID.Destroy();
				}
			}
		}

		private void ChangeRemoteGameState(int newState)
		{
			if (!_changeStateRequested)
			{
				_changeStateRequested = true;
				NetworkController.Instance.DispatchNetworkEvent(EventMainControllerChangeState, NetworkController.Instance.UniqueNetworkID, -1, newState);
			}
		}
#endif		

		void Update()
		{
			if (_gameState != null)
			{
				_gameState.Run();
			}
		}
	}
}