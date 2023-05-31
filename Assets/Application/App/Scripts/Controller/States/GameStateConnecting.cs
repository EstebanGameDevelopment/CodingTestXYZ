using UnityEngine;
using yourvrexperience.Utils;
using System;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
#if ENABLE_NETWORKING
	public class GameStateConnecting : IGameState
    {

		public const string EventGameStateConnectingCompleted = "EventGameStateConnectingCompleted";

		public void Initialize()
		{
			ScreenController.Instance.CreateForwardScreen(ScreenLoadingView.ScreenName, new Vector3(0,0,1), true, false, LanguageController.Instance.GetText("text.connecting"));

			NetworkController.Instance.Connect();
		}

        public void Destroy()
		{
		}

		public void Run()
		{
		}
	}
#endif	
}