using System;
using System.Text;
public class DiceScorer
{
    private Dictionary<String, bool> smallStraightCache = new Dictionary<String, bool>();
    private Dictionary<String, bool> largeStraightCache = new Dictionary<String, bool>();
    private Dictionary<String, bool> allDifferentCache = new Dictionary<String, bool>();
    private Dictionary<String, int> fullHouseCache = new Dictionary<String, int>();
    private Dictionary<String, int> schoonerCache = new Dictionary<String, int>();
    private Dictionary<String, int> fourOfaKindCache = new Dictionary<String, int>();
    private Dictionary<String, int> threeOfaKindCache = new Dictionary<String, int>();

    public List<Category> TopCategories(List<int> diceRoll, out int maxScore)
    {
        var scores = new Dictionary<Category, int>();
        maxScore = 0;
        var maxCategory = new List<Category>();
        foreach (var category in Enum.GetValues(typeof(Category)))
        {
            int score = Score((Category)category, diceRoll);
            if (score > maxScore)
            {
                maxScore = score;
                maxCategory = new List<Category>(){(Category)category};
            }
            else if (score == maxScore)
            {
                maxCategory.Add((Category)category);
            }
        }

        return maxCategory;
    }
    public int Score(Category category, List<int> diceRoll)
    {
        string diceRollStr = string.Join("",diceRoll);
        if (category >= Category.Ones && category <= Category.Eights)
        {
            // converting category id to int and equating with die roll number and then adding total
            return diceRoll.Where(die => die == (int)category).Sum();
        }
        else if (category == Category.ThreeOfaKind)
        {
            int score;
            // check if the 3 of a kind is in 4 of a kind
            if (fourOfaKindCache.TryGetValue(diceRollStr, out score))
            {
                return score;
            }

            if (threeOfaKindCache.TryGetValue(diceRollStr, out score))
            {
                return score;
            }

            var groupedRoll = diceRoll.GroupBy(die => die);
            foreach (var grouped in groupedRoll.ToList())
            {
                if (grouped.Count() >= 3)
                {
                    score = diceRoll.Sum();
                    fourOfaKindCache.Add(diceRollStr, score);
                    return diceRoll.Sum();
                }
            }
            return 0;
        }
        else if (category == Category.FourOfaKind)
        {
            if (fourOfaKindCache.TryGetValue(diceRollStr, out int score))
            {
                return score;
            }

            var groupedRoll = diceRoll.GroupBy(die => die);
            foreach(var grouped in groupedRoll.ToList())
            {
                if (grouped.Count() >= 4)
                {
                    score = diceRoll.Sum();
                    fourOfaKindCache.Add(diceRollStr, score);
                    return diceRoll.Sum();
                }
            }
            return 0;
        }
        else if (category == Category.FullHouse)
        {
            // checking cache
            if (fullHouseCache.TryGetValue(diceRollStr,out int score))
            {
                return score;
            }

            // group same numbers together and then check for full house
            var valueCount = diceRoll.GroupBy(die => die).Select(group => group.Count());
            if (valueCount.Count(count => count == 2 || count == 3) == 2)
            {
                fullHouseCache.Add(diceRollStr, 25);
                return 25;
            }
            return 0;
        }
        else if (category == Category.SmallStraight)
        {
            var sortedDice = diceRoll.Distinct().OrderBy(die => die).ToList();
            diceRollStr = string.Join("",sortedDice);
            if (!smallStraightCache.ContainsKey(diceRollStr))
            {
                // initialize the cache
                smallStraightCache.Add(diceRollStr,
                    (diceRollStr.Contains("1234")
                    || diceRollStr.Contains("2345")
                    || diceRollStr.Contains("3456")
                    || diceRollStr.Contains("4567")
                    || diceRollStr.Contains("5678")));
            }
            return smallStraightCache[diceRollStr] ? 30 : 0;
        }
        else if (category == Category.AllDifferent)
        {
            // If the distinct count is equal to diceroll then its all different.
            if (!allDifferentCache.ContainsKey(diceRollStr))
            {
                // Cache it for faster check next time
                allDifferentCache.Add(diceRollStr, diceRoll.Distinct().Count() == diceRoll.Count);
            }
            return allDifferentCache[diceRollStr] ? 35 : 0;
        }
        else if (category == Category.LargeStraight)
        {
            var sortedDice = diceRoll.Distinct().OrderBy(die => die).ToList();
            diceRollStr = string.Join("", sortedDice);
            if (!largeStraightCache.ContainsKey(diceRollStr))
            {
                // initialize the cache
                largeStraightCache.Add(diceRollStr,
                    (diceRollStr.Equals("12345")
                    || diceRollStr.Equals("23456")
                    || diceRollStr.Equals("34567")
                    || diceRollStr.Equals("45678")));
            }
            return largeStraightCache[diceRollStr] ? 40 : 0;
        }
        else if (category == Category.Schooner)
        {
            // Check if its in the cache
            if (schoonerCache.TryGetValue(diceRollStr, out var schoonerScore))
            {
                return schoonerScore;
            }

            for(int i = 1 ; i < diceRoll.Count; ++i)
            {
                if (diceRoll[i] != diceRoll[0])
                    return 0;
            }

            schoonerCache.Add(diceRollStr, 50);
            return 50;
        }
        else if (category == Category.Chance)
        {
            // no caching as there are too many probability
            return diceRoll.Sum();
        }
        else
        {
            throw new ArgumentException($"Invalid category: {category}");
        }
    }
}