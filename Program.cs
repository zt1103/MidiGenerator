using MidiGenerator.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MidiGenerator.Support;

namespace MidiGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== MIDI Generator ===");
            Console.WriteLine("Procedural music generator for multiple genres\n");

            // Discover all available generator types
            var generatorTypes = DiscoverGeneratorTypes();
            var generators = DiscoverGenerators(); // For menu display only
            
            if (!generators.Any())
            {
                Console.WriteLine("No MIDI generators found!");
                return;
            }

            // Main program loop
            while (true)
            {
                // Display menu
                Console.WriteLine("\nAvailable genres:");
                for (int i = 0; i < generators.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {generators[i].GenreName}");
                }
                Console.WriteLine("0. Exit");

                // Get user selection
                Type selectedGeneratorType = null;
                while (selectedGeneratorType == null)
                {
                    Console.Write($"\nSelect a genre (0-{generators.Count}): ");
                    var input = Console.ReadLine();
                    
                    if (int.TryParse(input, out int selection))
                    {
                        if (selection == 0)
                        {
                            Console.WriteLine("\nGoodbye!");
                            return;
                        }
                        else if (selection >= 1 && selection <= generators.Count)
                        {
                            selectedGeneratorType = generatorTypes[selection - 1];
                        }
                        else
                        {
                            Console.WriteLine("Invalid selection. Please try again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                    }
                }

                // Get duration
                int duration = 60; // Default
                Console.Write("\nEnter duration in seconds (default 60): ");
                var durationInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(durationInput) && int.TryParse(durationInput, out int userDuration) && userDuration > 0)
                {
                    duration = userDuration;
                }

                // Get number of files to generate
                int fileCount = 1; // Default
                Console.Write("\nHow many files to generate (default 1): ");
                var countInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(countInput) && int.TryParse(countInput, out int userCount) && userCount > 0)
                {
                    fileCount = Math.Min(userCount, 20); // Cap at 20 files
                    if (userCount > 20)
                    {
                        Console.WriteLine("Note: Capped at 20 files for safety.");
                    }
                }

                // Generate files
                var genreName = ((IMidiGenerator)Activator.CreateInstance(selectedGeneratorType)).GenreName;
                Console.WriteLine($"\nGenerating {fileCount} {genreName} file(s) ({duration} seconds each)...");
                
                var generatedFiles = new List<string>();
                
                for (int i = 0; i < fileCount; i++)
                {
                    try
                    {
                        // Create a fresh instance for each file to ensure different randomization
                        var generator = (IMidiGenerator)Activator.CreateInstance(selectedGeneratorType);
                        
                        string fileName = CreateFileName(generator.GenreName, i + 1, fileCount);
                        string filePath = generator.CreateMidiFile(duration, fileName);
                        generatedFiles.Add(filePath);
                        
                        if (fileCount > 1)
                        {
                            Console.WriteLine($"  Generated {i + 1}/{fileCount}: {Path.GetFileName(filePath)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error generating file {i + 1}: {ex.Message}");
                    }
                }

                // Show summary
                if (generatedFiles.Any())
                {
                    Console.WriteLine($"\n✓ Successfully generated {generatedFiles.Count} file(s):");
                    foreach (var file in generatedFiles)
                    {
                        Console.WriteLine($"  • {Path.GetFileName(file)}");
                    }
                    Console.WriteLine($"\nLocation: {Path.GetDirectoryName(generatedFiles[0])}");
                    Console.WriteLine($"Duration: {duration} seconds each");
                    Console.WriteLine($"Genre: {genreName}");
                }
                else
                {
                    Console.WriteLine("\n❌ No files were generated successfully.");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine("=== MIDI Generator ===");
                Console.WriteLine("Procedural music generator for multiple genres\n");
            }
        }

        private static List<Type> DiscoverGeneratorTypes()
        {
            var generatorTypes = new List<Type>();
            
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IMidiGenerator).IsAssignableFrom(t))
                    .OrderBy(t => t.Name);

                generatorTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering generator types: {ex.Message}");
            }

            return generatorTypes;
        }
        
        private static List<IMidiGenerator> DiscoverGenerators()
        {
            var generators = new List<IMidiGenerator>();
            
            try
            {
                var generatorTypes = DiscoverGeneratorTypes();
                foreach (var type in generatorTypes)
                {
                    var generator = (IMidiGenerator)Activator.CreateInstance(type);
                    generators.Add(generator);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discovering generators: {ex.Message}");
            }

            return generators;
        }

        private static string CreateFileName(string genre, int fileNumber, int totalFiles)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            if (totalFiles == 1)
            {
                return $"{genre}_{timestamp}.mid";
            }
            else
            {
                return $"{genre}_{timestamp}_{fileNumber:D2}.mid";
            }
        }
    }
}