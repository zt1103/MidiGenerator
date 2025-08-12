namespace MidiGenerator.Support
{
    public class MidiEvent
    {
        public int Tick { get; set; }
        public byte[] Data { get; set; }
        
        public MidiEvent(int tick, params byte[] data)
        {
            Tick = tick;
            Data = data;
        }
    }
}