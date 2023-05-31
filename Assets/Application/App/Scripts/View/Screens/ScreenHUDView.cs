using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using yourvrexperience.VR;

namespace companyX.codingtest
{
	public class ScreenHUDView : BaseScreenView, IScreenView
	{
		public const string EventScreenHUDViewCreate = "EventScreenHUDViewCreate";
		public const string EventScreenHUDViewPause = "EventScreenHUDViewPause";

		public const string ScreenNameNormal = "ScreenHUDView";
		public const string ScreenNameMobile = "ScreenHUDMobileView";

		[SerializeField] private TextMeshProUGUI score;
		[SerializeField] private TextMeshProUGUI time;
		[SerializeField] private Button buttonPause;
		[SerializeField] private CustomButton buttonMove;

		private RefocusScreen _refocusComponent;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			SystemEventController.Instance.Event += OnSystemEvent;
			
			buttonPause.onClick.AddListener(OnButtonPause);
			score.text = LanguageController.Instance.GetText("screen.hud.score") + GameLevelData.Instance.CurrentScore;
			time.text = LanguageController.Instance.GetText("screen.hud.time") + Utilities.GetFormattedTimeMinutes(GameLevelData.Instance.CurrentTime);
			buttonPause.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.hud.pause");
			if (buttonMove != null) buttonMove.gameObject.SetActive(false);

#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR		
			_refocusComponent = this.gameObject.GetComponent<RefocusScreen>();
			if (_refocusComponent == null)
			{
				_refocusComponent = this.gameObject.AddComponent<RefocusScreen>();
			}
			_refocusComponent.Activate(VRInputController.Instance.Camera, 3, 1, 0.4f);
#else
#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) && !UNITY_EDITOR
			if (buttonMove != null)
			{
				buttonMove.gameObject.SetActive(true);
				buttonMove.PointerDownButton += OnMoveDown;
				buttonMove.PointerUpButton += OnMoveUp;
				buttonMove.PointerExitButton += OnMoveUp;
			}
#endif
			buttonPause.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.hud.pause") +"(P)";
#endif			
		}

        private void OnMoveUp(CustomButton button)
        {
            SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerViewMovePlayerForward, false);
        }

        private void OnMoveDown(CustomButton button)
        {
            SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerViewMovePlayerForward, true);
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(GameLevelData.EventGameLevelDataScoreUpdated))
			{
				score.text = LanguageController.Instance.GetText("screen.hud.score") + GameLevelData.Instance.CurrentScore;
			}
			if (nameEvent.Equals(GameLevelData.EventGameLevelDataTimeUpdated))
			{
				time.text = LanguageController.Instance.GetText("screen.hud.time") + Utilities.GetFormattedTimeMinutes(GameLevelData.Instance.CurrentTime);
			}
        }

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

        private void OnButtonPause()
        {
			UIEventController.Instance.DispatchUIEvent(EventScreenHUDViewPause);            
        }
	}
}