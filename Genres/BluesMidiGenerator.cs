using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

namespace MidiGenerator.Genres
{
    public class BluesMidiGenerator : BaseMidiGenerator
    {
        public override string GenreName => "Blues";
        protected override int BeatsPerMinute => 100;
        
        private Random _random = new Random();
        private int _rootKey;
        private int _leadGuitarProgram;
        private float _shuffleAmount;
        
        public BluesMidiGenerator()
        {
            // Random blues keys
            var bluesKeys = new[] { 0, 2, 3, 5, 7, 10 }; // C, D, Eb, F, G, Bb
            _rootKey = bluesKeys[_random.Next(bluesKeys.Length)];
            
            // Random lead guitar sounds
            var guitarOptions = new[] { 27, 28, 29, 30 }; // Clean, Muted, Overdrive, Distortion
            _leadGuitarProgram = guitarOptions[_random.Next(guitarOptions.Length)];
            
            // Random shuffle intensity
            _shuffleAmount = 0.6f + (_random.NextSingle() * 0.3f); // 0.6 to 0.9
        }
        
        protected override List<MidiEvent> GenerateEvents(int durationSeconds)
        {
            var events = new List<MidiEvent>();
            int ticksPerBar = TicksPerBeat * 4;
            int totalBars = CalculateBarsFromDuration(durationSeconds);
            
            AddMetaEvents(events);
            AddProgramChanges(events);
            
            var structure = CreateBluesStructure(totalBars);
            
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
        
        private List<SongStructure> CreateBluesStructure(int totalBars)
        {
            var structure = new List<SongStructure>();
            
            if (totalBars <= 12)
            {
                structure.Add(new SongStructure { SectionName = "12-Bar Blues", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else
            {
                int currentBar = 0;
                
                if (totalBars > 16)
                {
                    structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Intro });
                    currentBar += 4;
                }
                
                int remaining = totalBars - currentBar;
                int firstChorusLength = Math.Min(12, remaining);
                structure.Add(new SongStructure { SectionName = "Verse 1", StartBar = currentBar, LengthBars = firstChorusLength, SectionType = SongSection.Verse });
                currentBar += firstChorusLength;
                
                while (totalBars - currentBar >= 12)
                {
                    structure.Add(new SongStructure { SectionName = "Solo Chorus", StartBar = currentBar, LengthBars = 12, SectionType = SongSection.Solo });
                    currentBar += 12;
                }
                
                int outroLength = totalBars - currentBar;
                if (outroLength > 0)
                {
                    structure.Add(new SongStructure { SectionName = "Outro", StartBar = currentBar, LengthBars = outroLength, SectionType = SongSection.Outro });
                }
            }
            
            return structure;
        }
        
        private void AddSectionBasedMusic(List<MidiEvent> events, int barStart, SongStructure section, int barInSection, int absoluteBar)
        {
            switch (section.SectionType)
            {
                case SongSection.Intro:
                    AddShuffleDrums(events, barStart, 0.6f);
                    if (barInSection >= 1)
                        AddBassLine(events, barStart, absoluteBar);
                    if (barInSection >= 2)
                        AddBluesChords(events, barStart, absoluteBar, 0.5f);
                    break;
                    
                case SongSection.Verse:
                    AddShuffleDrums(events, barStart, 1.0f);
                    AddBassLine(events, barStart, absoluteBar);
                    AddBluesChords(events, barStart, absoluteBar, 0.8f);
                    break;
                    
                case SongSection.Solo:
                    AddShuffleDrums(events, barStart, 1.0f);
                    AddBassLine(events, barStart, absoluteBar);
                    AddBluesChords(events, barStart, absoluteBar, 0.6f);
                    AddBluesGuitar(events, barStart, absoluteBar);
                    break;
                    
                case SongSection.Outro:
                    float intensity = Math.Max(0.3f, 1.0f - (float)barInSection / section.LengthBars);
                    AddShuffleDrums(events, barStart, intensity);
                    AddBassLine(events, barStart, absoluteBar);
                    AddBluesChords(events, barStart, absoluteBar, intensity);
                    
                    if (barInSection == section.LengthBars - 1)
                        AddBluesEnding(events, barStart);
                    break;
            }
        }
        
        private void AddProgramChanges(List<MidiEvent> events)
        {
            AddProgramChange(events, 0, 0, 32);              // Acoustic Bass
            AddProgramChange(events, 0, 1, 26);              // Electric Guitar (Jazz)
            AddProgramChange(events, 0, 2, _leadGuitarProgram); // Random Lead Guitar
            AddProgramChange(events, 0, 9, 0);               // Drums
        }
        
        private void AddShuffleDrums(List<MidiEvent> events, int barStart, float intensity = 1.0f)
        {
            // Randomize kick pattern
            var kickPatterns = new[]
            {
                new[] { 0, TicksPerBeat * 2 },
                new[] { 0, TicksPerBeat * 2, TicksPerBeat * 3 + TicksPerBeat/2 },
                new[] { 0, TicksPerBeat/2, TicksPerBeat * 2 }
            };
            
            var kickPattern = kickPatterns[_random.Next(kickPatterns.Length)];
            foreach (int kickTime in kickPattern)
            {
                int velocity = (int)((75 + _random.Next(-5, 10)) * intensity);
                AddNote(events, barStart + kickTime, 9, 36, velocity, TicksPerBeat / 8);
            }
            
            // Snare with slight timing variations
            int snareOffset = _random.Next(-20, 20);
            AddNote(events, barStart + TicksPerBeat + snareOffset, 9, 38, (int)((85 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            AddNote(events, barStart + TicksPerBeat * 3 + snareOffset, 9, 38, (int)((80 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            
            // Variable shuffle hi-hats
            for (int beat = 0; beat < 4; beat++)
            {
                int tick = barStart + beat * TicksPerBeat;
                int shuffleTick = tick + (int)(TicksPerBeat * _shuffleAmount);
                
                int velocity1 = (int)((45 + _random.Next(-5, 10)) * intensity);
                int velocity2 = (int)((30 + _random.Next(-5, 10)) * intensity);
                
                AddNote(events, tick, 9, 42, Math.Max(20, velocity1), TicksPerBeat / 8);
                if (_random.NextDouble() > 0.3) // Sometimes skip the shuffle note
                    AddNote(events, shuffleTick, 9, 42, Math.Max(15, velocity2), TicksPerBeat / 12);
            }
        }
        
        private void AddBassLine(List<MidiEvent> events, int barStart, int bar)
        {
            int[] bluesProgression = { 0, 0, 0, 0, 5, 5, 0, 0, 7, 5, 0, 0 };
            int progressionBar = bar % 12;
            int rootNote = 36 + _rootKey + bluesProgression[progressionBar]; // Transpose by root key
            
            // Random walking bass patterns
            var bassPatterns = new[]
            {
                // Pattern 1: Root-2nd-4th-3rd
                new[] { 0, 2, 4, 3 },
                // Pattern 2: Root-7th-5th-3rd
                new[] { 0, -2, 7, 3 },
                // Pattern 3: Root-3rd-5th-6th
                new[] { 0, 4, 7, 9 },
                // Pattern 4: Root-chromatic walk
                new[] { 0, 1, 2, 3 }
            };
            
            var pattern = bassPatterns[_random.Next(bassPatterns.Length)];
            
            for (int beat = 0; beat < 4; beat++)
            {
                int tick = barStart + beat * TicksPerBeat;
                int note = rootNote + pattern[beat];
                int velocity = 75 + _random.Next(-10, 15);
                
                // Add some timing variations
                if (beat % 2 == 1 && _random.NextDouble() > 0.7)
                    tick += _random.Next(-30, 30);
                    
                AddNote(events, tick, 0, note, velocity, TicksPerBeat / 2);
            }
        }
        
        private void AddBluesChords(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            int[] bluesProgression = { 0, 0, 0, 0, 5, 5, 0, 0, 7, 5, 0, 0 };
            int progressionBar = bar % 12;
            int rootNote = 60 + _rootKey + bluesProgression[progressionBar]; // Transpose by root key
            
            // Random chord voicings
            var chordVoicings = new[]
            {
                // Triad
                new[] { rootNote, rootNote + 4, rootNote + 7 },
                // 7th chord
                new[] { rootNote, rootNote + 4, rootNote + 7, rootNote + 10 },
                // 9th chord
                new[] { rootNote + 4, rootNote + 7, rootNote + 10, rootNote + 14 },
                // Shell voicing
                new[] { rootNote, rootNote + 4, rootNote + 10 }
            };
            
            var voicing = chordVoicings[_random.Next(chordVoicings.Length)];
            
            // Random comping patterns
            var compPatterns = new[]
            {
                new[] { 0, TicksPerBeat * 2 }, // On 1 and 3
                new[] { TicksPerBeat, TicksPerBeat * 3 }, // On 2 and 4
                new[] { TicksPerBeat/2, TicksPerBeat * 2 + TicksPerBeat/2 }, // Syncopated
                new[] { 0, TicksPerBeat, TicksPerBeat * 2, TicksPerBeat * 3 } // Four on the floor
            };
            
            var pattern = compPatterns[_random.Next(compPatterns.Length)];
            
            foreach (int compTime in pattern)
            {
                foreach (var note in voicing)
                {
                    int velocity = (int)((60 + _random.Next(-10, 15)) * intensity);
                    int duration = TicksPerBeat / 4 + _random.Next(-50, 50);
                    AddNote(events, barStart + compTime, 1, note, Math.Max(30, velocity), Math.Max(duration, TicksPerBeat/8));
                }
            }
        }
        
        private void AddBluesGuitar(List<MidiEvent> events, int barStart, int bar)
        {
            // Blues scale in the selected key
            int[] bluesScale = { 0, 3, 5, 6, 7, 10 }; // Intervals from root
            var scaleNotes = bluesScale.Select(interval => 60 + _rootKey + interval + 12).ToArray(); // Transpose and octave up
            
            // Random lick patterns
            var lickPatterns = new[]
            {
                // Pattern 1: Ascending blues run
                new[] { 0, 1, 2, 4 },
                // Pattern 2: Descending with bends
                new[] { 4, 3, 1, 0 },
                // Pattern 3: Call and response
                new[] { 0, 2, 0, 4 },
                // Pattern 4: Pentatonic phrase
                new[] { 2, 4, 2, 1, 0 }
            };
            
            var pattern = lickPatterns[_random.Next(lickPatterns.Length)];
            int notesInPattern = Math.Min(pattern.Length, 4);
            
            for (int i = 0; i < notesInPattern; i++)
            {
                int tick = barStart + i * TicksPerBeat;
                int note = scaleNotes[pattern[i] % scaleNotes.Length];
                int velocity = 70 + _random.Next(-10, 20);
                
                // Add timing variation for blues feel
                if (i % 2 == 1)
                    tick += (int)(TicksPerBeat * _shuffleAmount) - TicksPerBeat/2;
                    
                AddNote(events, tick, 2, note, velocity, TicksPerBeat / 2);
                
                // Random bend simulation
                if (_random.NextDouble() > 0.6)
                {
                    int bendTick = tick + TicksPerBeat/4;
                    int bendNote = note + (_random.NextDouble() > 0.5 ? 1 : 2); // Half or whole step bend
                    AddNote(events, bendTick, 2, bendNote, velocity - 20, TicksPerBeat / 8);
                }
            }
        }
        
        private void AddBluesEnding(List<MidiEvent> events, int barStart)
        {
            // Final chord in the selected key
            var finalChord = new[] 
            { 
                36 + _rootKey,      // Bass root
                48 + _rootKey,      // Mid root
                60 + _rootKey,      // High root
                64 + _rootKey,      // Third
                67 + _rootKey       // Fifth
            };
            
            foreach (var note in finalChord)
            {
                int channel = note < 48 ? 0 : 1;
                int velocity = 90 + _random.Next(-10, 20);
                AddNote(events, barStart, channel, note, velocity, TicksPerBeat * 4);
            }
            
            // Final drum hits
            AddNote(events, barStart, 9, 36, 110 + _random.Next(-10, 10), TicksPerBeat / 4);
            AddNote(events, barStart, 9, 49, 100 + _random.Next(-10, 10), TicksPerBeat * 2);
            
            // Random crash or ride ending
            if (_random.NextDouble() > 0.5)
                AddNote(events, barStart + TicksPerBeat, 9, 51, 80, TicksPerBeat);
        }
    }
}