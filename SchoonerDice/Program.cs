using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

public class Program
{
    static void Main(string[] args)
    {

        int mainMenuIndex = 0;
        while (mainMenuIndex < 9)
        {
            Console.WriteLine("Enter 5 Dice Roll: (example: 11458)\n9.Exit\nEnter your selection: ");

            string inputString = Console.ReadLine();
            if (string.IsNullOrEmpty(inputString) || string.Equals(inputString, "9"))
                break;

            // Validate string length and handle potential errors
            if (inputString.Length != 5)
            {
                Console.WriteLine("Invalid input. Please enter exactly 5 digits.");
                continue;
            }

            // Parse each character as an integer
            List<int> diceRoll = new List<int>();
            foreach (char character in inputString)
            {
                if (int.TryParse(character.ToString(), out int dieValue) && dieValue >= 1 && dieValue <= 8)
                {
                    diceRoll.Add(dieValue);
                }
                else
                {
                    Console.WriteLine("Invalid input detected. Please enter only digits between 1 and 8.");
                    return;
                }
            }

            Console.WriteLine("1.Score \n2.Top Category\nEnter your selection: ");

            int subMenu = 0;
            if (int.TryParse(Console.ReadLine(), out subMenu))
            {
                if (subMenu == 1)
                {
                    StringBuilder sb = new StringBuilder();
                    int i = 1;
                    foreach (Category val in Enum.GetValues(typeof(Category)))
                    {
                        sb.Append((i++) + "." + val + "\n");
                    }
                    Console.WriteLine("Enum category\n" + sb.ToString() + "\nType in the Category INDEX: ");
                    if (int.TryParse(Console.ReadLine(), out subMenu))
                    {
                        Console.WriteLine("Score: " + DiceScorer.Score((Category)mainMenuIndex, diceRoll));
                    }
                }
                else if (subMenu == 2)
                {
                    // Find top categories
                    var topCategories = DiceScorer.TopCategories(diceRoll);
                    if (topCategories.Count > 1)
                    {
                        Console.WriteLine("\nTop Categories:");
                        foreach (var category in topCategories)
                        {
                            Console.WriteLine("- {0}", category);
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nThere is only one top scoring category.");
                    }
                    Console.WriteLine("Top Category");
                }
            }
            else
            {
                Console.WriteLine("Not an int");
            }
        }
    }

    public enum Category
    {
        Ones = 1,
        Twos = 2,
        Threes = 3,
        Fours = 4,
        Fives = 5,
        Sixes = 6,
        Sevens = 7,
        Eights = 8,
        ThreeOfaKind = 9,
        FourOfaKind = 10,
        FullHouse = 11,
        SmallStraight = 12,
        AllDifferent = 13,
        LargeStraight = 14,
        Schooner = 15,
        Chance = 16
    }

    public static class DiceScorer
    {

        private static readonly Dictionary<List<int>, bool> smallStraightCache = new Dictionary<List<int>, bool>();
        private static readonly Dictionary<List<int>, bool> largeStraightCache = new Dictionary<List<int>, bool>();
        private static readonly Dictionary<List<int>, bool> allDifferentCache = new Dictionary<List<int>, bool>();
        private static readonly Dictionary<List<int>, int> fullHouseCache = new Dictionary<List<int>, int>();
        private static readonly Dictionary<List<int>, int> schoonerCache = new Dictionary<List<int>, int>();
        private static readonly Dictionary<List<int>, int> fourOfaKindCache = new Dictionary<List<int>, int>();
        private static readonly Dictionary<List<int>, int> threeOfaKindCache = new Dictionary<List<int>, int>();
        private static int numberOfDices = 5;
        private static int maxDiceFace = 8;

        public static void InitializeData()
        {
            for(int i = 0; i < maxDiceFace; i++)
            {
                var listOfSameNumbers = new List<int>();
                for (int j = 0; j < numberOfDices; j++)
                {
                    listOfSameNumbers.Add(i);
                }
                schoonerCache.Add(listOfSameNumbers, 50);
            }
        }

        public static int Score(Category category, List<int> diceRoll)
        {
            if (category >= Category.Ones && category <= Category.Eights)
            {
                // converting category id to int and equating with die roll number and then adding total
                return diceRoll.Where(die => die == (int)category).Sum();
            }
            else if (category == Category.ThreeOfaKind)
            {
                int score;
                // check if the 3 of a kind is in 4 of a kind
                if (fourOfaKindCache.TryGetValue(diceRoll, out score))
                {
                    return score;
                }

                if (threeOfaKindCache.TryGetValue(diceRoll, out score))
                {
                    return score;
                }

                var groupedRoll = diceRoll.GroupBy(die => die);
                foreach (var grouped in groupedRoll.ToList())
                {
                    if (grouped.Count() >= 3)
                    {
                        score = diceRoll.Sum();
                        fourOfaKindCache.Add(diceRoll, score);
                        return diceRoll.Sum();
                    }
                }
                return 0;
            }
            else if (category == Category.FourOfaKind)
            {
                if (fourOfaKindCache.TryGetValue(diceRoll, out int score))
                {
                    return score;
                }

                var groupedRoll = diceRoll.GroupBy(die => die);
                foreach(var grouped in groupedRoll.ToList())
                {
                    if (grouped.Count() >= 4)
                    {
                        score = diceRoll.Sum();
                        fourOfaKindCache.Add(diceRoll, score);
                        return diceRoll.Sum();
                    }
                }
                return 0;
            }
            else if (category == Category.FullHouse)
            {
                if (fullHouseCache.TryGetValue(diceRoll,out int score))
                {
                    return score;
                }

                var valueCount = diceRoll.GroupBy(die => die).Select(group => group.Count());
                if (valueCount.Count(count => count == 2 || count == 3) == 2)
                {
                    fullHouseCache.Add(diceRoll, 25);
                    return 25;
                }
                return 0;
            }
            else if (category == Category.SmallStraight)
            {
                // there is only two way for a smallstraight
                var sortedDice = diceRoll.Distinct().OrderBy(die => die).ToList();
                if (!smallStraightCache.ContainsKey(sortedDice))
                {
                    smallStraightCache.Add(sortedDice,
                        (sortedDice.SequenceEqual(new List<int> { 1, 2, 3, 4 })
                        || sortedDice.SequenceEqual(new List<int> { 2, 3, 4, 5 })
                        || sortedDice.SequenceEqual(new List<int> { 3, 4, 5, 6 })
                        || sortedDice.SequenceEqual(new List<int> { 4, 5, 6, 7 })
                        || sortedDice.SequenceEqual(new List<int> { 5, 6, 7, 8 })));
                }
                return smallStraightCache[sortedDice] ? 30 : 0;
            }
            else if (category == Category.AllDifferent)
            {
                // If the distinct count is equal to diceroll then its all different.
                if (!allDifferentCache.ContainsKey(diceRoll))
                {
                    // Cache it for faster check next time
                    allDifferentCache.Add(diceRoll, diceRoll.Distinct().Count() == diceRoll.Count);
                }
                return allDifferentCache[diceRoll] ? 35 : 0;
            }
            else if (category == Category.LargeStraight)
            {
                var sortedDice = diceRoll.Distinct().OrderBy(die => die).ToList();
                if (!largeStraightCache.ContainsKey(sortedDice))
                {
                    largeStraightCache.Add(sortedDice,
                        (sortedDice.SequenceEqual(new List<int> { 1, 2, 3, 4, 5 })
                        || sortedDice.SequenceEqual(new List<int> { 2, 3, 4, 5, 6 })
                        || sortedDice.SequenceEqual(new List<int> { 3, 4, 5, 6, 7 })
                        || sortedDice.SequenceEqual(new List<int> { 4, 5, 6, 7, 8 })));
                }
                return largeStraightCache[sortedDice] ? 40 : 0;
            }
            else if (category == Category.Schooner)
            {
                if (schoonerCache.TryGetValue(diceRoll, out var schoonerScore))
                {
                    return schoonerScore;
                }
                return 0;
            }
            else if (category == Category.Chance)
            {
                return diceRoll.Sum();
            }
            else
            {
                throw new ArgumentException($"Invalid category: {category}");
            }
        }

        public static bool HasThreeOfAKind(List<int> diceRoll)
        {
            var valueCount = diceRoll.GroupBy(die => die).Select(group => group.Count());
            return valueCount.Any(count => count >= 3);
        }

        public static bool HasFourOfAKind(List<int> diceRoll)
        {
            var valueCount = diceRoll.GroupBy(die => die).Select(group => group.Count());
            return valueCount.Any(count => count >= 4);
        }

        public static bool IsFullHouse(List<int> diceRoll)
        {
            var valueCount = diceRoll.GroupBy(die => die).Select(group => group.Count());
            return valueCount.Count(count => count == 2 || count == 3) == 2;
        }

        public static bool IsSmallStraight(List<int> diceRoll)
        {
            var sortedDice = diceRoll.Distinct().OrderBy(die => die).ToList();
            if (!smallStraightCache.ContainsKey(sortedDice))
            {
                smallStraightCache.Add(
                    sortedDice,
                    (sortedDice.SequenceEqual(new List<int> { 1, 2, 3, 4 }) || sortedDice.SequenceEqual(new List<int> { 2, 3, 4, 5 })));
            }
            return smallStraightCache[sortedDice];
        }

        public static bool IsAllDifferent(List<int> diceRoll)
        {
            if (!allDifferentCache.ContainsKey(diceRoll))
            {
                allDifferentCache.Add(diceRoll, diceRoll.Distinct().Count() == diceRoll.Count);
            }
            return allDifferentCache[diceRoll];
        }

        public static bool IsLargeStraight(List<int> diceRoll)
        {
            var sortedDice = diceRoll.Distinct().OrderBy(die => die).ToList();
            return sortedDice.SequenceEqual(new List<int> { 3, 4, 5, 6, 7 }) || sortedDice.SequenceEqual(new List<int> { 4, 5, 6, 7, 8 });
        }

        public static bool IsSchooner(List<int> diceRoll)
        {
            return diceRoll.Distinct().Count() == 1;
        }
        public static List<Category> TopCategories(List<int> diceRoll)
        {
            var scores = new Dictionary<Category, int>();
            foreach (var category in Enum.GetValues(typeof(Category)))
            {
                scores.Add((Category)category, Score((Category)category, diceRoll));
            }

            var maxScore = scores.Values.Max();
            return scores.Where(pair => pair.Value == maxScore).Select(pair => pair.Key).ToList();
        }
    }
}