using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
using yourvrexperience.VR;
using System.Collections.Generic;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif


namespace companyX.codingtest
{
	public class ScreenGameOverMultiplayerView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenGameOverMultiplayerView";

		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI description;
		[SerializeField] private PlayerScoreInfo[] playersScore;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			title.text = LanguageController.Instance.GetText("screen.level.game.over.title");
			description.text = LanguageController.Instance.GetText("screen.level.game.over.description");

#if ENABLE_NETWORKING
			int counter = 0;
			foreach (KeyValuePair<int,int> item in MainController.Instance.ScorePlayers) 
			{
				playersScore[counter].DisplayScore(item.Key, item.Value);
				counter++;
			}
#endif

#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR		
			RefocusScreen refocusComponent = this.gameObject.GetComponent<RefocusScreen>();
			if (refocusComponent == null)
			{
				refocusComponent = this.gameObject.AddComponent<RefocusScreen>();
			}
			refocusComponent.Activate(VRInputController.Instance.Camera, ScreenController.Instance.DistanceScreen, 1, 0.4f);
#endif			
		}
	}
}