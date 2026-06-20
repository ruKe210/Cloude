namespace STS.Actions
{
    public class WaitAction : ActionBase
    {
        private float _remaining;

        public WaitAction(float seconds)
        {
            _remaining = seconds;
        }

        protected override void OnStart()
        {
            if (_remaining <= 0f)
            {
                SetDone();
            }
        }

        public override void Tick(float deltaTime)
        {
            if (IsDone)
            {
                return;
            }

            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                SetDone();
            }
        }
    }
}
