using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Focus_Timer.Services
{
    public class AppSettings
    {
        public bool CloseToTray { get; set; } = true;
        public bool CloseToTrayOnlyWhenTimerRunning { get; set; } = true;
        public bool NotificationsEnabled { get; set; } = false;
        public bool PlaySound { get; set; } = true;

        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Focus-Timer", "settings.json");

        public static AppSettings Current { get; private set; } = Load();

        private static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new();
            }
            catch { /* corrupt file, fall back to defaults */ }
            return new();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
    }
}
