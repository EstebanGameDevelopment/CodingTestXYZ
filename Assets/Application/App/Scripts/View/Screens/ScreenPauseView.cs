using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using yourvrexperience.VR;

namespace companyX.codingtest
{
	public class ScreenPauseView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenPauseView";

		public const string EventScreenPauseViewResumeGame = "EventScreenPauseViewResumeGame";
		public const string EventScreenPauseViewExitGame = "EventScreenPauseViewExitGame";		

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private Button buttonResume;
		[SerializeField] private Button buttonExit;
		[SerializeField] private GameObject contentHandLeft;
		[SerializeField] private GameObject contentHandRight;
		[SerializeField] private TextMeshProUGUI leftHandTitle;
		[SerializeField] private TextMeshProUGUI rightHandTitle;
		[SerializeField] private Button buttonLocomotionLeft;
		[SerializeField] private Button buttonLocomotionRight;
		[SerializeField] private TextMeshProUGUI leftHandInfo;
		[SerializeField] private TextMeshProUGUI rightHandInfo;

		private LocomotionMode _leftHand;
		private LocomotionMode _rightHand;

		public override string NameScreen 
		{ 
			get { return ScreenName; }
		}

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonResume.onClick.AddListener(OnButtonResume);
			buttonExit.onClick.AddListener(OnButtonExit);

			titleScreen.text = LanguageController.Instance.GetText("screen.pause.title");

			buttonResume.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.pause.resume.game");
			buttonExit.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.pause.exit.game");

#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR					
			_leftHand = VRInputController.Instance.LocomotionLeftHand;
			_rightHand = VRInputController.Instance.LocomotionRightHand;
			buttonLocomotionLeft.onClick.AddListener(OnLocomotionLeft);
			buttonLocomotionRight.onClick.AddListener(OnLocomotionRight);
			leftHandInfo.text = _leftHand.ToString();
			rightHandInfo.text = _rightHand.ToString();
			leftHandTitle.text = LanguageController.Instance.GetText("screen.pause.left.hand.locomotion");
			rightHandTitle.text = LanguageController.Instance.GetText("screen.pause.right.hand.locomotion");
			
			RefocusScreen refocusComponent = this.gameObject.GetComponent<RefocusScreen>();
			if (refocusComponent == null)
			{
				refocusComponent = this.gameObject.AddComponent<RefocusScreen>();
			}
			refocusComponent.Activate(VRInputController.Instance.Camera, ScreenController.Instance.DistanceScreen, 1, 0.4f);
#else
			contentHandLeft.gameObject.SetActive(false);
			contentHandRight.gameObject.SetActive(false);
#endif			
		}

        private void OnButtonResume()
        {
			UIEventController.Instance.DispatchUIEvent(EventScreenPauseViewResumeGame);            
        }

        private void OnButtonExit()
		{
			UIEventController.Instance.DispatchUIEvent(EventScreenPauseViewExitGame, this.gameObject);
		}

		private void OnLocomotionRight()
		{
			_rightHand++;
			if ((int)_rightHand > 3) _rightHand = 0;
			rightHandInfo.text = _rightHand.ToString();
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR					
			VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerChangeLocomotion, true, _rightHand);
#endif			
		}

		private void OnLocomotionLeft()
		{
			_leftHand++;
			if ((int)_leftHand > 3) _leftHand = 0;
			leftHandInfo.text = _leftHand.ToString();
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR					
			VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerChangeLocomotion, false, _leftHand);
#endif			
		}
	}
}