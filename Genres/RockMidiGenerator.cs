using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

namespace MidiGenerator.Genres
{
    public class RockMidiGenerator : BaseMidiGenerator
    {
        public override string GenreName => "Rock";
        
        private static Random _staticRandom = new Random();
        private static int _instanceCounter = 0;
        private Random _random;
        private int _rootKey;
        private int _tempoVariation;
        private int _subgenre; // 0=Classic Rock, 1=Hard Rock, 2=Progressive, 3=Punk, 4=Alternative
        private int _leadGuitar;
        private int _rhythmGuitar;
        private int _bassGuitar;
        private int _keyboardInstrument;
        private float _distortionLevel;
        private int[] _currentProgression;
        private bool _hasSolo;
        private bool _hasKeyboard;
        private int _songStructure; // 0=Simple, 1=Extended, 2=Complex
        private int _drumComplexity;
        
        public RockMidiGenerator()
        {
            Console.WriteLine("ROCK GENERATOR CONSTRUCTOR CALLED!");
            // Create a unique Random instance with a different seed
            // Use both random number and counter to ensure uniqueness
            int uniqueSeed = _staticRandom.Next() + (++_instanceCounter * 1000) + Environment.TickCount;
            _random = new Random(uniqueSeed);
            
            // Random rock subgenre
            _subgenre = _random.Next(0, 5);
            
            // Debug output - remove this later
            Console.WriteLine($"Rock Generator #{_instanceCounter}: Seed={uniqueSeed}, Subgenre={_subgenre}");
            
            // Random rock keys (guitar-friendly)
            var rockKeys = new[] { 0, 2, 4, 5, 7, 9, 10 }; // C, D, E, F, G, A, Bb
            _rootKey = rockKeys[_random.Next(rockKeys.Length)];
            
            // Tempo variations by subgenre
            _tempoVariation = _subgenre switch
            {
                0 => _random.Next(-20, 21),  // Classic Rock: 100-140 BPM
                1 => _random.Next(-10, 31),  // Hard Rock: 110-150 BPM
                2 => _random.Next(-30, 41),  // Progressive: 90-160 BPM (wide range)
                3 => _random.Next(40, 81),   // Punk: 160-200 BPM
                4 => _random.Next(-15, 26),  // Alternative: 105-145 BPM
                _ => 0
            };
            
            // Random guitar programs
            var leadGuitars = new[] { 29, 30, 31, 32, 26, 27 }; // Distorted guitars, clean jazz/electric
            var rhythmGuitars = new[] { 25, 26, 27, 28, 29, 30 }; // Electric guitars (clean to distorted)
            var bassPrograms = new[] { 33, 34, 35, 36 }; // Electric and acoustic bass
            var keyboardOptions = new[] { 0, 1, 2, 3, 4, 5, 16, 17, 19 }; // Piano, EP, organs
            
            _leadGuitar = leadGuitars[_random.Next(leadGuitars.Length)];
            _rhythmGuitar = rhythmGuitars[_random.Next(rhythmGuitars.Length)];
            _bassGuitar = bassPrograms[_random.Next(bassPrograms.Length)];
            _keyboardInstrument = keyboardOptions[_random.Next(keyboardOptions.Length)];
            
            // Distortion level affects velocity and attack
            _distortionLevel = _subgenre switch
            {
                0 => 0.6f + (_random.NextSingle() * 0.3f), // Classic: 0.6-0.9
                1 => 0.8f + (_random.NextSingle() * 0.2f), // Hard: 0.8-1.0
                2 => 0.4f + (_random.NextSingle() * 0.4f), // Prog: 0.4-0.8 (varies)
                3 => 0.9f + (_random.NextSingle() * 0.1f), // Punk: 0.9-1.0
                4 => 0.5f + (_random.NextSingle() * 0.4f), // Alt: 0.5-0.9
                _ => 0.7f
            };
            
            // Random features
            _hasSolo = _random.NextDouble() > (_subgenre == 3 ? 0.7 : 0.4); // Punk less likely to have solos
            _hasKeyboard = _random.NextDouble() > (_subgenre == 3 ? 0.8 : 0.6); // Punk rarely has keyboards
            _songStructure = _random.Next(0, 3); // Simple, Extended, Complex
            _drumComplexity = _random.Next(1, 4); // 1=Simple, 2=Medium, 3=Complex
            
            // Generate chord progression
            GenerateProgression();
        }
        
        protected override int BeatsPerMinute => 120 + _tempoVariation;
        
        protected override List<MidiEvent> GenerateEvents(int durationSeconds)
        {
            var events = new List<MidiEvent>();
            int ticksPerBar = TicksPerBeat * 4;
            int totalBars = CalculateBarsFromDuration(durationSeconds);
            
            AddMetaEvents(events);
            AddProgramChanges(events);
            
            var structure = CreateRockStructure(totalBars);
            
            foreach (var section in structure)
            {
                for (int bar = section.StartBar; bar < section.StartBar + section.LengthBars; bar++)
                {
                    int barStart = bar * ticksPerBar;
                    AddSectionBasedMusic(events, barStart, section, bar - section.StartBar, bar);
                }
            }
            
            events.Add(new MidiEvent(totalBars * ticksPerBar, 0xFF, 0x2F, 0x00));
            return events;
        }
        
        private void GenerateProgression()
        {
            // Random rock progressions by subgenre
            var classicRockProgressions = new[]
            {
                new[] { 0, 5, 3, 7 },    // vi-IV-I-V (Em-C-G-D in G)
                new[] { 0, 10, 5, 7 },  // I-bVII-IV-V
                new[] { 0, 7, 5, 0 },   // I-V-IV-I
                new[] { 0, 3, 7, 5 }    // I-iii-V-IV
            };
            
            var hardRockProgressions = new[]
            {
                new[] { 0, 0, 5, 5 },   // I-I-IV-IV (power chord style)
                new[] { 0, 10, 0, 10 }, // I-bVII-I-bVII
                new[] { 0, 5, 10, 5 },  // I-IV-bVII-IV
                new[] { 0, 3, 5, 7 }    // I-iii-IV-V
            };
            
            var progressiveProgressions = new[]
            {
                new[] { 0, 2, 5, 8, 7, 10 }, // Complex 6-chord progression
                new[] { 0, 9, 5, 2, 7, 3 },  // Modal interchange
                new[] { 0, 4, 7, 11, 5, 9 }, // Jazz-influenced
                new[] { 0, 8, 3, 11, 5, 7 }  // Chromatic movement
            };
            
            var punkProgressions = new[]
            {
                new[] { 0, 5, 7, 5 },   // I-IV-V-IV (simple)
                new[] { 0, 0, 5, 7 },   // I-I-IV-V
                new[] { 0, 7, 5, 0 },   // I-V-IV-I
                new[] { 5, 5, 0, 7 }    // IV-IV-I-V
            };
            
            var alternativeProgressions = new[]
            {
                new[] { 0, 8, 5, 10 },  // I-bVI-IV-bVII
                new[] { 0, 3, 8, 5 },   // I-iii-bVI-IV
                new[] { 9, 5, 0, 7 },   // vi-IV-I-V
                new[] { 0, 5, 9, 8 }    // I-IV-vi-bVI
            };
            
            _currentProgression = _subgenre switch
            {
                0 => classicRockProgressions[_random.Next(classicRockProgressions.Length)],
                1 => hardRockProgressions[_random.Next(hardRockProgressions.Length)],
                2 => progressiveProgressions[_random.Next(progressiveProgressions.Length)],
                3 => punkProgressions[_random.Next(punkProgressions.Length)],
                4 => alternativeProgressions[_random.Next(alternativeProgressions.Length)],
                _ => classicRockProgressions[0]
            };
        }
        
        private List<SongStructure> CreateRockStructure(int totalBars)
        {
            var structure = new List<SongStructure>();
            
            if (totalBars <= 8)
            {
                structure.Add(new SongStructure { SectionName = "Riff", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else if (totalBars <= 16)
            {
                int verseLength = totalBars / 2;
                structure.Add(new SongStructure { SectionName = "Verse", StartBar = 0, LengthBars = verseLength, SectionType = SongSection.Verse });
                structure.Add(new SongStructure { SectionName = "Chorus", StartBar = verseLength, LengthBars = totalBars - verseLength, SectionType = SongSection.Chorus });
            }
            else
            {
                int currentBar = 0;
                
                // Intro
                int introLength = _songStructure == 0 ? 2 : 4;
                structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = introLength, SectionType = SongSection.Intro });
                currentBar += introLength;
                
                // Verse 1
                int verseLength = _subgenre == 3 ? 4 : 8; // Punk verses shorter
                structure.Add(new SongStructure { SectionName = "Verse 1", StartBar = currentBar, LengthBars = verseLength, SectionType = SongSection.Verse });
                currentBar += verseLength;
                
                // Chorus 1
                if (totalBars - currentBar >= 8)
                {
                    int chorusLength = _subgenre == 3 ? 4 : 8;
                    structure.Add(new SongStructure { SectionName = "Chorus 1", StartBar = currentBar, LengthBars = chorusLength, SectionType = SongSection.Chorus });
                    currentBar += chorusLength;
                }
                
                // Solo/Bridge (if enabled and space available)
                if (_hasSolo && totalBars - currentBar >= 12)
                {
                    int soloLength = _subgenre == 2 ? 8 : 4; // Prog solos longer
                    structure.Add(new SongStructure { SectionName = "Solo", StartBar = currentBar, LengthBars = soloLength, SectionType = SongSection.Solo });
                    currentBar += soloLength;
                }
                else if (totalBars - currentBar >= 8)
                {
                    structure.Add(new SongStructure { SectionName = "Bridge", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Bridge });
                    currentBar += 4;
                }
                
                // Final section
                int finalLength = totalBars - currentBar;
                if (finalLength > 0)
                {
                    var sectionType = finalLength >= 6 ? SongSection.Chorus : SongSection.Outro;
                    string sectionName = sectionType == SongSection.Chorus ? "Chorus 2" : "Outro";
                    structure.Add(new SongStructure { SectionName = sectionName, StartBar = currentBar, LengthBars = finalLength, SectionType = sectionType });
                }
            }
            
            return structure;
        }
        
        private void AddSectionBasedMusic(List<MidiEvent> events, int barStart, SongStructure section, int barInSection, int absoluteBar)
        {
            float sectionIntensity = section.SectionType switch
            {
                SongSection.Intro => 0.6f + (barInSection * 0.1f), // Build up
                SongSection.Verse => 0.8f,
                SongSection.Chorus => 1.0f,
                SongSection.Bridge => 0.7f,
                SongSection.Solo => 1.2f, // Solos are louder
                SongSection.Outro => Math.Max(0.4f, 1.0f - (barInSection * 0.15f)), // Fade out
                _ => 0.8f
            };
            
            switch (section.SectionType)
            {
                case SongSection.Intro:
                    if (barInSection == 0)
                    {
                        AddRockDrums(events, barStart, sectionIntensity * 0.7f);
                        if (_random.NextDouble() > 0.5) // Sometimes start with drums only
                            AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity * 0.6f);
                    }
                    else
                    {
                        AddRockDrums(events, barStart, sectionIntensity);
                        AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity);
                        AddBassGuitar(events, barStart, absoluteBar, sectionIntensity * 0.8f);
                        if (barInSection >= 2 && _hasKeyboard)
                            AddKeyboard(events, barStart, absoluteBar, sectionIntensity * 0.6f);
                    }
                    break;
                    
                case SongSection.Verse:
                    AddRockDrums(events, barStart, sectionIntensity);
                    AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity);
                    AddBassGuitar(events, barStart, absoluteBar, sectionIntensity);
                    if (_hasKeyboard && _random.NextDouble() > 0.6)
                        AddKeyboard(events, barStart, absoluteBar, sectionIntensity * 0.7f);
                    break;
                    
                case SongSection.Chorus:
                    AddRockDrums(events, barStart, sectionIntensity);
                    AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity);
                    AddBassGuitar(events, barStart, absoluteBar, sectionIntensity);
                    if (_hasKeyboard)
                        AddKeyboard(events, barStart, absoluteBar, sectionIntensity * 0.8f);
                    if (_random.NextDouble() > 0.5) // Sometimes lead guitar in chorus
                        AddLeadGuitar(events, barStart, absoluteBar, sectionIntensity * 0.9f);
                    break;
                    
                case SongSection.Bridge:
                    AddRockDrums(events, barStart, sectionIntensity * 0.8f);
                    if (_random.NextDouble() > 0.4)
                        AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity * 0.7f);
                    AddBassGuitar(events, barStart, absoluteBar, sectionIntensity * 0.9f);
                    if (_hasKeyboard)
                        AddKeyboard(events, barStart, absoluteBar, sectionIntensity);
                    break;
                    
                case SongSection.Solo:
                    AddRockDrums(events, barStart, sectionIntensity);
                    AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity * 0.8f);
                    AddBassGuitar(events, barStart, absoluteBar, sectionIntensity);
                    AddLeadGuitar(events, barStart, absoluteBar, sectionIntensity);
                    if (_hasKeyboard && _random.NextDouble() > 0.7)
                        AddKeyboard(events, barStart, absoluteBar, sectionIntensity * 0.6f);
                    break;
                    
                case SongSection.Outro:
                    AddRockDrums(events, barStart, sectionIntensity);
                    AddRhythmGuitar(events, barStart, absoluteBar, sectionIntensity);
                    AddBassGuitar(events, barStart, absoluteBar, sectionIntensity);
                    if (_hasKeyboard && _random.NextDouble() > 0.8)
                        AddKeyboard(events, barStart, absoluteBar, sectionIntensity);
                        
                    if (barInSection == section.LengthBars - 1)
                        AddRockEnding(events, barStart);
                    break;
            }
        }
        
        private void AddProgramChanges(List<MidiEvent> events)
        {
            AddProgramChange(events, 0, 0, _bassGuitar);         // Bass Guitar
            AddProgramChange(events, 0, 1, _rhythmGuitar);       // Rhythm Guitar
            AddProgramChange(events, 0, 2, _leadGuitar);         // Lead Guitar
            AddProgramChange(events, 0, 3, _keyboardInstrument); // Keyboard
            AddProgramChange(events, 0, 9, 0);                  // Drums
        }
        
        // Additional rock-specific methods would go here...
        // For brevity, implementing core drum patterns and basic guitar/bass patterns
        
        private void AddRockDrums(List<MidiEvent> events, int barStart, float intensity = 1.0f)
        {
            // Basic rock beat - kick on 1 and 3, snare on 2 and 4
            AddNote(events, barStart, 9, 36, (int)((100 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            AddNote(events, barStart + TicksPerBeat * 2, 9, 36, (int)((95 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            
            AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((95 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((90 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            
            // Hi-hat pattern with subgenre variations
            int hiHatDensity = _subgenre == 3 ? 16 : 8; // Punk uses 16th notes
            for (int i = 0; i < hiHatDensity; i++)
            {
                int velocity = (int)(((i % 4 == 0) ? 65 : 50) * intensity + _random.Next(-5, 5));
                AddNote(events, barStart + i * (TicksPerBeat * 4 / hiHatDensity), 9, 42, Math.Max(25, velocity), TicksPerBeat / 32);
            }
        }
        
        private void AddBassGuitar(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            int rootNote = 28 + _rootKey;
            int chordIndex = bar % _currentProgression.Length;
            int currentBassRoot = rootNote + _currentProgression[chordIndex];
            
            if (_subgenre == 3) // Punk - driving eighth notes
            {
                for (int eighth = 0; eighth < 8; eighth++)
                {
                    int note = (eighth % 4 == 2) ? currentBassRoot + 7 : currentBassRoot;
                    int velocity = (int)((85 + _random.Next(-10, 15)) * intensity);
                    AddNote(events, barStart + eighth * (TicksPerBeat/2), 0, note, Math.Max(50, velocity), TicksPerBeat/4);
                }
            }
            else // Standard rock bass
            {
                AddNote(events, barStart, 0, currentBassRoot, (int)((90 + _random.Next(-10, 15)) * intensity), TicksPerBeat);
                AddNote(events, barStart + TicksPerBeat * 2, 0, currentBassRoot, (int)((85 + _random.Next(-10, 15)) * intensity), TicksPerBeat);
                
                if (_random.NextDouble() > 0.6)
                {
                    int walkingNote = currentBassRoot + (_random.NextDouble() > 0.5 ? 2 : 5);
                    AddNote(events, barStart + TicksPerBeat + TicksPerBeat/2, 0, walkingNote, (int)((70 + _random.Next(-10, 15)) * intensity), TicksPerBeat/2);
                }
            }
        }
        
        private void AddRhythmGuitar(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            int rootNote = 40 + _rootKey;
            int chordIndex = bar % _currentProgression.Length;
            int currentChordRoot = rootNote + _currentProgression[chordIndex];
            
            // Power chord (root + fifth)
            var powerChord = new[] { currentChordRoot, currentChordRoot + 7 };
            
            if (_subgenre == 3) // Punk - fast downstrokes
            {
                for (int eighth = 0; eighth < 8; eighth++)
                {
                    foreach (int note in powerChord)
                    {
                        int velocity = (int)((90 + _random.Next(-5, 10)) * intensity * _distortionLevel);
                        AddNote(events, barStart + eighth * (TicksPerBeat/2), 1, note, Math.Max(60, velocity), TicksPerBeat/4);
                    }
                }
            }
            else // Standard rock rhythm
            {
                var strumPattern = new[] { 0, TicksPerBeat, TicksPerBeat * 2, TicksPerBeat * 3 };
                foreach (int offset in strumPattern)
                {
                    foreach (int note in powerChord)
                    {
                        int velocity = (int)((75 + _random.Next(-10, 15)) * intensity * _distortionLevel);
                        int duration = TicksPerBeat + _random.Next(-100, 200);
                        AddNote(events, barStart + offset, 1, note, Math.Max(45, velocity), Math.Max(duration, TicksPerBeat/4));
                    }
                }
            }
        }
        
        private void AddLeadGuitar(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            var scale = new[] { 0, 3, 5, 7, 10 }; // Minor pentatonic
            var scaleNotes = scale.Select(interval => 60 + _rootKey + interval + 12).ToArray();
            
            var pattern = _subgenre == 3 ? 
                new[] { 0, 2, 0, 2 } : // Punk - simple
                new[] { 0, 2, 4, 2, 3, 1, 0 }; // Standard rock
            
            int notesPerBar = Math.Min(pattern.Length, _subgenre == 3 ? 4 : 8);
            
            for (int i = 0; i < notesPerBar; i++)
            {
                int tick = barStart + i * (TicksPerBeat * 4 / notesPerBar);
                int note = scaleNotes[pattern[i] % scaleNotes.Length];
                
                int velocity = (int)((80 + _random.Next(-15, 20)) * intensity * _distortionLevel);
                int duration = TicksPerBeat / 2 + _random.Next(-100, 300);
                
                AddNote(events, tick, 2, note, Math.Max(50, velocity), Math.Max(duration, TicksPerBeat/8));
            }
        }
        
        private void AddKeyboard(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            if (!_hasKeyboard) return;
            
            int rootNote = 60 + _rootKey;
            int chordIndex = bar % _currentProgression.Length;
            int currentChordRoot = rootNote + _currentProgression[chordIndex];
            
            var chordVoicing = new[] { 0, 4, 7 }; // Basic triad
            
            foreach (int interval in chordVoicing)
            {
                int note = currentChordRoot + interval;
                int velocity = (int)((55 + _random.Next(-10, 15)) * intensity);
                int duration = TicksPerBeat * 4 + _random.Next(-200, 400);
                
                AddNote(events, barStart, 3, note, Math.Max(30, velocity), Math.Max(duration, TicksPerBeat * 2));
            }
        }
        
        private void AddRockEnding(List<MidiEvent> events, int barStart)
        {
            int rootNote = 28 + _rootKey;
            
            if (_subgenre == 3) // Punk - abrupt stop
            {
                AddNote(events, barStart, 0, rootNote, 110, TicksPerBeat / 8);
                AddNote(events, barStart, 1, rootNote + 12, 105, TicksPerBeat / 8);
                AddNote(events, barStart, 1, rootNote + 19, 100, TicksPerBeat / 8);
            }
            else // Standard rock ending
            {
                var finalChord = new[] { rootNote, rootNote + 12, rootNote + 19, rootNote + 24 };
                foreach (var note in finalChord)
                {
                    int channel = note < 36 ? 0 : 1;
                    int velocity = 100 + _random.Next(-10, 15);
                    AddNote(events, barStart, channel, note, velocity, TicksPerBeat * 4);
                }
                
                // Crash cymbal
                AddNote(events, barStart, 9, 49, 110 + _random.Next(-5, 10), TicksPerBeat * 4);
            }
        }
    }
}