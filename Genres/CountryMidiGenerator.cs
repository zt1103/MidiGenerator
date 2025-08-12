using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

public class CountryMidiGenerator : BaseMidiGenerator
{
    public override string GenreName => "Country";
    
    private Random _random = new Random();
    private int _rootKey;
    private int _tempoVariation;
    private int _leadInstrument;
    private int _rhythmGuitar;
    private int _bassProgram;
    private bool _isWaltzTime;
    private float _shuffleFeel;
    private int[] _currentProgression;
    private int _capoPosition;
    
    public CountryMidiGenerator()
    {
        // Random country-friendly keys
        var countryKeys = new[] { 0, 2, 4, 5, 7, 9 }; // C, D, E, F, G, A major
        _rootKey = countryKeys[_random.Next(countryKeys.Length)];
        
        // Random capo position (transpose up)
        _capoPosition = _random.Next(0, 5); // 0-4 frets
        _rootKey = (_rootKey + _capoPosition) % 12;
        
        // Tempo variations
        _tempoVariation = _random.Next(-15, 16); // ±15 BPM from base 120
        
        // Random lead instruments
        var leadOptions = new[] { 110, 25, 27, 105, 22 }; // Fiddle, Steel Guitar, Clean Electric, Banjo, Harmonica
        _leadInstrument = leadOptions[_random.Next(leadOptions.Length)];
        
        // Random rhythm guitar
        var rhythmOptions = new[] { 24, 25, 26, 12 }; // Acoustic Nylon, Steel, Electric Jazz, 12-string
        _rhythmGuitar = rhythmOptions[_random.Next(rhythmOptions.Length)];
        
        // Random bass
        var bassOptions = new[] { 32, 33, 43 }; // Acoustic Bass, Electric Bass, Contrabass
        _bassProgram = bassOptions[_random.Next(bassOptions.Length)];
        
        // Random time signature
        _isWaltzTime = _random.NextDouble() > 0.8; // 20% chance of 3/4 waltz
        
        // Random shuffle feel
        _shuffleFeel = _random.NextSingle() * 0.4f; // 0.0 to 0.4
        
        // Generate chord progression
        GenerateProgression();
    }
    
    protected override int BeatsPerMinute => 120 + _tempoVariation;
    
    protected override List<MidiEvent> GenerateEvents(int durationSeconds)
    {
        var events = new List<MidiEvent>();
        int ticksPerBar = _isWaltzTime ? TicksPerBeat * 3 : TicksPerBeat * 4;
        int totalBars = CalculateBarsFromDuration(durationSeconds);
        
        AddMetaEvents(events);
        AddProgramChanges(events);
        
        var structure = CreateCountryStructure(totalBars);
        
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
        // Random country chord progressions
        var progressions = new[]
        {
            // Pattern 1: Classic I-V-vi-IV
            new[] { 0, 7, 9, 5 },
            // Pattern 2: I-IV-V-I
            new[] { 0, 5, 7, 0 },
            // Pattern 3: vi-IV-I-V
            new[] { 9, 5, 0, 7 },
            // Pattern 4: I-vi-ii-V
            new[] { 0, 9, 2, 7 },
            // Pattern 5: Country shuffle
            new[] { 0, 0, 5, 5, 0, 0, 7, 7 }
        };
        
        _currentProgression = progressions[_random.Next(progressions.Length)];
    }
    
    private System.Collections.Generic.List<SongStructure> CreateCountryStructure(int totalBars)
    {
        var structure = new System.Collections.Generic.List<SongStructure>();
        
        if (totalBars <= 8)
        {
            structure.Add(new SongStructure { SectionName = "Verse", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
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
            
            int soloLength = System.Math.Max(4, totalBars - currentBar - 4);
            structure.Add(new SongStructure { SectionName = "Solo", StartBar = currentBar, LengthBars = soloLength, SectionType = SongSection.Solo });
            currentBar += soloLength;
            
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
                    AddAcousticGuitar(events, barStart, absoluteBar, 0.6f);
                }
                else
                {
                    AddAcousticGuitar(events, barStart, absoluteBar, 0.8f);
                    if (barInSection >= 1)
                        AddBassLine(events, barStart, absoluteBar, 0.6f);
                    if (barInSection >= 2)
                        AddCountryDrums(events, barStart, 0.5f);
                }
                break;
                
            case SongSection.Verse:
                AddCountryDrums(events, barStart, 0.8f);
                AddBassLine(events, barStart, absoluteBar, 1.0f);
                AddAcousticGuitar(events, barStart, absoluteBar, 0.9f);
                AddPianoAccents(events, barStart, absoluteBar, 0.7f);
                break;
                
            case SongSection.Chorus:
                AddCountryDrums(events, barStart, 1.0f);
                AddBassLine(events, barStart, absoluteBar, 1.0f);
                AddAcousticGuitar(events, barStart, absoluteBar, 1.0f);
                AddPianoAccents(events, barStart, absoluteBar, 1.0f);
                AddElectricGuitar(events, barStart, absoluteBar, 0.8f);
                break;
                
            case SongSection.Solo:
                AddCountryDrums(events, barStart, 1.0f);
                AddBassLine(events, barStart, absoluteBar, 1.0f);
                AddAcousticGuitar(events, barStart, absoluteBar, 0.8f);
                
                if (barInSection % 4 < 2)
                    AddFiddleSolo(events, barStart, absoluteBar);
                else
                    AddElectricGuitar(events, barStart, absoluteBar, 1.2f);
                break;
                
            case SongSection.Outro:
                float intensity = System.Math.Max(0.3f, 1.0f - (float)barInSection / section.LengthBars);
                AddCountryDrums(events, barStart, intensity);
                AddBassLine(events, barStart, absoluteBar, intensity);
                AddAcousticGuitar(events, barStart, absoluteBar, intensity);
                
                if (barInSection == section.LengthBars - 1)
                    AddCountryEnding(events, barStart);
                break;
        }
    }
    
    private void AddProgramChanges(List<MidiEvent> events)
    {
        AddProgramChange(events, 0, 0, _bassProgram);     // Random Bass
        AddProgramChange(events, 0, 1, _rhythmGuitar);   // Random Rhythm Guitar
        AddProgramChange(events, 0, 2, 27);             // Electric Guitar (clean) for fills
        AddProgramChange(events, 0, 3, 1);              // Acoustic Piano
        AddProgramChange(events, 0, 4, _leadInstrument); // Random Lead Instrument
        AddProgramChange(events, 0, 9, 0);              // Drums
    }
    
    private void AddCountryDrums(List<MidiEvent> events, int barStart, float intensity = 1.0f)
    {
        int beatsPerBar = _isWaltzTime ? 3 : 4;
        
        // Random drum patterns
        var drumPatterns = new[]
        {
            // Pattern 1: Train beat
            new[] { (0, 36, 80), (TicksPerBeat * 2, 36, 75) },
            // Pattern 2: Country shuffle
            new[] { (0, 36, 85), (TicksPerBeat + TicksPerBeat/4, 36, 60), (TicksPerBeat * 2, 36, 80) },
            // Pattern 3: Boom-chicka
            new[] { (0, 36, 90), (TicksPerBeat, 36, 50), (TicksPerBeat * 2, 36, 85), (TicksPerBeat * 3, 36, 45) },
            // Pattern 4: Waltz (if waltz time)
            _isWaltzTime ? new[] { (0, 36, 90), (TicksPerBeat, 36, 60), (TicksPerBeat * 2, 36, 65) } 
                        : new[] { (0, 36, 85), (TicksPerBeat * 2, 36, 80) }
        };
        
        var kickPattern = drumPatterns[_random.Next(drumPatterns.Length)];
        
        foreach (var (offset, drum, baseVelocity) in kickPattern)
        {
            int velocity = (int)((baseVelocity + _random.Next(-5, 10)) * intensity);
            AddNote(events, barStart + offset, 9, drum, Math.Max(40, velocity), TicksPerBeat / 8);
        }
        
        // Snare pattern
        if (_isWaltzTime)
        {
            // Waltz snare on beat 2
            if (_random.NextDouble() > 0.3)
                AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((70 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        }
        else
        {
            // Standard backbeat
            AddNote(events, barStart + TicksPerBeat, 9, 38, (int)((85 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
            AddNote(events, barStart + TicksPerBeat * 3, 9, 38, (int)((80 + _random.Next(-5, 10)) * intensity), TicksPerBeat / 8);
        }
        
        // Hi-hat with shuffle feel
        int hihatDivisions = _isWaltzTime ? 6 : 8;
        for (int division = 0; division < hihatDivisions; division++)
        {
            int tick = barStart + division * (TicksPerBeat * beatsPerBar / hihatDivisions);
            
            // Add shuffle timing
            if (division % 2 == 1 && _shuffleFeel > 0)
                tick += (int)(TicksPerBeat * _shuffleFeel / 4);
                
            int velocity = (int)(((division % 2 == 0) ? 50 : 35) * intensity + _random.Next(-5, 5));
            AddNote(events, tick, 9, 42, Math.Max(20, velocity), TicksPerBeat / 16);
        }
        
        // Random crash on downbeats
        if (barStart % (TicksPerBeat * beatsPerBar * 4) == 0 && _random.NextDouble() > 0.6)
        {
            AddNote(events, barStart, 9, 49, (int)((80 + _random.Next(-5, 10)) * intensity), TicksPerBeat);
        }
    }
    
    private void AddBassLine(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
    {
        // Use the random progression in the selected key
        int rootNote = 36 + _rootKey; // Bass register
        int chordIndex = bar % _currentProgression.Length;
        int currentBassRoot = rootNote + _currentProgression[chordIndex];
        
        int beatsPerBar = _isWaltzTime ? 3 : 4;
        
        // Random bass patterns
        var bassPatterns = new[]
        {
            // Pattern 1: Root-fifth (classic country)
            new[] { (0, 0, 80), (TicksPerBeat * 2, 7, 70) },
            // Pattern 2: Walking bass
            new[] { (0, 0, 80), (TicksPerBeat, 2, 65), (TicksPerBeat * 2, 4, 70), (TicksPerBeat * 3, 5, 65) },
            // Pattern 3: Train beat bass
            new[] { (0, 0, 85), (TicksPerBeat/2, 0, 50), (TicksPerBeat, 7, 75), (TicksPerBeat + TicksPerBeat/2, 7, 45), 
                   (TicksPerBeat * 2, 0, 80), (TicksPerBeat * 2 + TicksPerBeat/2, 0, 50), (TicksPerBeat * 3, 7, 70) },
            // Pattern 4: Waltz bass (if waltz time)
            _isWaltzTime ? new[] { (0, 0, 90), (TicksPerBeat, 7, 70), (TicksPerBeat * 2, 4, 65) }
                        : new[] { (0, 0, 80), (TicksPerBeat * 2, 7, 70) }
        };
        
        var pattern = bassPatterns[_random.Next(bassPatterns.Length)];
        
        foreach (var (offset, interval, baseVelocity) in pattern)
        {
            if (_isWaltzTime && offset >= TicksPerBeat * 3) continue; // Skip if beyond waltz bar
            
            int tick = barStart + offset;
            int note = currentBassRoot + interval;
            int velocity = (int)((baseVelocity + _random.Next(-10, 15)) * intensity);
            
            // Add shuffle timing
            if (offset % TicksPerBeat != 0 && offset % (TicksPerBeat/2) != 0 && _shuffleFeel > 0)
                tick += (int)(TicksPerBeat * _shuffleFeel / 4);
                
            int duration = TicksPerBeat / 2 + _random.Next(-100, 100);
            AddNote(events, tick, 0, note, Math.Max(40, velocity), Math.Max(duration, TicksPerBeat/8));
        }
    }
    
    private void AddAcousticGuitar(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
    {
        // Use the random progression in the selected key
        int rootNote = 48 + _rootKey; // Guitar register
        int chordIndex = bar % _currentProgression.Length;
        int currentChordRoot = rootNote + _currentProgression[chordIndex];
        
        int beatsPerBar = _isWaltzTime ? 3 : 4;
        
        // Random chord voicings
        var chordVoicings = new[]
        {
            // Open triad
            new[] { 0, 4, 7 },
            // Add9 chord
            new[] { 0, 4, 7, 14 },
            // Sus2 chord
            new[] { 0, 2, 7 },
            // Sus4 chord
            new[] { 0, 5, 7 }
        };
        
        var voicing = chordVoicings[_random.Next(chordVoicings.Length)];
        var chordNotes = voicing.Select(interval => currentChordRoot + interval).ToArray();
        
        // Random strum patterns
        var strumPatterns = new[]
        {
            // Pattern 1: Carter Family strum
            new[] { 0, TicksPerBeat/2, TicksPerBeat + TicksPerBeat/2, TicksPerBeat * 2, TicksPerBeat * 3 },
            // Pattern 2: Train beat strum
            new[] { 0, TicksPerBeat/4, TicksPerBeat, TicksPerBeat + TicksPerBeat/4, TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/4 },
            // Pattern 3: Country shuffle
            new[] { 0, TicksPerBeat + TicksPerBeat/3, TicksPerBeat * 2, TicksPerBeat * 3 + TicksPerBeat/3 },
            // Pattern 4: Waltz strum (if waltz time)
            _isWaltzTime ? new[] { 0, TicksPerBeat, TicksPerBeat * 2 }
                        : new[] { 0, TicksPerBeat/2, TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/2 }
        };
        
        var strumTimes = strumPatterns[_random.Next(strumPatterns.Length)];
        
        for (int i = 0; i < strumTimes.Length; i++)
        {
            if (_isWaltzTime && strumTimes[i] >= TicksPerBeat * 3) continue; // Skip if beyond waltz bar
            
            int tick = barStart + strumTimes[i];
            int velocity = (int)(((i % 2 == 0) ? 65 : 45) * intensity + _random.Next(-5, 10)); // Down strums louder
            
            // Add shuffle timing
            if (strumTimes[i] % TicksPerBeat != 0 && strumTimes[i] % (TicksPerBeat/2) != 0 && _shuffleFeel > 0)
                tick += (int)(TicksPerBeat * _shuffleFeel / 4);
            
            foreach (var note in chordNotes)
            {
                int noteVelocity = velocity + _random.Next(-5, 5);
                AddNote(events, tick, 1, note, Math.Max(30, noteVelocity), TicksPerBeat / 8);
            }
        }
    }
    
    private void AddPianoAccents(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
    {
        if (_random.NextDouble() > 0.6) // Random piano accents
        {
            int rootNote = 60 + _rootKey; // Piano register
            int chordIndex = bar % _currentProgression.Length;
            int currentChordRoot = rootNote + _currentProgression[chordIndex];
            
            // Random piano lick patterns
            var pianoPatterns = new[]
            {
                // Pattern 1: Classic honky-tonk
                new[] { (TicksPerBeat * 3, 0), (TicksPerBeat * 3 + TicksPerBeat/2, 2) },
                // Pattern 2: Walking treble
                new[] { (TicksPerBeat/2, 7), (TicksPerBeat + TicksPerBeat/2, 9), (TicksPerBeat * 2 + TicksPerBeat/2, 11) },
                // Pattern 3: Country fills
                new[] { (TicksPerBeat * 2, 4), (TicksPerBeat * 2 + TicksPerBeat/4, 7), (TicksPerBeat * 3, 0) }
            };
            
            var pattern = pianoPatterns[_random.Next(pianoPatterns.Length)];
            
            foreach (var (offset, interval) in pattern)
            {
                if (_isWaltzTime && offset >= TicksPerBeat * 3) continue;
                
                int tick = barStart + offset;
                int note = currentChordRoot + interval;
                int velocity = (int)((55 + _random.Next(-5, 15)) * intensity);
                
                AddNote(events, tick, 3, note, Math.Max(30, velocity), TicksPerBeat / 4);
            }
        }
    }
    
    private void AddElectricGuitar(List<MidiEvent> events, int barStart, int bar, float intensity = 1.0f)
    {
        // Country scale in the selected key
        var countryScales = new[]
        {
            // Major pentatonic
            new[] { 0, 2, 4, 7, 9 },
            // Major scale
            new[] { 0, 2, 4, 5, 7, 9, 11 },
            // Mixolydian
            new[] { 0, 2, 4, 5, 7, 9, 10 }
        };
        
        var scale = countryScales[_random.Next(countryScales.Length)];
        var scaleNotes = scale.Select(interval => 60 + _rootKey + interval + 12).ToArray(); // Higher register
        
        // Random lick patterns
        var lickPatterns = new[]
        {
            // Pattern 1: Telecaster bends
            new[] { 0, 1, 2, 4 },
            // Pattern 2: Country runs
            new[] { 4, 3, 2, 1, 0 },
            // Pattern 3: Double stops
            new[] { 0, 2, 0, 4 },
            // Pattern 4: Chicken pickin'
            new[] { 1, 0, 2, 1, 4, 2 }
        };
        
        var pattern = lickPatterns[_random.Next(lickPatterns.Length)];
        int notesPerBeat = _isWaltzTime ? 3 : 4;
        
        for (int i = 0; i < Math.Min(pattern.Length, notesPerBeat); i++)
        {
            int tick = barStart + i * TicksPerBeat;
            int note = scaleNotes[pattern[i] % scaleNotes.Length];
            int velocity = (int)((70 + _random.Next(-10, 15)) * intensity);
            
            // Add shuffle timing
            if (i % 2 == 1 && _shuffleFeel > 0)
                tick += (int)(TicksPerBeat * _shuffleFeel / 4);
                
            AddNote(events, tick, 2, note, Math.Max(40, velocity), TicksPerBeat / 3);
            
            // Random bend simulation
            if (_random.NextDouble() > 0.7)
            {
                int bendTick = tick + TicksPerBeat/4;
                int bendNote = note + (_random.NextDouble() > 0.5 ? 1 : 2);
                AddNote(events, bendTick, 2, bendNote, velocity - 15, TicksPerBeat / 8);
            }
        }
    }
    
    private void AddFiddleSolo(List<MidiEvent> events, int barStart, int bar)
    {
        // Different scales based on lead instrument
        var scaleTypes = new[]
        {
            // Major scale for fiddle
            new[] { 0, 2, 4, 5, 7, 9, 11 },
            // Major pentatonic for steel guitar
            new[] { 0, 2, 4, 7, 9 },
            // Mixolydian for banjo
            new[] { 0, 2, 4, 5, 7, 9, 10 }
        };
        
        var scale = scaleTypes[_random.Next(scaleTypes.Length)];
        var scaleNotes = scale.Select(interval => 60 + _rootKey + interval + 12).ToArray();
        
        // Random solo patterns
        var soloPatterns = new[]
        {
            // Pattern 1: Fiddle runs
            new[] { 0, 1, 2, 3, 4, 3, 2, 1 },
            // Pattern 2: Steel guitar licks
            new[] { 0, 2, 4, 2, 0, 4, 2, 0 },
            // Pattern 3: Banjo rolls
            new[] { 0, 2, 0, 4, 2, 0, 2, 4 },
            // Pattern 4: Harmonica bends
            new[] { 2, 1, 0, 2, 4, 3, 2, 0 }
        };
        
        var pattern = soloPatterns[_random.Next(soloPatterns.Length)];
        int notesPerBar = _isWaltzTime ? 6 : 8;
        
        for (int i = 0; i < Math.Min(pattern.Length, notesPerBar); i++)
        {
            int tick = barStart + i * (TicksPerBeat / 2);
            int note = scaleNotes[pattern[i] % scaleNotes.Length];
            int velocity = 75 + _random.Next(-10, 15);
            
            // Add shuffle timing
            if (i % 2 == 1 && _shuffleFeel > 0)
                tick += (int)(TicksPerBeat * _shuffleFeel / 4);
                
            // Adjust register based on instrument
            switch (_leadInstrument)
            {
                case 110: // Fiddle - higher register
                    note += 12;
                    break;
                case 25: // Steel Guitar - add bends
                    if (_random.NextDouble() > 0.6)
                        note += _random.Next(-2, 3); // Pitch bends
                    break;
                case 105: // Banjo - rapid notes
                    if (i % 2 == 0 && i < pattern.Length - 1)
                    {
                        AddNote(events, tick + TicksPerBeat/8, 4, note + 2, velocity - 20, TicksPerBeat/16);
                    }
                    break;
            }
            
            AddNote(events, tick, 4, note, Math.Max(50, velocity), TicksPerBeat / 3);
        }
    }
    
    private void AddCountryEnding(List<MidiEvent> events, int barStart)
    {
        // Final chord in the selected key
        int rootNote = 36 + _rootKey;
        
        var finalChord = new[] 
        { 
            rootNote,           // Bass root
            rootNote + 12,      // Guitar root
            rootNote + 16,      // Third
            rootNote + 19,      // Fifth
            rootNote + 24       // High root
        };
        
        // Add random chord extensions
        if (_random.NextDouble() > 0.5)
        {
            finalChord = finalChord.Concat(new[] { rootNote + 22 }).ToArray(); // Seventh
        }
        
        foreach (var note in finalChord)
        {
            int channel = note < 48 ? 0 : (note < 60 ? 1 : 3); // Bass, guitar, or piano
            int velocity = 85 + _random.Next(-10, 15);
            int duration = _isWaltzTime ? TicksPerBeat * 3 : TicksPerBeat * 4;
            AddNote(events, barStart, channel, note, velocity, duration);
        }
        
        // Final drum hits
        AddNote(events, barStart, 9, 36, 100 + _random.Next(-5, 10), TicksPerBeat / 4);
        AddNote(events, barStart, 9, 49, 90 + _random.Next(-5, 10), TicksPerBeat * 2);
        
        // Random final flourish
        if (_random.NextDouble() > 0.6)
        {
            AddNote(events, barStart + TicksPerBeat, 4, rootNote + 24 + 7, 80, TicksPerBeat / 2); // Lead instrument final note
        }
    }
}