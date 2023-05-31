using yourvrexperience.Utils;
using System;
using UnityEngine;
#if ENABLE_NETWORKING
using yourvrexperience.Networking;
#endif

namespace companyX.codingtest
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]	
	public class CubeView : MonoBehaviour, IEquatable<CubeView>
	{
		public const string EventCubeViewDestroyed = "EventCubeViewDestroyed";
		public const string EventCubeViewNetworkDestruction = "EventCubeViewNetworkDestruction";

		private int _id;
		private Vector3 _position;
		private float _rotationY;
		private float _size;
		private Color _color;
		private bool _destructionRequest = false;

		public int Id
		{
			get { return _id; }
		}

        public bool Equals(CubeView other)
        {
            return _id == other.Id;
        }

		public void Initialize(int id, Vector3 position, float rotationY, float size, Color color)
		{
			_id = id;
			_position = position;
			_rotationY = rotationY;
			_size = size;
			_color = color;
		}

		public void Animate(float delayTime, float timeAnimation)
		{
			this.transform.position = _position;
			this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

			iTween.ScaleTo(gameObject, iTween.Hash("x", _size, "y", _size, "z", _size, "time", timeAnimation, "delay", delayTime));
			iTween.RotateTo(gameObject, iTween.Hash("y", _rotationY, "time", timeAnimation, "delay", delayTime));
			iTween.ColorTo(gameObject, _color, timeAnimation);
		}

		public void DestroyCube(int bulletID, Vector3 posCube, int playerID)
		{
			if (!_destructionRequest)
			{
				_destructionRequest = true;
				SystemEventController.Instance.DispatchSystemEvent(EventCubeViewDestroyed, _id, bulletID, posCube, playerID);
				SoundsController.Instance.PlaySoundFX(GameSounds.FxCubeExplosion, false, 1);
				GameObject.Destroy(this.gameObject);
			}
		}

        void OnTriggerEnter(Collider collision)
        {
			BulletView bullet = collision.gameObject.GetComponent<BulletView>();
			if (bullet != null)
			{
#if ENABLE_NETWORKING
				if (MainController.Instance.NumberClients == 1)
				{
					DestroyCube(bullet.Id, this.transform.position, -1);
				}
				else
				{
					NetworkController.Instance.DispatchNetworkEvent(EventCubeViewNetworkDestruction, -1, -1, _id, bullet.Id, this.transform.position, bullet.PlayerID);
				}				
#else
				DestroyCube(bullet.Id, this.transform.position, -1);
#endif				
			}			
        }
    }
}
