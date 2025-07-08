using System;
namespace Citadel
{
    public class ConsoleViewer : IViewer
    {
        public void OnAttach(object? sender, Player.ViewerAttachEventArgs e)
        {
            Console.WriteLine("Attached player with id:{0}; name:{1}", e.playerId, e.playerName);
        }
        public void OnDettach(object? sender, Player.ViewerDetachEventArgs e)
        {
            Player? detachedPlayer = (Player?)sender;
            Console.WriteLine("Player detached with id:{0}", detachedPlayer?.GetId());
        }
        public void OnChangeName(object? sender, Player.ViewerChangeNameEventArgs e)
        {
            Player? player = (Player?)sender;
            Console.WriteLine("Player detached with id:{0}", player?.GetId());
            Console.WriteLine("New player name:{0}", e.newPlayerName);
        }
        public void OnGoldUpdated(object? sender, Player.ViewerGoldUpdatedEventArgs e)
        {
            Player? player = (Player?)sender;
            Console.WriteLine("Gold updated for player with id:{0}", player?.GetId());
            Console.WriteLine("Current gold:{0}", e.currentGoldOfPlayer);
        }
        public void OnCoronationChange(object? sender, Player.ViewerCoronationChange e)
        {
            Player? player = (Player?)sender;
            Console.WriteLine("Coronation changed for player with id:{0}", player?.GetId());
            Console.WriteLine("Has crown:{0}", e.hasCrown);
        }
        public void OnTownCardDeckUpdated(object? sender, Player.ViewerTownCardDeckUpdatedEventArgs e)
        {
            Player? player = (Player?)sender;
            Console.WriteLine("Town card updated for player with id:{0}", player?.GetId());
        }
        public void OnBuildedTownUpdated(object? sender, Player.ViewerBuildedTownUpdatedEventArgs e)
        {
            Player? player = (Player?)sender;
            Console.WriteLine("Builded town updated for player with id:{0}", player?.GetId());
        }
    }
}