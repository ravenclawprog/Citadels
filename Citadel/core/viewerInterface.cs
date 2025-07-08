namespace Citadel
{
    public interface IViewer
    {
        public void OnAttach(object? sender, Player.ViewerAttachEventArgs e);
        public void OnDettach(object? sender, Player.ViewerDetachEventArgs e);
        public void OnChangeName(object? sender, Player.ViewerChangeNameEventArgs e);
        public void OnGoldUpdated(object? sender, Player.ViewerGoldUpdatedEventArgs e);
        public void OnCoronationChange(object? sender, Player.ViewerCoronationChange e);
        public void OnTownCardDeckUpdated(object? sender, Player.ViewerTownCardDeckUpdatedEventArgs e);
        public void OnBuildedTownUpdated(object? sender, Player.ViewerBuildedTownUpdatedEventArgs e);
    }
}