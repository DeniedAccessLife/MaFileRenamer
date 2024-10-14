using System;
using System.IO;
using SteamAuth;
using System.Linq;
using Newtonsoft.Json;

namespace MaFileRenamer
{
    static class Renamer
    {
        private static string folder = string.Empty;
        private static string manifest = string.Empty;

        public static void Start()
        {
            folder = GetFolderPath();
            manifest = Path.Combine(folder, "manifest.json");

            RenameMaFiles();
            UpdateManifest();
        }

        private static string GetFolderPath()
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                string current_directory = Directory.GetCurrentDirectory();
                string potential_folder_path = Path.Combine(current_directory, "maFiles");

                if (Directory.Exists(potential_folder_path))
                {
                    return potential_folder_path;
                }

                Console.CursorVisible = true;
                Console.Write("Please enter the path to the maFiles folder: ");
                string input = Console.ReadLine();
                Console.WriteLine();

                while (!Directory.Exists(input) || string.IsNullOrWhiteSpace(input))
                {
                    Console.Clear();
                    Console.Write("Invalid path. Please try again: ");
                    input = Console.ReadLine();
                }

                Console.CursorVisible = false;

                return input;
            }

            return folder;
        }

        private static void RenameMaFiles()
        {
            string[] mafiles = Directory.GetFiles(folder, "*.maFile");

            foreach (string file in mafiles)
            {
                string json = File.ReadAllText(file);
                SteamGuardAccount mafile = JsonConvert.DeserializeObject<SteamGuardAccount>(json) ?? throw new Exception($"Failed to deserialize maFile: {file}!");

                string new_name = file.Contains(mafile.Session.SteamID.ToString()) ? $"{mafile.AccountName}.maFile" : $"{mafile.Session.SteamID}.maFile";
                string new_file = Path.Combine(folder, new_name);

                File.Move(file, new_file);
            }
        }

        private static void UpdateManifest()
        {
            if (!File.Exists(Renamer.manifest))
            {
                throw new Exception("Manifest file not found!");
            }

            string json = File.ReadAllText(Renamer.manifest);
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(json) ?? throw new Exception("Failed to deserialize manifest file!");

            foreach (Entry entry in manifest.entries)
            {
                string steamid = entry.steamid.ToString();
                string[] mafiles = Directory.GetFiles(folder, "*.maFile");
                SteamGuardAccount corresponding_mafile = mafiles.Select(file_path => JsonConvert.DeserializeObject<SteamGuardAccount>(File.ReadAllText(file_path))).FirstOrDefault(ma_file => ma_file.Session.SteamID.ToString() == steamid);

                if (corresponding_mafile != null)
                {
                    string current_file = (Directory.GetFiles(folder, $"{corresponding_mafile.Session.SteamID}.maFile").FirstOrDefault() ?? Directory.GetFiles(folder, $"{corresponding_mafile.AccountName}.maFile").FirstOrDefault()) ?? throw new Exception($"No corresponding maFile found for SteamID {steamid}!");
                    entry.filename = Path.GetFileName(current_file);
                    Console.WriteLine($"File updated: {entry.filename}");
                }
                else
                {
                    throw new Exception($"File for SteamID {steamid} not found!");
                }
            }

            File.WriteAllText(Renamer.manifest, JsonConvert.SerializeObject(manifest, Formatting.Indented));
            Console.WriteLine("Manifest updated successfully.");
        }

        private class Manifest
        {
            public bool encrypted { get; set; }
            public bool first_run { get; set; }
            public Entry[] entries { get; set; }
            public bool periodic_checking { get; set; }
            public int periodic_checking_interval { get; set; }
            public bool periodic_checking_checkall { get; set; }
            public bool auto_confirm_market_transactions { get; set; }
            public bool auto_confirm_trades { get; set; }
        }

        private class Entry
        {
            public object encryption_iv { get; set; }
            public object encryption_salt { get; set; }
            public string filename { get; set; }
            public long steamid { get; set; }
        }
    }
}