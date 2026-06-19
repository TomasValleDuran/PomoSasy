namespace Attack
{
    public readonly struct AttackExecutionResult
    {
        public AttackExecutionResult(bool finished, bool hit, bool? playSfx = null)
        {
            Finished = finished;
            Hit = hit;
            PlaySfx = playSfx ?? hit;
        }

        public bool Finished { get; }
        public bool Hit { get; }
        public bool PlaySfx { get; }
    }
}
