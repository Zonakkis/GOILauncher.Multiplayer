using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GOILauncher.Multiplayer.Core.Log;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Config
{
    /// <summary>
    /// Persists settings as <c>key=value</c> lines in a file under
    /// <see cref="Application.persistentDataPath"/>, which resolves to a writable per-user location on
    /// every platform the mod targets: <c>AppData\LocalLow</c> on Windows, app-private storage on
    /// Android (no storage permission required), the app sandbox on iOS.
    /// </summary>
    /// <remarks>
    /// The file is meant to be readable and hand-editable, which shapes two things. Whitespace around
    /// a key or a value is not preserved, because both sides of the separator are trimmed on read. And
    /// backslash, CR and LF are escaped on write, so no value can break the one-pair-per-line format.
    /// </remarks>
    public sealed class FileSettingsStore : ISettingsStore
    {
        public const string FileName = "GOILauncher.Multiplayer.cfg";

        // Sorted so that a rewritten file keeps a stable, diffable line order.
        private readonly SortedDictionary<string, string> _values =
            new SortedDictionary<string, string>(StringComparer.Ordinal);

        private readonly ILogger<FileSettingsStore> _logger;

        public FileSettingsStore(ILogger<FileSettingsStore> logger)
        {
            _logger = logger;
            FilePath = Path.Combine(Application.persistentDataPath, FileName);
            Load();
        }

        public string FilePath { get; private set; }

        public bool TryRead(string key, out string raw)
        {
            return _values.TryGetValue(key, out raw);
        }

        public void Write(string key, string raw)
        {
            _values[key] = raw ?? string.Empty;
            Save();
        }

        public void Flush()
        {
            // Every Write already reached the disk, so there is nothing buffered here.
        }

        private void Load()
        {
            string[] lines;
            try
            {
                if (!File.Exists(FilePath))
                {
                    _logger.Info($"No settings file at {FilePath} yet; using defaults.");
                    return;
                }

                lines = File.ReadAllLines(FilePath);
            }
            catch (Exception ex)
            {
                // A settings file we cannot read must never stop the mod from loading.
                _logger.Error(ex, $"Failed to read {FilePath}; using defaults.");
                return;
            }

            int malformed = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#')
                    continue;

                // Rejects a missing separator (-1) and an empty key (0) in one comparison. The key
                // cannot be blank past this point because the line itself is already trimmed.
                int separator = line.IndexOf('=');
                if (separator <= 0)
                {
                    malformed++;
                    continue;
                }

                string key = line.Substring(0, separator).Trim();
                _values[key] = Unescape(line.Substring(separator + 1).Trim());
            }

            // A corrupt line costs one setting, not the whole file.
            if (malformed > 0)
                _logger.Warn($"Ignored {malformed} malformed line(s) in {FilePath}.");
        }

        private void Save()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# GOILauncher.Multiplayer settings.");
            builder.AppendLine("# Rewritten whenever a setting changes; hand-added comments are not kept.");
            foreach (KeyValuePair<string, string> pair in _values)
                builder.Append(pair.Key).Append('=').AppendLine(Escape(pair.Value));

            string temporaryPath = FilePath + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Write beside the target first, so being killed mid-write costs the temporary file
                // rather than leaving a half-written config in place.
                File.WriteAllText(temporaryPath, builder.ToString());
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                File.Move(temporaryPath, FilePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to write {FilePath}; the change applies to this session only.");
                TryDelete(temporaryPath);
            }
        }

        private void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to remove the temporary settings file {path}. {ex.Message}");
            }
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                switch (character)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    default:
                        builder.Append(character);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string Unescape(string value)
        {
            if (value.IndexOf('\\') < 0)
                return value;

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i + 1 >= value.Length)
                {
                    builder.Append(value[i]);
                    continue;
                }

                char escaped = value[++i];
                switch (escaped)
                {
                    case '\\':
                        builder.Append('\\');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    default:
                        // Leave an unknown sequence as typed, so a hand-written Windows path survives.
                        builder.Append('\\').Append(escaped);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
