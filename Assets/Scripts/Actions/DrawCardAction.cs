namespace STS.Actions
{
    public class DrawCardAction : ActionBase
    {
        private readonly int _count;

        public DrawCardAction(int count)
        {
            _count = count;
        }

        protected override void OnStart()
        {
            Context.Player.DrawCards(_count);
            Manager.NotifyStateChanged();
            SetDone();
        }
    }
}
