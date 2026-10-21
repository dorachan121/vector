using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vector.Core;
using Vector.Core.Alchemia;
using Vector.Core.Colosseum;
using Vector.Core.Crown;
using Vector.Core.Delve;
using Vector.Core.Derby;
using Vector.Core.Hanabi;
using Vector.Core.Harpoon;
using Vector.Core.Orbit;
using Vector.Core.Rpg;
using Vector.Core.Sugoroku;

namespace Vector.Simulator
{
    /// <summary>
    /// フロアの主の挑戦が、どのくらいの確率で勝てるかを推定する。各ラウンドの打ち方は戦略ヒントと同じ強さ、
    /// 賭け方は「勝てば目標に届く額を賭け、届かないなら全額」（期待値が負のゲームで目標に届く確率を最大にする大胆な賭け方）。
    /// </summary>
    static class ChallengeSimulation
    {
        public static void Run(int trials, ulong seed)
        {
            Console.WriteLine($"CHALLENGES ({trials:N0} trials each, bold play)");
            foreach (var floor in Floors.All)
            {
                var spec = floor.Master;
                var watch = Stopwatch.StartNew();
                int wins = 0;
                var gate = new object();
                int workers = Math.Max(1, Environment.ProcessorCount);
                Parallel.For(0, workers, worker =>
                {
                    int share = trials / workers + (worker < trials % workers ? 1 : 0);
                    var player = new Bot(spec, new SeededRandom(seed + (ulong)worker * 7919UL + (ulong)floor.Id * 104729UL));
                    int local = 0;
                    for (int t = 0; t < share; t++)
                    {
                        if (player.Play())
                            local++;
                    }
                    lock (gate)
                        wins += local;
                });
                Console.WriteLine($"  {spec.Master,-6} {spec.Game,-10} {spec.Stack:N0} → {spec.Target:N0} in {spec.Rounds,2} rounds: win {wins / (double)trials:P1}  [{watch.ElapsedMilliseconds} ms]");
            }
        }

        /// <summary>1つの挑戦を最後まで打つ。</summary>
        sealed class Bot
        {
            readonly ChallengeSpec spec;
            readonly SeededRandom random;
            readonly RoundEngine vector;
            readonly PlayerAdvisor vectorAdvisor;
            readonly CrownEngine crown;
            readonly CrownAdvisor crownAdvisor = new CrownAdvisor(CrownRules.Standard);
            readonly BattleEngine battle;
            readonly BattleSolver solver;
            readonly DerbyEngine derby;
            readonly AlchemiaEngine alchemia;
            readonly DelveEngine delve;
            readonly OrbitEngine orbit;
            readonly HarpoonEngine harpoon;
            readonly SugorokuEngine sugoroku;
            readonly HanabiEngine hanabi;
            readonly SugorokuSolver sugorokuSolver = SugorokuSolver.Solve(SugorokuRules.Standard);

            public Bot(ChallengeSpec spec, SeededRandom random)
            {
                this.spec = spec;
                this.random = random;
                var rules = TableRules.Standard;
                vector = new RoundEngine(rules, new Shoe(rules.DeckCount, random), new HouseWay(new SeededRandom((ulong)random.Next(int.MaxValue)), rules.HouseWayPlayouts));
                vectorAdvisor = new PlayerAdvisor(new SeededRandom((ulong)random.Next(int.MaxValue)), rules, 60, 24);
                crown = new CrownEngine(CrownRules.Standard, random);
                battle = new BattleEngine(BattleRules.Standard, random);
                solver = BattleSolver.Solve(spec.Opponent ?? BattleRules.Standard.Setup);
                derby = new DerbyEngine(DerbyRules.Standard, random);
                alchemia = new AlchemiaEngine(AlchemiaRules.Standard, random);
                delve = new DelveEngine(DelveRules.Standard, random);
                orbit = new OrbitEngine(OrbitRules.Standard, random);
                harpoon = new HarpoonEngine(HarpoonRules.Standard, random);
                sugoroku = new SugorokuEngine(SugorokuRules.Standard, random);
                hanabi = new HanabiEngine(HanabiRules.Standard, random);
            }

            public bool Play()
            {
                var run = new ChallengeRun(spec);
                long minimum = Minimum();
                while (!run.IsOver)
                {
                    long stack = run.Bankroll.Balance;
                    long ret = Round(stack, run.Remaining);
                    run.Bankroll.TryDebit(Stake(stack, run.Remaining));
                    run.Bankroll.Credit(ret);
                    run.RecordRound(minimum);
                }
                return run.State == ChallengeState.Won;
            }

            long Minimum()
            {
                switch (spec.Game)
                {
                    case GameId.Derby: return DerbyRules.Standard.ExactaMinBet;
                    case GameId.Alchemia: return AlchemiaRules.Standard.BetLevels[0];
                    case GameId.Orbit: return OrbitRules.Standard.MinBet;
                    case GameId.Harpoon: return HarpoonRules.Standard.BetLevels[0];
                    case GameId.Sugoroku: return SugorokuRules.Standard.MinBet;
                    case GameId.Hanabi: return HanabiRules.Standard.MinBet;
                    default: return 10;
                }
            }

            /// <summary>大胆な賭け方の賭け金（勝てば目標に届く額。足りなければ全額）。</summary>
            long Stake(long stack, long remaining)
            {
                switch (spec.Game)
                {
                    case GameId.Alchemia:
                    {
                        // スロットは配当が読めないので、残りスピンで均等に使う
                        long per = Math.Max(stack / Math.Max(1, spec.Rounds / 4), AlchemiaRules.Standard.BetLevels[0]);
                        long best = AlchemiaRules.Standard.BetLevels[0];
                        foreach (long level in AlchemiaRules.Standard.BetLevels)
                        {
                            if (level <= Math.Min(per, stack))
                                best = level;
                        }
                        return best;
                    }
                    case GameId.Orbit:
                        return OrbitBet(stack, remaining).stake;
                    case GameId.Harpoon:
                        return HarpoonShot(stack, remaining).stake;
                    case GameId.Sugoroku:
                        return Math.Min(stack, SugorokuRules.Standard.MaxBet);
                    case GameId.Hanabi:
                        return Math.Min(stack, HanabiRules.Standard.MaxBet);
                    case GameId.Delve:
                    case GameId.Derby:
                        return Math.Min(stack, DelveRules.Standard.MaxBet);
                    default:
                        return Math.Min(stack, Math.Max(10, remaining));
                }
            }

            /// <summary>1ラウンドを打ち、払い戻しの総額を返す。</summary>
            long Round(long stack, long remaining)
            {
                long stake = Stake(stack, remaining);
                switch (spec.Game)
                {
                    case GameId.Vector: return PlayVector(stake);
                    case GameId.Crown: return PlayCrown(stake);
                    case GameId.Colosseum: return PlayBattle(stake);
                    case GameId.Derby: return PlayDerby(stake, stack + remaining);
                    case GameId.Alchemia: return PlayAlchemia(stake);
                    case GameId.Orbit: return PlayOrbit(stack, remaining);
                    case GameId.Harpoon: return PlayHarpoon(stack, remaining);
                    case GameId.Sugoroku: return PlaySugoroku(stake);
                    case GameId.Hanabi: return PlayHanabi(stake, remaining + stake);
                    default: return PlayDelve(stake, stack + remaining);
                }
            }

            long PlayVector(long stake)
            {
                var bets = new BetBoard();
                bets.Add((int)BetSpot.Main, stake);
                vector.BeginBetting();
                vector.Deal(GameMode.Player, bets);
                var estimates = vectorAdvisor.EstimateFamilies(vector.Board.ToValues(), vector.HandValues());
                vector.ChooseFamily(PlayerAdvisor.Best(estimates));
                while (vector.Phase != RoundPhase.Showdown)
                {
                    if (vector.Phase == RoundPhase.DealerTurn)
                    {
                        vector.PlayDealerCard();
                        continue;
                    }
                    var p = vectorAdvisor.SuggestPlacement(vector.Board.ToValues(), vector.HandValues(), vector.PlayerFamily);
                    vector.PlacePlayerCard(p.HandIndex, p.Cell);
                }
                return vector.Settle().TotalReturn;
            }

            long PlayCrown(long stake)
            {
                var bets = new BetBoard();
                bets.Add(CrownRules.Ante, stake);
                crown.BeginBetting();
                crown.Deal(bets);
                var plan = crownAdvisor.For(crown.Crown);
                while (crown.Phase == CrownPhase.PlayerTurn)
                {
                    var advice = plan.Advise(crown.PlayerDice, crown.PlayerRerollsLeft);
                    if (advice.Stand)
                        crown.Stand();
                    else
                        crown.Reroll(advice.HoldMask);
                }
                while (crown.Phase == CrownPhase.DealerTurn)
                    crown.PlayDealer();
                return crown.Settle().TotalReturn;
            }

            long PlayBattle(long stake)
            {
                var bets = new BetBoard();
                bets.Add(BattleRules.Main, stake);
                battle.BeginBetting();
                battle.Deal(bets, spec.Opponent);
                while (battle.Phase == BattlePhase.PlayerTurn)
                    battle.Act(solver.Best(battle.PlayerHp, battle.BeastHp, battle.Intent.Value));
                return battle.Settle().TotalReturn;
            }

            /// <summary>目標に届く倍率の単勝・馬単のうち、最も当たりやすい馬券に賭ける。</summary>
            long PlayDerby(long stake, long target)
            {
                if (derby.Phase == DerbyPhase.Complete)
                    derby.BeginBetting();
                var card = derby.Race;
                double need = target / (double)stake;
                int best = -1;
                for (int t = 0; t < DerbyTickets.Count; t++)
                {
                    if (!DerbyTickets.IsValid(t) || card.OddsTenths(t) == 0 || card.OddsTenths(t) / 10.0 < need)
                        continue;
                    if (best < 0 || card.Probabilities.Of(t) > card.Probabilities.Of(best))
                        best = t;
                }
                if (best < 0)
                    best = DerbyTickets.Exacta(0, 1);
                long max = card.MaxFor(best);
                var bets = new BetBoard();
                bets.Add(best, Math.Min(stake, max));
                derby.Start(bets);
                while (derby.Phase == DerbyPhase.Running)
                    derby.Step();
                long ret = derby.Settle().TotalReturn;
                return ret + Math.Max(0, stake - max);
            }

            long PlayAlchemia(long stake)
            {
                var bets = new BetBoard();
                bets.Add(AlchemiaRules.Spin, stake);
                return alchemia.Spin(bets).TotalReturn;
            }

            /// <summary>当たりやすい順の ORBIT の賭け先（天地 1:1 → 方角 3:1 → 内惑星 5:1 → 合 → 彗星 → 直列）。</summary>
            static readonly int[] OrbitLadder =
            {
                OrbitSpots.Half(0), OrbitSpots.Quadrant(0), OrbitSpots.Inner(0),
                OrbitSpots.InnerConjunction, OrbitSpots.OuterConjunction, OrbitSpots.Comet, OrbitSpots.Alignment,
            };

            /// <summary>当たれば目標に届く賭け先のうち、最も当たりやすいものと賭け金。届くものが無ければ天地に全額。</summary>
            static (int spot, long stake) OrbitBet(long stack, long remaining)
            {
                var rules = OrbitRules.Standard;
                foreach (int spot in OrbitLadder)
                {
                    var odds = rules.PaysFor(spot);
                    long stake = Math.Max(rules.MinBet, (remaining * odds.Per + odds.Win - 1) / odds.Win);
                    if (stake <= stack && stake <= rules.MaxFor(spot))
                        return (spot, stake);
                }
                return (OrbitLadder[0], Math.Min(stack, rules.MaxFor(OrbitLadder[0])));
            }

            long PlayOrbit(long stack, long remaining)
            {
                var (spot, stake) = OrbitBet(stack, remaining);
                var bets = new BetBoard();
                bets.Add(spot, stake);
                if (orbit.Phase == OrbitPhase.Complete)
                    orbit.BeginBetting();
                orbit.Spin(bets);
                return orbit.Settle().TotalReturn;
            }

            /// <summary>
            /// 捕まえれば目標に届く獲物のうち最も捕まえやすいものと、届く最小の BET の段階。
            /// 倍率の決まっている獲物は番号の順に倍率が大きい（金のクラゲは倍率が読めないので狙わない）。
            /// </summary>
            static (int quarry, long stake) HarpoonShot(long stack, long remaining)
            {
                var rules = HarpoonRules.Standard;
                for (int q = 0; q < Quarry.Count; q++)
                {
                    if (rules.IsMystery(q))
                        continue;
                    long net = rules.MinMultiple(q) - 1;
                    foreach (long level in rules.BetLevels)
                    {
                        if (level <= stack && level * net >= remaining)
                            return (q, level);
                    }
                }
                long most = rules.BetLevels[0];
                foreach (long level in rules.BetLevels)
                {
                    if (level <= stack)
                        most = level;
                }
                return (Quarry.Leviathan, most);
            }

            long PlayHarpoon(long stack, long remaining)
            {
                var (quarry, stake) = HarpoonShot(stack, remaining);
                var bets = new BetBoard();
                bets.Add(quarry, stake);
                return harpoon.Fire(bets).TotalReturn;
            }

            /// <summary>上限まで賭け、厳密な最善手（期待値が最大の目）で上りを目指す。</summary>
            long PlaySugoroku(long stake)
            {
                if (sugoroku.Phase == SugorokuPhase.Complete)
                    sugoroku.BeginBetting();
                var bets = new BetBoard();
                bets.Add(SugorokuRules.Main, stake);
                sugoroku.Start(bets);
                while (sugoroku.Phase == SugorokuPhase.Choosing)
                    sugoroku.Choose(sugorokuSolver.Best(sugoroku.TurnsLeft, sugoroku.Position, sugoroku.Tier, sugoroku.Dice.first, sugoroku.Dice.second));
                return sugoroku.Settle().TotalReturn;
            }

            /// <summary>上限まで賭け、払い戻しが needed に届く倍率で開く（届かない倍率なら上限の倍率）。</summary>
            long PlayHanabi(long stake, long needed)
            {
                var rules = HanabiRules.Standard;
                if (hanabi.Phase == HanabiPhase.Complete)
                    hanabi.BeginBetting();
                var bets = new BetBoard();
                bets.Add(HanabiRules.Main, stake);
                hanabi.Launch(bets);
                int target = (int)Math.Max(rules.MinCashOut, Math.Min(rules.MaxHundredths, (needed * HanabiRules.One + stake - 1) / stake));
                if (hanabi.HasBurstAt(target))
                    hanabi.Burst();
                else
                    hanabi.CashOut(target);
                return hanabi.Settle().TotalReturn;
            }

            /// <summary>目標に届く最も浅い深さまで潜って引き返す。</summary>
            long PlayDelve(long stake, long target)
            {
                var rules = DelveRules.Standard;
                int depth = rules.Depths;
                for (int d = 1; d <= rules.Depths; d++)
                {
                    if (stake * rules.MultiplierHundredths(d) / 100 >= target)
                    {
                        depth = d;
                        break;
                    }
                }
                var bets = new BetBoard();
                bets.Add(DelveRules.Main, stake);
                delve.BeginBetting();
                delve.Deal(bets);
                while (delve.Phase == DelvePhase.Choosing)
                {
                    if (delve.ClearedDepth >= depth)
                    {
                        delve.CashOut();
                        break;
                    }
                    delve.Open(random.Next(rules.Floors[delve.ClearedDepth].Doors));
                }
                return delve.Settle().TotalReturn;
            }
        }
    }
}
