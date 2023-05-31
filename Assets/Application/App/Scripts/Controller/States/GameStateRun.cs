using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using yourvrexperience.Utils;
using yourvrexperience.VR;
using static companyX.codingtest.GameLevelData;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	public class GameStateRun : IGameState
    {
		private GameLevelStates _gameLevelState = GameLevelStates.Initialization;
        private float _timerLevel = 0;
		private int _presentationCounter = 3;

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			_gameLevelState = GameLevelData.Instance.GameLevelState;
			_timerLevel = GameLevelData.Instance.TimerLevel;
			ApplyActionState();
			
			Assert.IsNotNull(MainController.Instance.PlayerView, "The player is null");
			Assert.IsNotNull(MainController.Instance.LevelView, "The level is null");
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			Cursor.lockState = CursorLockMode.None;
			GameLevelData.Instance.SaveGameLevelState(_gameLevelState, _timerLevel);
			if (SoundsController.Instance.CurrentAudioMelodyPlaying == GameSounds.MelodyInGameLevel)
			{
				SoundsController.Instance.PauseSoundBackground();
			}			
		}

		private void PlayerShoot()
		{
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
			Vector3 positionCurrentController = Vector3.zero;
			Vector3 forwardCurrentController = Vector3.zero;
			if (VRInputController.Instance.VRController.CurrentController != null)
			{
				positionCurrentController = VRInputController.Instance.VRController.CurrentController.transform.position;
				forwardCurrentController = VRInputController.Instance.VRController.CurrentController.transform.forward;
			}
#else
			Vector3 positionCurrentController = Camera.main.transform.position;
			Vector3 forwardCurrentController = Camera.main.transform.forward;
#endif
			
			positionCurrentController += Camera.main.transform.forward;
			BulletsController.Instance.ShootBullet(positionCurrentController, forwardCurrentController, GameLevelData.Instance.BulletSpeed);
			SoundsController.Instance.PlaySoundFX(SoundsController.ChannelsAudio.FX2, GameSounds.FxShoot, false, 1);
		}

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenHUDView.EventScreenHUDViewPause))
			{
				ChangeToPause();
			}
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(CubeView.EventCubeViewDestroyed))
			{
#if ENABLE_NETWORKING				
				if (MainController.Instance.NumberClients == 1)
				{
					GameLevelData.Instance.CurrentScore += 100;
				}
				else
				{
					int playerID = (int)parameters[3];				
					if (NetworkController.Instance.UniqueNetworkID == playerID)
					{
						GameLevelData.Instance.CurrentScore += 100;	
					}
				}
#else				
				GameLevelData.Instance.CurrentScore += 100;
#endif				
			}
			if (nameEvent.Equals(CubesController.EventCubesControllerAllDestroyed))
			{
				ChangeGameLevelState(GameLevelStates.GameOver);
			}
			if (nameEvent.Equals(ScreenHUDView.EventScreenHUDViewCreate))
			{
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
				Vector3 positionHUD = VRInputController.Instance.Camera.transform.position + VRInputController.Instance.Camera.transform.forward * 3;
				ScreenController.Instance.CreateWorldScreen(ScreenHUDView.ScreenNameNormal, positionHUD, 
																VRInputController.Instance.Camera.transform.forward, ScreenController.SizeVRScreen * 2, true, false);
#else
#if UNITY_ANDROID && !UNITY_EDITOR
				ScreenController.Instance.CreateScreen(ScreenHUDView.ScreenNameMobile, true, false);
#else
				ScreenController.Instance.CreateScreen(ScreenHUDView.ScreenNameNormal, true, false);
#endif
#endif			
			}
			if (nameEvent.Equals(MainController.EventMainControllerAllPlayersScoresReported))
			{
				Cursor.lockState = CursorLockMode.None;
				SoundsController.Instance.StopAllSounds();
				GameLevelData.Instance.RegisterNewHigscore(GameLevelData.Instance.CurrentScore, GameLevelData.Instance.CurrentTime);
				SoundsController.Instance.PlaySoundBackground(GameSounds.FxWin, false, 1);					
				ScreenController.Instance.CreateScreen(ScreenGameOverMultiplayerView.ScreenName, true, false);
				SystemEventController.Instance.DispatchSystemEvent(VRGunHandView.EventVRGunHandViewReleaseGun);
			}
        }

		private void ChangeToPause()
		{
#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients == 1)
			{
				SystemEventController.Instance.DispatchSystemEvent(BulletsController.EventBulletsControllerFreeze, true);
			}
			else
			{
				NetworkController.Instance.DelayNetworkEvent(BulletsController.EventBulletsControllerFreeze, 0.01f, -1, -1, true);
			}			
#else
			SystemEventController.Instance.DispatchSystemEvent(BulletsController.EventBulletsControllerFreeze, true);
#endif
			MainController.Instance.ChangeGameState(MainController.StatesGame.Pause);
		}

		private void ChangeGameLevelState(GameLevelStates newGameLevelState)
		{
			_gameLevelState = newGameLevelState;
			_timerLevel = 0;
			ApplyActionState();
		}

		private string GetWelcomeMessageCounter()
		{
			string messageWelcome = "";
			if (_presentationCounter > 0)
			{
				messageWelcome = LanguageController.Instance.GetText("screen.level.welcome.counter") + _presentationCounter;
			}
			else
			{
				messageWelcome = LanguageController.Instance.GetText("screen.level.welcome.go");
			}
			return messageWelcome;
		}

#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
		private bool IsCastingAgainstScreen()
		{
			Vector3	positionCurrentController = VRInputController.Instance.VRController.CurrentController.transform.position;
			Vector3	forwardCurrentController = VRInputController.Instance.VRController.CurrentController.transform.forward;
			RaycastHit ray = new RaycastHit();
			GameObject screenCasted = RaycastingTools.GetRaycastObject(positionCurrentController, forwardCurrentController, 100, ref ray, GameLevelData.Instance.LayerUI);
            return (screenCasted != null);
		}
#endif
		private void ApplyActionState()
		{
			switch (_gameLevelState)
			{
				case GameLevelStates.Initialization:
					CubesController.Instance.Initialize(MainController.Instance.PlayerView.gameObject.transform.position,
													MainController.Instance.LevelView.MinPosition,
													MainController.Instance.LevelView.MaxPosition);
					BulletsController.Instance.Initialize(GameLevelData.Instance.BulletPoolSize);
					FXsController.Instance.Initialize();
					break;

				case GameLevelStates.Presentation:
#if (!UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) || UNITY_EDITOR
					Cursor.lockState = CursorLockMode.Locked;
#endif					
					SoundsController.Instance.StopAllSounds();
					SoundsController.Instance.PlaySoundFX(GameSounds.FxCountdown, false, 1);
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)								
					ScreenController.Instance.CreateForwardScreen(ScreenLevelWelcomeView.ScreenName, new Vector3(0,0,1), true, false);
#else					
#if UNITY_ANDROID && !UNITY_EDITOR
					ScreenController.Instance.CreateForwardScreen(ScreenLevelWelcomeView.ScreenName, new Vector3(0,0,1), true, false);
#else
					Vector3 forwardWelcome = Camera.main.transform.forward;					
					Vector3 positionWelcome = Camera.main.transform.position + forwardWelcome * ScreenController.Instance.DistanceScreen;
					ScreenController.Instance.CreateWorldScreen(ScreenLevelWelcomeView.ScreenName, positionWelcome, forwardWelcome, ScreenController.SizeVRScreen, true, false);
#endif					
#endif					
					SystemEventController.Instance.DispatchSystemEvent(ScreenLevelWelcomeView.EventScreenLevelWelcomeViewUpdateText, GetWelcomeMessageCounter());
					break;

				case GameLevelStates.InGame:
#if (!UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) || UNITY_EDITOR
					Cursor.lockState = CursorLockMode.Locked;
#endif					
#if ENABLE_NETWORKING
					if (MainController.Instance.NumberClients == 1)
					{
						SystemEventController.Instance.DispatchSystemEvent(BulletsController.EventBulletsControllerFreeze, false);
					}
					else
					{
						NetworkController.Instance.DelayNetworkEvent(BulletsController.EventBulletsControllerFreeze, 0.1f, -1, -1, false);
					}				
#else
					SystemEventController.Instance.DispatchSystemEvent(BulletsController.EventBulletsControllerFreeze, false);
#endif					
					if (SoundsController.Instance.CurrentAudioMelodyPlaying != GameSounds.MelodyInGameLevel)
					{
						SoundsController.Instance.PlaySoundBackground(GameSounds.MelodyInGameLevel, true, 1);
					}
					else
					{
						SoundsController.Instance.ResumeSoundBackground();
					}
					
					ScreenController.Instance.DestroyScreens();
					MainController.Instance.PlayerView.ActivatePhysics(true);
					SystemEventController.Instance.DelaySystemEvent(ScreenHUDView.EventScreenHUDViewCreate, 0.2f);
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
					VRInputController.Instance.SpeedJoystickMovement = GameLevelData.Instance.PlayerVRSpeed;
					VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerEnableLocomotion, true);
#endif			
					break;

				case GameLevelStates.GameOver:
#if ENABLE_NETWORKING
					if (MainController.Instance.NumberClients == 1)
					{
						ShowScoreSinglePlayer();
					}
					else
					{
						NetworkController.Instance.DispatchNetworkEvent(MainController.EventMainControllerReportPlayerScore, -1, -1, NetworkController.Instance.UniqueNetworkID, GameLevelData.Instance.CurrentScore);
					}					
#else				
					ShowScoreSinglePlayer();	
#endif				
					break;
			}
		}

		private void ShowScoreSinglePlayer()
		{
			Cursor.lockState = CursorLockMode.None;
			SoundsController.Instance.StopAllSounds();
			GameLevelData.Instance.RegisterNewHigscore(GameLevelData.Instance.CurrentScore, GameLevelData.Instance.CurrentTime);
			SoundsController.Instance.PlaySoundBackground(GameSounds.FxWin, false, 1);					
			ScreenController.Instance.CreateScreen(ScreenGameOverView.ScreenName, true, false);
			SystemEventController.Instance.DispatchSystemEvent(VRGunHandView.EventVRGunHandViewReleaseGun);
		}
		
		public void Run()
		{
			switch (_gameLevelState)
			{
				case GameLevelStates.Initialization:
					_timerLevel += Time.deltaTime;
					if (_timerLevel > 0.2f)
					{
						ChangeGameLevelState(GameLevelStates.Presentation);
					}
					break;

				case GameLevelStates.Presentation:
#if !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
					MainController.Instance.PlayerView.RotateCamera();
#endif			
					_timerLevel += Time.deltaTime;
					if (_timerLevel > 1)
					{
						_timerLevel = 0;
						_presentationCounter--; 
						if (_presentationCounter >= 0)
						{
							SystemEventController.Instance.DispatchSystemEvent(ScreenLevelWelcomeView.EventScreenLevelWelcomeViewUpdateText, GetWelcomeMessageCounter());
						}
						else
						{
							ChangeGameLevelState(GameLevelStates.InGame);
						}
					}
					break;

				case GameLevelStates.InGame:
					_timerLevel += Time.deltaTime;
					if (_timerLevel > 1)
					{
						_timerLevel -= 1;
						GameLevelData.Instance.CurrentTime++;
					}

					MainController.Instance.PlayerView.Run();
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
					VRInputController.Instance.UpdateHandSideController();
#endif			
					if (MainController.Instance.GameInputController.ActionPrimaryDown())
					{
						bool shouldShoot = true;						
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
						shouldShoot = !IsCastingAgainstScreen();
#else
#if UNITY_ANDROID && !UNITY_EDITOR
						shouldShoot = !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#else
						shouldShoot = !EventSystem.current.IsPointerOverGameObject();
#endif
#endif
						if (shouldShoot) PlayerShoot();
					}			
					if (MainController.Instance.GameInputController.ActionSecondaryDown())
					{
						MainController.Instance.PlayerView.Jump();
					}
					if (MainController.Instance.GameInputController.ActionMenuPressed())
					{
						ChangeToPause();
					}
					break;

				case GameLevelStates.GameOver:
					_timerLevel += Time.deltaTime;
					if (_timerLevel > 6)
					{
						MainController.Instance.ChangeGameState(MainController.StatesGame.ReleaseMemory);
					}
					break;
			}
		}
	}
}