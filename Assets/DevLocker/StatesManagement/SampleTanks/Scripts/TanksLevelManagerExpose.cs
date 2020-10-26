using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks
{
	public class TanksLevelManagerExpose : MonoBehaviour
	{
		public void PopCurrentState()
		{
			TanksLevelManager.LevelInstance?.PopCurrentState();
		}
	}
}