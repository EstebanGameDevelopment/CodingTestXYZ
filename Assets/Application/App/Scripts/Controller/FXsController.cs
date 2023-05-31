using yourvrexperience.Utils;
using UnityEngine;

namespace companyX.codingtest
{
	public class FXsController : MonoBehaviour
	{
        private static FXsController _instance;

        public static FXsController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(FXsController)) as FXsController;
                }
                return _instance;
            }
        }

		[SerializeField] private GameObject CubeExplosionFX;

		public void Initialize()
		{	
			SystemEventController.Instance.Event += OnSystemEvent;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
 			if (nameEvent.Equals(MainController.EventMainControllerReleaseGameResources))
			{
				if (_instance != null)
				{
					_instance = null;
					GameObject.Destroy(this.gameObject);
				}
			}			
			if (nameEvent.Equals(CubeView.EventCubeViewDestroyed))
			{
				Vector3 positionFX = (Vector3)parameters[2];
				GameObject cubeExplosionFX = Instantiate(CubeExplosionFX);
				cubeExplosionFX.transform.position = positionFX;
				cubeExplosionFX.transform.parent = this.transform;
				GameObject.Destroy(cubeExplosionFX, 4);
			}
        }
	}
}