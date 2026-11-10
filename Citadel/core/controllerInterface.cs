namespace Citadel;

public enum PlayerActionKind
{
    ChooseCharacter,
    TakeGold,
    DrawTownCards,
    KeepTownCard,
    BuildQuarter,
    EndTurn,
    Quit
}

// Value is a character rank or a zero-based card index, depending on Kind.
public sealed record PlayerAction(PlayerActionKind Kind, int Value = 0);
public sealed record CharacterChoice(int Rank, string Name);

public interface IController
{
    PlayerAction ChooseCharacter(Game.PlayerChooseCharacterEventArgs message);
    PlayerAction TakeResources(Game.PlayerTakeResourcesEventArgs message);
    PlayerAction KeepTownCard(Game.PlayerKeepTownCardEventArgs message);
    PlayerAction ChooseTurnAction(Game.PlayerTurnActionEventArgs message);
}
