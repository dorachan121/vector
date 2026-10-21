namespace Vector.Core
{
    /// <summary>3マスからなる1本のライン。</summary>
    public readonly struct Line
    {
        public readonly LineFamily Family;
        public readonly int Index;
        public readonly int A;
        public readonly int B;
        public readonly int C;

        public Line(LineFamily family, int index, int a, int b, int c)
        {
            Family = family;
            Index = index;
            A = a;
            B = b;
            C = c;
        }

        public int Sum(int[] values) => values[A] + values[B] + values[C];

        public override string ToString() => $"{Family}{Index}({A},{B},{C})";
    }
}
