using yourvrexperience.Utils;
using yourvrexperience.VR;
using UnityEngine;
using System;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif


namespace companyX.codingtest
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]	
	public class PlayerHandView : MonoBehaviour
#if ENABLE_NETWORKING	
	, INetworkObject
#endif	
	{
#if ENABLE_NETWORKING		
		public const string EventPlayerViewHandHasStarted = "EventPlayerViewHandHasStarted";
		public const string EventPlayerViewHandDestroyedAvatar = "EventPlayerViewHandDestroyedAvatar";

		[SerializeField] private GameObject Mesh;
		[SerializeField] private XR_HAND Hand;
		
		private Color _color;
		private PlayerView _player;

		public PlayerView Player
		{
			get { return _player; }
			set { _player = value; }
		}

		private NetworkObjectID _networkGameID;
		public NetworkObjectID NetworkGameIDView
		{
			get
			{
				if (_networkGameID == null)
				{
					if (this != null)
					{
						_networkGameID = GetComponent<NetworkObjectID>();
					}
				}
				return _networkGameID;
			}
		}

		public Color PlayerColor
		{
			get {return _color;}
			set { _color = value; 
				SetInitData(Utilities.PackColor(_color));
			}
		}
		public string NameNetworkPrefab 
		{
			get { return null; }
		}

		public string NameNetworkPath 
		{
			get { return null; }
		}
		public bool LinkedToCurrentLevel
		{
			get { return false; }
		}

		void Start()
		{
			SystemEventController.Instance.Event += OnSystemEvent;

			NetworkGameIDView.InitedEvent += OnInitDataEvent;
#if ENABLE_MIRROR			
			NetworkGameIDView.RefreshAuthority();
#endif			

			if (NetworkGameIDView.AmOwner())
			{
				SystemEventController.Instance.DispatchSystemEvent(EventPlayerViewHandHasStarted, this);

				Mesh.SetActive(false);
				
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR
				VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerLinkWithHand, this.gameObject, Hand);
#endif
			}
		}

		void OnDestroy()
		{
			NetworkGameIDView.InitedEvent -= OnInitDataEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			_player = null;
		}

		public void SetInitData(string initializationData)
		{
			NetworkGameIDView.InitialInstantiationData = initializationData;
		}

		public void OnInitDataEvent(string initializationData)
		{
			PlayerColor = Utilities.UnpackColor(initializationData);
			Utilities.ApplyColor(Mesh.transform, PlayerColor);
		}

		public void ActivatePhysics(bool activation, bool force = false)
		{
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventPlayerViewHandDestroyedAvatar))
			{
				PlayerView playerDestroyed = (PlayerView)parameters[0];
				if (_player == playerDestroyed)
				{
					GameObject.Destroy(this.gameObject);
				}
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))	
			{
				DontDestroyOnLoad(this.gameObject);
			}			
		}
#endif	
	}
}
