namespace UnitTestMiscellaneous;

using Citadel;
using Moq;

public class UnitTestConsole
{
    private sealed class CallbackReader(Func<string?> read) : TextReader
    {
        public override string? ReadLine() => read();
    }

    private static void RunConsole(Game game, ConsoleViewer viewer, TextReader input)
    {
        var controller = new ConsoleController(input);
        foreach (var player in game.Players)
        {
            player.SetViewer(viewer);
            player.SetController(controller);
        }
        viewer.ShowWelcome();
        game.Run();
        viewer.ShowStopped();
    }

    private static void ChooseCharacters(Game game)
    {
        game.StartCharacterSelection();
        while (game.CurrentChoosingPlayerId is int id)
            game.ChooseCharacter(id, new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    public void PlayersRunTwoRoundsThroughGameAndViewer(int count)
    {
        var output = new StringWriter();
        var viewer = new ConsoleViewer(output, output);
        var game = new Game(count, viewer);
        int resourceActions = 0;
        int turnsEnded = 0;
        var input = new CallbackReader(() =>
        {
            Assert.True(resourceActions <= count * 2);
            if (game.RoundNumber >= 3) return "quit";
            if (game.Phase == GamePhase.CharacterSelection)
            {
                int id = game.CurrentChoosingPlayerId!.Value;
                return new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]).ToString();
            }
            if (!game.HasTakenResources)
            {
                resourceActions++;
                return "  GOLD  ";
            }
            turnsEnded++;
            return "end";
        });

        RunConsole(game, viewer, input);

        Assert.Equal(count * 2, resourceActions);
        Assert.Equal(count * 2, turnsEnded);
        Assert.Equal(3, game.RoundNumber);
        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        Assert.Contains("choose a character by rank:", output.ToString());
        Assert.Contains("(rank ", output.ToString());
        Assert.Contains("Your hand:", output.ToString());
        Assert.Contains("Game stopped.", output.ToString());
    }

    [Fact]
    public void InvalidRoleIsReportedAndDoesNotAdvanceSelection()
    {
        var output = new StringWriter();
        var viewer = new ConsoleViewer(output, output);
        var game = new Game(4, viewer);
        RunConsole(game, viewer, new StringReader("invalid\n999\nquit\n"));

        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        Assert.False(game.CharactersSelected);
        Assert.Contains("Enter the rank of an available character.", output.ToString());
        Assert.Equal(1, game.RoundNumber);
    }

    [Fact]
    public void CardCommandsKeepSecondCardAndRecoverFromInvalidInput()
    {
        var output = new StringWriter();
        var viewer = new ConsoleViewer(output, output);
        var game = new Game(4, viewer);
        ChooseCharacters(game);
        int id = game.CurrentPlayerId!.Value;
        string? expectedCard = null;
        var commands = new Queue<string>(["end", "unknown", "cards", "gold", "keep 999", "keep 2", "end", "quit"]);
        var input = new CallbackReader(() =>
        {
            if (game.GetDrawnTownCards(game.CurrentPlayerId!.Value).Count == 2)
                expectedCard = game.GetDrawnTownCards(id)[1].Name;
            return commands.Dequeue();
        });
        RunConsole(game, viewer, input);

        Assert.NotNull(expectedCard);
        Assert.Equal(expectedCard, game.GetPlayerHand(id)[^1].Name);
        Assert.Equal(5, game.GetPlayerHand(id).Count);
        Assert.Equal(2, game.GetPlayerGold(id));
        Assert.NotEqual(id, game.CurrentPlayerId);
        Assert.Contains("Take resources before", output.ToString());
        Assert.Contains("Unknown command.", output.ToString());
        Assert.Contains("Resources may only be taken once", output.ToString());
        Assert.Contains("Enter a number from the displayed list.", output.ToString());
        Assert.Contains("Choose a drawn district:", output.ToString());
    }

    [Fact]
    public void BuildCommandUsesDisplayedHandNumberAndGameEnforcesLimit()
    {
        var players = new Dictionary<int, Player>();
        var observer = new Mock<IViewer>();
        observer.Setup(v => v.OnAttach(It.IsAny<object>(), It.IsAny<Player.ViewerAttachEventArgs>()))
            .Callback<object, Player.ViewerAttachEventArgs>((sender, e) => players.Add(e.playerId, (Player)sender));
        var game = new Game(4, observer.Object);
        ChooseCharacters(game);
        int id = game.CurrentPlayerId!.Value;
        players[id].OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs
        {
            addTownCardDeck = [new TownCard(QuarterType.Commercial, 1, "Console district")]
        });
        int number = game.GetPlayerHand(id).Count;
        var output = new StringWriter();
        var viewer = new ConsoleViewer(output, output);
        var input = new StringReader($"gold\nbuild {number}\nbuild 1\nquit\n");
        RunConsole(game, viewer, input);

        Assert.Equal("Console district", Assert.Single(game.GetPlayerCity(id)).Name);
        Assert.Equal(3, game.GetPlayerGold(id));
        Assert.Equal(4, game.GetPlayerHand(id).Count);
        Assert.Contains("Only one district may be built per turn.", output.ToString());
        Assert.Contains("Your city:", output.ToString());
    }

    [Fact]
    public void EndOfInputStopsWithoutTakingResources()
    {
        var output = new StringWriter();
        var viewer = new ConsoleViewer(output, output);
        var game = new Game(4, viewer);
        ChooseCharacters(game);
        int id = game.CurrentPlayerId!.Value;
        RunConsole(game, viewer, new StringReader(""));
        Assert.Equal(id, game.CurrentPlayerId);
        Assert.False(game.HasTakenResources);
        Assert.Contains("Game stopped.", output.ToString());
    }

    [Theory]
    [InlineData("4", true, 4)]
    [InlineData("7", true, 7)]
    [InlineData("8", false, 8)]
    [InlineData("3", false, 3)]
    [InlineData("invalid", false, 0)]
    public void StartupArgumentsAreValidated(string argument, bool accepted, int count)
    {
        Assert.Equal(accepted, ConsoleController.TryParsePlayerCount([argument], out int actual));
        Assert.Equal(count, actual);
    }

    [Fact]
    public void StartupDefaultsToFourAndRejectsExtraArguments()
    {
        Assert.True(ConsoleController.TryParsePlayerCount([], out int count));
        Assert.Equal(4, count);
        Assert.False(ConsoleController.TryParsePlayerCount(["4", "extra"], out _));
    }
}
