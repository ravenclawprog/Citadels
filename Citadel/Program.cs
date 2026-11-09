using Citadel;

if (args.Length > 1 || (args.Length == 1 && (!int.TryParse(args[0], out int value) || value < 4 || value > 7)))
{
    Console.Error.WriteLine("Usage: Citadel [players: 4-7]");
    return 1;
}

int count = args.Length == 0 ? 4 : int.Parse(args[0]);
var game = new Game(count);
Console.WriteLine("Citadels: basic turns. Character abilities, special district effects and scoring are not implemented yet.");
Console.WriteLine("Pass the terminal to the indicated player. Enter 'quit' to stop.");
game.StartCharacterSelection();
while (true)
{
    if (game.Phase == GamePhase.CharacterSelection)
    {
        int playerId = game.CurrentChoosingPlayerId!.Value;
        Console.WriteLine($"Round {game.RoundNumber}. Player {playerId}, choose a character by rank:");
        var options = game.GetAvailableCharacters(playerId);
        var prerequisite = new BaseGamePrerequisite();
        foreach (var card in options)
            Console.WriteLine($"{prerequisite.Sequence(card)}: {card.Name}");
        string? input = ReadCommand();
        if (input == null) break;
        if (!int.TryParse(input, out int rank) || !options.Any(card => prerequisite.Sequence(card) == rank))
        {
            Console.WriteLine("Enter the rank of an available character.");
            continue;
        }
        if (!Console.IsOutputRedirected) Console.Clear();
        game.ChooseCharacter(playerId, rank);
        continue;
    }

    int activeId = game.CurrentPlayerId!.Value;
    Console.WriteLine($"Round {game.RoundNumber}. Player {activeId}, {game.CurrentCharacterName} (rank {game.CurrentCharacterRank}). Gold: {game.GetPlayerGold(activeId)}.");
    var drawn = game.GetDrawnTownCards(activeId);
    if (drawn.Count != 0)
    {
        ShowCards(drawn);
        Console.WriteLine("Choose a drawn district: keep <number>.");
    }
    else if (!game.HasTakenResources)
        Console.WriteLine("Take resources: gold or cards.");
    else
    {
        Console.WriteLine("Your hand:");
        ShowCards(game.GetPlayerHand(activeId));
        Console.WriteLine("Your city:");
        ShowCards(game.GetPlayerCity(activeId));
        Console.WriteLine(game.HasBuiltThisTurn ? "End your turn: end." : "Build a district: build <number>, or end your turn: end.");
    }

    string? command = ReadCommand();
    if (command == null) break;
    string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    try
    {
        if (parts.Length == 1 && parts[0] == "gold")
            game.TakeGold(activeId);
        else if (parts.Length == 1 && parts[0] == "cards")
            game.DrawTownCards(activeId);
        else if (parts.Length == 1 && parts[0] == "end")
        {
            if (!Console.IsOutputRedirected) Console.Clear();
            game.EndTurn(activeId);
        }
        else if (parts.Length == 2 && int.TryParse(parts[1], out int number) && number > 0)
        {
            if (parts[0] == "keep") game.KeepTownCard(activeId, number - 1);
            else if (parts[0] == "build") game.BuildQuarter(activeId, number - 1);
            else Console.WriteLine("Unknown command.");
        }
        else Console.WriteLine("Unknown command.");
    }
    catch (InvalidOperationException error)
    {
        Console.WriteLine(error.Message);
    }
    catch (ArgumentOutOfRangeException)
    {
        Console.WriteLine("Enter a number from the displayed list.");
    }
}
Console.WriteLine("Game stopped.");
return 0;

static string? ReadCommand()
{
    string? command = Console.ReadLine()?.Trim().ToLowerInvariant();
    return command == "quit" ? null : command;
}

static void ShowCards(IReadOnlyList<ITownCard> cards)
{
    if (cards.Count == 0) Console.WriteLine("(empty)");
    for (int i = 0; i < cards.Count; i++)
        Console.WriteLine($"{i + 1}: {cards[i].Name}, cost {cards[i].Price}, {cards[i].TypeOfQuarter}");
}
