namespace MidiGenerator.Support
{
    public interface IMidiGenerator
    {
        string CreateMidiFile(int durationSeconds, string fileName);
        string GenreName { get; }
    }
}