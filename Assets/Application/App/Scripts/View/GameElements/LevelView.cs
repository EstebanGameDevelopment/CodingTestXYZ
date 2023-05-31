using yourvrexperience.Utils;
using UnityEngine;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	public class LevelView : MonoBehaviour
	{
		public const string EventLevelViewStarted = "EventLevelViewStarted";
		public const string EventLevelViewSetGunBoxPosition = "EventLevelViewSetGunBoxPosition";

		[SerializeField] private GameObject initialPosition;
		[SerializeField] private BoxCollider gameArea;
		[SerializeField] private GameObject floor;
		[SerializeField] private GameObject gunsBox;

		private Vector3 _minPosition = Vector3.zero;
		private Vector3 _maxPosition = Vector3.zero;

		public Vector3 MinPosition
		{
			get { 
				if (_minPosition == Vector3.zero)
				{
					_minPosition = gameArea.bounds.center - gameArea.bounds.size / 2;
				}
				return _minPosition;
			}
		}
		public Vector3 MaxPosition
		{
			get { 
				if (_maxPosition == Vector3.zero)
				{
					_maxPosition = gameArea.bounds.center + gameArea.bounds.size / 2;
				}
				return _maxPosition;
			}
		}

		void Awake()
		{
			if (gunsBox != null)
			{
				gunsBox.SetActive(false);
			}
		}
        		
		void Start()
		{
			if (gameArea != null)
			{
				Utilities.ReverseNormals(gameArea.gameObject);
				if (floor != null)
				{
					Vector3 sizeGameArea = MaxPosition - MinPosition;
					Vector3 centerGameArea = (MaxPosition + MinPosition) / 2;
					floor.transform.localScale = sizeGameArea / 10;
					floor.transform.position = new Vector3(centerGameArea.x, floor.transform.position.y, centerGameArea.z);
				}
			}			
			SystemEventController.Instance.Event += OnSystemEvent;
		}

		void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		public void ReportInited()
		{
#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients == 1)
			{
				SystemEventController.Instance.DispatchSystemEvent(EventLevelViewStarted, initialPosition.transform.position, initialPosition.transform.rotation);
			}
			else
			{
				Vector3 startingPosition = initialPosition.transform.position;
				if (NetworkController.Instance.UniqueNetworkID != -1) startingPosition += ((NetworkController.Instance.UniqueNetworkID%2==0)?1:-1) * new Vector3(2, 0, 0) * NetworkController.Instance.UniqueNetworkID;
				SystemEventController.Instance.DelaySystemEvent(EventLevelViewStarted, 0.25f, startingPosition, initialPosition.transform.rotation);
			}
#else
			SystemEventController.Instance.DispatchSystemEvent(EventLevelViewStarted, initialPosition.transform.position, initialPosition.transform.rotation);
#endif
		}

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(MainController.EventMainControllerReleaseGameResources))
			{
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(EventLevelViewSetGunBoxPosition))
			{
				float heightBox = (float)parameters[0];
				if (gunsBox != null)
				{
					gunsBox.SetActive(true);
					float finalHeightBox = MainController.Instance.GameInputController.Camera.transform.position.y + heightBox;
					gunsBox.transform.position = new Vector3(gunsBox.transform.position.x, heightBox, gunsBox.transform.position.z);
				}
			}
        }
    }
}
