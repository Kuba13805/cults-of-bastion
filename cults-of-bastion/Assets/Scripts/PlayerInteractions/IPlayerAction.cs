namespace PlayerInteractions
{
    public interface IPlayerAction
    {
        public void Execute();
        public void Cancel();
        public void StopExecuting();
    }
}