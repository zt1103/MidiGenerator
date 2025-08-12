using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

namespace MidiGenerator.Genres
{
    public class ElectronicMidiGenerator : BaseMidiGenerator
    {
        public override string GenreName => "Electronic";
        
        private static Random _staticRandom = new Random();
        private Random _random;
        private int _rootKey;
        private int _tempoVariation;
        private int _subgenre; // 0=House, 1=Techno, 2=Trance, 3=Ambient, 4=DnB
        private int _leadSynth;
        private int _bassSynth;
        private int _padSynth;
        private int _arpSynth;
        private float _filterCutoff;
        private int[] _currentProgression;
        private bool _hasBreakdown;
        private int _energyLevel;
        
        public ElectronicMidiGenerator()
        {
            // Create a unique Random instance with a different seed
            _random = new Random(_staticRandom.Next());
            
            // Random subgenre determines overall characteristics
            _subgenre = _random.Next(0, 5);
            
            // Random electronic-friendly keys
            var electronicKeys = new[] { 0, 2, 5, 7, 9, 10 }; // C, D, F, G, A, Bb
            _rootKey = electronicKeys[_random.Next(electronicKeys.Length)];
            
            // Tempo variations by subgenre
            _tempoVariation = _subgenre switch
            {
                0 => _random.Next(-8, 12),  // House: 120-140 BPM
                1 => _random.Next(5, 25),   // Techno: 133-153 BPM  
                2 => _random.Next(-5, 15),  // Trance: 123-143 BPM
                3 => _random.Next(-48, -18), // Ambient: 80-110 BPM
                4 => _random.Next(40, 70),  // DnB: 168-198 BPM
                _ => 0
            };
            
            // Random synth programs
            var synthLeads = new[] { 80, 81, 82, 83, 84, 85, 86, 87 }; // Synth leads
            var synthBass = new[] { 38, 39, 81, 82 }; // Synth bass and some leads
            var synthPads = new[] { 88, 89, 90, 91, 92, 93, 94, 95 }; // Synth pads
            var synthArp = new[] { 80, 81, 82, 83, 102, 103 }; // Arpeggiated sounds
            
            _leadSynth = synthLeads[_random.Next(synthLeads.Length)];
            _bassSynth = synthBass[_random.Next(synthBass.Length)];
            _padSynth = synthPads[_random.Next(synthPads.Length)];
            _arpSynth = synthArp[_random.Next(synthArp.Length)];
            
            // Random filter cutoff simulation (affects velocity/timing)
            _filterCutoff = 0.4f + (_random.NextSingle() * 0.6f); // 0.4 to 1.0
            
            // Random breakdown sections
            _hasBreakdown = _random.NextDouble() > 0.6; // 40% chance
            
            // Energy level affects intensity
            _energyLevel = _random.Next(1, 4); // 1=Chill, 2=Medium, 3=High Energy
            
            // Generate chord progression
            GenerateProgression();
        }
        
        protected override int BeatsPerMinute => 128 + _tempoVariation;
        
        protected override List<MidiEvent> GenerateEvents(int durationSeconds)
        {
            var events = new List<MidiEvent>();
            int ticksPerBar = TicksPerBeat * 4;
            int totalBars = CalculateBarsFromDuration(durationSeconds);
            
            AddMetaEvents(events);
            AddProgramChanges(events);
            
            var structure = CreateElectronicStructure(totalBars);
            
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
            // Random electronic progressions by subgenre
            var houseProgressions = new[]
            {
                new[] { 0, 5, 3, 7 },    // vi-IV-I-V (relative minor)
                new[] { 0, 7, 10, 5 },  // i-V-bVII-IV
                new[] { 0, 10, 5, 7 }   // i-bVII-IV-V
            };
            
            var technoProgressions = new[]
            {
                new[] { 0, 0, 10, 10 }, // i-i-bVII-bVII (minimal)
                new[] { 0, 5, 0, 5 },   // i-IV-i-IV
                new[] { 0, 7, 5, 10 }   // i-V-IV-bVII
            };
            
            var tranceProgressions = new[]
            {
                new[] { 0, 5, 9, 7 },   // i-IV-vi-V
                new[] { 0, 10, 5, 7 },  // i-bVII-IV-V
                new[] { 9, 5, 0, 7 }    // vi-IV-I-V
            };
            
            var ambientProgressions = new[]
            {
                new[] { 0, 3, 7, 10 },  // i-bIII-V-bVII
                new[] { 0, 5, 8, 10 },  // i-IV-bVI-bVII
                new[] { 0, 2, 5, 7 }    // i-ii-IV-V
            };
            
            var dnbProgressions = new[]
            {
                new[] { 0, 0, 0, 0 },   // Single chord (rhythm focus)
                new[] { 0, 10, 0, 10 }, // i-bVII alternating
                new[] { 0, 5, 10, 5 }   // i-IV-bVII-IV
            };
            
            _currentProgression = _subgenre switch
            {
                0 => houseProgressions[_random.Next(houseProgressions.Length)],
                1 => technoProgressions[_random.Next(technoProgressions.Length)],
                2 => tranceProgressions[_random.Next(tranceProgressions.Length)],
                3 => ambientProgressions[_random.Next(ambientProgressions.Length)],
                4 => dnbProgressions[_random.Next(dnbProgressions.Length)],
                _ => houseProgressions[0]
            };
        }
        
        private List<SongStructure> CreateElectronicStructure(int totalBars)
        {
            var structure = new List<SongStructure>();
            
            if (totalBars <= 8)
            {
                structure.Add(new SongStructure { SectionName = "Loop", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else if (totalBars <= 16)
            {
                int buildLength = totalBars / 3;
                structure.Add(new SongStructure { SectionName = "Build", StartBar = 0, LengthBars = buildLength, SectionType = SongSection.Intro });
                structure.Add(new SongStructure { SectionName = "Drop", StartBar = buildLength, LengthBars = totalBars - buildLength, SectionType = SongSection.Chorus });
            }
            else
            {
                int currentBar = 0;
                
                // Intro build
                structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Intro });
                currentBar += 4;
                
                // First drop/main section
                int dropLength = _subgenre == 3 ? 12 : 8; // Ambient sections longer
                structure.Add(new SongStructure { SectionName = "Drop 1", StartBar = currentBar, LengthBars = dropLength, SectionType = SongSection.Verse });
                currentBar += dropLength;
                
                // Breakdown (if enabled)
                if (_hasBreakdown && totalBars - currentBar >= 8)
                {
                    structure.Add(new SongStructure { SectionName = "Breakdown", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Breakdown });
                    currentBar += 4;
                }
                
                // Final section
                int finalLength = totalBars - currentBar;
                if (finalLength > 0)
                {
                    var sectionType = finalLength >= 8 ? SongSection.Chorus : SongSection.Outro;
                    structure.Add(new SongStructure { SectionName = "Drop 2", StartBar = currentBar, LengthBars = finalLength, SectionType = sectionType });
                }
            }
            
            return structure;
        }
        
        private void AddSectionBasedMusic(List<MidiEvent> events, int barStart, SongStructure section, int barInSection, int absoluteBar)
        {
            float sectionIntensity = section.SectionType switch
            {
                SongSection.Intro => 0.3f + (barInSection * 0.15f), // Build up
                SongSection.Verse => 0.8f + (_energyLevel * 0.1f),
                SongSection.Chorus => 1.0f + (_energyLevel * 0.1f),
                SongSection.Breakdown => Math.Max(0.2f, 0.8f - (barInSection * 0.2f)), // Wind down
                SongSection.Outro => Math.Max(0.1f, 1.0f - (barInSection * 0.3f)),
                _ => 0.8f
            };
            
            switch (section.SectionType)
            {
                case SongSection.Intro:
                    AddElectronicDrums(events, barStart, sectionIntensity * 0.6f);
                    if (barInSection >= 1)
                        AddBassSynth(events, barStart, absoluteBar, sectionIntensity * 0.7f);
                    if (barInSection >= 2)
                        AddPadSynth(events, barStart, absoluteBar, sectionIntensity * 0.5f);
                    if (barInSection >= 3)
                        AddArpeggiator(events, barStart, absoluteBar, sectionIntensity * 0.6f);
                    break;
                    
                case SongSection.Verse:
                case SongSection.Chorus:
                    AddElectronicDrums(events, barStart, sectionIntensity);
                    AddBassSynth(events, barStart, absoluteBar, sectionIntensity);
                    AddPadSynth(events, barStart, absoluteBar, sectionIntensity * 0.8f);
                    AddArpeggiator(events, barStart, absoluteBar, sectionIntensity);
                    
                    if (section.SectionType == SongSection.Chorus || _random.NextDouble() > 0.6)
                        AddLeadSynth(events, barStart, absoluteBar, sectionIntensity);
                    break;
                    
                case SongSection.Breakdown:
                    AddElectronicDrums(events, barStart, sectionIntensity * 0.3f);
                    if (_random.NextDouble() > 0.5)
                        AddPadSynth(events, barStart, absoluteBar, sectionIntensity);
                    if (barInSection >= 2)
                        AddArpeggiator(events, barStart, absoluteBar, sectionIntensity * 0.5f);
                    break;
                    
                case SongSection.Outro:
                    AddElectronicDrums(events, barStart, sectionIntensity);
                    AddBassSynth(events, barStart, absoluteBar, sectionIntensity);
                    if (_random.NextDouble() > 0.7)
                        AddPadSynth(events, barStart, absoluteBar, sectionIntensity);
                        
                    if (barInSection == section.LengthBars - 1)
                        AddElectronicEnding(events, barStart);
                    break;
            }
        }
        
        private void AddProgramChanges(List<MidiEvent> events)
        {
            AddProgramChange(events, 0, 0, _bassSynth);    // Bass Synth
            AddProgramChange(events, 0, 1, _leadSynth);    // Lead Synth
            AddProgramChange(events, 0, 2, _padSynth);     // Pad Synth
            AddProgramChange(events, 0, 3, _arpSynth);     // Arp Synth
            AddProgramChange(events, 0, 9, 0);            // Drums
        }
        
        private void AddElectronicDrums(List<MidiEvent> events, int barStart, float intensity = 1.0f)
        {
            switch (_subgenre)
            {
                case 0: AddHouseDrums(events, barStart, intensity); break;
                case 1: AddTechnoDrums(events, barStart, intensity); break;
                case 2: AddTranceDrums(events, barStart, intensity); break;
                case 3: AddAmbientDrums(events, barStart, intensity); break;
                case 4: AddDnBDrums(events, barStart, intensity); break;
            }
        }
        
        private void AddHouseDrums(List<MidiEvent> events, int barStart, float intensity)
        {
            for (int beat = 0; beat < 4; beat++)
            {
                int velocity = (int)((100 + _random.Next(-5, 10)) * intensity);
                AddNote(events, barStart + beat * TicksPerBeat, 9, 36, Math.Max(50, velocity), TicksPerBeat / 8);
            }
            AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((90 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((85 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            for (int eighth = 1; eighth < 8; eighth += 2)
            {
                int velocity = (int)((60 + _random.Next(-10, 15)) * intensity);
                AddNote(events, barStart + eighth * (TicksPerBeat / 2), 9, 42, Math.Max(25, velocity), TicksPerBeat / 16);
            }
        }
        
        private void AddTechnoDrums(List<MidiEvent> events, int barStart, float intensity)
        {
            for (int beat = 0; beat < 4; beat++)
            {
                int velocity = (int)((110 + _random.Next(-5, 5)) * intensity);
                AddNote(events, barStart + beat * TicksPerBeat, 9, 36, Math.Max(60, velocity), TicksPerBeat / 16);
            }
            if (_random.NextDouble() > 0.3)
                AddNote(events, barStart + TicksPerBeat * 2, 9, 38, (int)((80 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            for (int sixteenth = 2; sixteenth < 16; sixteenth += 4)
            {
                if (_random.NextDouble() > 0.4)
                {
                    int velocity = (int)((45 + _random.Next(-5, 10)) * intensity);
                    AddNote(events, barStart + sixteenth * (TicksPerBeat / 4), 9, 42, Math.Max(20, velocity), TicksPerBeat / 32);
                }
            }
        }
        
        private void AddTranceDrums(List<MidiEvent> events, int barStart, float intensity)
        {
            for (int beat = 0; beat < 4; beat++)
            {
                int velocity = (int)((105 + _random.Next(-5, 10)) * intensity);
                AddNote(events, barStart + beat * TicksPerBeat, 9, 36, Math.Max(55, velocity), TicksPerBeat / 8);
            }
            AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((95 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((90 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            for (int sixteenth = 0; sixteenth < 16; sixteenth++)
            {
                int velocity = (int)(((sixteenth % 4 == 0) ? 55 : 35) * intensity + _random.Next(-5, 5));
                AddNote(events, barStart + sixteenth * (TicksPerBeat / 4), 9, 42, Math.Max(20, velocity), TicksPerBeat / 32);
            }
        }
        
        private void AddAmbientDrums(List<MidiEvent> events, int barStart, float intensity)
        {
            if (_random.NextDouble() > 0.6)
                AddNote(events, barStart, 9, 36, (int)((60 + _random.Next(-10, 15)) * intensity), TicksPerBeat / 4);
            if (_random.NextDouble() > 0.7)
                AddNote(events, barStart + TicksPerBeat * 2, 9, 38, (int)((50 + _random.Next(-10, 15)) * intensity), TicksPerBeat / 8);
            if (_random.NextDouble() > 0.8)
            {
                int percTick = barStart + _random.Next(0, TicksPerBeat * 4);
                AddNote(events, percTick, 9, 80, (int)((30 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 16);
            }
        }
        
        private void AddDnBDrums(List<MidiEvent> events, int barStart, float intensity)
        {
            var dnbPatterns = new[]
            {
                new[] { (0, 36, 100), (TicksPerBeat/3, 38, 70), (TicksPerBeat, 38, 95), (TicksPerBeat + TicksPerBeat/3, 36, 80),
                       (TicksPerBeat * 2, 36, 90), (TicksPerBeat * 2 + TicksPerBeat/2, 38, 85) },
                new[] { (0, 36, 100), (TicksPerBeat/2, 38, 80), (TicksPerBeat + TicksPerBeat/4, 36, 85), 
                       (TicksPerBeat * 2, 38, 95), (TicksPerBeat * 3, 36, 90) }
            };
            var pattern = dnbPatterns[_random.Next(dnbPatterns.Length)];
            foreach (var (offset, drum, baseVelocity) in pattern)
            {
                int velocity = (int)((baseVelocity + _random.Next(-10, 10)) * intensity);
                AddNote(events, barStart + offset, 9, drum, Math.Max(40, velocity), TicksPerBeat / 16);
            }
        }
        
        private void AddBassSynth(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            int rootNote = 36 + _rootKey;
            int chordIndex = bar % _currentProgression.Length;
            int currentBassRoot = rootNote + _currentProgression[chordIndex];
            
            switch (_subgenre)
            {
                case 0: case 2:
                    for (int beat = 0; beat < 4; beat++)
                    {
                        int velocity = (int)((80 + _random.Next(-10, 15)) * intensity * _filterCutoff);
                        int duration = TicksPerBeat / 2 + _random.Next(-50, 50);
                        AddNote(events, barStart + beat * TicksPerBeat, 0, currentBassRoot, Math.Max(40, velocity), Math.Max(duration, TicksPerBeat/8));
                    }
                    break;
                case 1:
                    AddNote(events, barStart, 0, currentBassRoot, (int)((90 + _random.Next(-5, 10)) * intensity), TicksPerBeat * 2);
                    if (_random.NextDouble() > 0.6)
                        AddNote(events, barStart + TicksPerBeat * 2, 0, currentBassRoot, (int)((75 + _random.Next(-5, 10)) * intensity), TicksPerBeat * 2);
                    break;
                case 3:
                    AddNote(events, barStart, 0, currentBassRoot, (int)((60 + _random.Next(-10, 15)) * intensity), TicksPerBeat * 4);
                    break;
                case 4:
                    var dnbBassPattern = new[] { 0, TicksPerBeat/2, TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/3 };
                    foreach (int offset in dnbBassPattern)
                    {
                        int velocity = (int)((85 + _random.Next(-10, 15)) * intensity);
                        AddNote(events, barStart + offset, 0, currentBassRoot, Math.Max(50, velocity), TicksPerBeat / 4);
                    }
                    break;
            }
        }
        
        private void AddLeadSynth(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            var scaleTypes = new[]
            {
                new[] { 0, 2, 3, 5, 7, 8, 10 }, new[] { 0, 3, 5, 7, 10 }, new[] { 0, 2, 3, 5, 7, 9, 10 }, new[] { 0, 2, 3, 5, 7, 8, 11 }
            };
            var scale = scaleTypes[_random.Next(scaleTypes.Length)];
            var scaleNotes = scale.Select(interval => 60 + _rootKey + interval + 12).ToArray();
            var leadPatterns = new[]
            {
                new[] { 0, 2, 4, 2, 1, 3, 1, 0 }, new[] { 0, 1, 2, 3, 4, 3, 2, 1 }, new[] { 0, 4, 1, 5, 2, 6, 3, 0 }, new[] { 0, 0, 2, 4, 2, 0, 1, 3 }
            };
            var pattern = leadPatterns[_random.Next(leadPatterns.Length)];
            int notesPerBar = _subgenre == 3 ? 4 : 8;
            for (int i = 0; i < notesPerBar && i < pattern.Length; i++)
            {
                int tick = barStart + i * (TicksPerBeat * 4 / notesPerBar);
                int note = scaleNotes[pattern[i] % scaleNotes.Length];
                if (_random.NextDouble() > 0.8)
                    note += 12 * (_random.NextDouble() > 0.5 ? 1 : -1);
                note = Math.Max(60, Math.Min(96, note));
                int velocity = (int)((70 + _random.Next(-15, 20)) * intensity * _filterCutoff);
                int duration = TicksPerBeat / 2 + _random.Next(-100, 200);
                AddNote(events, tick, 1, note, Math.Max(40, velocity), Math.Max(duration, TicksPerBeat/8));
            }
        }
        
        private void AddPadSynth(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            int rootNote = 60 + _rootKey;
            int chordIndex = bar % _currentProgression.Length;
            int currentChordRoot = rootNote + _currentProgression[chordIndex];
            var chordVoicings = new[] { new[] { 0, 3, 7, 10 }, new[] { 0, 4, 7, 11 }, new[] { 0, 3, 7, 10, 14 }, new[] { 0, 4, 7, 11, 14 } };
            var voicing = chordVoicings[_random.Next(chordVoicings.Length)];
            foreach (int interval in voicing)
            {
                int note = currentChordRoot + interval;
                int velocity = (int)((50 + _random.Next(-10, 15)) * intensity);
                int duration = TicksPerBeat * 4 + _random.Next(-200, 400);
                AddNote(events, barStart, 2, note, Math.Max(25, velocity), Math.Max(duration, TicksPerBeat * 2));
            }
        }
        
        private void AddArpeggiator(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
        {
            int rootNote = 60 + _rootKey;
            int chordIndex = bar % _currentProgression.Length;
            int currentChordRoot = rootNote + _currentProgression[chordIndex];
            var arpPattern = new[] { 0, 3, 7, 10, 7, 3 };
            int arpSpeed = _subgenre == 3 ? 8 : 16;
            for (int i = 0; i < arpSpeed && i < arpPattern.Length; i++)
            {
                int tick = barStart + i * (TicksPerBeat * 4 / arpSpeed);
                int note = currentChordRoot + arpPattern[i % arpPattern.Length];
                if (i >= arpPattern.Length / 2) note += 12;
                int velocity = (int)((55 + _random.Next(-10, 15)) * intensity * _filterCutoff);
                int duration = TicksPerBeat / 8 + _random.Next(-20, 50);
                AddNote(events, tick, 3, note, Math.Max(30, velocity), Math.Max(duration, TicksPerBeat/16));
            }
        }
        
        private void AddElectronicEnding(List<MidiEvent> events, int barStart)
        {
            int rootNote = 36 + _rootKey;
            var finalChord = new[] { rootNote, rootNote + 12, rootNote + 15, rootNote + 19, rootNote + 22, rootNote + 26 };
            switch (_subgenre)
            {
                case 0: case 2:
                    foreach (var note in finalChord)
                    {
                        int channel = note < 48 ? 0 : (note < 72 ? 2 : 1);
                        int velocity = 90 + _random.Next(-10, 15);
                        AddNote(events, barStart, channel, note, velocity, TicksPerBeat * 4);
                    }
                    break;
                case 1:
                    AddNote(events, barStart, 0, rootNote, 80, TicksPerBeat * 4);
                    AddNote(events, barStart, 2, rootNote + 12, 60, TicksPerBeat * 4);
                    break;
                case 3:
                    foreach (var note in finalChord.Take(4))
                    {
                        int velocity = 50 + _random.Next(-10, 10);
                        AddNote(events, barStart, 2, note, velocity, TicksPerBeat * 4);
                    }
                    break;
                case 4:
                    AddNote(events, barStart, 0, rootNote, 100, TicksPerBeat / 4);
                    AddNote(events, barStart + TicksPerBeat/2, 1, rootNote + 12, 80, TicksPerBeat / 4);
                    AddNote(events, barStart + TicksPerBeat, 0, rootNote, 90, TicksPerBeat * 3);
                    break;
            }
            if (_subgenre != 3)
            {
                AddNote(events, barStart, 9, 36, 110 + _random.Next(-5, 10), TicksPerBeat / 4);
                AddNote(events, barStart, 9, 49, 100 + _random.Next(-5, 10), TicksPerBeat * 2);
            }
            if (_random.NextDouble() > 0.6)
            {
                for (int i = 1; i <= 3; i++)
                {
                    int sweepNote = rootNote + 12 + (i * 7);
                    int sweepVelocity = 40 + (i * 15);
                    AddNote(events, barStart + i * (TicksPerBeat/2), 1, sweepNote, sweepVelocity, TicksPerBeat/4);
                }
            }
        }
    }
}