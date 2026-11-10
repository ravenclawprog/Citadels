namespace UnitTestMiscellaneous;

using Citadel;
using Moq;

public class UnitTestPlayerActions
{
    private static Game StartTurns()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        game.StartCharacterSelection();
        while (game.CurrentChoosingPlayerId is int id)
        {
            int rank = new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]);
            game.ChooseCharacter(id, rank);
        }
        return game;
    }

    [Fact]
    public void GameAppliesAnswerOnlyToPolledPlayer()
    {
        var game = StartTurns();
        int activeId = game.CurrentPlayerId!.Value;
        int nextId = game.GetTurnOrder()[1];
        var activeController = new Mock<IController>(MockBehavior.Strict);
        activeController.Setup(c => c.TakeResources(It.Is<Game.PlayerTakeResourcesEventArgs>(r => r.PlayerId == activeId)))
            .Returns(new PlayerAction(PlayerActionKind.TakeGold));
        activeController.Setup(c => c.ChooseTurnAction(It.Is<Game.PlayerTurnActionEventArgs>(r => r.PlayerId == activeId)))
            .Returns(new PlayerAction(PlayerActionKind.Quit));
        var otherController = new Mock<IController>(MockBehavior.Strict);
        game.GetPlayer(activeId).SetController(activeController.Object);
        game.GetPlayer(nextId).SetController(otherController.Object);

        game.Run();

        Assert.Equal(4, game.GetPlayerGold(activeId));
        Assert.Equal(2, game.GetPlayerGold(nextId));
        Assert.Equal(activeId, game.CurrentPlayerId);
        otherController.VerifyNoOtherCalls();
    }

    [Fact]
    public void MissingControllerActionDoesNotChangeGame()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        var controller = new Mock<IController>();
        controller.Setup(c => c.ChooseCharacter(It.IsAny<Game.PlayerChooseCharacterEventArgs>())).Returns((PlayerAction)null!);
        foreach (var player in game.Players) player.SetController(controller.Object);

        var error = Assert.Throws<InvalidOperationException>(() => game.Run());

        Assert.Contains("did not produce an action", error.Message);
        Assert.False(game.CharactersSelected);
        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        Assert.All(game.Players, player => Assert.Equal(2, game.GetPlayerGold(player.GetId())));
    }
    [Fact]
    public void PlayerPublishesSeparateMessageWithoutChangingIncomingMessage()
    {
        var player = new Player(2, false);
        var controller = new Mock<IController>(MockBehavior.Strict);
        var message = new Game.PlayerChooseCharacterEventArgs(player.GetId(), 1,
            new[] { new CharacterChoice(4, "King") });
        var decision = new PlayerAction(PlayerActionKind.ChooseCharacter, 4);
        controller.Setup(c => c.ChooseCharacter(message)).Returns(decision);
        player.SetController(controller.Object);
        var sent = new List<Player.GameActionEventArgs>();
        player.OnGameAction += (sender, outgoing) =>
        {
            Assert.Same(player, sender);
            sent.Add(outgoing);
        };

        player.OnChooseCharacter(null, message);
        player.OnChooseCharacter(null, message);

        Assert.Equal(2, sent.Count);
        Assert.NotSame(sent[0], sent[1]);
        Assert.All(sent, outgoing =>
        {
            Assert.Equal(player.GetId(), outgoing.PlayerId);
            Assert.Same(decision, outgoing.Action);
        });
        Assert.Equal(4, Assert.Single(message.Characters).Rank);
        controller.Verify(c => c.ChooseCharacter(message), Times.Exactly(2));
    }

    [Fact]
    public void IncomingMessagePublishesActionThatGameHandlesWithoutRun()
    {
        var game = StartTurns();
        int id = game.CurrentPlayerId!.Value;
        var player = game.GetPlayer(id);
        var controller = new Mock<IController>(MockBehavior.Strict);
        controller.Setup(c => c.TakeResources(It.IsAny<Game.PlayerTakeResourcesEventArgs>()))
            .Returns(new PlayerAction(PlayerActionKind.TakeGold));
        player.SetController(controller.Object);
        Player.GameActionEventArgs? sent = null;
        player.OnGameAction += (_, message) => sent = message;

        player.OnTakeResources(game, new Game.PlayerTakeResourcesEventArgs(id, game.RoundNumber,
            game.CurrentCharacterName!, game.CurrentCharacterRank!.Value, 2));

        Assert.NotNull(sent);
        Assert.Equal(PlayerActionKind.TakeGold, sent.Action.Kind);
        Assert.Equal(4, game.GetPlayerGold(id));
        Assert.True(game.HasTakenResources);
    }

    [Fact]
    public void PlayerRejectsMessageForAnotherRecipientBeforeConsultingController()
    {
        var player = new Player(2, false);
        var controller = new Mock<IController>(MockBehavior.Strict);
        player.SetController(controller.Object);
        var message = new Game.PlayerChooseCharacterEventArgs(player.GetId() + 1, 1,
            Array.Empty<CharacterChoice>());

        Assert.Throws<InvalidOperationException>(() => player.OnChooseCharacter(null, message));

        controller.VerifyNoOtherCalls();
    }

    [Fact]
    public void GameRejectsForeignSenderAndForgedPlayerId()
    {
        var game = StartTurns();
        int id = game.CurrentPlayerId!.Value;
        var action = new PlayerAction(PlayerActionKind.TakeGold);
        var foreignPlayer = new Player(2, false, newId: id);
        Assert.Throws<InvalidOperationException>(() =>
            game.OnPlayerAction(foreignPlayer, new Player.GameActionEventArgs(id, action)));
        Assert.Throws<InvalidOperationException>(() =>
            game.OnPlayerAction(game.GetPlayer(id), new Player.GameActionEventArgs(id + 1, action)));
        int otherId = game.GetTurnOrder()[1];
        Assert.Throws<InvalidOperationException>(() =>
            game.OnPlayerAction(game.GetPlayer(otherId), new Player.GameActionEventArgs(otherId, action)));

        Assert.False(game.HasTakenResources);
        Assert.Equal(2, game.GetPlayerGold(id));
    }

    [Fact]
    public void RejectionMessageDoesNotConsultControllerOrPublishAnAction()
    {
        var viewer = new Mock<IViewer>();
        var player = new Player(2, false, viewer: viewer.Object);
        var controller = new Mock<IController>(MockBehavior.Strict);
        player.SetController(controller.Object);
        var error = new InvalidOperationException("Invalid move.");
        player.OnGameAction += (_, _) => throw new Xunit.Sdk.XunitException("Unexpected action.");

        player.OnActionRejected(null, new Game.PlayerActionRejectedEventArgs(player.GetId(), 1, error));

        viewer.Verify(v => v.OnActionRejected(error), Times.Once);
        controller.VerifyNoOtherCalls();
    }

}
