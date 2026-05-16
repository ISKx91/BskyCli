using System;
using System.Collections.Generic;
using System.Text;

namespace BskyCli.bsky.cli
{
    internal partial class Cli
    {
        internal class ConsoleCommon
        {
            /// <summary>
            /// 画面をクリアする
            /// </summary>
            internal static void ClearScreen() => Console.Clear();

            /// <summary>
            /// y/nでユーザーの確認を求めるメソッドです。
            /// ユーザーが「y」を入力した場合はtrueを返し、それ以外の場合はfalseを返します。
            /// </summary>
            /// <returns></returns>
            internal static bool Confirm()
            {
                Console.Write("(y/n) > ");
                var confirm = Console.ReadLine();
                return confirm!.Trim().ToLower() == "y";
            }

            /// <summary>
            /// パスワードの入力を受付けるメソッドです。入力された文字は画面に表示されません。
            /// </summary>
            /// <returns></returns>
            internal static string ReadPassword()
            {
                string password = string.Empty;
                ConsoleKeyInfo info = Console.ReadKey(true);
                while (info.Key != ConsoleKey.Enter)
                {
                    if (info.Key != ConsoleKey.Backspace)
                    {
                        password += info.KeyChar;
                    }
                    else if (info.Key == ConsoleKey.Backspace)
                    {
                        if (!string.IsNullOrEmpty(password))
                        {
                            password = password.Substring(0, password.Length - 1);
                        }
                    }
                    info = Console.ReadKey(true);
                }
                Console.WriteLine();
                return password;
            }

            /// <summary>
            /// 引数で与えられたテキストを編集するためのコンソールベースのテキストエディタです。
            /// ユーザーは矢印キーでカーソルを移動し、文字を入力、削除、改行することができます。
            /// 編集が完了したら、Escapeキーを押すことで編集を終了し、最終的なテキストが返されます。
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            internal static string ConsoleTextEdit(string text)
            {
                List<string> lines = text.Split(Environment.NewLine).ToList();
                int cursorLeft = 0; // カーソルの水平位置
                int cursorTop  = 0; // カーソルの垂直位置
                int viewTop    = 0; // 表示の最上行のインデックス

                Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                Console.CursorVisible = true;

                while (true)
                {
                    // 表示開始行を調整
                    if (cursorTop < viewTop)
                    {
                        viewTop = cursorTop;
                    }
                    else if (cursorTop >= viewTop + Console.WindowHeight)
                    {
                        viewTop = cursorTop - Console.WindowHeight + 1;
                    }
                    ClearScreen();
                    Draw(lines, viewTop); // テキスト表示

                    int drawTop = (cursorTop - viewTop) + 1/* ヘッダー考慮*/;
                    drawTop = Math.Clamp(drawTop, 0, Console.WindowHeight - 1);
                    int paddingLeft = DigitCount(lines.Count) + 1;
                    int drawLeft = paddingLeft + GetDisplayWidth(lines[cursorTop], cursorLeft);
                    drawLeft = Math.Clamp(drawLeft, 0, Console.WindowWidth - 1);

                    Console.SetCursorPosition(drawLeft, drawTop);

                    // キー入力を待機
                    var keyInfo = Console.ReadKey(true);

                    // Escapeキーで編集終了
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.LeftArrow:
                            if (cursorLeft > 0)
                            {
                                cursorLeft--;
                            }
                            break;

                        case ConsoleKey.RightArrow:
                            if (cursorLeft < lines[cursorTop].Length)
                            {
                                cursorLeft++;
                            }
                            break;

                        case ConsoleKey.UpArrow:
                            if (cursorTop > 0)
                            {
                                cursorTop--;
                                cursorLeft = Math.Min(cursorLeft, lines[cursorTop].Length);
                            }
                            break;

                        case ConsoleKey.DownArrow:
                            if (cursorTop < lines.Count - 1)
                            {
                                cursorTop++;
                                cursorLeft = Math.Min(cursorLeft, lines[cursorTop].Length);
                            }
                            break;

                        case ConsoleKey.Backspace:
                            if (cursorLeft > 0)
                            {
                                lines[cursorTop] = lines[cursorTop].Remove(cursorLeft - 1, 1);
                                cursorLeft--;
                            }
                            else if (cursorTop > 0)
                            {
                                int prevLen = lines[cursorTop - 1].Length; // 前の行の長さを保存
                                lines[cursorTop - 1] += lines[cursorTop]; // 前の行と現在の行を結合
                                lines.RemoveAt(cursorTop); // 現在の行を削除
                                cursorTop--;
                                cursorLeft = prevLen;
                            }
                            break;

                        case ConsoleKey.Delete:
                            if (cursorLeft < lines[cursorTop].Length)
                            {
                                lines[cursorTop] = lines[cursorTop].Remove(cursorLeft, 1);
                            }
                            else if (cursorTop < lines.Count - 1)
                            {
                                lines[cursorTop] += lines[cursorTop + 1]; // 現在の行と次の行を結合
                                lines.RemoveAt(cursorTop + 1); // 次の行を削除
                            }
                            break;

                        case ConsoleKey.Enter:
                            string newLine   = lines[cursorTop].Substring(cursorLeft);    // 現在の行のカーソル以降の部分を新しい行にする
                            lines[cursorTop] = lines[cursorTop].Substring(0, cursorLeft); // 現在の行をカーソルまでにする
                            lines.Insert(cursorTop + 1, newLine); // 新しい行を挿入
                            cursorTop++;
                            cursorLeft = 0;
                            break;

                        default:
                            if (!char.IsControl(keyInfo.KeyChar))
                            {
                                lines[cursorTop] = lines[cursorTop].Insert(cursorLeft, keyInfo.KeyChar.ToString());
                                cursorLeft++;
                            }
                            break;
                    }
                }

                Console.Clear();
                return string.Join(Environment.NewLine, lines);

                // テキストを画面に表示するヘルパーメソッド
                void Draw(List<string> lines, int startIdx)
                {
                    ConsoleInfo("< Enter your post. Press Esc to confirm. >");
                    int digitCount = DigitCount(lines.Count);
                    int endIdx     = Math.Min(startIdx + Console.WindowHeight - 1/*ヘッダー分1行引く*/, lines.Count);

                    for (int rowIdx = startIdx; rowIdx < endIdx; rowIdx++)
                    {
                        int rowNum = rowIdx + 1;
                        DrawLineNumber(rowNum, digitCount);
                        Console.Write(lines[rowIdx]);
                        // 最終行以外は改行
                        if (rowIdx + 1 < endIdx)
                        {
                            Console.WriteLine();
                        }
                    }
                }

                // 行番号を画面に表示するヘルパーメソッド
                void DrawLineNumber(int number, int width) => ConsoleWriteWithColor($"{number.ToString().PadLeft(width)} ", ConsoleColor.Yellow);
            }

            internal static void ConsoleInfo(string message) => ConsoleWriteWithColor(message, ConsoleColor.Cyan, true);

            internal static void ConsoleSuccess(string message) => ConsoleWriteWithColor(message, ConsoleColor.Green, true);

            internal static void ConsoleWarning(string message) => ConsoleWriteWithColor(message, ConsoleColor.Yellow, true);

            internal static void ConsoleError(string message) => ConsoleWriteWithColor(message, ConsoleColor.Red, true);

            private static void ConsoleWriteWithColor(string s, ConsoleColor color, bool newLine = false)
            {
                var original = Console.ForegroundColor;
                Console.ForegroundColor = color;
                if (newLine)
                {
                    Console.WriteLine(s);
                }
                else
                {
                    Console.Write(s);
                }
                Console.ForegroundColor = original;
            }

            // lengthで指定された文字数分の表示幅を計算するヘルパーメソッド
            private static int GetDisplayWidth(string s, int length)
            {
                int width = 0;
                for (int i = 0; i < length && i < s.Length; i++)
                {
                    width += GetCharWidth(s[i]);
                }
                return width;
            }

            // 文字の表示幅を計算するヘルパーメソッド。
            // 半角文字は1、全角文字は2を返します。
            private static int GetCharWidth(char c) => IsHankaku(c) ? 1 : 2;

            // 半角文字かどうかを判定するヘルパーメソッド。
            // ASCII文字と半角カタカナは半角とみなします。
            private static bool IsHankaku(char c) => c <= 0x007F || (0xFF61 <= c && c <= 0xFF9F);

            // 引数で与えられた整数nの桁数を計算するヘルパーメソッド。
            private static int DigitCount(int n) => (int)Math.Floor(Math.Log10(n)) + 1;
        }
    }
}
