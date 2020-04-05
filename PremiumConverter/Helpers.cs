﻿using EnhancedMap.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PremiumConverter
{
    public static class Helpers
    {
        public static bool IgnoreLine(string line)
        {
            if (line.StartsWith("#") || line.StartsWith("overrideid") || line.StartsWith("overridemap") || line.StartsWith("overridemintime") || line.StartsWith("overridemaxtime")
                || line.StartsWith("//"))
                return true;
            return false;
        }

        /// <summary>
        /// The first 6 slots of a given line are for the object types to spawn. Can be mobiles & items (or whatever that is spawnable, actually...)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool HasObjectsToSpawn(string[] data)
        {
            if (!string.IsNullOrEmpty(data[1]) || !string.IsNullOrEmpty(data[2]) || !string.IsNullOrEmpty(data[3]) || !string.IsNullOrEmpty(data[4]) || !string.IsNullOrEmpty(data[5]) ||
                !string.IsNullOrEmpty(data[6]))
                    return true;
            return false;
        }

        private static void AddSubTypes(string data, SpawnDefinition spawnDef)
        {
            var subTypes = data.Split(':');
            foreach (var subType in subTypes)
                spawnDef.Mobiles.Add(subType);
        }
        public static void AddSpawnTypesToDefinition(string[] data, SpawnDefinition def)
        {
            if (!string.IsNullOrEmpty(data[1]))
            {
                if(data[1].Contains(":"))
                    Helpers.AddSubTypes(data[1], def);
                else
                    def.Mobiles.Add(data[1]);
            }

            if (!string.IsNullOrEmpty(data[2]))
            {
                if (data[2].Contains(":"))
                    Helpers.AddSubTypes(data[2], def);
                else
                    def.Mobiles.Add(data[2]);
            }
            if (!string.IsNullOrEmpty(data[3]))
            {
                if (data[3].Contains(":"))
                    Helpers.AddSubTypes(data[3], def);
                else
                    def.Mobiles.Add(data[3]);
            }

            if (!string.IsNullOrEmpty(data[4]))
            {
                if (data[4].Contains(":"))
                    Helpers.AddSubTypes(data[4], def);
                else
                    def.Mobiles.Add(data[4]);
            }

            if (!string.IsNullOrEmpty(data[5]))
            {
                if (data[5].Contains(":"))
                    Helpers.AddSubTypes(data[5], def);
                else
                    def.Mobiles.Add(data[5]);
            }

            if (!string.IsNullOrEmpty(data[6]))
            {
                if (data[6].Contains(":"))
                    Helpers.AddSubTypes(data[6], def);
                else
                    def.Mobiles.Add(data[6]);
            }

        }

        public static void Write(List<SpawnDefinition> spawnDefs, string fileName)
        {
            using (var streamWriter = new StreamWriter(fileName))
            {
                streamWriter.WriteLine("//'{0}' -- this file has been automatically generated by a tool.", fileName);
                streamWriter.WriteLine("//---------------------------------------------------------------");
                foreach (var spawnDef in spawnDefs)
                {
                    var line = string.Empty;

                    // Basic format
                    // +name [MobileTypeA, MobileTypeB, etc]: X Y MapID NPCCount HomeRange BringToHome MinTime MaxTime Team Description
                    // "," can also be "|" depending on spawn being unique or not

                    var template = @"+{0} [{1}]: {2} {3} {4} {5} {6} {7} {8} {9} {10}";

                    var mobBuilder = new StringBuilder();
                    var separator = string.Empty;
                    if (spawnDef.UniqueSpawn)
                        separator = ",";
                    else
                        separator = "|";
                    var mobIndex = 1;
                    for (var i = 0; i < spawnDef.Mobiles.Count; i++)
                    {
                        mobBuilder.Append(spawnDef.Mobiles[i]);
                        if (mobIndex < spawnDef.Mobiles.Count)
                        {
                            mobBuilder.Append(separator);
                            mobIndex++;
                        }
                    }

                    line = string.Format(template, spawnDef.SpawnerName.ToLower().Replace(':','_'),
                        mobBuilder.ToString(), spawnDef.X, spawnDef.Y, spawnDef.MapId, spawnDef.NPCCount, spawnDef.HomeRange, spawnDef.BringToHome, spawnDef.MinTime, spawnDef.MaxTime, spawnDef.Team);

                    streamWriter.WriteLine(line);
                }
            }
        }

        internal static List<string> LoadSpawnTypesFrom(string mapPath)
        {
            var types = new List<string>();
            if (File.Exists(mapPath))
            {
                var globalSpawnNames = new List<string>();

                using (var reader = new StreamReader(mapPath))
                {
                    var line = string.Empty;
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Helpers.IgnoreLine(line)) continue;

                        var spawnLine = line.Substring(line.IndexOf('['), line.LastIndexOf(']') - line.IndexOf('['));
                        var spawnNames = new List<string>();

                        if (spawnLine.Contains('|'))
                            spawnNames = spawnLine.Split('|').ToList();
                        else if (spawnLine.Contains(','))
                            spawnNames = spawnLine.Split(',').ToList();
                        else
                        {
                            spawnNames.Add(spawnLine.Trim());
                        }

                        foreach (var name in spawnNames)
                        {
                            var finalName = name.Trim();
                            if (name.Contains("["))
                                finalName = name.Replace("[","");
                            else if(name.Contains(","))
                                finalName = name.Replace(",","");
                            globalSpawnNames.Add(finalName);
                        }
                            
                    }
                }

                var uniqueNames = globalSpawnNames.Distinct();
                foreach (var uniqueName in uniqueNames)
                    types.Add(uniqueName);  // this is shit but it works
            }
            else
                Console.WriteLine("ERROR: file not found or does not exist!");
            return types;
        }

        internal static void FixDataTypes(string mapPath, List<string> partialMatches)
        {
            var bufferNewFile = new List<string>();
            using (var reader = new StreamReader(mapPath))
            {
                var line = string.Empty;
                var culture = CultureInfo.CurrentCulture;
                const char TEMP_SEPARATOR = '£';
                
                while ((line = reader.ReadLine()) != null)
                {
                    if (Helpers.IgnoreLine(line))
                    {
                        bufferNewFile.Add(line);
                        continue;
                    }

                    var oldLine = line;
                    var lineCorrected = false;

                    foreach (var partialMatch in partialMatches)
                    {
                        var oldSeparator = ' ';

                        // The 'hacky' partial match is constructed in a way to have strict delimiters to better match occurences in a given line and
                        // avoid bugs like EvilMageLord becoming EvilMagelord when only "EvilMage" matches, thus losing any kind of previously existing
                        // formatting.
                        // I know, this blows.

                        // Moreover, we temporarily HAVE to replace | and , separators with temporary ones because otherwise the regex engine will
                        // go completely balls to the wall INSANE and fuck up the correction algorithm.
                        // Once the type mismatch is corrected, we revert back to the previous separator styles.
                        // I know, this blows.
                        var hackyPartialMatch = string.Empty;
                        if (line.Contains("|"))
                        {
                            oldSeparator = '|';
                            hackyPartialMatch = TEMP_SEPARATOR + partialMatch + TEMP_SEPARATOR;
                            line = line.Replace('|', TEMP_SEPARATOR);
                        }

                        else if (line.Contains(","))
                        {
                            oldSeparator = ',';
                            hackyPartialMatch = TEMP_SEPARATOR + partialMatch + TEMP_SEPARATOR;
                            line = line.Replace(',', TEMP_SEPARATOR);
                        }
                        else
                            hackyPartialMatch = partialMatch;

                        // Add 'fake' boundaries to the line to have a change to get ALL matches 
                        line = line.Insert(line.IndexOf("[") + 1, TEMP_SEPARATOR.ToString());
                        line = line.Insert(line.LastIndexOf("]"), TEMP_SEPARATOR.ToString());

                        if (culture.CompareInfo.IndexOf(line, hackyPartialMatch, CompareOptions.IgnoreCase) >= 0)
                        {
                            var regex = new Regex(hackyPartialMatch, RegexOptions.IgnoreCase);
                            line = regex.Replace(line, hackyPartialMatch);
                        }
                        // Remove the fake boundary
                        line = line.Remove(line.IndexOf(TEMP_SEPARATOR), 1);
                        line = line.Remove(line.LastIndexOf(TEMP_SEPARATOR), 1);
                        // Remove the temp separator
                        line = line.Replace(TEMP_SEPARATOR, oldSeparator);

                        lineCorrected = true;
                    }
                    if (lineCorrected)
                        bufferNewFile.Add(line);
                    else
                        bufferNewFile.Add(oldLine);
                }
            }

            using(var writer = new StreamWriter(mapPath))
            {
                foreach(var line in bufferNewFile)
                {
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine("'{0}' has been fixed wherever possible.", mapPath);
        }
    }
}
