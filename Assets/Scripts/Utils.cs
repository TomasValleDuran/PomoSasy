using System;
using System.Collections.Generic;
using System.Linq;

public class Utils
{
    public class LootGenerator
    {
        private readonly Dictionary<PomoSasyConstants.CoinType, int> _coinMap;
        private readonly Random _rng;
    
        private readonly float _minChunkRatio;
        private readonly float _maxChunkRatio;
        private readonly int _valueJitter;
    
        public LootGenerator(
            Dictionary<PomoSasyConstants.CoinType, int> coinMap,
            int? seed = null,
            float minChunkRatio = 0.1f,
            float maxChunkRatio = 0.5f,
            int valueJitter = 3)
        {
            _coinMap = coinMap;
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    
            _minChunkRatio = minChunkRatio;
            _maxChunkRatio = maxChunkRatio;
            _valueJitter = valueJitter;
        }
    
        public List<PomoSasyConstants.CoinType> Generate(int totalPoints)
        {
            var result = new List<PomoSasyConstants.CoinType>();

            if (totalPoints <= 0)
                return result;

            var chunks = SplitIntoChunks(totalPoints);

            foreach (var chunk in chunks)
            {
                GenerateChunk(chunk, totalPoints, result);
            }

            Shuffle(result);
            return result;
        }
        
        private PomoSasyConstants.CoinType WeightedPick(
            List<KeyValuePair<PomoSasyConstants.CoinType, int>> coins,
            Func<int, float> weightFunc)
        {
            var totalWeight = 0f;

            var weights = new List<(PomoSasyConstants.CoinType coin, float weight)>();

            foreach (var kv in coins)
            {
                var w = weightFunc(kv.Value);
                if (w <= 0) continue;

                weights.Add((kv.Key, w));
                totalWeight += w;
            }

            var roll = (float)_rng.NextDouble() * totalWeight;

            foreach (var entry in weights)
            {
                if (roll < entry.weight)
                    return entry.coin;

                roll -= entry.weight;
            }

            return weights.Last().coin; // fallback
        }
    
        private List<int> SplitIntoChunks(int total)
        {
            var chunks = new List<int>();
            var remaining = total;
    
            while (remaining > 0)
            {
                var minChunk = Math.Max(1, (int)(remaining * _minChunkRatio));
                var maxChunk = Math.Max(1, (int)(remaining * _maxChunkRatio));
    
                if (maxChunk < minChunk)
                    maxChunk = minChunk;
    
                var chunk = _rng.Next(minChunk, maxChunk + 1);
                chunk = Math.Min(chunk, remaining);
    
                chunks.Add(chunk);
                remaining -= chunk;
            }
    
            return chunks;
        }
    
        private void GenerateChunk(int chunkValue, int totalPoints, List<PomoSasyConstants.CoinType> output)
        {
            var remaining = chunkValue;

            var t = Math.Clamp((float)remaining / totalPoints, 0f, 1f);
            
            var exponent = 1f + t * PomoSasyConstants.Config.HighValueLootBias;

            while (remaining > 0)
            {
                var validCoins = _coinMap
                    .Where(kv => kv.Value <= remaining)
                    .ToList();

                var chosen = WeightedPick(validCoins, value =>
                {
                    var baseWeight = (float)Math.Pow(value, exponent);

                    var jitter = 1f + (float)(_rng.NextDouble() * 0.2 - 0.1);

                    return baseWeight * jitter;
                });

                output.Add(chosen);
                remaining -= _coinMap[chosen];
            }
        }
    
        private void Shuffle(List<PomoSasyConstants.CoinType> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
    
    public static int GetCoinValueFromType(PomoSasyConstants.CoinType type)
    {
        return type switch
        {
            PomoSasyConstants.CoinType.Copper => PomoSasyConstants.CoinValues.Copper,
            PomoSasyConstants.CoinType.Silver => PomoSasyConstants.CoinValues.Silver,
            PomoSasyConstants.CoinType.Gold => PomoSasyConstants.CoinValues.Gold,
            PomoSasyConstants.CoinType.Platinum => PomoSasyConstants.CoinValues.Platinum,
            _ => 0
        };
    }
}
