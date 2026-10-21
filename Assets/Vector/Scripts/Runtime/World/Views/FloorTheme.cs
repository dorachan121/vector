using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>フロアごとの色づかい（絨毯・壁・水・テーブルのフェルト）。</summary>
    public readonly struct FloorTheme
    {
        public readonly Color Ground;
        public readonly Color Motif;
        public readonly Color Rug;
        public readonly Color Carpet;
        public readonly Color Wall;
        public readonly Color Trim;
        public readonly Color Water;
        public readonly Color FeltCenter;
        public readonly Color FeltEdge;

        FloorTheme(string ground, string motif, string rug, string carpet, string wall, string trim, string water, Color feltCenter, Color feltEdge)
        {
            Ground = Theme.Hex(ground);
            Motif = Theme.Hex(motif);
            Rug = Theme.Hex(rug);
            Carpet = Theme.Hex(carpet);
            Wall = Theme.Hex(wall);
            Trim = Theme.Hex(trim);
            Water = Theme.Hex(water);
            FeltCenter = feltCenter;
            FeltEdge = feltEdge;
        }

        public static FloorTheme Of(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Lobby:
                    return new FloorTheme("5A1420", "B8862F", "3E2A14", "8E1B2C", "1C1210", "C9A24E", "2E7FB8", Theme.FeltCenter, Theme.FeltEdge);
                case FloorId.DiceParlor:
                    return new FloorTheme("142A52", "6E8FD6", "1E3B6E", "8E1B2C", "0D1426", "9DB6F0", "2E7FB8", Crown.CrownView.Felt, Crown.CrownView.FeltEdge);
                case FloorId.Arena:
                    return new FloorTheme("5C3A20", "A9773E", "6E4A2A", "7A1A1A", "24160D", "C79A4B", "3A2A1E", Colosseum.ColosseumView.Sand, Colosseum.ColosseumView.SandEdge);
                case FloorId.BeastTrack:
                    return new FloorTheme("173A22", "3F7A4B", "234E2E", "8E1B2C", "0C1A10", "C9A24E", "2F6B35", Derby.DerbyView.Turf, Derby.DerbyView.TurfEdge);
                case FloorId.AlchemyHall:
                    return new FloorTheme("24184C", "7B5BD6", "33236A", "6A1B5E", "0E0A22", "C79BFF", "7A3FD0", Alchemia.AlchemiaView.Felt, Alchemia.AlchemiaView.FeltEdge);
                case FloorId.Undervault:
                    return new FloorTheme("2E2A26", "4E473F", "3A342E", "4A2A1A", "121010", "8A7A60", "1E3A44", Delve.DelveView.Stone, Delve.DelveView.StoneEdge);
                case FloorId.Aquarium:
                    return new FloorTheme("0B2A3E", "1E6E8C", "0F3A52", "14506E", "041018", "34C6E0", "17608F", Harpoon.HarpoonView.Water, Harpoon.HarpoonView.Deep);
                case FloorId.PostTown:
                    return new FloorTheme("5E6A34", "8E9A4E", "6E5A36", "8E2A1E", "2A1A10", "C9A24E", "2E6E8C", Sugoroku.SugorokuView.Lacquer, Sugoroku.SugorokuView.LacquerEdge);
                case FloorId.Rooftop:
                    return new FloorTheme("1A1F3A", "3A4570", "2A2350", "8E2A1E", "0A0C18", "FF6FAE", "1E3A6E", Hanabi.HanabiView.Night, Hanabi.HanabiView.NightEdge);
                case FloorId.Observatory:
                    return new FloorTheme("10163A", "3E5AA8", "1A2458", "2A1A5E", "05060F", "7FE3FF", "1E4E8C", Orbit.OrbitView.SkyCenter, Orbit.OrbitView.SkyEdge);
                default:
                    return new FloorTheme("101014", "C9A24E", "2A2420", "1A1A20", "060608", "FFE7A3", "2E7FB8", Theme.Hex("3A0E16"), Theme.Hex("12040A"));
            }
        }
    }
}
