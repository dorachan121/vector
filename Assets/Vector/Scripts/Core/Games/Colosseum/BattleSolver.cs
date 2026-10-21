using System;

namespace Vector.Core.Colosseum
{
    /// <summary>
    /// 最善の打ち方（厳密解）。状態は「自分の体力・獣の体力・獣の予告（A〜K）」だけなので、
    /// 体力の小さい局面から順に、勝つ確率を最大にする手を求める。相殺（何も起きない）で同じ局面に戻る分は不動点反復で解く。
    /// 戦略ヒント・時間切れの自動進行・RTP の検証に使う。作成後は読み取り専用でスレッド安全。
    /// </summary>
    public sealed class BattleSolver
    {
        const int Values = BattleRules.CardValues;

        readonly BattleSetup setup;
        readonly double[,] start;
        readonly double[,,] strike;
        readonly double[,,] guard;

        BattleSolver(BattleSetup setup)
        {
            this.setup = setup;
            start = new double[setup.PlayerHp + 1, setup.BeastHp + 1];
            strike = new double[setup.PlayerHp + 1, setup.BeastHp + 1, Values + 1];
            guard = new double[setup.PlayerHp + 1, setup.BeastHp + 1, Values + 1];
        }

        public BattleSetup Setup => setup;

        /// <summary>最初から最善に打ったときの勝率。</summary>
        public double WinProbability => start[setup.PlayerHp, setup.BeastHp];

        public static BattleSolver Solve(BattleSetup setup)
        {
            var solver = new BattleSolver(setup);
            for (int p = 1; p <= setup.PlayerHp; p++)
            {
                for (int b = 1; b <= setup.BeastHp; b++)
                    solver.SolveState(p, b);
            }
            return solver;
        }

        /// <summary>この局面（予告が出る前）から最善に打ったときの勝率。</summary>
        public double WinProbabilityAt(int playerHp, int beastHp)
        {
            if (beastHp <= 0)
                return 1;
            if (playerHp <= 0)
                return 0;
            return start[Clamp(playerHp, setup.PlayerHp), Clamp(beastHp, setup.BeastHp)];
        }

        /// <summary>予告を見たうえで、その手を選んだときの勝率。</summary>
        public double ValueOf(BattleAction action, int playerHp, int beastHp, int intentValue)
        {
            if (intentValue < 1 || intentValue > Values)
                throw new ArgumentOutOfRangeException(nameof(intentValue));
            int p = Clamp(playerHp, setup.PlayerHp), b = Clamp(beastHp, setup.BeastHp);
            return action == BattleAction.Strike ? strike[p, b, intentValue] : guard[p, b, intentValue];
        }

        /// <summary>勝率の高い方の手。同じなら斬る。</summary>
        public BattleAction Best(int playerHp, int beastHp, int intentValue)
            => ValueOf(BattleAction.Guard, playerHp, beastHp, intentValue) > ValueOf(BattleAction.Strike, playerHp, beastHp, intentValue) + 1e-12
                ? BattleAction.Guard
                : BattleAction.Strike;

        void SolveState(int p, int b)
        {
            double w = 0.5;
            for (int iteration = 0; iteration < 200; iteration++)
            {
                double sum = 0;
                for (int intent = 1; intent <= Values; intent++)
                {
                    int damage = setup.IntentDamage(intent, b);
                    double s = 0, g = 0;
                    for (int x = 1; x <= Values; x++)
                    {
                        s += Outcome(p - damage, b - BattleRules.StrikeDamage(x));
                        if (x > damage)
                            g += Outcome(p, b - (x - damage));
                        else if (x < damage)
                            g += Outcome(p - (damage - x), b);
                        else
                            g += w;
                    }
                    strike[p, b, intent] = s / Values;
                    guard[p, b, intent] = g / Values;
                    sum += Math.Max(s, g) / Values;
                }
                double next = sum / Values;
                bool converged = Math.Abs(next - w) < 1e-15;
                w = next;
                if (converged)
                    break;
            }
            start[p, b] = w;
        }

        /// <summary>1手の後の勝率。獣が倒れていればその時点で勝ち（斬って倒した場合、獣の一撃は受けない）。</summary>
        double Outcome(int playerHp, int beastHp)
        {
            if (beastHp <= 0)
                return 1;
            if (playerHp <= 0)
                return 0;
            return start[playerHp, beastHp];
        }

        static int Clamp(int hp, int max) => Math.Max(1, Math.Min(hp, max));
    }
}
