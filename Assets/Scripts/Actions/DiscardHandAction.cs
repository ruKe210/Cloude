namespace STS.Actions
{
    public class DiscardHandAction : ActionBase
    {
        protected override void OnStart()
        {
            Context.Player.DiscardHand();
            Manager.NotifyStateChanged();
            SetDone();
        }
    }
}
