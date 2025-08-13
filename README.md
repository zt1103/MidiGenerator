# MIDI Generator Project

A 100% Claude coded (human guided) C# framework for generating procedural MIDI files in different musical genres.

Does not make particularly good music, but it is vaguely music, as interpreted by Claude. 

Feel free to riff on the project and/or use your generated files for any purpose whatsoever.

Usage: Open the solution in Visual Studio (or your preferred IDE and run. If you'd like to add a genre, Implement IMidiGenerator and it will automatically add your genre to the list for selection. (see below for more details)

## Project Structure

### Core Files
- **`IMidiGenerator.cs`** - Interface defining the contract for all MIDI generators
- **`MidiEvent.cs`** - Data structure representing a single MIDI event with timing
- **`BaseMidiGenerator.cs`** - Abstract base class containing shared MIDI file generation logic
- **`Program.cs`** - Main entry point for LinqPad execution

### Genre Implementations
- **`GroovyMidiGenerator.cs`** - Funk/groove style with syncopated bass and tight drums
- **`MetalMidiGenerator.cs`** - Heavy metal with power chords, double bass, and aggressive drumming
- **`JazzMidiGenerator.cs`** - Jazz with walking bass, swing feel, and improvised solos

## Architecture Rules

### Interface Contract (`IMidiGenerator`)
All generators must implement:
- `string CreateMidiFile(int durationSeconds)` - Generate and save MIDI file
- `string GenreName { get; }` - Return genre name for identification

### Base Class Responsibilities (`BaseMidiGenerator`)
Handles all MIDI format complexity:
- MIDI header construction (format, tracks, timing)
- Track data compilation and delta-time calculation
- Variable-length quantity encoding
- Endian conversion for binary data
- Event sorting by timestamp

### Generator Implementation Rules

#### Required Overrides
- `string GenreName` - Genre identifier
- `int BeatsPerMinute` - Tempo for the genre
- `List<MidiEvent> GenerateEvents(int durationSeconds)` - Core music generation logic

#### Helper Methods Available
- `AddNote(events, tick, channel, note, velocity, duration)` - Add note on/off pair
- `AddProgramChange(events, tick, channel, program)` - Set instrument
- `AddMetaEvents(events)` - Add tempo and time signature (call this first)
- `CalculateBarsFromDuration(durationSeconds)` - Convert time to musical bars

#### Music Generation Guidelines

1. **Start with meta events**: Always call `AddMetaEvents(events)` first
2. **Set instruments**: Use `AddProgramChange()` for each channel
3. **Calculate structure**: Use `CalculateBarsFromDuration()` to determine song length
4. **Generate per bar**: Loop through bars, adding patterns for each instrument
5. **End properly**: Add end-of-track event at final tick

#### MIDI Channel Conventions
- **Channel 0**: Bass instruments
- **Channel 1**: Chord/comping instruments (piano, guitar)
- **Channel 2**: Lead/solo instruments (sax, lead guitar)
- **Channel 3**: Additional melody instruments
- **Channel 9**: Drums (GM standard)

#### Timing Constants
- `TicksPerBeat = 480` - High resolution timing
- `ticksPerBar = TicksPerBeat * 4` - Assumes 4/4 time signature
- Use fractions like `TicksPerBeat/2` for eighth notes, `TicksPerBeat/4` for sixteenths

## Development Guidelines

### Adding New Genres

1. Create new class inheriting from `BaseMidiGenerator`
2. Implement required properties and methods
3. Follow musical conventions for the genre
4. Use appropriate tempo (`BeatsPerMinute`)
5. Select suitable GM instruments for `AddProgramChange()`

### Code Style Rules

- **Single Responsibility**: Each method should handle one musical element
- **DRY Principle**: Extract common patterns into helper methods
- **Concise Methods**: Keep methods focused and readable
- **No Repetition**: Use loops and patterns rather than hardcoded sequences

### Musical Considerations

#### Groove Generators
- Focus on rhythm and syncopation
- Use moderate tempos (100-140 BPM)
- Emphasize bass and drums interaction

#### Metal Generators  
- Fast tempos (140-180 BPM)
- Power chords and palm muting simulation
- Aggressive drum patterns with double bass
- Use distorted guitar programs (29-32)

#### Jazz Generators
- Swing feel with delayed off-beats
- Complex harmony (7th chords, extensions)
- Walking bass lines with chord tones
- Improvised solos with appropriate scales
- Sparse comping to leave space

### Randomization Guidelines

For non-deterministic genres (like Jazz):
- Use `Random` instance as private field
- Vary note selection, timing, and dynamics
- Maintain musical coherence despite randomness
- Avoid rapid-fire patterns that sound like "cellphone beeping"
- Use longer note durations for legato instruments

### File Organization

- Keep each generator in its own file
- Use descriptive method names (`AddWalkingBass`, `AddPowerChords`)
- Group related functionality (all drum methods together)
- Include comments for complex musical concepts

