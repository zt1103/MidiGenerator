using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

public class MetalMidiGenerator : BaseMidiGenerator
{
    public override string GenreName => "Metal";
    
    private Random _random = new Random();
    private int _rootKey;
    private int _tempoVariation;
    private int[] _currentRiffPattern;
    private int _leadGuitarProgram;
    
    public MetalMidiGenerator()
    {
        // Random metal keys (minor keys work best)
        var metalKeys = new[] { 0, 2, 3, 5, 7, 10 }; // C, D, Eb, F, G, Bb (all as minor)
        _rootKey = metalKeys[_random.Next(metalKeys.Length)];
        
        // Tempo micro-variations
        _tempoVariation = _random.Next(-5, 6); // ±5 BPM from base 160
        
        // Random lead guitar programs
        var leadOptions = new[] { 29, 30, 31 }; // Overdrive, Distortion, Guitar Harmonics
        _leadGuitarProgram = leadOptions[_random.Next(leadOptions.Length)];
        
        // Generate random riff pattern for this song
        GenerateRiffPattern();
    }
    
    protected override int BeatsPerMinute => 160 + _tempoVariation;
    
    protected override System.Collections.Generic.List<MidiEvent> GenerateEvents(int durationSeconds)
    {
        var events = new System.Collections.Generic.List<MidiEvent>();
        int ticksPerBar = TicksPerBeat * 4;
        int totalBars = CalculateBarsFromDuration(durationSeconds);
        
        AddMetaEvents(events);
        AddProgramChanges(events);
        
        var structure = CreateMetalSongStructure(totalBars);
        
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
    
    private System.Collections.Generic.List<SongStructure> CreateMetalSongStructure(int totalBars)
    {
        var structure = new System.Collections.Generic.List<SongStructure>();
        
        if (totalBars <= 8)
        {
            // Short song - just main riff
            structure.Add(new SongStructure { SectionName = "Main Riff", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
        }
        else if (totalBars <= 16)
        {
            // Medium song - riff and breakdown
            int riffLength = (int)(totalBars * 0.7f);
            structure.Add(new SongStructure { SectionName = "Main Riff", StartBar = 0, LengthBars = riffLength, SectionType = SongSection.Verse });
            structure.Add(new SongStructure { SectionName = "Breakdown", StartBar = riffLength, LengthBars = totalBars - riffLength, SectionType = SongSection.Breakdown });
        }
        else
        {
            // Full metal song structure
            int currentBar = 0;
            
            // Intro - building tension (4 bars)
            structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Intro });
            currentBar += 4;
            
            // Main riff/verse (8 bars)
            structure.Add(new SongStructure { SectionName = "Verse", StartBar = currentBar, LengthBars = 8, SectionType = SongSection.Verse });
            currentBar += 8;
            
            // Chorus - more intense (8 bars)
            if (totalBars - currentBar >= 12)
            {
                structure.Add(new SongStructure { SectionName = "Chorus", StartBar = currentBar, LengthBars = 8, SectionType = SongSection.Chorus });
                currentBar += 8;
            }
            
            // Solo section (remaining - 4 for outro)
            int soloLength = System.Math.Max(4, totalBars - currentBar - 4);
            structure.Add(new SongStructure { SectionName = "Solo", StartBar = currentBar, LengthBars = soloLength, SectionType = SongSection.Solo });
            currentBar += soloLength;
            
            // Outro - breakdown to big finish
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
				// Build tension - start minimal
				if (barInSection == 0)
				{
					// Just single low note
					AddNote(events, barStart, 0, 28, 60, TicksPerBeat * 4);
				}
				else
				{
					// Add drums gradually
					AddDoubleBass(events, barStart, 0.3f + (barInSection * 0.2f));
					if (barInSection >= 2)
						AddRiffPattern(events, absoluteBar, barStart, 0.5f);
					if (barInSection >= 3)
						AddDrumPattern(events, barStart, 0.7f);
				}
				break;

			case SongSection.Verse:
				// Main riff with full power
				AddRiffPattern(events, absoluteBar, barStart, 1.0f);
				AddDrumPattern(events, barStart, 1.0f);
				AddDoubleBass(events, barStart, 1.0f);
				break;

			case SongSection.Chorus:
				// More intense - higher register, more layers
				AddRiffPattern(events, absoluteBar, barStart, 1.2f);
				AddDrumPattern(events, barStart, 1.2f);
				AddDoubleBass(events, barStart, 1.0f);
				// Add lead guitar harmonies
				if (barInSection % 2 == 0)
					AddLeadGuitar(events, absoluteBar, barStart, 1.0f);
				break;

			case SongSection.Solo:
				// Rhythm section continues, lead guitar solos
				AddRiffPattern(events, absoluteBar, barStart, 0.8f);
				AddDrumPattern(events, barStart, 1.0f);
				AddDoubleBass(events, barStart, 0.8f);
				AddLeadGuitar(events, absoluteBar, barStart, 1.2f);
				break;

			case SongSection.Breakdown:
				// Sparse, heavy hits
				if (barInSection % 2 == 0)
				{
					AddNote(events, barStart, 0, 28, 120, TicksPerBeat / 2);
					AddNote(events, barStart, 9, 36, 127, TicksPerBeat / 4);
				}
				AddNote(events, barStart + TicksPerBeat, 9, 38, 127, TicksPerBeat / 8);
				break;

			case SongSection.Outro:
				// Build to massive finish
				float intensity = barInSection < section.LengthBars - 2 ? 1.0f : 1.5f;

				AddRiffPattern(events, absoluteBar, barStart, intensity);
				AddDrumPattern(events, barStart, intensity);
				AddDoubleBass(events, barStart, intensity);

				if (barInSection == section.LengthBars - 1) // Final bar
					AddMetalFinale(events, barStart);
				break;
		}
    }
    
    private void AddProgramChanges(System.Collections.Generic.List<MidiEvent> events)
    {
        AddProgramChange(events, 0, 0, 30);           // Distortion Guitar
        AddProgramChange(events, 0, 1, 30);           // Rhythm Guitar 2
        AddProgramChange(events, 0, 2, _leadGuitarProgram); // Random Lead Guitar
        AddProgramChange(events, 0, 9, 0);            // Drums
    }
    
    private void GenerateRiffPattern()
    {
        // Random riff patterns based on minor scale intervals
        var riffPatterns = new[]
        {
            // Pattern 1: Classic power chord progression
            new[] { 0, 5, 7, 3 },    // i - v - bVII - bIII
            // Pattern 2: Darker progression
            new[] { 0, 3, 5, 8 },    // i - bIII - v - bVI
            // Pattern 3: Ascending pattern
            new[] { 0, 2, 5, 7 },    // i - ii - v - bVII
            // Pattern 4: Chromatic descent
            new[] { 7, 5, 3, 0 }     // bVII - v - bIII - i
        };
        
        _currentRiffPattern = riffPatterns[_random.Next(riffPatterns.Length)];
    }
    
    private void AddRiffPattern(System.Collections.Generic.List<MidiEvent> events, int bar, int barStart, float intensity = 1.0f)
    {
        // Use the random riff pattern in the selected key
        int rootNote = 40 + _rootKey; // E3 transposed to selected key
        int chordIndex = bar % _currentRiffPattern.Length;
        int currentChordRoot = rootNote + _currentRiffPattern[chordIndex];
        
        // Random rhythmic patterns
        var rhythmPatterns = new[]
        {
            // Pattern 1: Classic palm-muted
            new[] { 0, TicksPerBeat/2, TicksPerBeat, TicksPerBeat + TicksPerBeat/4, 
                   TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/2,
                   TicksPerBeat * 3, TicksPerBeat * 3 + TicksPerBeat/4 },
            // Pattern 2: Syncopated
            new[] { 0, TicksPerBeat/4, TicksPerBeat + TicksPerBeat/4, TicksPerBeat * 2,
                   TicksPerBeat * 2 + TicksPerBeat/4, TicksPerBeat * 3 + TicksPerBeat/8 },
            // Pattern 3: Galloping
            new[] { 0, TicksPerBeat/3, TicksPerBeat/2, TicksPerBeat,
                   TicksPerBeat + TicksPerBeat/3, TicksPerBeat * 2, TicksPerBeat * 3 },
            // Pattern 4: Machine gun
            new[] { 0, TicksPerBeat/4, TicksPerBeat/2, TicksPerBeat * 3/4,
                   TicksPerBeat, TicksPerBeat + TicksPerBeat/4, TicksPerBeat * 2, TicksPerBeat * 3 }
        };
        
        var rhythmPattern = rhythmPatterns[_random.Next(rhythmPatterns.Length)];
        
        foreach (int offset in rhythmPattern)
        {
            int velocity = (int)((110 + _random.Next(-10, 15)) * intensity);
            
            // Root note (power chord root)
            AddNote(events, barStart + offset, 0, currentChordRoot, Math.Max(40, velocity), TicksPerBeat/8);
            // Fifth (power chord)
            AddNote(events, barStart + offset, 0, currentChordRoot + 7, Math.Max(35, velocity - 5), TicksPerBeat/8);
            
            // Doubled on rhythm guitar channel with slight variation
            int rhythmVelocity = velocity - 10 + _random.Next(-5, 5);
            AddNote(events, barStart + offset, 1, currentChordRoot - 12, Math.Max(30, rhythmVelocity), TicksPerBeat/8);
            AddNote(events, barStart + offset, 1, currentChordRoot - 5, Math.Max(25, rhythmVelocity - 5), TicksPerBeat/8);
        }
    }
    
    private void AddDrumPattern(System.Collections.Generic.List<MidiEvent> events, int barStart, float intensity = 1.0f)
    {
        // Random kick patterns
        var kickPatterns = new[]
        {
            // Pattern 1: Double bass classic
            new[] { 0, TicksPerBeat/4, TicksPerBeat, TicksPerBeat + TicksPerBeat/4,
                   TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/4,
                   TicksPerBeat * 3, TicksPerBeat * 3 + TicksPerBeat/4 },
            // Pattern 2: Blast beat style
            new[] { 0, TicksPerBeat/2, TicksPerBeat, TicksPerBeat + TicksPerBeat/2,
                   TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/2,
                   TicksPerBeat * 3, TicksPerBeat * 3 + TicksPerBeat/2 },
            // Pattern 3: Galloping kicks
            new[] { 0, TicksPerBeat/3, TicksPerBeat * 2/3, TicksPerBeat,
                   TicksPerBeat + TicksPerBeat/3, TicksPerBeat * 2, TicksPerBeat * 3 },
            // Pattern 4: Syncopated
            new[] { 0, TicksPerBeat/8, TicksPerBeat * 3/4, TicksPerBeat + TicksPerBeat/8,
                   TicksPerBeat * 2, TicksPerBeat * 2 + TicksPerBeat/8, TicksPerBeat * 3 }
        };
        
        var kickPattern = kickPatterns[_random.Next(kickPatterns.Length)];
        
        foreach (int kickTime in kickPattern)
        {
            int velocity = (int)((115 + _random.Next(-10, 15)) * intensity);
            AddNote(events, barStart + kickTime, 9, 36, Math.Max(40, velocity), TicksPerBeat/16);
        }
        
        // Snare with variations
        var snareHits = new[] { TicksPerBeat, TicksPerBeat * 3 };
        foreach (int snareTime in snareHits)
        {
            int velocity = (int)((120 + _random.Next(-5, 10)) * intensity);
            int timing = snareTime + _random.Next(-15, 15); // Slight timing variation
            AddNote(events, barStart + timing, 9, 38, Math.Max(50, velocity), TicksPerBeat/8);
        }
        
        // Crash cymbal on beat 1 every 4 bars (with randomization)
        if (barStart % (TicksPerBeat * 16) == 0 && _random.NextDouble() > 0.3)
        {
            int velocity = (int)((110 + _random.Next(-10, 15)) * intensity);
            AddNote(events, barStart, 9, 49, Math.Max(60, velocity), TicksPerBeat);
        }
        
        // Random hi-hat patterns
        var hihatPatterns = new[]
        {
            // Pattern 1: 16th notes
            16,
            // Pattern 2: 8th notes
            8,
            // Pattern 3: Quarter notes
            4,
            // Pattern 4: Syncopated
            12
        };
        
        int hihatDivision = hihatPatterns[_random.Next(hihatPatterns.Length)];
        
        for (int division = 0; division < hihatDivision; division++)
        {
            int velocity = (int)(((division % 4 == 0) ? 80 : 60) * intensity + _random.Next(-10, 10));
            AddNote(events, barStart + division * (TicksPerBeat * 4 / hihatDivision), 9, 42, 
                   System.Math.Max(20, velocity), TicksPerBeat/32);
        }
    }
    
    private void AddDoubleBass(System.Collections.Generic.List<MidiEvent> events, int barStart, float intensity = 1.0f)
    {
        // Use the current riff pattern for bass notes
        int bassNote = 28 + _rootKey; // E1 transposed to selected key
        int chordIndex = (barStart / (TicksPerBeat * 4)) % _currentRiffPattern.Length;
        int currentBassNote = bassNote + _currentRiffPattern[chordIndex];
        
        // Random bass patterns
        var bassPatterns = new[]
        {
            // Pattern 1: Every other 16th
            new[] { 0, 2, 4, 6, 8, 10, 12, 14 },
            // Pattern 2: Galloping
            new[] { 0, 1, 3, 4, 5, 7, 8, 9, 11, 12, 13, 15 },
            // Pattern 3: Blast beat bass
            new[] { 0, 2, 4, 6, 8, 10, 12, 14 },
            // Pattern 4: Syncopated
            new[] { 0, 1, 4, 5, 8, 9, 12, 13 }
        };
        
        var pattern = bassPatterns[_random.Next(bassPatterns.Length)];
        
        foreach (int sixteenth in pattern)
        {
            int velocity = (int)(((sixteenth % 4 == 0) ? 105 : 90) * intensity + _random.Next(-10, 10));
            AddNote(events, barStart + sixteenth * (TicksPerBeat/4), 0, currentBassNote, 
                   System.Math.Max(30, velocity), TicksPerBeat/8);
        }
    }
    
    private void AddLeadGuitar(System.Collections.Generic.List<MidiEvent> events, int bar, int barStart, float intensity = 1.0f)
    {
        // Metal scales in the selected key
        var scaleTypes = new[]
        {
            // Natural minor (Aeolian)
            new[] { 0, 2, 3, 5, 7, 8, 10 },
            // Harmonic minor
            new[] { 0, 2, 3, 5, 7, 8, 11 },
            // Minor pentatonic + blues
            new[] { 0, 3, 5, 6, 7, 10 },
            // Dorian mode
            new[] { 0, 2, 3, 5, 7, 9, 10 }
        };
        
        var scale = scaleTypes[_random.Next(scaleTypes.Length)];
        var scaleNotes = scale.Select(interval => 52 + _rootKey + interval).ToArray(); // E4 register
        
        // Random lead guitar patterns
        var leadPatterns = new[]
        {
            // Pattern 1: Fast tremolo picking
            new[] { 0, 1, 2, 4, 3, 5, 4, 6 },
            // Pattern 2: Descending runs
            new[] { 6, 5, 4, 3, 2, 1, 0, 2 },
            // Pattern 3: Harmonic intervals
            new[] { 0, 4, 1, 5, 2, 6, 3, 0 },
            // Pattern 4: Chromatic shred
            new[] { 0, 1, 0, 2, 1, 3, 2, 4 }
        };
        
        var pattern = leadPatterns[_random.Next(leadPatterns.Length)];
        
        for (int eighth = 0; eighth < 8; eighth++)
        {
            int note = scaleNotes[pattern[eighth] % scaleNotes.Length];
            int tick = barStart + eighth * (TicksPerBeat/2);
            
            // Add random octave variations
            if (_random.NextDouble() > 0.7)
                note += 12 * (_random.NextDouble() > 0.5 ? 1 : -1);
                
            note = System.Math.Max(48, System.Math.Min(96, note)); // Keep in reasonable range
            
            int velocity = (int)((100 + _random.Next(-15, 20)) * intensity);
            AddNote(events, tick, 2, note, System.Math.Max(40, velocity), TicksPerBeat/4);
            
            // Random harmonics and pinch squeals
            if (_random.NextDouble() > 0.8)
            {
                int harmonicNote = note + 12; // Octave harmonic
                AddNote(events, tick + TicksPerBeat/8, 2, harmonicNote, (int)(velocity * 0.7), TicksPerBeat/8);
            }
            
            // Random sweep picking simulation
            if (eighth % 2 == 0 && _random.NextDouble() > 0.6)
            {
                int sweepNote = note + 7; // Fifth above
                AddNote(events, tick + TicksPerBeat/16, 2, sweepNote, velocity - 20, TicksPerBeat/16);
            }
        }
    }
    
    private void AddMetalFinale(System.Collections.Generic.List<MidiEvent> events, int barStart)
    {
        // Massive final chord in the selected key
        int rootNote = 28 + _rootKey; // Bass register
        
        // Power chord finale with random variations
        var finalChordNotes = new[]
        {
            rootNote,           // Bass root
            rootNote + 12,      // Guitar root
            rootNote + 19,      // Guitar fifth
            rootNote + 24       // High root
        };
        
        // Add random extensions
        if (_random.NextDouble() > 0.5)
        {
            finalChordNotes = finalChordNotes.Concat(new[] { rootNote + 15 }).ToArray(); // Minor third
        }
        
        foreach (var note in finalChordNotes)
        {
            int channel = note < 40 ? 0 : (note < 60 ? 1 : 2); // Bass, rhythm, or lead
            int velocity = 115 + _random.Next(-10, 15);
            AddNote(events, barStart, channel, note, velocity, TicksPerBeat * 4);
        }
        
        // Epic drum finale
        AddNote(events, barStart, 9, 36, 127, TicksPerBeat / 4); // Massive kick
        AddNote(events, barStart, 9, 49, 120, TicksPerBeat * 2); // Crash
        AddNote(events, barStart + TicksPerBeat, 9, 57, 115, TicksPerBeat); // Crash 2
        
        // Random additional percussion
        if (_random.NextDouble() > 0.5)
        {
            AddNote(events, barStart + TicksPerBeat/2, 9, 38, 100, TicksPerBeat/4); // Snare roll simulation
            AddNote(events, barStart + TicksPerBeat * 3/2, 9, 51, 90, TicksPerBeat); // Ride bell
        }
    }
}
