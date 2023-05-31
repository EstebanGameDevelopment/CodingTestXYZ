using UnityEngine;
using UnityEngine.Assertions;
using yourvrexperience.Utils;
using yourvrexperience.VR;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	public class GameStateMenu : IGameState
    {
		public const float BoxGunShiftFromCamera = -1;

		public const string EventGameStateMenuPositionReady = "EventGameStateMenuPositionReady";
		public const string EventGameStateMenuQuitGame = "EventGameStateMenuQuitGame";

		private GameObject _source;

		public void Initialize()
		{
			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;
						
			if (MainController.Instance.PreviousState == MainController.StatesGame.None)
			{				
				MainController.Instance.FadeOutCamera();
			}
			MainController.Instance.CreateMenuLevelView();		
			GameLevelData.Instance.ResetGameLevelData();	

			Assert.IsNull(MainController.Instance.PlayerView, "The player is not null");

			SystemEventController.Instance.DelaySystemEvent(LevelView.EventLevelViewSetGunBoxPosition, 0.1F, BoxGunShiftFromCamera);
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
			VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerEnableLocomotion, false);
#else
			ScreenController.Instance.CreateScreen(ScreenMainMenuView.ScreenName, true, false);			
#endif			
		}

		public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

        private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenMainMenuView.EventScreenMainMenuViewPlayGame))
			{		
#if ENABLE_NETWORKING
				bool isMultiplayer = (bool)parameters[1];
				if (isMultiplayer)
				{
					MainController.Instance.NumberClients = 2;
					MainController.Instance.ChangeGameState(MainController.StatesGame.Connecting);
				}
				else
				{
					MainController.Instance.NumberClients = 1;
					MainController.Instance.ChangeGameState(MainController.StatesGame.Loading);
				}
#else
				MainController.Instance.ChangeGameState(MainController.StatesGame.Loading);
#endif
			}
			if (nameEvent.Equals(ScreenMainMenuView.EventScreenMainMenuViewExitGame))
			{
				_source = (GameObject)parameters[0];
				string titleWarning = LanguageController.Instance.GetText("text.warning");
				string textAskToExit = LanguageController.Instance.GetText("screen.main.do.you.want.to.exit");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, _source, titleWarning, textAskToExit);
			}
			if (nameEvent.Equals(ScreenInformationView.EventScreenInformationResponse))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{
					SoundsController.Instance.PlaySoundFX(GameSounds.FxSelection, false, 1);
					ScreenController.Instance.DestroyScreens();
					string titleInfo = LanguageController.Instance.GetText("text.info");
					string textNowExiting = LanguageController.Instance.GetText("screen.main.now.exiting");
					ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenLoading, _source, titleInfo, textNowExiting);
					SystemEventController.Instance.DelaySystemEvent(EventGameStateMenuQuitGame, 2);
				}
			}			
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(CameraFader.EventCameraFaderFadeCompleted))
			{
				bool isFadeIn = (bool)parameters[0];
				if (!isFadeIn)
				{
					SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewSetGunBoxPosition, MainController.Instance.GameInputController.Camera.transform.position.y + BoxGunShiftFromCamera);
				}
			}			
            if (nameEvent.Equals(PlayerView.EventPlayerViewPositionUpdated))
			{
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)				
				ScreenController.Instance.CreateForwardScreen(ScreenMainMenuView.ScreenName, new Vector3(0, 0, 1), true, false);
#endif				
			}
        }
		
		public void Run()
		{
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)				
			if (MainController.Instance.GameInputController.ActionPrimaryDown() || MainController.Instance.GameInputController.ActionSecondaryDown())
			{
				Vector3	positionCurrentController = VRInputController.Instance.VRController.CurrentController.transform.position;
				Vector3	forwardCurrentController = VRInputController.Instance.VRController.CurrentController.transform.forward;
				RaycastHit ray = new RaycastHit();
				GameObject gunCasted = RaycastingTools.GetRaycastObject(positionCurrentController, forwardCurrentController, 100, ref ray, GameLevelData.Instance.LayerGun);
				if (gunCasted != null)
				{
					GameObject.Destroy(gunCasted);
					SystemEventController.Instance.DispatchSystemEvent(VRGunHandView.EventVRGunHandViewGrabGun);					
				}
			}			
#endif
		}
	}
}