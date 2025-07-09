namespace Citadel
{
    public class IncorrectNumberOfPlayers : Exception
    {
        public IncorrectNumberOfPlayers()
        {
        }
        public IncorrectNumberOfPlayers(string message)
        : base(message)
        {
        }

        public IncorrectNumberOfPlayers(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    public class WrongPlayerCard : Exception
    {
        public WrongPlayerCard()
        {
        }
        public WrongPlayerCard(string message)
        : base(message)
        {
        }

        public WrongPlayerCard(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
    public class UnchoosedPlayerCard : Exception
    {
        public UnchoosedPlayerCard()
        {
        }
        public UnchoosedPlayerCard(string message)
        : base(message)
        {
        }

        public UnchoosedPlayerCard(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}