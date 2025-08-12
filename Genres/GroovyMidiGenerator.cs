using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

namespace MidiGenerator.Genres
{
    public class GroovyMidiGenerator : BaseMidiGenerator
    {
        public override string GenreName => "Groovy";
        
        private Random _random = new Random();
        private int _rootKey;
        private int _tempoVariation;
        private int _bassProgram;
        private int _leadProgram;
        private int _padProgram;
        private float _swingAmount;
        private int[] _currentGroovePattern;
        
        public GroovyMidiGenerator()
        {
            // Random funk/groove keys
            var grooveKeys = new[] { 0, 2, 4, 5, 7, 9, 10 }; // C, D, E, F, G, A, Bb
            _rootKey = grooveKeys[_random.Next(grooveKeys.Length)];
            
            // Tempo micro-variations
            _tempoVariation = _random.Next(-10, 11); // ±10 BPM from base 120
            
            // Random bass instruments
            var bassOptions = new[] { 32, 33, 34, 35, 36, 37 }; // Acoustic Bass, Electric Bass Finger, Electric Bass Pick, Fretless, Slap Bass 1, Slap Bass 2
            _bassProgram = bassOptions[_random.Next(bassOptions.Length)];
            
            // Random lead instruments
            var leadOptions = new[] { 81, 82, 83, 84, 25, 26, 27, 4, 5 }; // Synth leads, guitars, EP, piano
            _leadProgram = leadOptions[_random.Next(leadOptions.Length)];
            
            // Random pad/texture instruments
            var padOptions = new[] { 88, 89, 90, 91, 92, 50, 51, 52 }; // Synth pads and strings
            _padProgram = padOptions[_random.Next(padOptions.Length)];
            
            // Random swing amount
            _swingAmount = 0.1f + (_random.NextSingle() * 0.3f); // 0.1 to 0.4
            
            // Generate groove pattern
            GenerateGroovePattern();
        }
        
        protected override int BeatsPerMinute => 120 + _tempoVariation;
        
        protected override List<MidiEvent> GenerateEvents(int durationSeconds)
        {
            var events = new List<MidiEvent>();
            int ticksPerBar = TicksPerBeat * 4;
            int totalBars = CalculateBarsFromDuration(durationSeconds);
            
            AddMetaEvents(events);
            AddProgramChanges(events);
            
            var structure = CreateGroovySongStructure(totalBars);
            
            foreach (var section in structure)
            {
                for (int bar = section.StartBar; bar < section.StartBar + section.LengthBars; bar++)
                {
                    int barStart = bar * ticksPerBar;
                    AddSectionBasedMusic(events, barStart, section, bar - section.StartBar);
                }
            }
            
            events.Add(new MidiEvent(totalBars * ticksPerBar, 0xFF, 0x2F, 0x00));
            return events;
        }
        
        private List<SongStructure> CreateGroovySongStructure(int totalBars)
        {
            var structure = new List<SongStructure>();
            
            if (totalBars <= 8)
            {
                structure.Add(new SongStructure { SectionName = "Groove", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else if (totalBars <= 16)
            {
                int buildupLength = Math.Min(4, totalBars / 3);
                structure.Add(new SongStructure { SectionName = "Buildup", StartBar = 0, LengthBars = buildupLength, SectionType = SongSection.Intro });
                structure.Add(new SongStructure { SectionName = "Main Groove", StartBar = buildupLength, LengthBars = totalBars - buildupLength, SectionType = SongSection.Verse });
            }
            else
            {
                int currentBar = 0;
                
                structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Intro });
                currentBar += 4;
                
                int grooveLength = Math.Min(12, Math.Max(8, (totalBars - currentBar) / 2));
                structure.Add(new SongStructure { SectionName = "Main Groove", StartBar = currentBar, LengthBars = grooveLength, SectionType = SongSection.Verse });
                currentBar += grooveLength;
                
                if (totalBars - currentBar >= 8)
                {
                    structure.Add(new SongStructure { SectionName = "Breakdown", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Breakdown });
                    currentBar += 4;
                }
                
                int remainingBars = totalBars - currentBar;
                if (remainingBars > 0)
                {
                    structure.Add(new SongStructure { SectionName = "Outro Groove", StartBar = currentBar, LengthBars = remainingBars, SectionType = SongSection.Outro });
                }
            }
            
            return structure;
        }
        
        private void AddSectionBasedMusic(List<MidiEvent> events, int barStart, SongStructure section, int barInSection)
        {
            switch (section.SectionType)
            {
                case SongSection.Intro:
                    if (barInSection == 0)
                    {
                        AddNote(events, barStart, 9, 36, 80, TicksPerBeat / 8);
                        AddNote(events, barStart + TicksPerBeat * 2, 9, 36, 80, TicksPerBeat / 8);
                    }
                    else
                    {
                        AddBassLine(events, barInSection, barStart);
                        AddDrumPattern(events, barStart, 0.6f);
                        if (barInSection >= 2)
                        {
                            AddHiHats(events, barStart, 0.5f);
                            AddPadTexture(events, barStart, barInSection, 0.3f);
                        }
                    }
                    break;
                    
                case SongSection.Verse:
                    AddBassLine(events, barInSection, barStart);
                    AddDrumPattern(events, barStart, 1.0f);
                    AddHiHats(events, barStart, 1.0f);
                    AddPadTexture(events, barStart, barInSection, 0.6f);
                    if (barInSection % 2 == 1)
                    {
                        AddGhostNotes(events, barInSection, barStart);
                        if (_random.NextDouble() > 0.6)
                            AddLeadMelody(events, barStart, barInSection);
                    }
                    break;
                    
                case SongSection.Breakdown:
                    if (barInSection % 2 == 0)
                        AddBassLine(events, barInSection, barStart);
                    AddNote(events, barStart + TicksPerBeat, 9, 38, 60, TicksPerBeat / 8);
                    AddNote(events, barStart + TicksPerBeat * 3, 9, 38, 60, TicksPerBeat / 8);
                    if (_random.NextDouble() > 0.7)
                        AddPadTexture(events, barStart, barInSection, 0.4f);
                    break;
                    
                case SongSection.Outro:
                    float intensity = barInSection < section.LengthBars / 2 ? 
                        0.5f + (barInSection * 0.5f / (section.LengthBars / 2)) :
                        1.0f - ((barInSection - section.LengthBars / 2) * 0.7f / (section.LengthBars / 2));
                        
                    AddBassLine(events, barInSection, barStart);
                    AddDrumPattern(events, barStart, intensity);
                    AddHiHats(events, barStart, intensity);
                    AddPadTexture(events, barStart, barInSection, intensity * 0.7f);
                    
                    if (_random.NextDouble() > 0.5)
                        AddLeadMelody(events, barStart, barInSection);
                    
                    if (barInSection == section.LengthBars - 1)
                        AddFinalHit(events, barStart);
                    break;
            }
        }
        
        private void AddProgramChanges(List<MidiEvent> events)
        {
            AddProgramChange(events, 0, 0, _bassProgram);    // Random Bass
            AddProgramChange(events, 0, 1, _leadProgram);    // Random Lead
            AddProgramChange(events, 0, 2, _padProgram);     // Random Pad/Texture
            AddProgramChange(events, 0, 9, 0);              // Drums
        }
        
        private void GenerateGroovePattern()
        {
            // Random groove chord progressions
            var grooveProgressions = new[]
            {
                // Pattern 1: Classic funk (i-bVII-IV-i)
                new[] { 0, 10, 5, 0 },
                // Pattern 2: Modal groove (i-bIII-bVII-IV)
                new[] { 0, 3, 10, 5 },
                // Pattern 3: Jazz-funk (i-ii-V-i)
                new[] { 0, 2, 7, 0 },
                // Pattern 4: Dorian groove (i-IV-bVII-i)
                new[] { 0, 5, 10, 0 },
                // Pattern 5: Extended funk (i-bVII-IV-V)
                new[] { 0, 10, 5, 7 }
            };
            
            _currentGroovePattern = grooveProgressions[_random.Next(grooveProgressions.Length)];
        }
        
        private void AddBassLine(List<MidiEvent> events, int bar, int barStart)
        {
            // Use the random groove pattern in the selected key
            int rootNote = 36 + _rootKey; // C2 transposed to selected key
            int chordIndex = bar % _currentGroovePattern.Length;
            int currentBassRoot = rootNote + _currentGroovePattern[chordIndex];
            
            // Random funk bass patterns
            var bassPatterns = new[]
            {
                // Pattern 1: Classic "The One" emphasis
                new[] { (0, 0, 90), (TicksPerBeat * 3 + TicksPerBeat/2, 7, 75) },
                // Pattern 2: Syncopated funk
                new[] { (0, 0, 85), (TicksPerBeat/2, 0, 60), (TicksPerBeat * 2, 0, 80), (TicksPerBeat * 3 + TicksPerBeat/4, 5, 70) },
                // Pattern 3: Walking funk
                new[] { (0, 0, 85), (TicksPerBeat, 2, 70), (TicksPerBeat * 2, 4, 75), (TicksPerBeat * 3, 3, 65) },
                // Pattern 4: Slap/pop simulation
                new[] { (0, 0, 95), (TicksPerBeat/4, 0, 50), (TicksPerBeat * 2, 7, 85), (TicksPerBeat * 2 + TicksPerBeat/4, 7, 45) },
                // Pattern 5: Disco-funk
                new[] { (0, 0, 90), (TicksPerBeat, 5, 70), (TicksPerBeat * 2, 0, 85), (TicksPerBeat * 3, 7, 75) }
            };
            
            var pattern = bassPatterns[_random.Next(bassPatterns.Length)];
            
            foreach (var (offset, interval, baseVelocity) in pattern)
            {
                int tick = barStart + offset;
                int note = currentBassRoot + interval;
                int velocity = baseVelocity + _random.Next(-10, 15);
                
                // Add swing timing
                if (offset % TicksPerBeat != 0 && offset % (TicksPerBeat/2) != 0)
                    tick += (int)(TicksPerBeat * _swingAmount / 4);
                    
                int duration = TicksPerBeat / 2 + _random.Next(-50, 50);
                AddNote(events, tick, 0, note, Math.Max(40, velocity), Math.Max(duration, TicksPerBeat/8));
            }
        }
        
        private void AddDrumPattern(List<MidiEvent> events, int barStart, float intensity = 1.0f)
        {
            // Random kick patterns
            var kickPatterns = new[]
            {
                // Pattern 1: Classic "The One"
                new[] { 0, TicksPerBeat * 2 },
                // Pattern 2: Syncopated funk
                new[] { 0, TicksPerBeat * 2, TicksPerBeat * 3 + TicksPerBeat/2 },
                // Pattern 3: Linear funk
                new[] { 0, TicksPerBeat + TicksPerBeat/4, TicksPerBeat * 2 + TicksPerBeat/2 },
                // Pattern 4: Disco four-on-floor
                new[] { 0, TicksPerBeat, TicksPerBeat * 2, TicksPerBeat * 3 },
                // Pattern 5: Broken beat
                new[] { 0, TicksPerBeat/2, TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat * 3/4 }
            };
            
            var kickPattern = kickPatterns[_random.Next(kickPatterns.Length)];
            
            foreach (int kickTime in kickPattern)
            {
                int velocity = (int)((90 + _random.Next(-10, 15)) * intensity);
                AddNote(events, barStart + kickTime, 9, 36, Math.Max(50, velocity), TicksPerBeat / 8);
            }
            
            // Snare with variations and ghost notes
            var snareHits = new[] { TicksPerBeat, TicksPerBeat * 3 };
            foreach (int snareTime in snareHits)
            {
                int velocity = (int)((100 + _random.Next(-5, 10)) * intensity);
                int timing = snareTime + _random.Next(-20, 20); // Timing variation
                AddNote(events, barStart + timing, 9, 38, Math.Max(60, velocity), TicksPerBeat / 8);
            }
            
            // Random ghost notes (light snare hits)
            if (_random.NextDouble() > 0.6)
            {
                var ghostPositions = new[] { TicksPerBeat/2, TicksPerBeat + TicksPerBeat/2, TicksPerBeat * 2 + TicksPerBeat/2, TicksPerBeat * 3 + TicksPerBeat/2 };
                foreach (int ghostPos in ghostPositions)
                {
                    if (_random.NextDouble() > 0.7)
                    {
                        int velocity = (int)((30 + _random.Next(-5, 10)) * intensity);
                        AddNote(events, barStart + ghostPos, 9, 38, Math.Max(20, velocity), TicksPerBeat / 16);
                    }
                }
            }
        }
        
        private void AddHiHats(List<MidiEvent> events, int barStart, float intensity = 1.0f)
        {
            // Random hi-hat patterns
            var hihatPatterns = new[]
            {
                // Pattern 1: 16th note groove
                16,
                // Pattern 2: 8th note swing
                8,
                // Pattern 3: Syncopated
                12,
                // Pattern 4: Linear groove
                10
            };
            
            int hihatDivision = hihatPatterns[_random.Next(hihatPatterns.Length)];
            
            for (int division = 0; division < hihatDivision; division++)
            {
                int tick = barStart + division * (TicksPerBeat * 4 / hihatDivision);
                
                // Add swing timing
                if (division % 2 == 1)
                    tick += (int)(TicksPerBeat * _swingAmount / 4);
                    
                int velocity = (int)(((division % 4 == 0) ? 55 : 35) * intensity + _random.Next(-5, 10));
                
                // Random open hi-hat
                int hihatNote = (_random.NextDouble() > 0.9) ? 46 : 42; // Open vs closed
                AddNote(events, tick, 9, hihatNote, Math.Max(20, velocity), TicksPerBeat / 16);
            }
        }
        
        private void AddFinalHit(List<MidiEvent> events, int barStart)
        {
            // Final hit in the selected key
            int rootNote = 36 + _rootKey;
            
            AddNote(events, barStart, 0, rootNote, 120 + _random.Next(-10, 10), TicksPerBeat * 4); // Bass
            AddNote(events, barStart, 9, 36, 120 + _random.Next(-5, 5), TicksPerBeat / 4); // Kick
            AddNote(events, barStart, 9, 49, 110 + _random.Next(-5, 5), TicksPerBeat * 2); // Crash
            
            // Random additional elements
            if (_random.NextDouble() > 0.6)
            {
                AddNote(events, barStart, 1, rootNote + 12, 100, TicksPerBeat * 4); // Lead harmony
                AddNote(events, barStart, 2, rootNote + 24, 80, TicksPerBeat * 4); // Pad
            }
        }
        
        private void AddLeadMelody(List<MidiEvent> events, int barStart, int bar)
        {
            // Funk/groove scales in the selected key
            var scaleTypes = new[]
            {
                // Dorian mode (funk favorite)
                new[] { 0, 2, 3, 5, 7, 9, 10 },
                // Minor pentatonic
                new[] { 0, 3, 5, 7, 10 },
                // Major pentatonic
                new[] { 0, 2, 4, 7, 9 },
                // Mixolydian mode
                new[] { 0, 2, 4, 5, 7, 9, 10 },
                // Blues scale
                new[] { 0, 3, 5, 6, 7, 10 }
            };
            
            var scale = scaleTypes[_random.Next(scaleTypes.Length)];
            var scaleNotes = scale.Select(interval => 60 + _rootKey + interval).ToArray(); // Middle register
            
            // Random melody patterns
            var melodyPatterns = new[]
            {
                // Pattern 1: Syncopated phrases
                new[] { (0, 0), (TicksPerBeat/2, 2), (TicksPerBeat * 2, 1), (TicksPerBeat * 3 + TicksPerBeat/4, 4) },
                // Pattern 2: Stepwise motion
                new[] { (0, 0), (TicksPerBeat, 1), (TicksPerBeat * 2, 2), (TicksPerBeat * 3, 3) },
                // Pattern 3: Intervallic jumps
                new[] { (TicksPerBeat/4, 0), (TicksPerBeat + TicksPerBeat/2, 4), (TicksPerBeat * 2 + TicksPerBeat/4, 2) },
                // Pattern 4: Call and response
                new[] { (0, 2), (TicksPerBeat/2, 4), (TicksPerBeat * 2, 1), (TicksPerBeat * 2 + TicksPerBeat/2, 3) }
            };
            
            var pattern = melodyPatterns[_random.Next(melodyPatterns.Length)];
            
            foreach (var (offset, scaleIndex) in pattern)
            {
                int tick = barStart + offset;
                int note = scaleNotes[scaleIndex % scaleNotes.Length];
                
                // Add swing timing
                if (offset % TicksPerBeat != 0 && offset % (TicksPerBeat/2) != 0)
                    tick += (int)(TicksPerBeat * _swingAmount / 4);
                    
                // Random octave variations
                if (_random.NextDouble() > 0.8)
                    note += 12 * (_random.NextDouble() > 0.5 ? 1 : -1);
                    
                note = Math.Max(48, Math.Min(84, note)); // Keep in reasonable range
                
                int velocity = 70 + _random.Next(-10, 15);
                int duration = TicksPerBeat / 2 + _random.Next(-100, 100);
                
                AddNote(events, tick, 1, note, Math.Max(40, velocity), Math.Max(duration, TicksPerBeat/8));
            }
        }
        
        private void AddPadTexture(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            // Use the current groove pattern for harmony
            int rootNote = 60 + _rootKey; // Middle C transposed
            int chordIndex = bar % _currentGroovePattern.Length;
            int currentChordRoot = rootNote + _currentGroovePattern[chordIndex];
            
            // Random chord voicings
            var chordTypes = new[]
            {
                // Sus2 chord
                new[] { 0, 2, 7 },
                // Add9 chord
                new[] { 0, 4, 7, 14 },
                // Minor 7th
                new[] { 0, 3, 7, 10 },
                // Major 7th
                new[] { 0, 4, 7, 11 },
                // Sus4 chord
                new[] { 0, 5, 7 }
            };
            
            var chordIntervals = chordTypes[_random.Next(chordTypes.Length)];
            
            // Random voicing positions
            var voicingPositions = new[]
            {
                new[] { 0, TicksPerBeat * 2 }, // Half notes
                new[] { 0, TicksPerBeat, TicksPerBeat * 2, TicksPerBeat * 3 }, // Quarter notes
                new[] { TicksPerBeat/2, TicksPerBeat * 2 + TicksPerBeat/2 }, // Syncopated
                new[] { 0 } // Whole note
            };
            
            var positions = voicingPositions[_random.Next(voicingPositions.Length)];
            
            foreach (int position in positions)
            {
                foreach (int interval in chordIntervals)
                {
                    int note = currentChordRoot + interval;
                    int velocity = (int)((50 + _random.Next(-10, 15)) * intensity);
                    int duration = TicksPerBeat * 2 + _random.Next(-200, 200);
                    
                    AddNote(events, barStart + position, 2, note, Math.Max(30, velocity), Math.Max(duration, TicksPerBeat));
                }
            }
        }
        
        private void AddGhostNotes(List<MidiEvent> events, int bar, int barStart)
        {
            // Use the current groove pattern for ghost notes
            int rootNote = 36 + _rootKey;
            int chordIndex = bar % _currentGroovePattern.Length;
            int bassNote = rootNote + _currentGroovePattern[chordIndex];
            
            // Random ghost note patterns
            var ghostPatterns = new[]
            {
                // Pattern 1: Subtle accents
                new[] { (TicksPerBeat/4, 0, 45), (TicksPerBeat * 2 + TicksPerBeat/4, 12, 55) },
                // Pattern 2: Chromatic approach
                new[] { (TicksPerBeat/2, 1, 50), (TicksPerBeat * 2 + TicksPerBeat/2, -1, 50) },
                // Pattern 3: Octave jumps
                new[] { (TicksPerBeat/4, 12, 60), (TicksPerBeat * 3 + TicksPerBeat/8, 0, 45) },
                // Pattern 4: Walking approach
                new[] { (TicksPerBeat/3, 2, 50), (TicksPerBeat * 2 + TicksPerBeat/3, 4, 55) }
            };
            
            var pattern = ghostPatterns[_random.Next(ghostPatterns.Length)];
            
            foreach (var (offset, interval, baseVelocity) in pattern)
            {
                int tick = barStart + offset;
                int note = bassNote + interval;
                int velocity = baseVelocity + _random.Next(-10, 10);
                
                // Add swing timing
                if (offset % TicksPerBeat != 0 && offset % (TicksPerBeat/2) != 0)
                    tick += (int)(TicksPerBeat * _swingAmount / 4);
                    
                AddNote(events, tick, 0, note, Math.Max(30, velocity), TicksPerBeat / 16);
            }
        }
    }
}