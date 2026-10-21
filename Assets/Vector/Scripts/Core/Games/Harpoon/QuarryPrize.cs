namespace Vector.Core.Harpoon
{
    /// <summary>獲物を捕まえたときの払い戻しの倍率（賭け金を含む）と、その倍率が選ばれる重み。</summary>
    public readonly struct QuarryPrize
    {
        public readonly int Multiple;
        public readonly int Weight;

        public QuarryPrize(int multiple, int weight = 1)
        {
            Multiple = multiple;
            Weight = weight;
        }
    }
}
