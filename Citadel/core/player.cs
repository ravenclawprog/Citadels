using System.Collections.Generic;
using System.Linq.Expressions;
namespace Citadel;

public partial class Player
{
    private static int idCount = 0;
    public Player(int gold, bool crown, String? name = null, IPlayerCard? plCard = null, List<ITownCard>? townCardDeck = null, List<ITownCard>? ownTown = null, int? newId = null)
    {
        _goldCount = gold;
        _hasCrown = crown;
        _firstGetMaxBuildings = false;
        if (name != null)
        {
            _name = name;
        }
        if (plCard != null)
        {
            _playerCard = plCard;
        }
        if (townCardDeck != null)
        {
            _townCardsDeck.AddRange(townCardDeck);
        }
        if (ownTown != null)
        {
            _ownTown.AddRange(ownTown);
        }
        if (newId != null)
        {
            _id = (int)newId;
        }
        else
        {
            idCount++;
        }
            
    }
    bool Build(int index)
    {
        if (index >= _townCardsDeck.Count)
        {
            return false;
        }
        ITownCard townCard = _townCardsDeck[index];
        if (_goldCount < townCard.Price)
        {
            return false;
        }
        _ownTown.Add(townCard);
        _townCardsDeck.Remove(townCard);
        _goldCount -= townCard.Price;
        return true;
    }
    private string _name = "Default Name";
    private bool _hasCrown = false;
    private int _goldCount = 0;
    private IPlayerCard? _playerCard = null;
    private List<ITownCard> _townCardsDeck = new List<ITownCard>();
    private List<ITownCard> _ownTown = new List<ITownCard>();
    private int _id = idCount;
    private bool _firstGetMaxBuildings = false;
    public int GetNumberOfGold()
    {
        return _goldCount;
    }
    public int GetNumberOfTownCards()
    {
        return _townCardsDeck.Count;
    }
    public void AppendTownCard(ITownCard townCard)
    {
        _townCardsDeck.Add(townCard);
    }
    public List<ITownCard> GetOwnCity()
    {
        return _ownTown.Select(s => (ITownCard)s.Clone()).ToList();
    }
    public void AddGold(int gold)
    {
        _goldCount += gold;
    }
    public void RemoveGold(int gold)
    {
        _goldCount -= gold;
        if (_goldCount < 0) _goldCount = 0;
    }
    public int getId()
    {
        return _id;
    }
    public void setCrown()
    {
        _hasCrown = true;
    }
}