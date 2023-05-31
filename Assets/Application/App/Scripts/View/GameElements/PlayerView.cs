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
	public class PlayerView : MonoBehaviour, ICameraPlayer
#if ENABLE_NETWORKING
	, INetworkObject
#endif	
	{
		public const string EventPlayerAppHasStarted = "EventPlayerAppHasStarted";
		public const string EventPlayerAppEnableMovement = "EventPlayerAppEnableMovement";
		public const string EventPlayerViewPositionUpdated = "EventPlayerViewPositionUpdated";
		public const string EventPlayerViewMovePlayerForward = "EventPlayerViewMovePlayerForward";

		[SerializeField] private GameObject Body;
        		
		private float _rotationY = 0F;
		private Vector3 _forwardCamera = Vector3.zero;
		private bool _enableMovement = true;
		private Camera _camera;
		private Collider _collider;
		private Rigidbody _rigidBody;
		private bool _moveForwardActivated = false;
		private bool _isOnFloor = true;
		private int _layerFloor = -1;

#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) && !UNITY_EDITOR
    	private Gyroscope _gyro;
		private GameObject _cameraContainer;
		private Quaternion _rotationInitial;
#endif

#if ENABLE_NETWORKING
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

        public void SetInitData(string initializationData)
        {
        }

        public void OnInitDataEvent(string initializationData)
        {
        }		

		private void InitializeNetwork()
		{
			bool shouldRun = true;
			NetworkGameIDView.InitedEvent += OnInitDataEvent;
#if ENABLE_MIRROR			
			NetworkGameIDView.RefreshAuthority();
#endif			
			NetworkController.Instance.NetworkEvent += OnNetworkEvent;
			if (NetworkGameIDView.AmOwner())
			{
				Body.SetActive(false);
			}
			else
			{
				shouldRun = false;
			}
			if (shouldRun)
			{
				Body.SetActive(false);
			}
		}
		private void DestroyNetwork()
		{
			NetworkGameIDView.InitedEvent -= OnInitDataEvent;
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
		}

        private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
        {
            
        }
#endif

        public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		public Vector3 PositionCamera 
		{ 
			get { return _camera.transform.position; } 
			set { _camera.transform.position = value; } 
		}
		public Vector3 ForwardCamera 
		{
			get { return _camera.transform.forward; } 
			set { _camera.transform.forward = value; } 
		}
		public Vector3 PositionBase
		{ 
			get {  return this.transform.position + new Vector3(0, transform.localScale.y, 0); } 
		}

        public bool IsOwner()
        {
            return true;
        }

		void Awake()
		{
			_collider = this.GetComponent<Collider>();
			_rigidBody = this.GetComponent<Rigidbody>();

			_collider.isTrigger = true;
			_rigidBody.useGravity = false;
			_rigidBody.isKinematic = true;
		}

		void Start()
		{
			SystemEventController.Instance.DispatchSystemEvent(EventPlayerAppHasStarted, this);
		}

		public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;			
			SystemEventController.Instance.DispatchSystemEvent(CameraXRController.EventCameraPlayerReadyForCamera, this);

			_layerFloor = LayerMask.NameToLayer("Floor");

#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients > 1)
			{
				InitializeNetwork();
			}			
			else
			{
				Body.SetActive(false);
			}
#else			
			Body.SetActive(false);
#endif
		}

		void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;			

#if ENABLE_NETWORKING
			if (MainController.Instance.NumberClients > 1)
			{
				DestroyNetwork();
				SystemEventController.Instance.DispatchSystemEvent(PlayerHandView.EventPlayerViewHandDestroyedAvatar, this);
			}			
#endif			
		}

		public void ActivatePhysics(bool activation, bool force = false)
		{
			_collider.isTrigger = !activation;
			_rigidBody.useGravity = activation;
			_rigidBody.isKinematic = !activation;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventPlayerViewMovePlayerForward))
			{
				_moveForwardActivated = (bool)parameters[0];
			}
			if (nameEvent.Equals(CameraXRController.EventCameraResponseToPlayer))
			{
				_camera = (Camera)parameters[0];
			}
			if (nameEvent.Equals(EventPlayerAppEnableMovement))
			{
				_enableMovement = (bool)parameters[0];
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))	
			{
				DontDestroyOnLoad(this.gameObject);
			}
			if (nameEvent.Equals(MainController.EventMainControllerReleaseGameResources))
			{
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(LevelView.EventLevelViewStarted))
			{
				Vector3 position = (Vector3)parameters[0];
				Quaternion orientation = (Quaternion)parameters[1];
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
				VRInputController.Instance.DispatchVREvent(VRInputController.EventVRInputControllerResetToInitial, position, orientation);
#endif
				transform.position = position;
				transform.rotation = orientation;
				SystemEventController.Instance.DispatchSystemEvent(EventPlayerViewPositionUpdated);
			}
		}

		private void Move()
        {
			float axisVertical = Input.GetAxis("Vertical");
			float axisHorizontal = Input.GetAxis("Horizontal");
			if (_moveForwardActivated)
			{
				axisVertical = 1;
				axisHorizontal = 0;
			}
			float finalSpeed = GameLevelData.Instance.PlayersDesktopSpeed;
#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) && !UNITY_EDITOR
			finalSpeed = 10;
#endif
			Vector3 forward = axisVertical * Camera.main.transform.forward * finalSpeed * Time.deltaTime;
			Vector3 lateral = axisHorizontal * Camera.main.transform.right * finalSpeed * Time.deltaTime;
			Vector3 increment = forward + lateral;
			increment.y = 0;
			transform.GetComponent<Rigidbody>().MovePosition(transform.position + increment);
#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) && !UNITY_EDITOR
			if (_cameraContainer != null) _cameraContainer.transform.position = this.transform.position;
#else			
			Camera.main.transform.position = this.transform.position;
#endif
        }

        public void RotateCamera()
        {
#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)) && !UNITY_EDITOR
			if (_gyro == null)
			{
				_gyro = Input.gyro;
            	_gyro.enabled = true;

				_cameraContainer = new GameObject("Camera Container");
				Camera.main.transform.SetParent(_cameraContainer.transform);
            	_cameraContainer.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            	_rotationInitial = new Quaternion(0, 0, 1, 0);
			}
			if (_gyro != null)
			{
				Camera.main.transform.localRotation = _gyro.attitude * _rotationInitial;
				_forwardCamera = Camera.main.transform.forward;
				this.transform.forward = new Vector3(_forwardCamera.x, 0, _forwardCamera.z);
			}
#else			
			float rotationX = Camera.main.transform.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * GameLevelData.Instance.SensitivityCamera;
			_rotationY = _rotationY + Input.GetAxis("Mouse Y") * GameLevelData.Instance.SensitivityCamera;
			_rotationY = Mathf.Clamp(_rotationY, -60, 60);
			Quaternion rotation = Quaternion.Euler(-_rotationY, rotationX, 0);
			_forwardCamera = rotation * Vector3.forward;
			this.transform.forward = new Vector3(_forwardCamera.x, 0, _forwardCamera.z);
			Camera.main.transform.forward = _forwardCamera;
#endif			
        }

		public void Jump()
		{
			if (_isOnFloor)
			{
				_isOnFloor = false;
				transform.GetComponent<Rigidbody>().AddForce(Vector3.up * 20, ForceMode.Impulse);
			}
		}

 		void OnCollisionEnter(Collision collision)
        {
			if (!_isOnFloor)
			{
				if (collision.gameObject.layer == _layerFloor)
				{
					_isOnFloor = true;
				}
			}			
        }

		public void Run()
		{
#if !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
			if (_enableMovement)
			{
				bool runLogic = true;
#if ENABLE_NETWORKING
				if (MainController.Instance.NumberClients > 1)
				{
					runLogic = NetworkGameIDView.AmOwner();
				}				
#endif
				if (runLogic)
				{
					Move();
					RotateCamera();
				}
			}
#endif			
		}
    }
}
