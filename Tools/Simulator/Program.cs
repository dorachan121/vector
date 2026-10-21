using System;
using System.Globalization;
using Vector.Core;
using Vector.Core.Alchemia;
using Vector.Core.Colosseum;
using Vector.Core.Crown;
using Vector.Core.Delve;
using Vector.Core.Derby;
using Vector.Core.Harpoon;
using Vector.Core.Hanabi;
using Vector.Core.Orbit;
using Vector.Core.Sugoroku;

namespace Vector.Simulator
{
    /// <summary>
    /// 配当表の検証ツール。
    /// <code>
    /// dotnet run -c Release -- bonus
    /// dotnet run -c Release -- main [rounds] [advisor|greedy|random] [seed] [mainPays=1:1]
    /// dotnet run -c Release -- crown [rounds] [advisor|greedy|random] [seed] [crownYachtBonus=5:1]
    /// dotnet run -c Release -- colosseum [rounds] [advisor|greedy|random] [seed] [playerHp:beastHp=30:42]
    /// dotnet run -c Release -- derby [races] [seed] [targetReturn=0.97]
    /// dotnet run -c Release -- delve [dives] [seed] [targetReturn=0.97]
    /// dotnet run -c Release -- alchemia [spins] [seed] [pays=tier3,tier4,tier5:transmutation:multipliers]
    /// dotnet run -c Release -- orbit [spins] [seed]
    /// dotnet run -c Release -- harpoon [shots] [seed]
    /// dotnet run -c Release -- sugoroku [games] [seed] [pays=hundredths by tier]
    /// dotnet run -c Release -- hanabi [launches] [seed]
    /// dotnet run -c Release -- challenge [trials] [seed]
    /// </code>
    /// </summary>
    static class Program
    {
        static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            string mode = args.Length > 0 ? args[0] : "main";
            try
            {
                switch (mode)
                {
                    case "bonus":
                        BonusCalculator.Run(TableRules.Standard);
                        return 0;
                    case "main":
                        var rules = args.Length > 4 ? new TableRules(ParseOdds(args[4])) : TableRules.Standard;
                        MainGameSimulation.Run(
                            rules,
                            rounds: args.Length > 1 ? int.Parse(args[1]) : 24_000,
                            strategy: args.Length > 2 ? ParseStrategy(args[2]) : PlayerStrategy.Advisor,
                            seed: args.Length > 3 ? ulong.Parse(args[3]) : 20261021UL);
                        return 0;
                    case "crown":
                        CrownSimulation.Run(
                            args.Length > 4 ? new CrownRules(ParseOdds(args[4])) : CrownRules.Standard,
                            rounds: args.Length > 1 ? int.Parse(args[1]) : 200_000,
                            strategy: args.Length > 2 ? ParseStrategy(args[2]) : PlayerStrategy.Advisor,
                            seed: args.Length > 3 ? ulong.Parse(args[3]) : 20261021UL);
                        return 0;
                    case "colosseum":
                        ColosseumSimulation.Run(
                            args.Length > 4 ? new BattleRules(ParseSetup(args[4])) : BattleRules.Standard,
                            rounds: args.Length > 1 ? int.Parse(args[1]) : 400_000,
                            strategy: args.Length > 2 ? ParseStrategy(args[2]) : PlayerStrategy.Advisor,
                            seed: args.Length > 3 ? ulong.Parse(args[3]) : 20261021UL);
                        return 0;
                    case "derby":
                        DerbySimulation.Run(
                            args.Length > 3 ? new DerbyRules(double.Parse(args[3])) : DerbyRules.Standard,
                            races: args.Length > 1 ? int.Parse(args[1]) : 100_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "delve":
                        DelveSimulation.Run(
                            args.Length > 3 ? new DelveRules(double.Parse(args[3])) : DelveRules.Standard,
                            rounds: args.Length > 1 ? int.Parse(args[1]) : 2_000_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "alchemia":
                        AlchemiaSimulation.Run(
                            args.Length > 3 ? AlchemiaSimulation.Parse(args[3]) : AlchemiaRules.Standard,
                            spins: args.Length > 1 ? long.Parse(args[1]) : 2_000_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "orbit":
                        OrbitSimulation.Run(
                            OrbitRules.Standard,
                            spins: args.Length > 1 ? int.Parse(args[1]) : 2_000_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "harpoon":
                        HarpoonSimulation.Run(
                            HarpoonRules.Standard,
                            shots: args.Length > 1 ? int.Parse(args[1]) : 2_000_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "sugoroku":
                        SugorokuSimulation.Run(
                            args.Length > 3 ? SugorokuSimulation.Parse(args[3]) : SugorokuRules.Standard,
                            games: args.Length > 1 ? int.Parse(args[1]) : 1_000_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "hanabi":
                        HanabiSimulation.Run(
                            HanabiRules.Standard,
                            launches: args.Length > 1 ? int.Parse(args[1]) : 2_000_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    case "challenge":
                        ChallengeSimulation.Run(
                            trials: args.Length > 1 ? int.Parse(args[1]) : 2_000,
                            seed: args.Length > 2 ? ulong.Parse(args[2]) : 20261021UL);
                        return 0;
                    default:
                        Console.Error.WriteLine($"unknown mode: {mode}");
                        return 1;
                }
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }

        static Odds ParseOdds(string text)
        {
            var parts = text.Split(':');
            if (parts.Length != 2)
                throw new FormatException($"odds must be like 5:4 (got {text})");
            return new Odds(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        static BattleSetup ParseSetup(string text)
        {
            var parts = text.Split(':');
            if (parts.Length != 2)
                throw new FormatException($"setup must be like 30:42 (got {text})");
            return new BattleSetup(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        static PlayerStrategy ParseStrategy(string text)
        {
            if (Enum.TryParse(text, true, out PlayerStrategy strategy))
                return strategy;
            throw new FormatException($"strategy must be advisor, greedy or random (got {text})");
        }
    }
}
