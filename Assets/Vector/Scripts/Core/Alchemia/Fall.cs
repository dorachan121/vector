namespace Vector.Core.Alchemia
{
    /// <summary>重力でシンボルが落ちる動き（同じ列の上のマスから下のマスへ）。</summary>
    public readonly struct Fall
    {
        public readonly int From;
        public readonly int To;

        public Fall(int from, int to)
        {
            From = from;
            To = to;
        }
    }
}
