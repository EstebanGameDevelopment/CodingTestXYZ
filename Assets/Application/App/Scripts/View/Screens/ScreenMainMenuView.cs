using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace companyX.codingtest
{
	public class ScreenMainMenuView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenMainMenuView";

		public const string EventScreenMainMenuViewPlayGame = "EventScreenMainMenuViewPlayGame";
		public const string EventScreenMainMenuViewExitGame = "EventScreenMainMenuViewExitGame";		

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private Button buttonPlaySinglePlayer;
		[SerializeField] private Button buttonPlayMultiPlayer;
		[SerializeField] private Button buttonExit;
		[SerializeField] private TextMeshProUGUI highscoresTitle;
		[SerializeField] private SlotManagerView listHighscores;
		[SerializeField] private GameObject HighscoreItemPrefab;
		[SerializeField] private GameObject VRInstructionsContainer;
		[SerializeField] private GameObject ArrowImage;
		[SerializeField] private TextMeshProUGUI instructionsVR;

		private int _gunsGrabbed = 0;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonPlaySinglePlayer.onClick.AddListener(OnButtonPlaySinglePlayer);
			buttonPlayMultiPlayer.onClick.AddListener(OnButtonPlayMultiPlayer);
			buttonExit.onClick.AddListener(OnButtonExit);

			titleScreen.text = LanguageController.Instance.GetText("screen.main.menu.title");
#if ENABLE_NETWORKING
			buttonPlaySinglePlayer.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.main.menu.play.singleplayer.game");
			buttonPlayMultiPlayer.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.main.menu.play.multiplayer.game");
#else
			buttonPlaySinglePlayer.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.main.menu.play.game");
			buttonPlayMultiPlayer.gameObject.SetActive(false);
#endif
			buttonExit.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.main.menu.exit.game");

			highscoresTitle.text = LanguageController.Instance.GetText("screen.main.menu.highscores.title");

			if (GameLevelData.Instance.Highscores.Count > 0)
			{
				List<ItemMultiObjectEntry> highscoreItems = new List<ItemMultiObjectEntry>();
				for (int i = 0; i < GameLevelData.Instance.Highscores.Count; i++)
				{
					ItemMultiObjectEntry data = new ItemMultiObjectEntry(i + 1, GameLevelData.Instance.Highscores[i]);
					highscoreItems.Add(new ItemMultiObjectEntry(this.gameObject, i, data));
				}
				listHighscores.Initialize(GameLevelData.Instance.Highscores.Count, highscoreItems, HighscoreItemPrefab);
			}

#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
			instructionsVR.text = LanguageController.Instance.GetText("screen.main.menu.instructions.vr");
			buttonPlaySinglePlayer.interactable = false;
			buttonPlayMultiPlayer.interactable = false;
			buttonExit.interactable = false;
#else		
			VRInstructionsContainer.SetActive(false);	
#endif
			SoundsController.Instance.PlaySoundBackground(GameSounds.MelodyMainMenu, true, 1);

			SystemEventController.Instance.Event += OnSystemEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

        private void OnButtonExit()
        {
			SoundsController.Instance.PlaySoundFX(GameSounds.FxSelection, false, 1);				
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewExitGame, this.gameObject);            
        }

        private void OnButtonPlaySinglePlayer()
		{
			SoundsController.Instance.PlaySoundFX(GameSounds.FxSelection, false, 1);				
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewPlayGame, this.gameObject, false);
		}

        private void OnButtonPlayMultiPlayer()
		{
			SoundsController.Instance.PlaySoundFX(GameSounds.FxSelection, false, 1);				
			UIEventController.Instance.DispatchUIEvent(EventScreenMainMenuViewPlayGame, this.gameObject, true);
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(VRGunHandView.EventVRGunHandViewGrabGun))
			{
				_gunsGrabbed++;
				if (_gunsGrabbed == 2)
				{
					buttonPlaySinglePlayer.interactable = true;
					buttonPlayMultiPlayer.interactable = true;
					buttonExit.interactable = true;
					ArrowImage.SetActive(false);
					instructionsVR.text = LanguageController.Instance.GetText("screen.main.menu.instructions.play.now.vr");
				}
			}
        }
	}
}