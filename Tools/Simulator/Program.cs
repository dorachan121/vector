using System;
using System.Globalization;
using Vector.Core;

namespace Vector.Simulator
{
    /// <summary>
    /// 配当表の検証ツール。
    /// <code>
    /// dotnet run -c Release -- bonus
    /// dotnet run -c Release -- main [rounds] [advisor|greedy|random] [seed] [mainPays=1:1]
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

        static PlayerStrategy ParseStrategy(string text)
        {
            if (Enum.TryParse(text, true, out PlayerStrategy strategy))
                return strategy;
            throw new FormatException($"strategy must be advisor, greedy or random (got {text})");
        }
    }
}
