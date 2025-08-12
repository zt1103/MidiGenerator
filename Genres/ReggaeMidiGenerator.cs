
using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

public class ReggaeMidiGenerator : BaseMidiGenerator
{
    public override string GenreName => "Reggae";
    
    private static Random _staticRandom = new Random();
    private Random _random;
    private int _rootKey;
    private int _tempoVariation;
    private int _leadInstrument;
    private int _rhythmGuitar;
    private int _keyboardProgram;
    private int _currentRiddimType;
    private float _skankTightness;
    private int[] _currentProgression;
    private bool _isDubStyle;
    
    public ReggaeMidiGenerator()
    {
        _random = new Random(_staticRandom.Next());
        
        // Random reggae keys (often minor or modal)
        var reggaeKeys = new[] { 0, 2, 3, 5, 7, 8, 10 }; // C, D, Eb, F, G, Ab, Bb
        _rootKey = reggaeKeys[_random.Next(reggaeKeys.Length)];
        
        // Tempo variations (reggae is typically 60-90 BPM)
        _tempoVariation = _random.Next(-10, 16); // 65-90 BPM range
        
        // Random lead instruments
        var leadOptions = new[] { 73, 74, 81, 82, 65, 66, 4, 5 }; // Flute, Recorder, Synth leads, Sax, EP, Piano
        _leadInstrument = leadOptions[_random.Next(leadOptions.Length)];
        
        // Random rhythm guitar
        var guitarOptions = new[] { 26, 27, 28, 30 }; // Electric Jazz, Clean, Muted, Distortion
        _rhythmGuitar = guitarOptions[_random.Next(guitarOptions.Length)];
        
        // Random keyboard/organ
        var keyboardOptions = new[] { 16, 17, 18, 19, 4, 5 }; // Hammond organs, EP, Piano
        _keyboardProgram = keyboardOptions[_random.Next(keyboardOptions.Length)];
        
        // Random riddim type
        _currentRiddimType = _random.Next(0, 4); // 0=one drop, 1=rockers, 2=steppers, 3=nyabinghi
        
        // Random skank tightness
        _skankTightness = 0.7f + (_random.NextSingle() * 0.3f); // 0.7 to 1.0
        
        // Random dub style elements
        _isDubStyle = _random.NextDouble() > 0.7; // 30% chance
        
        // Generate chord progression
        GenerateProgression();
    }
    
    protected override int BeatsPerMinute => 75 + _tempoVariation;
    
    protected override List<MidiEvent> GenerateEvents(int durationSeconds)
    {
        var events = new List<MidiEvent>();
        int ticksPerBar = TicksPerBeat * 4;
        int totalBars = CalculateBarsFromDuration(durationSeconds);
        
        AddMetaEvents(events);
        AddProgramChanges(events);
        
        var structure = CreateReggaeStructure(totalBars);
        
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
        // Random reggae chord progressions
        var progressions = new[]
        {
            // Pattern 1: Classic i-bVII-bVI-bVII
            new[] { 0, 10, 8, 10 },
            // Pattern 2: i-iv-bVII-iv
            new[] { 0, 5, 10, 5 },
            // Pattern 3: i-bVI-bVII-i
            new[] { 0, 8, 10, 0 },
            // Pattern 4: i-bIII-bVII-iv
            new[] { 0, 3, 10, 5 },
            // Pattern 5: Modal interchange
            new[] { 0, 7, 8, 10 }
        };
        
        _currentProgression = progressions[_random.Next(progressions.Length)];
    }
    
    private System.Collections.Generic.List<SongStructure> CreateReggaeStructure(int totalBars)
    {
        var structure = new System.Collections.Generic.List<SongStructure>();
        
        if (totalBars <= 8)
        {
            structure.Add(new SongStructure { SectionName = "Riddim", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
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
            
            structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Intro });
            currentBar += 4;
            
            structure.Add(new SongStructure { SectionName = "Verse", StartBar = currentBar, LengthBars = 8, SectionType = SongSection.Verse });
            currentBar += 8;
            
            if (totalBars - currentBar >= 12)
            {
                structure.Add(new SongStructure { SectionName = "Chorus", StartBar = currentBar, LengthBars = 8, SectionType = SongSection.Chorus });
                currentBar += 8;
            }
            
            int bridgeLength = System.Math.Max(4, totalBars - currentBar - 4);
            structure.Add(new SongStructure { SectionName = "Bridge", StartBar = currentBar, LengthBars = bridgeLength, SectionType = SongSection.Bridge });
            currentBar += bridgeLength;
            
            int outroLength = totalBars - currentBar;
            if (outroLength > 0)
            {
                structure.Add(new SongStructure { SectionName = "Outro", StartBar = currentBar, LengthBars = outroLength, SectionType = SongSection.Outro });
            }
        }
        
        return structure;
    }
    
    private void AddSectionBasedMusic(System.Collections.Generic.List<MidiEvent> events, int barStart, SongStructure section, int barInSection, int absoluteBar)
    {
        switch (section.SectionType)
        {
            case SongSection.Intro:
                if (barInSection == 0)
                {
                    AddReggaeDrums(events, barStart, 0.5f);
                }
                else
                {
                    AddReggaeDrums(events, barStart, 0.7f + (barInSection * 0.1f));
                    if (barInSection >= 1)
                        AddBassLine(events, barStart, absoluteBar);
                    if (barInSection >= 2)
                        AddSkankGuitar(events, barStart, 0.6f);
                }
                break;
                
            case SongSection.Verse:
                AddReggaeDrums(events, barStart, 1.0f);
                AddBassLine(events, barStart, absoluteBar);
                AddSkankGuitar(events, barStart, 1.0f);
                AddKeyboards(events, barStart, absoluteBar, 0.7f);
                break;
                
            case SongSection.Chorus:
                AddReggaeDrums(events, barStart, 1.0f);
                AddBassLine(events, barStart, absoluteBar);
                AddSkankGuitar(events, barStart, 1.2f);
                AddKeyboards(events, barStart, absoluteBar, 1.0f);
                AddMelody(events, barStart, absoluteBar);
                break;
                
            case SongSection.Bridge:
                AddReggaeDrums(events, barStart, 0.8f);
                AddBassLine(events, barStart, absoluteBar);
                AddKeyboards(events, barStart, absoluteBar, 0.9f);
                if (barInSection % 2 == 0)
                    AddMelody(events, barStart, absoluteBar);
                break;
                
            case SongSection.Outro:
                float intensity = System.Math.Max(0.2f, 1.0f - (float)barInSection / section.LengthBars);
                AddReggaeDrums(events, barStart, intensity);
                AddBassLine(events, barStart, absoluteBar);
                AddSkankGuitar(events, barStart, intensity);
                
                if (barInSection == section.LengthBars - 1)
                    AddReggaeEnding(events, barStart);
                break;
        }
    }
    
    private void AddProgramChanges(List<MidiEvent> events)
    {
        AddProgramChange(events, 0, 0, 32);              // Acoustic Bass (often electric bass in reggae)
        AddProgramChange(events, 0, 1, _rhythmGuitar);   // Random Rhythm Guitar
        AddProgramChange(events, 0, 2, _keyboardProgram); // Random Keyboard/Organ
        AddProgramChange(events, 0, 3, _leadInstrument); // Random Lead Instrument
        AddProgramChange(events, 0, 9, 0);              // Drums
    }
    
    private void AddReggaeDrums(List<MidiEvent> events, int barStart, float intensity = 1.0f)
    {
        // Different riddim types
        switch (_currentRiddimType)
        {
            case 0: // One Drop
                AddOneDropDrums(events, barStart, intensity);
                break;
            case 1: // Rockers
                AddRockersDrums(events, barStart, intensity);
                break;
            case 2: // Steppers
                AddSteppersDrums(events, barStart, intensity);
                break;
            case 3: // Nyabinghi-influenced
                AddNyabinghiDrums(events, barStart, intensity);
                break;
        }
        
        // Random percussion elements
        if (_random.NextDouble() > 0.7)
        {
            // Cowbell or woodblock
            int percNote = _random.NextDouble() > 0.5 ? 56 : 76; // Cowbell or Hi Wood Block
            int percTick = barStart + _random.Next(0, TicksPerBeat * 4);
            AddNote(events, percTick, 9, percNote, (int)((40 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 16);
        }
    }
    
    private void AddOneDropDrums(List<MidiEvent> events, int barStart, float intensity)
    {
        // Classic one drop - emphasis on beat 3, no kick on beat 1
        AddNote(events, barStart + TicksPerBeat * 2, 9, 36, (int)((95 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        
        // Snare on beats 2 and 4
        AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((85 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((80 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        
        // Hi-hat on off-beats with variations
        for (int offBeat = 0; offBeat < 4; offBeat++)
        {
            int tick = barStart + offBeat * TicksPerBeat + TicksPerBeat / 2;
            int velocity = (int)((55 + _random.Next(-5, 10)) * intensity);
            
            if (_random.NextDouble() > 0.2) // Sometimes skip for groove
                AddNote(events, tick, 9, 42, Math.Max(20, velocity), TicksPerBeat / 16);
        }
    }
    
    private void AddRockersDrums(List<MidiEvent> events, int barStart, float intensity)
    {
        // Rockers - kick on every beat
        for (int beat = 0; beat < 4; beat++)
        {
            int velocity = (int)((beat == 0 ? 95 : 80) * intensity + _random.Next(-5, 5));
            AddNote(events, barStart + beat * TicksPerBeat, 9, 36, Math.Max(50, velocity), TicksPerBeat / 8);
        }
        
        // Snare on 2 and 4
        AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((90 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((85 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        
        // Hi-hat with more activity
        for (int eighth = 0; eighth < 8; eighth++)
        {
            int tick = barStart + eighth * (TicksPerBeat / 2);
            int velocity = (int)(((eighth % 2 == 0) ? 60 : 45) * intensity + _random.Next(-5, 5));
            
            if (_random.NextDouble() > 0.15)
                AddNote(events, tick, 9, 42, Math.Max(20, velocity), TicksPerBeat / 16);
        }
    }
    
    private void AddSteppersDrums(List<MidiEvent> events, int barStart, float intensity)
    {
        // Steppers - kick on every beat, more mechanical
        for (int beat = 0; beat < 4; beat++)
        {
            int velocity = (int)((85 + _random.Next(-3, 3)) * intensity);
            AddNote(events, barStart + beat * TicksPerBeat, 9, 36, Math.Max(50, velocity), TicksPerBeat / 8);
        }
        
        // Snare with slight variations
        AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((80 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((75 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        
        // Consistent hi-hat
        for (int beat = 0; beat < 4; beat++)
        {
            int tick = barStart + beat * TicksPerBeat + TicksPerBeat / 2;
            int velocity = (int)((50 + _random.Next(-3, 5)) * intensity);
            AddNote(events, tick, 9, 42, Math.Max(25, velocity), TicksPerBeat / 16);
        }
    }
    
    private void AddNyabinghiDrums(List<MidiEvent> events, int barStart, float intensity)
    {
        // Nyabinghi-influenced - more traditional/roots feel
        // Kick on 1 and 3
        AddNote(events, barStart, 9, 36, (int)((90 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        AddNote(events, barStart + TicksPerBeat * 2, 9, 36, (int)((85 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        
        // Snare with traditional feel
        AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((70 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((65 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        
        // Sparse hi-hat
        if (_random.NextDouble() > 0.6)
        {
            AddNote(events, barStart + TicksPerBeat / 2, 9, 42, (int)((40 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 16);
            AddNote(events, barStart + TicksPerBeat * 2 + TicksPerBeat / 2, 9, 42, (int)((35 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 16);
        }
    }
    
    private void AddBassLine(List<MidiEvent> events, int barStart, int bar)
    {
        // Use the random progression in the selected key
        int rootNote = 36 + _rootKey; // Bass register
        int chordIndex = bar % _currentProgression.Length;
        int currentBassRoot = rootNote + _currentProgression[chordIndex];
        
        // Random reggae bass patterns
        var bassPatterns = new[]
        {
            // Pattern 1: Classic one drop bass
            new[] { (0, 0, 85), (TicksPerBeat * 2 + TicksPerBeat/2, 0, 70) },
            // Pattern 2: Rockers bass
            new[] { (0, 0, 90), (TicksPerBeat, 7, 65), (TicksPerBeat * 2, 0, 80), (TicksPerBeat * 3, 5, 60) },
            // Pattern 3: Walking reggae
            new[] { (0, 0, 85), (TicksPerBeat * 2, 7, 75), (TicksPerBeat * 3 + TicksPerBeat/4, 2, 60) },
            // Pattern 4: Steppers bass
            new[] { (0, 0, 80), (TicksPerBeat/2, 0, 50), (TicksPerBeat, 0, 80), (TicksPerBeat * 2, 0, 80), (TicksPerBeat * 3, 0, 80) },
            // Pattern 5: Syncopated
            new[] { (0, 0, 90), (TicksPerBeat + TicksPerBeat/4, 5, 65), (TicksPerBeat * 2 + TicksPerBeat/2, 0, 75) }
        };
        
        var pattern = bassPatterns[_random.Next(bassPatterns.Length)];
        
        foreach (var (offset, interval, baseVelocity) in pattern)
        {
            int tick = barStart + offset;
            int note = currentBassRoot + interval;
            int velocity = (int)((baseVelocity + _random.Next(-10, 15)));
            
            // Add slight timing variations for human feel
            if (_random.NextDouble() > 0.8)
                tick += _random.Next(-20, 20);
                
            int duration = TicksPerBeat + _random.Next(-100, 200);
            AddNote(events, tick, 0, note, Math.Max(50, velocity), Math.Max(duration, TicksPerBeat/4));
        }
        
        // Random dub-style dropouts
        if (_isDubStyle && _random.NextDouble() > 0.7)
        {
            // Skip some notes for dub effect - this is just a placeholder for the concept
            // In practice, you'd need more sophisticated dropout logic
        }
    }
    
    private void AddSkankGuitar(List<MidiEvent> events, int barStart, float intensity = 1.0f)
    {
        // Use the random progression in the selected key
        int rootNote = 48 + _rootKey; // Guitar register
        int chordIndex = (barStart / (TicksPerBeat * 4)) % _currentProgression.Length;
        int currentChordRoot = rootNote + _currentProgression[chordIndex];
        
        // Random chord voicings
        var chordVoicings = new[]
        {
            // Basic triad
            new[] { 0, 4, 7 },
            // Minor triad (for minor progressions)
            new[] { 0, 3, 7 },
            // Sus2
            new[] { 0, 2, 7 },
            // Sus4
            new[] { 0, 5, 7 },
            // Add9
            new[] { 0, 4, 7, 14 }
        };
        
        var voicing = chordVoicings[_random.Next(chordVoicings.Length)];
        var chordNotes = voicing.Select(interval => currentChordRoot + interval).ToArray();
        
        // Random skank patterns
        var skankPatterns = new[]
        {
            // Pattern 1: Classic skank (off-beats)
            new[] { TicksPerBeat + TicksPerBeat/2, TicksPerBeat * 2 + TicksPerBeat/2, TicksPerBeat * 3 + TicksPerBeat/2 },
            // Pattern 2: Double skank
            new[] { TicksPerBeat/2, TicksPerBeat + TicksPerBeat/2, TicksPerBeat * 2 + TicksPerBeat/2, TicksPerBeat * 3 + TicksPerBeat/2 },
            // Pattern 3: Rockers skank
            new[] { TicksPerBeat/4, TicksPerBeat + TicksPerBeat/4, TicksPerBeat * 2 + TicksPerBeat/4, TicksPerBeat * 3 + TicksPerBeat/4 },
            // Pattern 4: One drop skank
            new[] { TicksPerBeat * 2 + TicksPerBeat/2, TicksPerBeat * 3 + TicksPerBeat/2 }
        };
        
        var pattern = skankPatterns[_random.Next(skankPatterns.Length)];
        
        foreach (int offset in pattern)
        {
            int tick = barStart + offset;
            
            // Add slight timing variations
            if (_random.NextDouble() > 0.7)
                tick += _random.Next(-15, 15);
            
            foreach (var note in chordNotes)
            {
                int velocity = (int)((45 + _random.Next(-5, 10)) * intensity * _skankTightness);
                int duration = TicksPerBeat / 8 + _random.Next(-20, 20); // Short, tight chops
                
                AddNote(events, tick, 1, note, Math.Max(25, velocity), Math.Max(duration, TicksPerBeat/16));
            }
        }
    }
    
    private void AddKeyboards(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
    {
        // Use the random progression in the selected key
        int rootNote = 60 + _rootKey; // Keyboard register
        int chordIndex = bar % _currentProgression.Length;
        int currentChordRoot = rootNote + _currentProgression[chordIndex];
        
        // Random keyboard patterns
        var keyboardPatterns = new[]
        {
            // Pattern 1: Classic bubble
            new[] { (TicksPerBeat/2, new[] { 0, 4, 7 }), (TicksPerBeat * 2 + TicksPerBeat/2, new[] { 0, 4, 7 }) },
            // Pattern 2: Organ stabs
            new[] { (TicksPerBeat, new[] { 0, 3, 7 }), (TicksPerBeat * 3, new[] { 0, 3, 7 }) },
            // Pattern 3: Bubbling eighth notes
            new[] { (TicksPerBeat/2, new[] { 7 }), (TicksPerBeat + TicksPerBeat/2, new[] { 4 }), (TicksPerBeat * 2 + TicksPerBeat/2, new[] { 0 }), (TicksPerBeat * 3 + TicksPerBeat/2, new[] { 4 }) },
            // Pattern 4: Syncopated chords
            new[] { (TicksPerBeat/4, new[] { 0, 4, 7, 10 }), (TicksPerBeat * 2 + TicksPerBeat/4, new[] { 0, 4, 7, 10 }) }
        };
        
        var pattern = keyboardPatterns[_random.Next(keyboardPatterns.Length)];
        
        foreach (var (offset, intervals) in pattern)
        {
            int tick = barStart + offset;
            
            // Add slight timing variations
            if (_random.NextDouble() > 0.8)
                tick += _random.Next(-10, 10);
            
            foreach (int interval in intervals)
            {
                int note = currentChordRoot + interval;
                int velocity = (int)((40 + _random.Next(-5, 15)) * intensity);
                int duration = TicksPerBeat / 4 + _random.Next(-50, 50);
                
                AddNote(events, tick, 2, note, Math.Max(25, velocity), Math.Max(duration, TicksPerBeat/8));
            }
        }
        
        // Random dub-style effects
        if (_isDubStyle && _random.NextDouble() > 0.8)
        {
            // Add echo simulation
            int echoTick = barStart + TicksPerBeat * 3;
            int echoNote = currentChordRoot + 7; // Fifth
            AddNote(events, echoTick, 2, echoNote, (int)(30 * intensity), TicksPerBeat / 8);
        }
    }
    
    private void AddMelody(List<MidiEvent> events, int barStart, int bar)
    {
        // Different scales based on lead instrument
        var scaleTypes = new[]
        {
            // Minor pentatonic
            new[] { 0, 3, 5, 7, 10 },
            // Natural minor
            new[] { 0, 2, 3, 5, 7, 8, 10 },
            // Dorian mode
            new[] { 0, 2, 3, 5, 7, 9, 10 },
            // Harmonic minor
            new[] { 0, 2, 3, 5, 7, 8, 11 }
        };
        
        var scale = scaleTypes[_random.Next(scaleTypes.Length)];
        var scaleNotes = scale.Select(interval => 60 + _rootKey + interval + 12).ToArray(); // Higher register
        
        // Random melody patterns
        var melodyPatterns = new[]
        {
            // Pattern 1: Flowing phrases
            new[] { 0, 1, 2, 4, 3, 1 },
            // Pattern 2: Call and response
            new[] { 0, 2, 4, 2 },
            // Pattern 3: Descending runs
            new[] { 4, 3, 2, 1, 0 },
            // Pattern 4: Syncopated melody
            new[] { 1, 0, 3, 2, 4 }
        };
        
        var pattern = melodyPatterns[_random.Next(melodyPatterns.Length)];
        
        for (int i = 0; i < Math.Min(pattern.Length, 4); i++)
        {
            int tick = barStart + i * TicksPerBeat;
            int note = scaleNotes[pattern[i] % scaleNotes.Length];
            
            // Add timing variations for reggae feel
            if (i % 2 == 1 && _random.NextDouble() > 0.6)
                tick += TicksPerBeat / 4; // Slight delay
                
            // Adjust register and style based on lead instrument
            switch (_leadInstrument)
            {
                case 73: // Flute - breathy, flowing
                case 74: // Recorder
                    note += _random.Next(0, 12); // Higher register variation
                    break;
                case 81: // Synth lead - can go anywhere
                case 82:
                    if (_random.NextDouble() > 0.7)
                        note += 12 * (_random.NextDouble() > 0.5 ? 1 : -1);
                    break;
                case 65: // Sax - mid register, soulful
                case 66:
                    note = Math.Max(60, Math.Min(84, note)); // Keep in comfortable sax range
                    break;
            }
            
            int velocity = 65 + _random.Next(-10, 15);
            int duration = TicksPerBeat / 2 + _random.Next(-100, 200);
            
            AddNote(events, tick, 3, note, Math.Max(40, velocity), Math.Max(duration, TicksPerBeat/4));
        }
    }
    
    private void AddReggaeEnding(List<MidiEvent> events, int barStart)
    {
        // Final chord in the selected key
        int rootNote = 36 + _rootKey;
        
        var finalChord = new[] 
        { 
            rootNote,           // Bass root
            rootNote + 12,      // Guitar root
            rootNote + 15,      // Minor third (often used in reggae)
            rootNote + 19,      // Fifth
            rootNote + 24       // High root
        };
        
        // Add random chord extensions
        if (_random.NextDouble() > 0.6)
        {
            finalChord = finalChord.Concat(new[] { rootNote + 22 }).ToArray(); // Seventh
        }
        
        // Hit on beat 1 with characteristic reggae timing
        foreach (var note in finalChord)
        {
            int channel = note < 48 ? 0 : (note < 60 ? 1 : 2); // Bass, guitar, or keyboard
            int velocity = 80 + _random.Next(-10, 15);
            AddNote(events, barStart, channel, note, velocity, TicksPerBeat * 4);
        }
        
        // Final drum accent on beat 1
        AddNote(events, barStart, 9, 36, 100 + _random.Next(-5, 10), TicksPerBeat / 4);
        AddNote(events, barStart, 9, 49, 90 + _random.Next(-5, 10), TicksPerBeat * 2);
        
        // Random final lead flourish
        if (_random.NextDouble() > 0.5)
        {
            int flourishNote = rootNote + 24 + 7; // Fifth above the root
            AddNote(events, barStart + TicksPerBeat, 3, flourishNote, 70, TicksPerBeat);
        }
        
        // Dub-style echo if applicable
        if (_isDubStyle)
        {
            AddNote(events, barStart + TicksPerBeat * 2, 2, rootNote + 19, 40, TicksPerBeat / 2); // Echo the fifth
        }
    }
}