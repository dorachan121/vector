using System;

namespace Vector.Core
{
    /// <summary>3×3の盤面。カードと、それを置いた席を保持する。</summary>
    public sealed class Board
    {
        readonly Card?[] cells = new Card?[Lines.CellCount];
        readonly Seat[] owners = new Seat[Lines.CellCount];

        public Card? this[int cell] => cells[CheckCell(cell)];

        public Seat OwnerOf(int cell) => owners[CheckCell(cell)];

        public bool IsEmpty(int cell) => !cells[CheckCell(cell)].HasValue;

        public bool IsFull => Array.TrueForAll(cells, c => c.HasValue);

        public void Place(int cell, Card card, Seat by)
        {
            if (!IsEmpty(cell))
                throw new InvalidOperationException($"セル{cell}には既にカードがあります。");
            if (by == Seat.None)
                throw new ArgumentException("置いた席を指定してください。", nameof(by));
            cells[cell] = card;
            owners[cell] = by;
        }

        public void Clear()
        {
            Array.Clear(cells, 0, cells.Length);
            Array.Clear(owners, 0, owners.Length);
        }

        /// <summary>判定・戦略計算用に点数だけを取り出す（空きマスは0）。</summary>
        public int[] ToValues()
        {
            var values = new int[Lines.CellCount];
            for (int i = 0; i < values.Length; i++)
                values[i] = cells[i]?.Value ?? 0;
            return values;
        }

        static int CheckCell(int cell)
        {
            if (cell < 0 || cell >= Lines.CellCount)
                throw new ArgumentOutOfRangeException(nameof(cell));
            return cell;
        }
    }
}
