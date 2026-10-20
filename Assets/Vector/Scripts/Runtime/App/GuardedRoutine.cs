using System;
using System.Collections;
using System.Collections.Generic;

namespace Vector.Game
{
    /// <summary>
    /// 入れ子のコルーチンを1本に平らにして実行し、どこで例外が起きても捕まえて通知する。
    /// Unity のコルーチンは例外で黙って止まるため、ゲームの進行が固まらないようにこれで包む。
    /// </summary>
    public static class GuardedRoutine
    {
        public static IEnumerator Run(IEnumerator routine, Action<Exception> onError)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(routine);
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                bool moved;
                try
                {
                    moved = top.MoveNext();
                }
                catch (Exception e)
                {
                    onError(e);
                    yield break;
                }
                if (!moved)
                {
                    stack.Pop();
                    continue;
                }
                if (top.Current is IEnumerator nested)
                {
                    stack.Push(nested);
                    continue;
                }
                yield return top.Current;
            }
        }
    }
}
