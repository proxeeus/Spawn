﻿using EnhancedMap.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PremiumConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var showMenu = true;

            while(showMenu)
            {
                showMenu = MainMenu();
            }
        }

        static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("Premium Spawner Converter");
            Console.WriteLine("==========================");
            Console.WriteLine("Choose an option:");
            Console.WriteLine("");
            Console.WriteLine("1) Convert a PremiumSpawner .map to a Proxeeus .map");
            Console.WriteLine("2) Check types in a .map file");
            Console.WriteLine("Q) Quit");

            switch(Console.ReadLine().ToLower())
            {
                case "1":
                    Convert();
                    return true;
                case "2":
                    Check();
                    return true;
                case "q":
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Asks for a .map path, processes it, and creates a Proxeeus .map file.
        /// </summary>
        static void Convert()
        {
            Console.Clear();
            Console.WriteLine("Enter the path of the Premium Spawner .map file:");
            var premiumPath = Console.ReadLine();
            if(File.Exists(premiumPath))
            {
                Console.WriteLine("Now reading from '{0}'...", premiumPath);
                Console.WriteLine();
                using (var streamReader = new StreamReader(premiumPath))
                {
                    var line = string.Empty;
                    var mapFileInfo = new FileInfo(premiumPath);

                    var spawnDefs = new List<SpawnDefinition>();

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (Helpers.IgnoreLine(line)) continue;
                        Console.WriteLine("Converting '{0}'", line);
                        var splitData = line.Split('|');
                        if (!Helpers.HasObjectsToSpawn(splitData))
                            Console.WriteLine("WARNING: Line '{0}' has no spawn data, skipping...", line);
                        else
                        {
                            /*
                             * Here's the rough format of the various .map files across distros, with a couple of sample entries to better visualize.
                             * Array indexes are above the colum names
                             
    1        2   3  4    5   6   7    8    9    10      11      12       13         14        15      16        17          18          19          20          21
*|typename |s1 |s2 |s3 |s4 |s5 | x  | y  | z | map | mindelay maxdelay homerange spawnrange spawnid maxcount | maxcount1 | maxcount2 | maxcount3 | maxcount4 | maxcount5
*|Herbalist|   |   |   |   |   |1685|2985|0  |1    |5       |10       |20       |10         |1     |2        |0			 |0			 |0			 |0			 |0
*|Ratmanmage|||||			   |5850|219 |-3 |2	   |5	    |10       |20       |10         |1     |1        |0          |0          |0          |0          |0

                            */

                            
                            var spawnDef = new SpawnDefinition();

                            Helpers.AddSpawnTypesToDefinition(splitData, spawnDef);
                            if(spawnDef.Mobiles.Count > 6)
                            {
                                Console.WriteLine("WARNING: '{0}' currently defines more than 6 mobiles!", line);
                                Console.ReadLine();
                            }
                            spawnDef.SpawnerName = mapFileInfo.Name + "_" + splitData[1];
                            spawnDef.X = splitData[7];
                            spawnDef.Y = splitData[8];
                            spawnDef.MapId = splitData[10]; 
                            spawnDef.MinTime = splitData[11];
                            spawnDef.MaxTime = splitData[12];
                            spawnDef.Team = "0";  // Hardcoded for now, not sure what this does
                            spawnDef.NPCCount = splitData[16];
                            spawnDef.HomeRange = splitData[13];
                            spawnDef.BringToHome = true; // WARNING: for mobs it can be "ok" to have them spawn around the spawner. But for a clump of items, this sucks.

                            spawnDefs.Add(spawnDef);
                        }
                    }
                    Console.WriteLine("");
                    Console.WriteLine("Done parsing '{0}', {1} spawners converted.", mapFileInfo.Name, spawnDefs.Count);
                    var convertedMapFile = "converted_" + mapFileInfo.Name;
                    Console.WriteLine("Now generating '{0}'...", convertedMapFile);

                    var spawnWriter = new SpawnWriter();
                    Helpers.Write(spawnDefs, convertedMapFile);
                    Console.WriteLine("'{0}' created successfully!", convertedMapFile);
                }
            }
            else
            {
                Console.WriteLine("Error: file doesn't exist!");
            }
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        /// <summary>
        /// Checks a Proxeeus .map file for any missing types from Items / Mobiles scripts.
        /// </summary>
        static void Check()
        {
            Console.WriteLine("Not implemented yet!");
            Console.ReadLine();
        }
    }
}