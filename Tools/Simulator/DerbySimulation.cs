using System;
using System.Diagnostics;
using Vector.Core;
using Vector.Core.Derby;

namespace Vector.Simulator
{
    /// <summary>
    /// FOUR BEASTS DERBY の検証。抽選されうる全ての出走表（距離 × ゲートの組み合わせ）について馬券ごとの厳密な RTP を求め、
    /// 実際の DerbyEngine でレースを繰り返して、着順の確率が厳密計算と食い違わないことを確かめる。
    /// </summary>
    static class DerbySimulation
    {
        public static void Run(DerbyRules rules, int races, ulong seed)
        {
            var watch = Stopwatch.StartNew();
            double winSum = 0, exactaSum = 0, winMin = 1, winMax = 0, exactaMin = 1, exactaMax = 0;
            int winOffered = 0, exactaOffered = 0, winClosed = 0, exactaClosed = 0, cards = 0;
            double bestPrice = 0;
            var gates = new int[DerbyTickets.Runners];
            for (int distance = rules.MinDistance; distance <= rules.MaxDistance; distance++)
            {
                int combos = 1;
                for (int r = 0; r < gates.Length; r++)
                    combos *= rules.MaxGate + 1;
                for (int code = 0; code < combos; code++)
                {
                    int x = code;
                    for (int r = 0; r < gates.Length; r++)
                    {
                        gates[r] = x % (rules.MaxGate + 1);
                        x /= rules.MaxGate + 1;
                    }
                    var card = new RaceCard(rules, distance, gates);
                    cards++;
                    for (int t = 0; t < DerbyTickets.Count; t++)
                    {
                        if (!DerbyTickets.IsValid(t))
                            continue;
                        bool win = DerbyTickets.IsWin(t);
                        if (card.OddsTenths(t) == 0)
                        {
                            if (win) winClosed++;
                            else exactaClosed++;
                            continue;
                        }
                        double rtp = card.ReturnOf(t);
                        bestPrice = Math.Max(bestPrice, card.OddsTenths(t) / 10.0);
                        if (win)
                        {
                            winSum += rtp;
                            winOffered++;
                            winMin = Math.Min(winMin, rtp);
                            winMax = Math.Max(winMax, rtp);
                        }
                        else
                        {
                            exactaSum += rtp;
                            exactaOffered++;
                            exactaMin = Math.Min(exactaMin, rtp);
                            exactaMax = Math.Max(exactaMax, rtp);
                        }
                    }
                }
            }
            Console.WriteLine($"FOUR BEASTS DERBY (target return {rules.TargetReturn:P1}, {cards:N0} possible race cards, {watch.ElapsedMilliseconds} ms)");
            Console.WriteLine($"  WIN    average RTP {winSum / winOffered:P3}  (min {winMin:P2}, max {winMax:P2}, closed {winClosed / (double)(winOffered + winClosed):P2})");
            Console.WriteLine($"  EXACTA average RTP {exactaSum / exactaOffered:P3}  (min {exactaMin:P2}, max {exactaMax:P2}, closed {exactaClosed / (double)(exactaOffered + exactaClosed):P2})");
            Console.WriteLine($"  longest odds offered {bestPrice:F1}x");

            if (races <= 0)
                return;
            watch.Restart();
            var random = new SeededRandom(seed);
            var engine = new DerbyEngine(rules, random);
            double expectedWin = 0, observedWin = 0, expectedExacta = 0, observedExacta = 0, maxError = 0;
            for (int i = 0; i < races; i++)
            {
                if (i > 0)
                    engine.BeginBetting();
                var bets = new BetBoard();
                bets.Add(DerbyTickets.Win(0), rules.WinMinBet);
                engine.Start(bets);
                while (engine.Phase == DerbyPhase.Running)
                    engine.Step();
                var result = engine.Settle();
                // 走者0の単勝と、0→1 の馬単の的中率を厳密値と比べる
                expectedWin += result.Card.Probabilities.Win[0];
                observedWin += result.First == 0 ? 1 : 0;
                expectedExacta += result.Card.Probabilities.Exacta[0, 1];
                observedExacta += result.First == 0 && result.Second == 1 ? 1 : 0;
            }
            maxError = Math.Max(Math.Abs(expectedWin - observedWin), Math.Abs(expectedExacta - observedExacta)) / races;
            Console.WriteLine($"  simulation ({races:N0} races): WIN#0 expected {expectedWin / races:P3} observed {observedWin / races:P3}  " +
                              $"EXACTA 0-1 expected {expectedExacta / races:P3} observed {observedExacta / races:P3}  [{watch.ElapsedMilliseconds} ms]");
        }
    }
}
