using DevLocker.StatesManagement.SampleTanks.Player;
using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks
{
	public abstract class TanksLevelManager : MonoBehaviour
	{
		public static TanksLevelManager LevelInstance { get; private set; }

		public TanksPlayer Player { get; protected set; }

		protected bool IsActingManager { get; private set; }

		public abstract void PopCurrentState();

		protected virtual void Awake()
		{
			if (LevelInstance != null) {
				Debug.LogError($"{name} found another acting level manager - {LevelInstance.name} ({GetType().Name} )!", this);
				gameObject.SetActive(false);
				IsActingManager = false;
				return;
			}

			IsActingManager = true;
			LevelInstance = this;
		}

		protected virtual void OnDestroy()
		{
			if (IsActingManager) {
				LevelInstance = null;
			}
		}
	}

}