namespace STS.Actions
{
    public class EndEnemyTurnAction : ActionBase
    {
        protected override void OnStart()
        {
            Manager.CompleteEnemyTurn();
            SetDone();
        }
    }
}
