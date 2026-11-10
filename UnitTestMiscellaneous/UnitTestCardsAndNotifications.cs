namespace UnitTestMiscellaneous;

using Citadel;
using Moq;

public class UnitTestCardsAndNotifications
{
    [Fact]
    public void TownCardAndCloneKeepDescription()
    {
        var card = new TownCard(QuarterType.Special, 5, "Library", "Keep both drawn cards.");
        var clone = Assert.IsType<TownCard>(card.Clone());
        Assert.Equal("Keep both drawn cards.", card.Description);
        Assert.Equal(card.Description, clone.Description);
        Assert.Equal(card.Name, clone.Name);
        Assert.Equal(card.Price, clone.Price);
        Assert.Equal(card.TypeOfQuarter, clone.TypeOfQuarter);
        Assert.NotSame(card, clone);
    }

    [Fact]
    public void AddingCardsNotifiesViewerWithIndependentHandSnapshot()
    {
        var snapshots = new List<List<ITownCard>>();
        var viewer = new Mock<IViewer>();
        viewer.Setup(v => v.OnTownCardDeckUpdated(It.IsAny<object>(),
                It.IsAny<Player.ViewerTownCardDeckUpdatedEventArgs>()))
            .Callback<object, Player.ViewerTownCardDeckUpdatedEventArgs>((_, e) => snapshots.Add(e.townCardDeck));
        var player = new Player(0, false, viewer: viewer.Object);
        Assert.Empty(Assert.Single(snapshots));
        snapshots.Clear();
        var card = new TownCard(QuarterType.Commercial, 1, "Tavern");
        player.OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs { addTownCardDeck = [card] });
        player.OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs { addTownCardDeck = [card] });
        Assert.Equal(2, snapshots.Count);
        Assert.Single(snapshots[0]);
        Assert.Equal(2, snapshots[1].Count);
        Assert.NotSame(card, snapshots[0][0]);
        snapshots[1].Clear();
        Assert.Equal(2, player.GetNumberOfTownCards());
    }

}
