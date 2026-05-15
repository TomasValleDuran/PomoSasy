public static class PomoSasyConstants 
{
    public class CoinValues
    {
        public const int Copper = 1;
        public const int Silver = 5;
        public const int Gold = 10;
        public const int Platinum = 25;
    }

    public class Config
    {
        public const float HighValueLootBias = 5f;

        public class Leveling
        {
            public const float XpPerLevelMultiplier = 4f;
            public const int MaxLevel = 20;
            public const int BaseXpForLevelUp = 30;
            public const int BasePlayerHealth = 100;
            public const float PlayerHealthMultiplierPerLevel = 1.2f;
        }
    }
    public enum CoinType { Copper, Silver, Gold, Platinum }
}