namespace BskyCli.bsky.cli
{
    internal partial class Cli
    {
        /// <summary>
        /// ConsoleCommonクラスは、CLIアプリケーションで共通して使用されるコンソール操作に関連するユーティリティメソッドを提供するためのクラスです。
        /// </summary>
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
                int digitWidth = DigitCount(lines.Count);

                if (OperatingSystem.IsWindows())
                {
                    Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                }
                Console.CursorVisible = true;

                while (true)
                {
                    ClearScreen();                    // 画面クリア
                    UpdateViewTop();                  // 表示開始行を調整
                    Draw(lines, viewTop, digitWidth); // 画面描画

                    // カーソル位置計算
                    var (drawLeft, drawTop) = CalculateCursorPosition();
                    Console.SetCursorPosition(drawLeft, drawTop);

                    // キー入力を待機
                    var keyInfo = Console.ReadKey(true);

                    // EscapeキーまたはCtrl + Enterで編集終了
                    if (ShouldExit(keyInfo))
                    {
                        break;
                    }

                    HandleKey(keyInfo);
                }

                ClearScreen();
                return string.Join(Environment.NewLine, lines);

                // -------------------------
                // 位置・スクロール計算
                // -------------------------

                // 行数が増えて行番号の桁数が変わる可能性があるため、
                // 必要に応じて行番号の幅を更新するヘルパーメソッド
                void UpdateDigitWidthIfNeeded()
                {
                    int newWidth = DigitCount(lines.Count);
                    if (newWidth != digitWidth)
                    {
                        digitWidth = newWidth;
                    }
                }

                // カーソルの垂直位置に応じて表示開始行を調整するヘルパーメソッド
                void UpdateViewTop()
                {
                    if (cursorTop < viewTop)
                    {
                        viewTop = cursorTop;
                    }
                    else if (cursorTop >= viewTop + Console.WindowHeight)
                    {
                        viewTop = cursorTop - Console.WindowHeight + 1;
                    }
                }

                // カーソルの位置に応じて、画面上の描画位置を計算するヘルパーメソッド
                (int left, int top) CalculateCursorPosition()
                {
                    int drawTop = (cursorTop - viewTop) + 1/* ヘッダー考慮*/;
                    drawTop = Math.Clamp(drawTop, 0, Console.WindowHeight - 1);
                    int rowNumWidth = DigitCount(lines.Count) + 1;
                    int drawLeft = rowNumWidth + GetDisplayWidth(lines[cursorTop], cursorLeft);
                    drawLeft = Math.Clamp(drawLeft, 0, Console.WindowWidth - 1);
                    return (drawLeft, drawTop);
                }

                // -------------------------
                // 終了判定
                // -------------------------

                // EscapeキーまたはCtrl + Enterで編集終了するかどうかを判定するヘルパーメソッド
                bool ShouldExit(ConsoleKeyInfo keyInfo)
                {
                    bool isEscape    = keyInfo.Key == ConsoleKey.Escape;
                    bool isCtrlEnter = keyInfo.Key == ConsoleKey.Enter && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
                    return isEscape || isCtrlEnter;
                }

                // -------------------------
                // キー処理
                // -------------------------

                // キー入力に応じてカーソル移動や編集処理を行うヘルパーメソッド
                void HandleKey(ConsoleKeyInfo keyInfo)
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.LeftArrow:  MoveCursorLeft();    break;
                        case ConsoleKey.RightArrow: MoveCursorRight();   break;
                        case ConsoleKey.UpArrow:    MoveCursorUp();      break;
                        case ConsoleKey.DownArrow:  MoveCursorDown();    break;
                        case ConsoleKey.Backspace:  EditBackspace();     break;
                        case ConsoleKey.Delete:     EditDelete();        break;
                        case ConsoleKey.Enter:      EditEnter();         break;
                        default:                    InsertChar(keyInfo); break;
                    }
                }

                // -------------------------
                // カーソル移動
                // -------------------------

                // 左矢印キーでカーソルを移動するヘルパーメソッド
                void MoveCursorLeft()
                {
                    if (cursorLeft <= 0) return;
                    cursorLeft--;
                }

                // 右矢印キーでカーソルを移動するヘルパーメソッド
                void MoveCursorRight()
                {
                    if (cursorLeft == lines[cursorTop].Length) return;
                    cursorLeft++;
                }

                // 上矢印キーでカーソルを移動するヘルパーメソッド
                void MoveCursorUp()
                {
                    if (cursorTop == 0) return;
                    cursorTop--;
                    cursorLeft = Math.Min(cursorLeft, lines[cursorTop].Length);
                }

                // 下矢印キーでカーソルを移動するヘルパーメソッド
                void MoveCursorDown()
                {
                    if (cursorTop >= lines.Count - 1) return;
                    cursorTop++;
                    cursorLeft = Math.Min(cursorLeft, lines[cursorTop].Length);
                }

                // -------------------------
                // 編集処理
                // -------------------------

                // バックスペースキーで文字を削除するヘルパーメソッド
                void EditBackspace()
                {
                    // 何も削除できない
                    if (cursorLeft == 0 && cursorTop == 0)
                    {
                        return;
                    }
                    // 同じ行で削除
                    if (cursorLeft > 0)
                    {
                        lines[cursorTop] = lines[cursorTop].Remove(cursorLeft - 1, 1);
                        cursorLeft--;
                        return;
                    }
                    // 行結合（前の行と現在の行を結合し、現在の行を削除）
                    int prevLen = lines[cursorTop - 1].Length; // 前の行の長さを保存
                    MergeLineWithNext(cursorTop - 1, cursorTop);
                    UpdateDigitWidthIfNeeded();
                    cursorTop--;
                    cursorLeft = prevLen;
                }

                // Deleteキーで文字を削除するヘルパーメソッド
                void EditDelete()
                {
                    // 何も削除できない
                    if (cursorLeft == lines[cursorTop].Length && cursorTop == lines.Count - 1)
                    {
                        return;
                    }
                    // 同じ行で削除
                    if (cursorLeft < lines[cursorTop].Length)
                    {
                        lines[cursorTop] = lines[cursorTop].Remove(cursorLeft, 1);
                        return;
                    }
                    // 行結合（現在の行と次の行を結合し、次の行を削除）
                    MergeLineWithNext(cursorTop, cursorTop + 1);
                    UpdateDigitWidthIfNeeded();
                }

                // Enterキーで改行するヘルパーメソッド
                void EditEnter()
                {
                    string newLine = lines[cursorTop].Substring(cursorLeft);    // 現在の行のカーソル以降の部分を新しい行にする
                    lines[cursorTop] = lines[cursorTop].Substring(0, cursorLeft); // 現在の行をカーソルまでにする
                    lines.Insert(cursorTop + 1, newLine); // 新しい行を挿入
                    UpdateDigitWidthIfNeeded();
                    cursorTop++;
                    cursorLeft = 0;
                }

                // 文字を挿入するヘルパーメソッド
                void InsertChar(ConsoleKeyInfo keyInfo)
                {
                    // 制御文字なら何もしない
                    if (char.IsControl(keyInfo.KeyChar)) return;
                    lines[cursorTop] = lines[cursorTop].Insert(cursorLeft, keyInfo.KeyChar.ToString());
                    cursorLeft++;
                }

                // 2行を結合するヘルパーメソッド
                void MergeLineWithNext(int baseIndex, int removeIndex)
                {
                    lines[baseIndex] += lines[removeIndex];
                    lines.RemoveAt(removeIndex);
                }

                // -------------------------
                // 描画処理
                // -------------------------

                // テキストを画面に表示するヘルパーメソッド
                void Draw(List<string> lines, int startIdx, int digitWidth)
                {
                    DrawHeader();
                    int endIdx     = Math.Min(startIdx + Console.WindowHeight - 1/*ヘッダー分1行引く*/, lines.Count);

                    for (int rowIdx = startIdx; rowIdx < endIdx; rowIdx++)
                    {
                        DrawLine(lines[rowIdx], rowIdx + 1, digitWidth);

                        // 最終行以外は改行
                        if (rowIdx + 1 < endIdx)
                        {
                            Console.WriteLine();
                        }
                    }
                }

                // ヘッダーを表示するする
                void DrawHeader() => ConsoleInfo("< Write your post. Press Esc or Ctrl + Enter to confirm. >");

                // 行を表示する
                void DrawLine(string text, int rowNumber, int digitWidth)
                {
                    DrawLineNumber(rowNumber, digitWidth);
                    Console.Write(text);
                }

                // 行番号を画面に表示する
                void DrawLineNumber(int number, int width) => ConsoleWriteWithColor($"{number.ToString().PadLeft(width)} ", ConsoleColor.Yellow);
            }

            /// <summary>
            /// CenterTextメソッドは、与えられたテキストを指定された幅の中で中央に配置するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="text"></param>
            /// <param name="width"></param>
            /// <returns></returns>
            internal static string CenterText(string text, int width)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return new string(' ', width);
                }
                // 幅が足りない場合はそのまま返す
                if (text.Length >= width)
                {
                    return text;
                }
                int totalPadding = width - text.Length;
                int leftPadding  = totalPadding / 2;
                int rightPadding = totalPadding - leftPadding;

                return new string(' ', leftPadding) + text + new string(' ', rightPadding);
            }

            /// <summary>
            /// PrintLineメソッドは、指定された文字を指定された長さだけ繰り返して表示するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="c"></param>
            /// <param name="length"></param>
            internal static void PrintLine(char c = '=', int length = 60)
            {
                ConsoleInfo(new string(c, length));
            }

            /// <summary>
            /// ConsoleInfoメソッドは、引数で与えられたメッセージをシアン色でコンソールに表示するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="message"></param>
            internal static void ConsoleInfo(string message) => ConsoleWriteWithColor(message, ConsoleColor.Cyan, true);

            /// <summary>
            /// ConsoleSuccessメソッドは、引数で与えられたメッセージを緑色でコンソールに表示するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="message"></param>
            internal static void ConsoleSuccess(string message) => ConsoleWriteWithColor(message, ConsoleColor.Green, true);

            /// <summary>
            /// ConsoleWarningメソッドは、引数で与えられたメッセージを黄色でコンソールに表示するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="message"></param>
            internal static void ConsoleWarning(string message) => ConsoleWriteWithColor(message, ConsoleColor.Yellow, true);

            /// <summary>
            /// ConsoleErrorメソッドは、引数で与えられたメッセージを赤色でコンソールに表示するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="message"></param>
            internal static void ConsoleError(string message) => ConsoleWriteWithColor(message, ConsoleColor.Red, true);

            /// <summary>
            /// ConsoleWriteWithColorメソッドは、引数で与えられたテキストを指定された色でコンソールに表示するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="s"></param>
            /// <param name="color"></param>
            /// <param name="newLine"></param>
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

            /// <summary>
            /// GetDisplayWidthメソッドは、与えられた文字列の指定された長さまでの表示幅を計算するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="s"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            private static int GetDisplayWidth(string s, int length)
            {
                int width = 0;
                for (int i = 0; i < length && i < s.Length; i++)
                {
                    width += GetCharWidth(s[i]);
                }
                return width;
            }

            /// <summary>
            /// GetCharWidthメソッドは、与えられた文字の表示幅を計算するためのヘルパーメソッドです。
            /// 半角文字は1、全角文字は2を返します。
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            private static int GetCharWidth(char c) => IsHankaku(c) ? 1 : 2;

            /// <summary>
            /// IsHankakuメソッドは、与えられた文字が半角文字かどうかを判定するためのヘルパーメソッドです。
            /// ASCII文字と半角カタカナは半角とみなします。
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            private static bool IsHankaku(char c) => c <= 0x007F || (0xFF61 <= c && c <= 0xFF9F);

            /// <summary>
            /// DigitCountメソッドは、与えられた整数の桁数を計算するためのヘルパーメソッドです。
            /// </summary>
            /// <param name="n"></param>
            /// <returns></returns>
            private static int DigitCount(int n) => (int)Math.Floor(Math.Log10(n)) + 1;
        }
    }
}
