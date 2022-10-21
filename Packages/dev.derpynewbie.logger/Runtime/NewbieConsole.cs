using System;
using System.Globalization;
using DerpyNewbie.Common;
using DerpyNewbie.Common.Role;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;

namespace DerpyNewbie.Logger
{
    [DefaultExecutionOrder(-10000)] [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NewbieConsole : PrintableBase
    {
        private const string Version = "0.1.3";

        [SerializeField]
        private LogLevels defaultLogLevel = LogLevels.Info;
        [SerializeField]
        private InputField consoleIn;
        [SerializeField]
        private Selectable fakeSelectable;
        [SerializeField]
        private PrintableBase consoleOut;
        [FormerlySerializedAs("_roleProvider")]
        [SerializeField]
        private RoleProvider roleProvider;
        [SerializeField]
        private bool isMasterSuperUser;

        private bool _isInitialized;

        private NewbieConsoleCommandHandler[] _commandHandlers = new NewbieConsoleCommandHandler[0];
        private string[] _environmentVariables = new string[0];
        private string[] _lastExecutedCommands = { "" };
        private int _lastFilledExecutedCommandIndex = 0;

        private string[][] _scheduledCommandTables = { new[] { "" } };

        [PublicAPI]
        public PrintableBase Out => consoleOut;
        [PublicAPI]
        public InputField In => consoleIn;
        [PublicAPI]
        public RoleData CurrentRole { get; protected set; }
        [PublicAPI]
        public bool IsSuperUser => (CurrentRole != null && CurrentRole.RoleProperties.ContainsItem("moderator")) ||
                                   (isMasterSuperUser && Networking.IsMaster);

        public override int LogLevel
        {
            get => Out.LogLevel;
            set => Out.LogLevel = value;
        }

        public override int MaxChars
        {
            get => Out.MaxChars;
            set => Out.MaxChars = value;
        }

        public void Start()
        {
            if (roleProvider != null)
                CurrentRole = roleProvider.GetPlayerRole();
            _environmentVariables = _environmentVariables.AddAsSet("");
            _isInitialized = true;

            LogLevel = (int)defaultLogLevel;

            SendCustomEventDelayedFrames(nameof(LateStart), 1);
        }

        public void LateStart()
        {
            Evaluate("autoExec");
        }

        private void Update()
        {
            if (!Utilities.IsValid(Networking.LocalPlayer))
                return;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    SendCustomEvent(nameof(_Shortcut_QuickCommand));

                if (Input.GetKeyDown(KeyCode.T))
                    SendCustomEvent(nameof(_Shortcut_ToggleConsole));

                if (Input.GetKeyDown(KeyCode.H))
                    SendCustomEvent(nameof(_Shortcut_TeleportHere));

                if (Input.GetKeyDown(KeyCode.F) && consoleIn.gameObject.activeInHierarchy)
                    SendCustomEvent(nameof(_Shortcut_FocusInput));
            }

            if (consoleIn.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    SendCustomEvent(nameof(_Shortcut_FillInPreviousCommand));

                if (Input.GetKeyDown(KeyCode.DownArrow))
                    SendCustomEvent(nameof(_Shortcut_FillInNextCommand));
            }
        }

        public override void Print(string text)
        {
            Out.Print(text);
        }

        public override void Clear()
        {
            Out.Clear();
        }

        public override void ClearLine()
        {
            Out.ClearLine();
        }

        [PublicAPI]
        public void RegisterHandler(NewbieConsoleCommandHandler handler)
        {
            if (handler == null) return;

            _commandHandlers = _commandHandlers.AddAsSet(handler);
            handler.OnRegistered(this);
        }

        [PublicAPI]
        public void UnregisterHandler(NewbieConsoleCommandHandler handler)
        {
            _commandHandlers = _commandHandlers.RemoveItem(handler);
            if (handler != null)
                handler.OnUnregistered(this);
        }

        [PublicAPI]
        public void OnValueChanged()
        {
            if (consoleIn.isFocused)
            {
                LogInternal("Console::Immobilizing player because I/O is focused");
                Networking.LocalPlayer.Immobilize(true);
            }
        }

        [PublicAPI]
        public void OnEndEdit()
        {
            Networking.LocalPlayer.Immobilize(false);
            var input = consoleIn.text.Trim();
            if (input == "")
            {
                LogInternal("Console::Aborting because input was empty");
                this.Println(">");
                return;
            }

            consoleIn.text = "";
            fakeSelectable.Select();
            this.Println($"> {input}");
            Evaluate(input);
            _lastExecutedCommands = _lastExecutedCommands.InsertItemAtIndex(0, input);
            _lastFilledExecutedCommandIndex = 0;
        }

        [PublicAPI]
        public string Evaluate(string input)
        {
            if (!_isInitialized)
            {
                this.Println("<color=red>Console not yet initialized!</color>");
                return "";
            }

            LogInternal($"Console::Resolving command: {input}");
            var tokens = ConsoleParser.Tokenize(input);
            LogInternal($"Console::TokenizerResult: '{string.Join(",", tokens)}'");

            var parseResult =
                ConsoleParser.Parse(tokens,
                    out var commandTables,
                    out var resultIndex,
                    out var resultMessage);

            if (!parseResult || commandTables == null)
            {
                PrintSyntaxError(input, resultIndex, resultMessage);
                return ConsoleLiteral.GetNone();
            }

            LogInternal($"Console::ParserResult: length: '{commandTables.Length}'");
            for (int i = 0; i < commandTables.Length; i++)
                if (commandTables[i] != null)
                    LogInternal($"Console::ParserResult: Table:{i} '{string.Join(",", commandTables[i])}'");
                else
                    this.LogError($"Console::ParserResult: Table {i} is null!!!");

            return _Internal_Executor(commandTables);
        }

        private string _Internal_Executor(string[][] commandTables)
        {
            var variableTable = new string[commandTables.Length + 1];

            for (int i = 0; i < commandTables.Length; i++)
            {
                LogInternal(
                    $"Console::Executor: Executing command table {i}: '{(commandTables[i] != null ? string.Join(",", commandTables[i]) : "NULL!!!")}'");
                if (commandTables[i] == null)
                {
                    this.LogError($"Console::Executor: Table {i} is null, will not execute!");
                    continue;
                }

                var processedCommandTable = _Internal_ReplaceVariable(commandTables[i], variableTable);
                if (processedCommandTable.Length == 0 || processedCommandTable[0] == "")
                {
                    LogInternal("Console::Executor: Invalid command table");
                    continue;
                }

                switch (processedCommandTable[0].ToLower())
                {
                    case "help":
                    case "?":
                    {
                        LogInternal("Console::Executor: Found 'HELP' instruction");

                        if (processedCommandTable.Length != 1)
                        {
                            var helpProvider = FindCommandHandlerOf(processedCommandTable[1]);
                            if (helpProvider == null)
                            {
                                this.Println("No help information found for this command.");
                                continue;
                            }

                            this.Println(
                                $"<color=green>Information retrieved from {processedCommandTable[1]}</color>");
                            this.Println($"Label      : {helpProvider.Label}");
                            this.Println($"Description: {helpProvider.Description}");
                            this.Println($"Aliases    : {string.Join(", ", helpProvider.Aliases)}");
                            this.Println($"Usage      : {helpProvider.Usage}");
                            continue;
                        }

                        var output =
                            "<color=green>For more information on specific command, type `help <command-name>`";
                        foreach (var c in _commandHandlers)
                            if (c != null)
                                output = string.Join("\n", output, $"{c.Label,-15}: {c.Description}");
                        output += "</color>";
                        this.Println(output);
                        continue;
                    }
                    case "version":
                    case "v":
                    {
                        LogInternal("Console::Executor: Found 'VERSION' instruction");

                        this.Println($"NewbieLogger/Console - v{GetVersion()}");
                        continue;
                    }
                    case "license":
                    {
                        LogInternal("Console::Executor: Found 'LICENSE' instruction");

                        this.Println("NewbieLogger/Console and NewbieCommons is licensed under MIT license");
                        this.NewLine();
                        this.Println(GetLicense());
                        continue;
                    }
                    case "wait":
                    {
                        LogInternal("Console::Executor: Found 'WAIT' instruction");
                        if (commandTables.Length <= i + 1)
                        {
                            LogInternal("Console::Executor: 'WAIT' instruction was present at last. ignoring!");
                            continue;
                        }

                        var queueTime = processedCommandTable.Length >= 2
                            ? ConsoleParser.TryParseFloat(processedCommandTable[1])
                            : 1F;

                        _scheduledCommandTables = _scheduledCommandTables.AppendArray(
                            commandTables.GetSpanOfArray(i + 1, commandTables.Length - (i + 1)));
                        SendCustomEventDelayedSeconds(nameof(_ExecuteQueuedCommand), queueTime);

                        LogInternal($"Console::Executor: Scheduled command at {queueTime}");

                        return ConsoleLiteral.GetNone();
                    }
                    case "clear":
                    {
                        LogInternal("Console::Executor: Found 'CLEAR' instruction");
                        Clear();
                        continue;
                    }
                    case "t":
                    case "true":
                    {
                        LogInternal("Console::Executor: Found 'TRUE' instruction");
                        variableTable[i] = ConsoleLiteral.GetTrue();
                        continue;
                    }
                    case "f":
                    case "false":
                    {
                        LogInternal("Console::Executor:Found 'FALSE' instruction");
                        variableTable[i] = ConsoleLiteral.GetFalse();
                        continue;
                    }
                    case "if":
                    {
                        LogInternal("Console::Executor:Found 'IF' instruction");
                        if (processedCommandTable.Length <= 2)
                        {
                            PrintSyntaxError(string.Join(" ", processedCommandTable), 0, "Not enough arguments");
                            return ConsoleLiteral.GetNone();
                        }

                        var condition = processedCommandTable[1].ToLower() == "true";
                        LogInternal($"Console::Executor:Condition was {condition}");

                        if (condition)
                        {
                            processedCommandTable =
                                processedCommandTable.GetSpanOfArray(2, processedCommandTable.Length - 2);
                            break;
                        }

                        continue;
                    }
                    case "is":
                    {
                        LogInternal("Console::Executor:Found 'IS' instruction");
                        if (processedCommandTable.Length <= 2)
                        {
                            PrintSyntaxError(string.Join(" ", processedCommandTable), 0, "Not enough arguments");
                            return ConsoleLiteral.GetNone();
                        }

                        var condition = processedCommandTable[1]
                            .Equals(processedCommandTable[2], StringComparison.OrdinalIgnoreCase);
                        LogInternal($"Console::Executor:Condition was {condition}, writing at {i}");

                        variableTable[i] = condition ? ConsoleLiteral.GetTrue() : ConsoleLiteral.GetFalse();
                        continue;
                    }
                    case "or":
                    {
                        LogInternal("Console::Executor:Found 'OR' instruction");
                        if (processedCommandTable.Length <= 2)
                        {
                            PrintSyntaxError(string.Join(" ", processedCommandTable), 0, "Not enough arguments");
                            return ConsoleLiteral.GetNone();
                        }

                        var condition = processedCommandTable[1].Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                        processedCommandTable[2].Equals("true", StringComparison.OrdinalIgnoreCase);
                        LogInternal($"Console::Executor:Condition was {condition}, writing at {i}");

                        variableTable[i] = condition ? ConsoleLiteral.GetTrue() : ConsoleLiteral.GetFalse();
                        continue;
                    }
                    case "not":
                    {
                        LogInternal("Console::Executor:Found 'NOT' instruction");
                        if (processedCommandTable.Length <= 1)
                        {
                            PrintSyntaxError(string.Join(" ", processedCommandTable), 0, "Not enough arguments");
                            return ConsoleLiteral.GetNone();
                        }

                        var condition = processedCommandTable[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                        LogInternal($"Console::Executor:Condition was {condition}, writing inverted result at {i}");

                        variableTable[i] = condition ? ConsoleLiteral.GetFalse() : ConsoleLiteral.GetTrue();
                        continue;
                    }
                    case "held":
                    {
                        LogInternal("Console::Executor:Found 'HELD' instruction");
                        if (processedCommandTable.Length <= 1)
                        {
                            PrintSyntaxError(string.Join(" ", processedCommandTable), 0, "Not enough arguments");
                            return ConsoleLiteral.GetNone();
                        }

                        var hand = processedCommandTable[1].Equals("left", StringComparison.OrdinalIgnoreCase)
                            ? VRC_Pickup.PickupHand.Left
                            : VRC_Pickup.PickupHand.Right;

                        var condition = Networking.LocalPlayer.GetPickupInHand(hand) != null;
                        LogInternal($"Console::Executor:Condition was {condition}, writing at {i}");

                        variableTable[i] = condition
                            ? ConsoleLiteral.GetTrue()
                            : ConsoleLiteral.GetFalse();
                        continue;
                    }
                }

                var args = new string[0];
                if (processedCommandTable.Length > 1)
                    args = processedCommandTable.GetSpanOfArray(1, processedCommandTable.Length - 1);

                var handler = FindCommandHandlerOf(processedCommandTable[0]);
                if (handler == null)
                {
                    this.Println("<color=red>Command not found</color>");
                    return ConsoleLiteral.GetNone();
                }

                variableTable[i] =
                    handler.OnCommand
                    (
                        this,
                        processedCommandTable[0],
                        args,
                        ref _environmentVariables
                    );
                LogInternal($"Console::Executor: Processed result:{i} '{variableTable[i]}'");
            }

            return string.Join(" ", variableTable);
        }

        private string[] _Internal_ReplaceVariable(string[] commandTable, string[] variableTable)
        {
            for (int i = 0; i < commandTable.Length; i++)
            {
                var command = commandTable[i];
                for (int j = 0; j < variableTable.Length; j++)
                    command = command.Replace($"${(variableTable.Length - 2 - j)}", variableTable[j]);

                commandTable[i] = command;
            }

            return commandTable;
        }

        public void _ExecuteQueuedCommand()
        {
            LogInternal("Console::Executing queued command");
            var scheduledCommandTables = _scheduledCommandTables;
            _scheduledCommandTables = new[] { new string[0] };
            _Internal_Executor(scheduledCommandTables);
        }

        private NewbieConsoleCommandHandler FindCommandHandlerOf(string label)
        {
            LogInternal($"Console::Searching CommandHandler with label: {label}");
            var target = label.Trim().ToLower();
            foreach (var c in _commandHandlers)
                if (c.Label.Equals(target, StringComparison.OrdinalIgnoreCase) ||
                    c.Aliases.ContainsString(target, StringComparison.OrdinalIgnoreCase))
                    return c;
            return null;
        }

        private void LogInternal(string obj)
        {
            Out.LogWithLevelFormatted(obj, (int)LogLevels.Internal);
        }

        #region Shortcuts

        public void _Shortcut_QuickCommand()
        {
            LogInternal("Console::Shortcut: Quick Command");
            Evaluate("console enable true");
            Evaluate("console here");
            fakeSelectable.Select();
            consoleIn.Select();
        }

        public void _Shortcut_ToggleConsole()
        {
            LogInternal("Console::Shortcut: Toggle Console");
            Evaluate("console enable toggle");
        }

        public void _Shortcut_TeleportHere()
        {
            LogInternal("Console::Shortcut: Teleport Here");
            Evaluate("console here");
        }

        public void _Shortcut_FocusInput()
        {
            LogInternal("Console::Shortcut: Focus Input");
            fakeSelectable.Select();
            consoleIn.Select();
        }

        public void _Shortcut_FillInPreviousCommand()
        {
            LogInternal("Console::Shortcut: Fill Last");

            consoleIn.text = _lastExecutedCommands[_lastFilledExecutedCommandIndex++];
            _lastFilledExecutedCommandIndex =
                Mathf.Clamp(_lastFilledExecutedCommandIndex, 0, _lastExecutedCommands.Length - 1);
        }

        public void _Shortcut_FillInNextCommand()
        {
            LogInternal("Console::Shortcut: Fill Next");
            consoleIn.text = "";

            consoleIn.text = _lastExecutedCommands[_lastFilledExecutedCommandIndex--];
            _lastFilledExecutedCommandIndex =
                Mathf.Clamp(_lastFilledExecutedCommandIndex, 0, _lastExecutedCommands.Length - 1);
        }

        #endregion

        public static string[] EmptyAlias()
        {
            return new string[0];
        }

        public static string GetVersion()
        {
            return Version;
        }

        public static string GetLicense()
        {
            return "MIT License\n" +
                   "\nCopyright (c) 2022 DerpyNewbie\n" +
                   "\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n" +
                   "\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n" +
                   "\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";
        }

        public string PrintUsage(NewbieConsoleCommandHandler handler)
        {
            this.Println($"<color=red>Usage: {handler.Usage}</color>");
            return ConsoleLiteral.GetNone();
        }

        public void PrintSyntaxError(string cmd, int offset, string message)
        {
            this.LogError($"{cmd}\nfound unexpected token at {offset}: {message}");
        }
    }

    public static class ConsoleParser
    {
        [PublicAPI]
        public static string[] Tokenize(string input)
        {
            var cmd = input.Trim() + " ";

            var tokens = new string[0];
            var length = 0;
            var inDoubleQuotation = false;
            for (var offset = 0; offset < cmd.Length; offset++)
            {
                var current = cmd[offset];

                if (current == '"')
                {
                    inDoubleQuotation = !inDoubleQuotation;
                    if (!inDoubleQuotation)
                    {
                        tokens = tokens.AddAsList(cmd.Substring(offset - length + 1, length - 1));
                        length = 0;
                        continue;
                    }
                }

                if (inDoubleQuotation)
                {
                    ++length;
                    continue;
                }

                switch (current)
                {
                    case '(':
                    case ')':
                    case ';':
                    case '|':
                    case '&':
                    {
                        if (length > 0)
                            tokens = tokens.AddAsList(cmd.Substring(offset - length, length));
                        tokens = tokens.AddAsList(current.ToString());
                        length = 0;
                        continue;
                    }
                    case ' ':
                    {
                        tokens = tokens.AddAsList(cmd.Substring(offset - length, length));
                        length = 0;
                        continue;
                    }
                    default:
                    {
                        ++length;
                        continue;
                    }
                }
            }

            // TODO: there should be better way to do this - maybe forward check chars?
            while (true)
            {
                tokens = tokens.RemoveItem("", out var result);
                if (!result)
                    break;
            }

            return tokens;
        }

        public static bool Parse(
            string[] tokens,
            [CanBeNull] out string[][] commandTables,
            out int resultIndex,
            out string resultMessage)
        {
            var commandTable = new string[1][];
            commandTable[0] = new string[0];
            var depthHistory = new[] { 0 };
            int depthPtr = 0, currentDepth = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                var currentToken = tokens[i];
                switch (currentToken)
                {
                    case "(":
                        depthHistory = depthHistory.AddAsList(commandTable.Length);
                        currentDepth = depthHistory[++depthPtr];
                        commandTable = commandTable.AddAsList(new string[0]);
                        commandTable[currentDepth - 1] = commandTable[currentDepth - 1].AddAsList($"${currentDepth}");
                        continue;
                    case ")":
                        --depthPtr;
                        if (depthPtr < 0)
                        {
                            resultMessage = $"depthPtr {depthPtr} < 0. bracket is closed before opened";
                            resultIndex = i;
                            commandTables = null;
                            return false;
                        }

                        currentDepth = depthHistory[depthPtr];
                        continue;
                    case ";":
                        if (currentDepth != 0)
                        {
                            resultMessage =
                                $"currentDepth {currentDepth} != 0. semicolon within brackets not yet supported";
                            resultIndex = i;
                            commandTables = null;
                            return false;
                        }

                        commandTable = commandTable.InsertItemAtIndex(0, new string[0], out var result);
                        if (result == false)
                        {
                            resultMessage = "Unexpected error; InsertItemAtIndex `result == false`";
                            resultIndex = i;
                            commandTables = null;
                            return false;
                        }

                        currentDepth = 0;
                        continue;
                    default:
                        commandTable[currentDepth] = commandTable[currentDepth].AddAsList(currentToken);
                        continue;
                }
            }

            if (currentDepth != 0)
            {
                resultMessage = $"currentDepth {currentDepth} != 0. bracket is not closed";
                resultIndex = string.Join(" ", tokens).Length;
                commandTables = null;
                return false;
            }

            Array.Reverse(commandTable);
            resultMessage = "Success";
            resultIndex = 0;
            commandTables = commandTable;
            return true;
        }

        [PublicAPI]
        public static bool TryParseBoolean(string literal, bool current)
        {
            var formatted = literal.Trim().ToLower();
            if (formatted == "true" || formatted == "yes" || formatted == "1")
                return true;
            if (formatted == "toggle")
                return !current;
            return false;
        }

        [PublicAPI]
        public static int TryParseBoolAsInt(string literal, bool current)
        {
            return TryParseBoolean(literal, current) ? 1 : 0;
        }

        [PublicAPI]
        public static int TryParseInt(string literal)
        {
            int value;
            return int.TryParse(literal, out value) ? value : -1;
        }

        [PublicAPI]
        public static byte TryParseByte(string literal)
        {
            byte value;
            return byte.TryParse(literal, NumberStyles.Any, null, out value) ? value : byte.MaxValue;
        }

        [PublicAPI]
        public static float TryParseFloat(string literal)
        {
            float value;
            return float.TryParse(literal, out value) ? value : float.NaN;
        }

        [PublicAPI]
        public static VRCPlayerApi TryParsePlayer(string literal)
        {
            var playerApis = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            playerApis = VRCPlayerApi.GetPlayers(playerApis);

            foreach (var playerApi in playerApis)
                if (literal.Equals(playerApi.displayName, StringComparison.OrdinalIgnoreCase) ||
                    literal.Equals($"{playerApi.playerId}"))
                    return playerApi;

            return null;
        }
    }

    public static class ConsoleLiteral
    {
        [PublicAPI]
        public static string GetNone() => "";

        [PublicAPI]
        public static string GetTrue() => "true";

        [PublicAPI]
        public static string GetFalse() => "false";

        [PublicAPI]
        public static string Of(string s)
        {
            return "\"s\"";
        }

        [PublicAPI]
        public static string Of(bool b)
        {
            return b ? GetTrue() : GetFalse();
        }

        [PublicAPI]
        public static string Of(int i)
        {
            return $"{i}";
        }

        [PublicAPI]
        public static string Of(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }
    }
}