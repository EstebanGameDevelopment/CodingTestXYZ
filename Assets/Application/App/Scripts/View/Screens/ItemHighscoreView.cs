using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace companyX.codingtest
{
    public class ItemHighscoreView : MonoBehaviour, ISlotView
    {
        public const string EventItemHighscoreViewSelected = "EventItemHighscoreViewSelected";

        private GameObject _parent;
        private int _index;
        private ItemMultiObjectEntry _data;
        private Image _background;
        private bool _selected = false;

        public int Index
        {
            get { return _index; }
        }
        public ItemMultiObjectEntry Data
        {
            get { return _data; }
        }
        public virtual bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (_selected)
                {
                    _background.color = Color.magenta;
                }
                else
                {
                    _background.color = Color.white;
                }
            }
        }

        public void Initialize(params object[] parameters)
        {
            _parent = (GameObject)((ItemMultiObjectEntry)parameters[0]).Objects[0];
            _index = (int)((ItemMultiObjectEntry)parameters[0]).Objects[1];
            _data = (ItemMultiObjectEntry)((ItemMultiObjectEntry)parameters[0]).Objects[2];

            transform.Find("Position").GetComponent<TextMeshProUGUI>().text = ((int)_data.Objects[0]).ToString();
			transform.Find("ScoreTitle").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.hud.score");
			transform.Find("TimeTitle").GetComponent<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.hud.time");
			transform.Find("ScoreValue").GetComponent<TextMeshProUGUI>().text = (((HighscoreData)_data.Objects[1]).Score).ToString();
			transform.Find("TimeValue").GetComponent<TextMeshProUGUI>().text = Utilities.GetFormattedTimeMinutes((((HighscoreData)_data.Objects[1]).Time));

            _background = transform.GetComponent<Image>();
            transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

            UIEventController.Instance.Event += OnUIEvent;
        }

        void OnDestroy()
        {
            Destroy();
        }

        public bool Destroy()
        {
            if (_parent != null)
            {
                _parent = null;
                if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ButtonPressed()
        {
            ItemSelected();
        }

        public void ItemSelected(bool dispatchEvent = true)
        {
            Selected = !Selected;
			UIEventController.Instance.DispatchUIEvent(EventItemHighscoreViewSelected, _parent, this.gameObject, (Selected ? _index : -1), _data, dispatchEvent);
        }

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(EventItemHighscoreViewSelected))
            {
                if ((GameObject)parameters[0] == _parent)
                {
                    if ((GameObject)parameters[1] != this.gameObject)
                    {
                        Selected = false;
                    }
                }
            }
		}
	}

}