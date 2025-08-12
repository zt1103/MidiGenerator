using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenerator.Support;

namespace MidiGenerator.Genres
{
    public class JazzMidiGenerator : BaseMidiGenerator
    {
        public override string GenreName => "Jazz";
        protected override int BeatsPerMinute => 140;
        
        private static Random _staticRandom = new Random();
        private Random _random;
        private int _saxProgram;
        
        public JazzMidiGenerator()
        {
            _random = new Random(_staticRandom.Next());
            
            var windOptions = new[] { 66, 67, 68, 70 }; // Tenor Sax, Baritone Sax, English Horn, Bassoon
            _saxProgram = windOptions[_random.Next(windOptions.Length)];
        }
        
        protected override List<MidiEvent> GenerateEvents(int durationSeconds)
        {
            var events = new List<MidiEvent>();
            int ticksPerBar = TicksPerBeat * 4;
            int totalBars = CalculateBarsFromDuration(durationSeconds);
            
            AddMetaEvents(events);
            AddProgramChanges(events);
            
            var structure = CreateJazzSongStructure(totalBars);
            var chordProgression = GenerateChordProgression(totalBars);
            
            foreach (var section in structure)
            {
                for (int bar = section.StartBar; bar < section.StartBar + section.LengthBars; bar++)
                {
                    int barStart = bar * ticksPerBar;
                    var chord = chordProgression[bar];
                    
                    AddSectionBasedMusic(events, barStart, chord, section, bar - section.StartBar);
                }
            }
            
            events.Add(new MidiEvent(totalBars * ticksPerBar, 0xFF, 0x2F, 0x00));
            return events;
        }
        
        private List<SongStructure> CreateJazzSongStructure(int totalBars)
        {
            var structure = new List<SongStructure>();
            
            if (totalBars <= 8)
            {
                structure.Add(new SongStructure { SectionName = "Theme", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else if (totalBars <= 16)
            {
                int themeLength = totalBars / 2;
                structure.Add(new SongStructure { SectionName = "Theme", StartBar = 0, LengthBars = themeLength, SectionType = SongSection.Verse });
                structure.Add(new SongStructure { SectionName = "Solo", StartBar = themeLength, LengthBars = totalBars - themeLength, SectionType = SongSection.Solo });
            }
            else
            {
                int currentBar = 0;
                
                structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Intro });
                currentBar += 4;
                
                structure.Add(new SongStructure { SectionName = "Head", StartBar = currentBar, LengthBars = 8, SectionType = SongSection.Verse });
                currentBar += 8;
                
                int soloLength = Math.Max(8, totalBars - currentBar - 4);
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
        
        private void AddSectionBasedMusic(List<MidiEvent> events, int barStart, JazzChord chord, SongStructure section, int barInSection)
        {
            AddWalkingBass(events, barStart, chord);
            AddSwingDrums(events, barStart, section, barInSection);
            
            switch (section.SectionType)
            {
                case SongSection.Intro:
                    if (barInSection >= 2)
                        AddJazzChords(events, barStart, chord);
                    break;
                    
                case SongSection.Verse:
                    AddJazzChords(events, barStart, chord);
                    if (_random.NextDouble() > 0.7)
                        AddSyncopatedAccents(events, barStart);
                    break;
                    
                case SongSection.Solo:
                    if (_random.NextDouble() > 0.8)
                        AddJazzChords(events, barStart, chord);
                        
                    if (_random.NextDouble() > 0.4)
                        AddPianoSolo(events, barStart, chord);
                    if (_random.NextDouble() > 0.7)
                        AddSaxSolo(events, barStart, chord);
                    break;
                    
                case SongSection.Outro:
                    float fadeAmount = (float)barInSection / section.LengthBars;
                    if (_random.NextDouble() > fadeAmount * 0.5)
                        AddJazzChords(events, barStart, chord);
                    if (barInSection == section.LengthBars - 1)
                        AddFinalChord(events, barStart, chord);
                    break;
            }
        }
        
        private void AddProgramChanges(List<MidiEvent> events)
        {
            AddProgramChange(events, 0, 0, 32);         // Acoustic Bass
            AddProgramChange(events, 0, 1, 2);          // Electric Piano
            AddProgramChange(events, 0, 2, _saxProgram); // Random Wind Instrument
            AddProgramChange(events, 0, 3, 1);          // Acoustic Piano
            AddProgramChange(events, 0, 9, 0);          // Drums
        }
        
        private JazzChord[] GenerateChordProgression(int totalBars)
        {
            var progressions = new[]
            {
                new[] { "Cmaj7", "Am7", "Dm7", "G7" },
                new[] { "Cmaj7", "C7", "Fmaj7", "G7" },
                new[] { "Am7", "D7", "Dm7", "G7" },
                new[] { "Em7", "A7", "Dm7", "G7" },
                new[] { "Fmaj7", "Fm7", "Em7", "Am7" },
            };
            
            var chords = new List<JazzChord>();
            
            for (int bar = 0; bar < totalBars; bar += 4)
            {
                var progression = progressions[_random.Next(progressions.Length)];
                foreach (var chordName in progression)
                {
                    if (chords.Count < totalBars)
                        chords.Add(ParseChord(chordName));
                }
            }
            
            return chords.Take(totalBars).ToArray();
        }
        
        private struct JazzChord
        {
            public int Root;
            public string Quality;
            public int[] Extensions;
        }
        
        private JazzChord ParseChord(string chordName)
        {
            var noteMap = new Dictionary<char, int> { {'C', 60}, {'D', 62}, {'E', 64}, {'F', 65}, {'G', 67}, {'A', 69}, {'B', 71} };
            char rootChar = chordName[0];
            int root = noteMap[rootChar];
            
            if (chordName.Contains("maj7"))
                return new JazzChord { Root = root, Quality = "maj7", Extensions = new[] { 0, 4, 7, 11 } };
            else if (chordName.Contains("m7"))
                return new JazzChord { Root = root, Quality = "m7", Extensions = new[] { 0, 3, 7, 10 } };
            else if (chordName.Contains("7"))
                return new JazzChord { Root = root, Quality = "7", Extensions = new[] { 0, 4, 7, 10 } };
            else
                return new JazzChord { Root = root, Quality = "maj", Extensions = new[] { 0, 4, 7 } };
        }
        
        private void AddWalkingBass(List<MidiEvent> events, int barStart, JazzChord chord)
        {
            var bassNotes = GenerateWalkingBassLine(chord);
            
            for (int beat = 0; beat < 4; beat++)
            {
                int tick = barStart + beat * TicksPerBeat;
                int note = bassNotes[beat];
                int velocity = 75 + _random.Next(-5, 10);
                
                if (beat % 2 == 1)
                    tick += TicksPerBeat / 20;
                    
                AddNote(events, tick, 0, note, velocity, TicksPerBeat * 3/4);
            }
        }
        
        private int[] GenerateWalkingBassLine(JazzChord chord)
        {
            var notes = new int[4];
            notes[0] = chord.Root - 24; // Bass register
            
            for (int i = 1; i < 4; i++)
            {
                var lastNote = notes[i-1];
                var candidates = new List<int>();
                
                // Prefer stepwise motion
                for (int step = -3; step <= 3; step++)
                    candidates.Add(lastNote + step);
                    
                // Add chord tones as targets
                foreach (var ext in chord.Extensions)
                    candidates.Add(chord.Root - 24 + ext);
                    
                notes[i] = candidates[_random.Next(candidates.Count)];
                
                // Keep in bass range
                notes[i] = Math.Max(28, Math.Min(52, notes[i]));
            }
            
            return notes;
        }
        
        private void AddSwingDrums(List<MidiEvent> events, int barStart, SongStructure section, int barInSection)
        {
            float intensity = section.SectionType switch
            {
                SongSection.Intro => 0.5f,
                SongSection.Verse => 0.8f,
                SongSection.Solo => 1.0f,
                SongSection.Outro => Math.Max(0.3f, 1.0f - (float)barInSection / section.LengthBars),
                _ => 0.8f
            };
            
            for (int beat = 0; beat < 4; beat++)
            {
                int tick = barStart + beat * TicksPerBeat;
                int velocity = (int)((beat % 2 == 0 ? 55 : 40) * intensity);
                AddNote(events, tick, 9, 42, Math.Max(20, velocity), TicksPerBeat/4);
                
                if (_random.NextDouble() > 0.6)
                {
                    int swingTick = tick + TicksPerBeat * 2/3;
                    AddNote(events, swingTick, 9, 42, Math.Max(15, velocity - 10), TicksPerBeat/8);
                }
            }
            
            if (_random.NextDouble() > (0.7 / intensity))
                AddNote(events, barStart, 9, 36, (int)(60 * intensity), TicksPerBeat/8);
            if (_random.NextDouble() > (0.5 / intensity))
                AddNote(events, barStart + TicksPerBeat * 2, 9, 38, (int)(65 * intensity), TicksPerBeat/8);
                
            if (_random.NextDouble() > 0.9 && intensity > 0.5)
            {
                int sweepStart = barStart + TicksPerBeat + _random.Next(TicksPerBeat);
                AddNote(events, sweepStart, 9, 44, (int)(35 * intensity), TicksPerBeat/4);
            }
        }
        
        private void AddJazzChords(List<MidiEvent> events, int barStart, JazzChord chord)
        {
            if (_random.NextDouble() > 0.7) return;
            
            int numComps = _random.Next(1, 3);
            
            for (int i = 0; i < numComps; i++)
            {
                float beatPosition = 1f + _random.Next(3) + _random.NextSingle() * 0.5f;
                int tick = barStart + (int)(beatPosition * TicksPerBeat);
                
                var voicing = GenerateChordVoicing(chord);
                
                foreach (var note in voicing)
                {
                    int velocity = 50 + _random.Next(-10, 15);
                    int duration = TicksPerBeat/3 + _random.Next(-50, 50);
                    AddNote(events, tick, 1, note, velocity, duration);
                }
            }
        }
        
        private int[] GenerateChordVoicing(JazzChord chord)
        {
            var voicing = new List<int>();
            
            if (_random.NextDouble() > 0.6)
                voicing.Add(chord.Root);
                
            foreach (var ext in chord.Extensions.Skip(1))
            {
                if (_random.NextDouble() > 0.3)
                    voicing.Add(chord.Root + ext + 12);
            }
            
            return voicing.ToArray();
        }
        
        private void AddPianoSolo(List<MidiEvent> events, int barStart, JazzChord chord)
        {
            var scale = GetScaleForChord(chord);
            int numNotes = _random.Next(2, 5);
            
            for (int i = 0; i < numNotes; i++)
            {
                float beatPosition = i * (4f / numNotes);
                int tick = barStart + (int)(beatPosition * TicksPerBeat);
                
                if (i % 2 == 1)
                    tick += TicksPerBeat / 15;
                    
                int note = scale[_random.Next(scale.Length)];
                
                if (_random.NextDouble() > 0.8)
                    note += 12 * (_random.NextDouble() > 0.5 ? 1 : -1);
                    
                note = Math.Max(48, Math.Min(84, note));
                
                int velocity = 60 + _random.Next(-10, 15);
                int duration = TicksPerBeat/2 + _random.Next(-100, 100);
                
                AddNote(events, tick, 3, note, velocity, Math.Max(duration, TicksPerBeat/8));
            }
        }
        
        private void AddSaxSolo(List<MidiEvent> events, int barStart, JazzChord chord)
        {
            var scale = GetScaleForChord(chord);
            int phraseLength = _random.Next(2, 4);
            
            for (int i = 0; i < phraseLength; i++)
            {
                float beatPosition = i * (4f / phraseLength);
                int tick = barStart + (int)(beatPosition * TicksPerBeat);
                
                int note = scale[_random.Next(scale.Length)];
                
                // Adjust register based on instrument type
                switch (_saxProgram)
                {
                    case 66: note += 12; break; // Tenor Sax - mid register
                    case 67: note += 0;  break; // Baritone Sax - lowest
                    case 68: note += 12; break; // English Horn - mid register
                    case 70: note -= 12; break; // Bassoon - very low register
                }
                
                note = Math.Max(48, Math.Min(96, note));
                
                int velocity = 70 + _random.Next(-10, 15);
                
                // Much longer durations - hold notes for 1-2 beats
                int baseDuration = TicksPerBeat + (TicksPerBeat / 2); // 1.5 beats base
                int duration = baseDuration + _random.Next(-TicksPerBeat/4, TicksPerBeat/2);
                
                // Make sure we don't overlap too much with next note
                if (i < phraseLength - 1)
                {
                    int nextNoteTick = barStart + (int)((i + 1) * (4f / phraseLength) * TicksPerBeat);
                    int maxDuration = nextNoteTick - tick - (TicksPerBeat / 8); // Leave small gap
                    duration = Math.Min(duration, maxDuration);
                }
                
                duration = Math.Max(duration, TicksPerBeat/2); // Minimum half beat
                
                AddNote(events, tick, 2, note, velocity, duration);
            }
        }
        
        private int[] GetScaleForChord(JazzChord chord)
        {
            var scales = new Dictionary<string, int[]>
            {
                { "maj7", new[] { 0, 2, 4, 5, 7, 9, 11 } }, // Major scale
                { "m7", new[] { 0, 2, 3, 5, 7, 8, 10 } },   // Dorian mode
                { "7", new[] { 0, 2, 4, 5, 7, 9, 10 } }     // Mixolydian mode
            };
            
            var scaleIntervals = scales.ContainsKey(chord.Quality) ? scales[chord.Quality] : scales["maj7"];
            return scaleIntervals.Select(interval => chord.Root + interval).ToArray();
        }
        
        private void AddFinalChord(List<MidiEvent> events, int barStart, JazzChord chord)
        {
            var voicing = new List<int>
            {
                chord.Root - 24, // Bass note
                chord.Root,      // Root
                chord.Root + 4,  // Third
                chord.Root + 7,  // Fifth
                chord.Root + 11  // Seventh
            };
            
            foreach (var note in voicing)
            {
                int channel = note < 48 ? 0 : 1; // Bass or piano
                AddNote(events, barStart, channel, note, 80, TicksPerBeat * 4); // Hold for full bar
            }
        }
        
        private void AddSyncopatedAccents(List<MidiEvent> events, int barStart)
        {
            float beatPosition = 1.5f + _random.NextSingle() * 2f; // Between beats 1.5-3.5
            int tick = barStart + (int)(beatPosition * TicksPerBeat);
            
            var notes = new[] { 60, 64, 67 }; // Simple triad
            foreach (var note in notes)
            {
                int velocity = 65 + _random.Next(-5, 10);
                AddNote(events, tick, 1, note, velocity, TicksPerBeat/4);
            }
        }
    }
}