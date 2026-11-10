namespace Citadel
{
    public partial class Player
    {
        public void OnAddGold(object? sender, Game.PlayerAddGoldEventArgs e)
        {
            AddGold(e.goldToAdd);
        }
        public void OnAddTownCards(object? sender, Game.PlayerAddTownCardsEventArgs e)
        {
            AppendTownCards(e.addTownCardDeck);
        }
        public void OnCoronation(object? sender, Game.PlayerCoronationEventArgs e)
        {
            Crown = true;
        }

        public void OnDecoronation(object? sender, Game.PlayerDecoronationEventArgs e)
        {
            Crown = false;
        }
        public void OnChoosePlayerCard(object? sender, Game.PlayerChoosePlayerCardEventArgs e)
        {
            _characterChoices = new ViewerPlayerCardsToChooseEventArgs
            {
                roundNumber = e.roundNumber,
                characters = e.characters,
                playerCardToChoose = e.playerCardToChoose,
                playerCardExcludedOpen = e.playerCardExcludedOpen,
                numberOfPlayerCardExcludedClose = e.numberOfPlayerCardExcludedClose
            };
            OnViewerPlayerCardsToChoose?.Invoke(this, _characterChoices);
        }

        public void OnChooseCharacter(object? sender, Game.PlayerChooseCharacterEventArgs e) =>
            ResolveAction(e, controller => controller.ChooseCharacter(e));

        public void OnTakeResources(object? sender, Game.PlayerTakeResourcesEventArgs e) =>
            ResolveAction(e, controller => controller.TakeResources(e));

        public void OnKeepTownCard(object? sender, Game.PlayerKeepTownCardEventArgs e) =>
            ResolveAction(e, controller => controller.KeepTownCard(e));

        public void OnTurnAction(object? sender, Game.PlayerTurnActionEventArgs e) =>
            ResolveAction(e, controller => controller.ChooseTurnAction(e));

        public void OnActionRejected(object? sender, Game.PlayerActionRejectedEventArgs e)
        {
            RequireRecipient(e);
            OnActionRejected(e.Error);
        }

        private void RequireRecipient(Game.PlayerInteractionEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            if (e.PlayerId != GetId())
                throw new InvalidOperationException("The message is not addressed to this player.");
        }

        private void ResolveAction(Game.PlayerInteractionEventArgs e, Func<IController, PlayerAction> decide)
        {
            RequireRecipient(e);
            var controller = _controller ?? throw new InvalidOperationException("The player has no controller.");
            PlayerAction action;
            while (true)
            {
                try
                {
                    action = decide(controller);
                    break;
                }
                catch (FormatException error)
                {
                    OnActionRejected(error);
                }
            }
            if (action == null)
                throw new InvalidOperationException("The controller did not produce an action.");
            var recipient = OnGameAction
                ?? throw new InvalidOperationException("The player has no message recipient.");
            recipient(this, new GameActionEventArgs(GetId(), action));
        }

        internal void OnActionRejected(Exception error) => _viewer?.OnActionRejected(error);

        public void OnCharacterAssigned(object? sender, Game.PlayerCharacterAssignedEventArgs e)
        {
            AssignCharacter(e.Character, e.Rank);
        }

        public void OnTurnStateChanged(object? sender, Game.PlayerTurnStateChangedEventArgs e)
        {
            _turnState = new ViewerTurnStateChangedEventArgs(e.RoundNumber, e.Stage,
                e.CharacterName, e.CharacterRank, e.CanBuild,
                e.DrawnCards.Select(card => (ITownCard)card.Clone()).ToList().AsReadOnly());
            _characterChoices = null;
            OnViewerTurnStateChanged?.Invoke(this, _turnState);
        }
    }
}
