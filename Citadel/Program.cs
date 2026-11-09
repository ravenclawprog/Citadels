using Citadel;

if (args.Length > 1 || (args.Length == 1 && (!int.TryParse(args[0], out int value) || value < 4 || value > 7)))
{
    Console.Error.WriteLine("Usage: Citadel [players: 4-7]");
    return 1;
}

int count = args.Length == 0 ? 4 : int.Parse(args[0]);
var game = new Game(count);
game.StartCharacterSelection();
Console.WriteLine("Character selection prototype. Pass the terminal to the indicated player.");
while (game.CurrentChoosingPlayerId is int playerId)
{
    Console.WriteLine($"Player {playerId}, choose a character by rank:");
    var options = game.GetAvailableCharacters(playerId);
    var prerequisite = new BaseGamePrerequisite();
    foreach (var card in options)
        Console.WriteLine($"{prerequisite.Sequence(card)}: {card.Name}");
    string? input = Console.ReadLine();
    if (input == null)
    {
        Console.WriteLine("Character selection cancelled.");
        return 0;
    }
    if (!int.TryParse(input, out int rank) || !options.Any(card => prerequisite.Sequence(card) == rank))
    {
        Console.WriteLine("Enter the rank of an available character.");
        continue;
    }
    game.ChooseCharacter(playerId, rank);
    Console.Clear();
}
Console.WriteLine("Characters selected. Player turn order: " + string.Join(", ", game.GetTurnOrder()));
Console.WriteLine("Turn actions and character abilities are not implemented yet.");
return 0;
