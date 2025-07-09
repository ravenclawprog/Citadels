using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Mail;
namespace Citadel;

public partial class Player
{
    private static int idCount = 0;
    public Player(int gold, bool crown, String? name = null, IPlayerCard? plCard = null, List<ITownCard>? townCardDeck = null, List<ITownCard>? ownTown = null, IViewer? viewer = null, int? newId = null)
    {
        // TODO: change tp 
        Gold = gold;
        Crown = crown;
        _viewer = viewer;
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
            Id = (int)newId;
        }
        else
        {
            idCount++;
        }
        if (viewer != null)
        {
            AttachViewer(viewer);            
        }
    }
    ~Player()
    {
        if (_viewer != null)
        {
            DetachViewer(_viewer);            
        }

    }
    bool Build(int index)
    {
        if (index >= _townCardsDeck.Count)
        {
            return false;
        }
        ITownCard townCard = _townCardsDeck[index];
        if (Gold < townCard.Price)
        {
            return false;
        }
        _ownTown.Add(townCard);
        _townCardsDeck.Remove(townCard);
        Gold -= townCard.Price;
        return true;
    }
    private string _name = "Default Name";
    private bool _hasCrown = false;
    private int _goldCount = 0;
    private IPlayerCard? _playerCard = null;
    private List<ITownCard> _townCardsDeck = new List<ITownCard>();
    private List<ITownCard> _ownTown = new List<ITownCard>();
    private int _id = idCount;
    private IViewer? _viewer = null;
    protected string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
            if (OnViewerChangeName != null)
            {
                OnViewerChangeName(this, new ViewerChangeNameEventArgs() { newPlayerName = _name });
            }
        }
    }
    protected int Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = value;
        }
    }
    protected int Gold
    {
        get
        {
            return _goldCount;
        }
        set
        {
            _goldCount = value;
            if (_goldCount < 0) _goldCount = 0;
            if (OnViewerGoldUpdated != null)
            {
                OnViewerGoldUpdated(this, new ViewerGoldUpdatedEventArgs() { currentGoldOfPlayer = _goldCount });
            }
        }
    }
    protected bool Crown
    {
        get
        {
            return _hasCrown;
        }
        set
        {
            _hasCrown = value;
            if (OnViewerCoronationChange != null)
            {
                OnViewerCoronationChange(this, new ViewerCoronationChange() { hasCrown = _hasCrown });
            }
        }
    }
    protected bool BuildMaxBuildings
    {
        get
        {
            return _ownTown.Count >= Game.maxNumOfBuildingToWin;
        }
    }
    private void AttachViewer(IViewer viewer)
    {

        OnViewerAttach += viewer.OnAttach;
        OnViewerDetach += viewer.OnDettach;
        OnViewerChangeName += viewer.OnChangeName;
        OnViewerGoldUpdated += viewer.OnGoldUpdated;
        OnViewerCoronationChange += viewer.OnCoronationChange;
        OnViewerTownCardDeckUpdated += viewer.OnTownCardDeckUpdated;
        OnViewerBuildedTownUpdatedEventArgs += viewer.OnBuildedTownUpdated;
        if (OnViewerAttach != null)
        {
            OnViewerAttach(this, new ViewerAttachEventArgs() { playerId = Id, playerName = Name });
        }
    }
    private void DetachViewer(IViewer viewer)
    {
        if (OnViewerDetach != null)
        {
            OnViewerDetach(this, new ViewerDetachEventArgs() { });
        }
        OnViewerAttach -= viewer.OnAttach;
        OnViewerDetach -= viewer.OnDettach;
        OnViewerChangeName -= viewer.OnChangeName;
        OnViewerGoldUpdated -= viewer.OnGoldUpdated;
        OnViewerCoronationChange -= viewer.OnCoronationChange;
        OnViewerTownCardDeckUpdated -= viewer.OnTownCardDeckUpdated;
        OnViewerBuildedTownUpdatedEventArgs -= viewer.OnBuildedTownUpdated;
    }
    public int GetNumberOfTownCards()
    {
        return _townCardsDeck.Count;
    }
    protected void AppendTownCards(List<ITownCard> townCards)
    {
        _townCardsDeck.AddRange(townCards);
    }
    protected List<ITownCard> GetOwnCity()
    {
        return _ownTown.Select(s => (ITownCard)s.Clone()).ToList();
    }
    protected void AddGold(int gold)
    {
        Gold += gold;
    }
    protected void RemoveGold(int gold)
    {
        Gold -= gold;
    }
    public int GetId()
    {
        return Id;
    }
    public bool HasCrown()
    {
        return Crown;
    }
}