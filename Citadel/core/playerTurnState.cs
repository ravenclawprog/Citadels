namespace Citadel;

public partial class Player
{
    internal int GoldCount => Gold;
    internal IReadOnlyList<ITownCard> HandSnapshot => _townCardsDeck
        .Select(card => (ITownCard)card.Clone()).ToList().AsReadOnly();
    internal IReadOnlyList<ITownCard> CitySnapshot => GetOwnCity().AsReadOnly();

    internal void ValidateBuilding(int index)
    {
        if (index < 0 || index >= _townCardsDeck.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var card = _townCardsDeck[index];
        if (Gold < card.Price)
            throw new InvalidOperationException("Not enough gold to build this district.");
        if (_ownTown.Any(built => built.Name == card.Name))
            throw new InvalidOperationException("This district already exists in the player's city.");
    }
}
