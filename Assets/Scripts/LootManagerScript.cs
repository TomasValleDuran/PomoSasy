using System.Collections.Generic;
using UnityEngine;

public class LootManagerScript : MonoBehaviour
{
    public static LootManagerScript Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject copperPrefab;
    [SerializeField] private GameObject silverPrefab;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject platinumPrefab;

    private Dictionary<PomoSasyConstants.CoinType, GameObject> _prefabMap;
    private Dictionary<PomoSasyConstants.CoinType, int> _coinMap;

    private Utils.LootGenerator _generator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _prefabMap = new Dictionary<PomoSasyConstants.CoinType, GameObject>
        {
            { PomoSasyConstants.CoinType.Copper, copperPrefab },
            { PomoSasyConstants.CoinType.Silver, silverPrefab },
            { PomoSasyConstants.CoinType.Gold, goldPrefab },
            { PomoSasyConstants.CoinType.Platinum, platinumPrefab }
        };

        _coinMap = new Dictionary<PomoSasyConstants.CoinType, int>
        {
            { PomoSasyConstants.CoinType.Copper, PomoSasyConstants.CoinValues.Copper },
            { PomoSasyConstants.CoinType.Silver, PomoSasyConstants.CoinValues.Silver },
            { PomoSasyConstants.CoinType.Gold, PomoSasyConstants.CoinValues.Gold },
            { PomoSasyConstants.CoinType.Platinum, PomoSasyConstants.CoinValues.Platinum }
        };

        _generator = new Utils.LootGenerator(_coinMap);
    }

    public void DropCoins(int points, Vector3 position)
    {
        var coins = _generator.Generate(points);

        foreach (var coin in coins)
        {
            SpawnCoin(coin, position);
        }
    }

    private void SpawnCoin(PomoSasyConstants.CoinType type, Vector3 position)
    {
        var prefab = _prefabMap[type];

        Instantiate(prefab, position, Quaternion.identity);
    }
}