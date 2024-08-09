using System.Collections.Generic;
namespace Citadel;
public class Player
{
    private string _name = "Default Name";
    private bool _hasCrown = false;
    IPlayerCard _playerCard;
    ITownCard[] _townCardsDeck;
    ITownCard[] _ownTown;
}