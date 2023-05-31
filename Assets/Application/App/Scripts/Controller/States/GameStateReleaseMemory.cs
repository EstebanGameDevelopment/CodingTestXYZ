using yourvrexperience.Utils;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{	
	public class GameStateReleaseMemory : IGameState
    {
		public const string EventGameStateReleaseMemoryStageCompleted = "EventGameStateReleaseMemoryStageCompleted";

		private ScreenReleaseResourcesView _screen;

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			SystemEventController.Instance.DispatchSystemEvent(VRGunHandView.EventVRGunHandViewReleaseGun);
			ScreenController.Instance.CreateScreen(ScreenReleaseResourcesView.ScreenName, true, false);
#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients > 1)
			{
				NetworkController.Instance.Disconnect();
			}			
#endif
		}

        public void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			_screen = null;
		}
		
        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
			if (nameEvent.Equals(ScreenReleaseResourcesView.EventScreenReleaseResourcesViewStarted))
			{
				ScreenReleaseResourcesView target = (ScreenReleaseResourcesView)parameters[0];
				if (_screen == null)
				{
					_screen = target;
					_screen.UpdateText(LanguageController.Instance.GetText("screen.release.resources.phase.1"));
					MainController.Instance.FadeInCamera();
				}
				else
				{
					target.UpdateText(LanguageController.Instance.GetText("screen.release.resources.phase.2"));					
				}
			}
			if (nameEvent.Equals(CameraFader.EventCameraFaderFadeCompleted))
			{
				bool isFadeIn = (bool)parameters[0];
				if (isFadeIn)
				{
					SystemEventController.Instance.DispatchSystemEvent(MainController.EventMainControllerReleaseGameResources);
					MainController.Instance.CreateMenuLevelView();
					MainController.Instance.FadeOutCamera();
				}
				else
				{
					ScreenController.Instance.DestroyScreens();
					MainController.Instance.ChangeGameState(MainController.StatesGame.MainMenu);
				}
			}
            if (nameEvent.Equals(PlayerView.EventPlayerViewPositionUpdated))
			{
				if (_screen != null)
				{
					ScreenController.Instance.DestroyScreens();
					ScreenController.Instance.CreateScreen(ScreenReleaseResourcesView.ScreenName, true, false);
				}				
			}
        }
		
		public void Run()
		{
		}
	}
}