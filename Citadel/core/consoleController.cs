namespace Citadel;

// Reads decisions only. Display and error reporting belong to the player's IViewer.
public class ConsoleController : IController
{
    private readonly TextReader _input;

    public ConsoleController(TextReader? input = null) => _input = input ?? Console.In;

    public static bool TryParsePlayerCount(string[] args, out int count)
    {
        count = 4;
        return args.Length == 0 || (args.Length == 1 && int.TryParse(args[0], out count) && count >= 4 && count <= 7);
    }

    public PlayerAction ChooseCharacter(Game.PlayerChooseCharacterEventArgs message) => ReadAction(true);
    public PlayerAction TakeResources(Game.PlayerTakeResourcesEventArgs message) => ReadAction();
    public PlayerAction KeepTownCard(Game.PlayerKeepTownCardEventArgs message) => ReadAction();
    public PlayerAction ChooseTurnAction(Game.PlayerTurnActionEventArgs message) => ReadAction();

    private PlayerAction ReadAction(bool characterSelection = false)
    {
        string? command = _input.ReadLine()?.Trim().ToLowerInvariant();
        if (command == null || command == "quit") return new(PlayerActionKind.Quit);
        if (characterSelection)
        {
            if (int.TryParse(command, out int rank)) return new(PlayerActionKind.ChooseCharacter, rank);
            throw new FormatException("Enter the rank of an available character.");
        }

        string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            switch (parts[0])
            {
                case "gold": return new(PlayerActionKind.TakeGold);
                case "cards": return new(PlayerActionKind.DrawTownCards);
                case "end": return new(PlayerActionKind.EndTurn);
            }
        }
        else if (parts.Length == 2 && int.TryParse(parts[1], out int number) && number > 0)
        {
            switch (parts[0])
            {
                case "keep": return new(PlayerActionKind.KeepTownCard, number - 1);
                case "build": return new(PlayerActionKind.BuildQuarter, number - 1);
            }
        }
        throw new FormatException("Unknown command.");
    }
}
