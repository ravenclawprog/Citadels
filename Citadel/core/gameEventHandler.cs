namespace Citadel
{
    public partial class Game
    {
        public void OnPlayerAction(object? sender, Player.GameActionEventArgs message)
        {
            ArgumentNullException.ThrowIfNull(message);
            if (sender is not Player player || !_players.Contains(player))
                throw new InvalidOperationException("The player does not belong to this game.");
            if (message.PlayerId != player.GetId())
                throw new InvalidOperationException("The message does not belong to its sender.");
            Player? activePlayer = Phase == GamePhase.CharacterSelection ? _selectionOrder[_selectionIndex]
                : Phase == GamePhase.PlayerTurns ? _turnOrder[_turnIndex] : null;
            if (!ReferenceEquals(activePlayer, player))
                throw new InvalidOperationException("It is not this player's turn to send an action.");
            var action = message.Action;
            if (action.Kind == PlayerActionKind.Quit)
            {
                _stopRequested = true;
                return;
            }

            try
            {
                switch (action.Kind)
                {
                    case PlayerActionKind.ChooseCharacter: ChooseCharacter(message.PlayerId, action.Value); break;
                    case PlayerActionKind.TakeGold: TakeGold(message.PlayerId); break;
                    case PlayerActionKind.DrawTownCards: DrawTownCards(message.PlayerId); break;
                    case PlayerActionKind.KeepTownCard: KeepTownCard(message.PlayerId, action.Value); break;
                    case PlayerActionKind.BuildQuarter: BuildQuarter(message.PlayerId, action.Value); break;
                    case PlayerActionKind.EndTurn: EndTurn(message.PlayerId); break;
                    default: throw new ArgumentOutOfRangeException(nameof(action), "Unknown player action.");
                }
            }
            catch (Exception error) when (error is WrongPlayerCard or InvalidOperationException or ArgumentOutOfRangeException)
            {
                OnRejectAction = player.OnActionRejected;
                OnRejectAction(this, new PlayerActionRejectedEventArgs(message.PlayerId, RoundNumber, error));
            }
        }
    }
}
