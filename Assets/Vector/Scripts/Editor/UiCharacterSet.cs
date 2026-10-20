using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Vector.EditorTools
{
    /// <summary>
    /// 画面に出る可能性のある文字の一覧。ランタイムのソースにある文字列リテラルの文字と、印字可能な ASCII を集める。
    /// 日本語フォントの焼き込みと、その網羅を確かめるテストの両方で使う。
    /// </summary>
    public static class UiCharacterSet
    {
        public const string SourceFolder = "Assets/Vector/Scripts/Runtime";

        static readonly Regex stringLiteral = new Regex("\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

        public static string Collect()
        {
            var chars = new SortedSet<char>();
            for (char c = ' '; c <= '~'; c++)
                chars.Add(c);
            foreach (var file in Directory.GetFiles(SourceFolder, "*.cs", SearchOption.AllDirectories))
            {
                foreach (Match literal in stringLiteral.Matches(File.ReadAllText(file, Encoding.UTF8)))
                {
                    foreach (char c in literal.Value)
                    {
                        if (!char.IsControl(c) && !char.IsSurrogate(c))
                            chars.Add(c);
                    }
                }
            }
            return new string(chars.ToArray());
        }
    }
}
