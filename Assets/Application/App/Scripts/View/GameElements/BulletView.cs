using UnityEngine;

namespace companyX.codingtest
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]	
	public class BulletView : MonoBehaviour
	{
		private int _id;
		private Rigidbody _rigidBody;
		private int _counterCollision = 0;
		private Vector3 _velocity;
		private int _playerID;

		public int Id
		{
			get { return _id; }
		}
		public int PlayerID
		{
			get { return _playerID; }
		}

		public void Initialize()
		{
			_rigidBody = this.gameObject.GetComponent<Rigidbody>();
			Free();
		}

		public void Shoot(int id, int playerID, Vector3 position, Vector3 forward, float speed)
		{
			_id = id;
			_playerID = playerID;
			_counterCollision = 0;
			this.gameObject.SetActive(true);
			this.transform.position = position;
			_rigidBody.position = position;
			_rigidBody.velocity = Vector3.zero;
			_rigidBody.AddForce(forward * speed, ForceMode.Impulse);
		}

		public bool IsFree()
		{
			return !this.gameObject.activeSelf;
		}

		public void Free()
		{
			this.transform.position = Vector3.zero;
			_rigidBody.position = Vector3.zero;
			_rigidBody.velocity = Vector3.zero;
			this.gameObject.SetActive(false);
		}

		public void Freeze()
		{
			_velocity = _rigidBody.velocity;
			_rigidBody.Sleep();
		}

		public void Resume()
		{
			_rigidBody.WakeUp();
			_rigidBody.velocity = _velocity;
		}

		void OnCollisionEnter(Collision collision)
        {
			_counterCollision++;
			if (_counterCollision > 2)
			{
				Free();
			}
        }
    }
}
