using System.Text;
DiceScorer diceScorer = new DiceScorer();
while (true)
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

    if (int.TryParse(Console.ReadLine(), out int subMenu))
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
                Console.WriteLine("Score: " + diceScorer.Score((Category)subMenu, diceRoll));
            }
        }
        else if (subMenu == 2)
        {
            // Find top categories
            int maxScore = 0;
            var topCategories = diceScorer.TopCategories(diceRoll, out maxScore);
            if (topCategories.Count > 0)
            {
                Console.WriteLine("\nTop Categories with max score of {0}:", maxScore);
                foreach (var category in topCategories)
                {
                    Console.WriteLine("- {0}", category);
                }
            }
            else
            {
                Console.WriteLine("\nThere is NO top scoring category.");
            }
        }
    }
    else
    {
        Console.WriteLine("Not an int");
    }
}