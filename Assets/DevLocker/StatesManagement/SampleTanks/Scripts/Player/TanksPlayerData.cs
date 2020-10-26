using System;

namespace DevLocker.StatesManagement.SampleTanks.Player
{
    [Serializable]
    public struct TanksPlayerData
    {
        public string Username;
        public int Money;
        public int Ammo;

        public override string ToString()
        {
            return $"{Username} (Money: {Money}; Ammo: {Ammo})";
        }
    }
}