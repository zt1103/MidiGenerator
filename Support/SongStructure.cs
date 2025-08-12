namespace MidiGenerator.Support
{
    public class SongStructure
    {
        public string SectionName { get; set; } = "";
        public int StartBar { get; set; }
        public int LengthBars { get; set; }
        public SongSection SectionType { get; set; }
    }

    public enum SongSection
    {
        Intro,
        Verse,
        Chorus,
        Bridge,
        Solo,
        Outro,
        PreChorus,
        Breakdown
    }
}