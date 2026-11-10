using Citadel;

var viewer = new ConsoleViewer();
if (!ConsoleController.TryParsePlayerCount(args, out int count))
{
    viewer.ShowUsage();
    return 1;
}

var game = new Game(count, viewer);
var controller = new ConsoleController();
foreach (var player in game.Players)
    player.SetController(controller);
viewer.ShowWelcome();
game.Run();
viewer.ShowStopped();
return 0;
