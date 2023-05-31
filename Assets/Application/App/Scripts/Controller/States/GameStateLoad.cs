using UnityEngine;
using yourvrexperience.Utils;
using System;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	public class GameStateLoad : IGameState
    {
		public const string EventGameStateLoadCompleted = "EventGameStateLoadCompleted";

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			ScreenController.Instance.CreateForwardScreen(ScreenLoadingView.ScreenName, new Vector3(0,0,1), true, false, LanguageController.Instance.GetText("text.loading"));
			SoundsController.Instance.FadeOut(3);

			MainController.Instance.FadeInCamera();

#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients > 1)
			{
				SystemEventController.Instance.DelaySystemEvent(ScreenLoadingView.EventScreenLoadingViewUpdateText, 1, LanguageController.Instance.GetText("screen.loading.wait.for.players"));

				NetworkController.Instance.NetworkEvent += OnNetworkEvent;
			}
#endif			
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;

#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients > 1)
			{
				if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
			}
#endif			
		}

#if ENABLE_NETWORKING
        private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
        {
			if (nameEvent.Equals(MainController.EventMainControllerGameReadyToStart))
			{
				SystemEventController.Instance.DispatchSystemEvent(MainController.EventMainControllerReleaseGameResources);
				MainController.Instance.CreateGameElementsView();
				MainController.Instance.FadeOutCamera();
			}
            if (nameEvent.Equals(MainController.EventMainControllerAllPlayerViewReadyToStartGame))
			{
				ScreenController.Instance.DestroyScreens();
				MainController.Instance.ChangeGameState(MainController.StatesGame.Run);
			}
        }
#endif			
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(CameraFader.EventCameraFaderFadeCompleted))
			{
				if (MainController.Instance.NumberClients == 1)
				{
					bool isFadeIn = (bool)parameters[0];
					if (isFadeIn)
					{
						SystemEventController.Instance.DispatchSystemEvent(MainController.EventMainControllerReleaseGameResources);
						MainController.Instance.CreateGameElementsView();
						MainController.Instance.FadeOutCamera();
					}
				}
			}
			if (nameEvent.Equals(PlayerView.EventPlayerViewPositionUpdated))
			{
				ScreenController.Instance.DestroyScreens();
				ScreenController.Instance.CreateScreen(ScreenLoadingView.ScreenName, true, false);
			}

			if (nameEvent.Equals(MainController.EventMainControllerLocalPlayerViewAssigned))
			{
				if (MainController.Instance.NumberClients == 1)
				{
					ScreenController.Instance.DestroyScreens();
					MainController.Instance.ChangeGameState(MainController.StatesGame.Run);
				}
			}
        }
		
		public void Run()
		{
		}
	}
}