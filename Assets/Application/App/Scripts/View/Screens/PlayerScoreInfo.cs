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
	public class PlayerScoreInfo : MonoBehaviour
	{
		public const float TotalTimeAnimationScore = 3;

		[SerializeField] private Image background;
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI scoreTitle;
		[SerializeField] private TextMeshProUGUI scoreValue;
		[SerializeField] private TextMeshProUGUI timeTitle;
		[SerializeField] private TextMeshProUGUI timeValue;

		private float _timer = -1;
		private int _score = -1;

		public void DisplayScore(int playerID, int score)
		{
#if ENABLE_NETWORKING
			background.gameObject.SetActive(playerID == NetworkController.Instance.UniqueNetworkID);
#endif
			
			title.text = LanguageController.Instance.GetText("screen.hud.score.player") + " " + playerID;
			scoreTitle.text = LanguageController.Instance.GetText("screen.hud.score");
			timeTitle.text = LanguageController.Instance.GetText("screen.hud.time");

			scoreValue.text = "0";
			_score = score;
			timeValue.text = Utilities.GetFormattedTimeMinutes(GameLevelData.Instance.CurrentTime);
			_timer = 0;
		}

		void Update()
		{
			if (_timer != -1)
			{
				_timer += Time.deltaTime;
				if (_timer < TotalTimeAnimationScore)
				{
					float progressScore = ((float)_score * (_timer / TotalTimeAnimationScore));
					scoreValue.text = ((int)progressScore).ToString();
				}
				else
				{
					scoreValue.text = _score.ToString();
					_timer = -1;
				}
			}
		}
	}
}