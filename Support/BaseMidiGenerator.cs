using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MidiGenerator.Support
{
    public abstract class BaseMidiGenerator : IMidiGenerator
    {
        protected const int TicksPerBeat = 480;
        
        public abstract string GenreName { get; }
        protected abstract int BeatsPerMinute { get; }
        
        public string CreateMidiFile(int durationSeconds, string fileName)
        {
            var events = GenerateEvents(durationSeconds);
            var midiData = ConvertToMidiFormat(events);
            
            File.WriteAllBytes(fileName, midiData);
            return Path.GetFullPath(fileName);
        }
        
        protected abstract List<MidiEvent> GenerateEvents(int durationSeconds);
        
        protected virtual List<SongStructure> CreateSongStructure(int totalBars)
        {
            // Default simple structure - can be overridden by genres
            var structure = new List<SongStructure>();
            
            if (totalBars <= 4)
            {
                structure.Add(new SongStructure { SectionName = "Main", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else if (totalBars <= 8)
            {
                structure.Add(new SongStructure { SectionName = "Verse", StartBar = 0, LengthBars = totalBars, SectionType = SongSection.Verse });
            }
            else
            {
                // Basic verse-chorus structure
                int remaining = totalBars;
                int currentBar = 0;
                
                // Intro (2 bars)
                if (remaining > 6)
                {
                    structure.Add(new SongStructure { SectionName = "Intro", StartBar = currentBar, LengthBars = 2, SectionType = SongSection.Intro });
                    currentBar += 2;
                    remaining -= 2;
                }
                
                // Verse (4 bars)
                if (remaining > 4)
                {
                    structure.Add(new SongStructure { SectionName = "Verse", StartBar = currentBar, LengthBars = 4, SectionType = SongSection.Verse });
                    currentBar += 4;
                    remaining -= 4;
                }
                
                // Chorus (4 bars)
                if (remaining > 2)
                {
                    int chorusLength = Math.Min(4, remaining - 2);
                    structure.Add(new SongStructure { SectionName = "Chorus", StartBar = currentBar, LengthBars = chorusLength, SectionType = SongSection.Chorus });
                    currentBar += chorusLength;
                    remaining -= chorusLength;
                }
                
                // Outro (remaining bars)
                if (remaining > 0)
                {
                    structure.Add(new SongStructure { SectionName = "Outro", StartBar = currentBar, LengthBars = remaining, SectionType = SongSection.Outro });
                }
            }
            
            return structure;
        }
        
        protected int CalculateBarsFromDuration(int durationSeconds)
        {
            double barsPerSecond = (double)BeatsPerMinute / 60 / 4;
            return Math.Max(1, (int)Math.Ceiling(durationSeconds * barsPerSecond));
        }
        
        protected void AddMetaEvents(List<MidiEvent> events)
        {
            int microsecondsPerBeat = 60_000_000 / BeatsPerMinute;
            var tempoBytes = new byte[]
            {
                (byte)((microsecondsPerBeat >> 16) & 0xFF),
                (byte)((microsecondsPerBeat >> 8) & 0xFF),
                (byte)(microsecondsPerBeat & 0xFF)
            };
            
            events.Add(new MidiEvent(0, 0xFF, 0x51, 0x03, tempoBytes[0], tempoBytes[1], tempoBytes[2]));
            events.Add(new MidiEvent(0, 0xFF, 0x58, 0x04, 0x04, 0x02, 0x18, 0x08));
        }
        
        protected void AddNote(List<MidiEvent> events, int startTick, int channel, int note, int velocity, int duration)
        {
            events.Add(new MidiEvent(startTick, (byte)(0x90 | channel), (byte)note, (byte)velocity));
            events.Add(new MidiEvent(startTick + duration, (byte)(0x80 | channel), (byte)note, 0));
        }
        
        protected void AddProgramChange(List<MidiEvent> events, int tick, int channel, int program)
        {
            events.Add(new MidiEvent(tick, (byte)(0xC0 | channel), (byte)program));
        }
        
        private byte[] ConvertToMidiFormat(List<MidiEvent> events)
        {
            var trackData = BuildTrackData(events);
            var midiData = new List<byte>();
            
            AddMidiHeader(midiData);
            AddTrackChunk(midiData, trackData);
            
            return midiData.ToArray();
        }
        
        private List<byte> BuildTrackData(List<MidiEvent> events)
        {
            var sortedEvents = events.OrderBy(e => e.Tick).ThenBy(e => e.Data[0]).ToList();
            var trackData = new List<byte>();
            int lastTick = 0;
            
            foreach (var evt in sortedEvents)
            {
                WriteVariableLength(trackData, evt.Tick - lastTick);
                trackData.AddRange(evt.Data);
                lastTick = evt.Tick;
            }
            
            return trackData;
        }
        
        private void AddMidiHeader(List<byte> midiData)
        {
            midiData.AddRange(System.Text.Encoding.ASCII.GetBytes("MThd"));
            midiData.AddRange(BitConverter.GetBytes(SwapEndian(6)));
            midiData.AddRange(BitConverter.GetBytes(SwapEndian((short)0)));
            midiData.AddRange(BitConverter.GetBytes(SwapEndian((short)1)));
            midiData.AddRange(BitConverter.GetBytes(SwapEndian((short)TicksPerBeat)));
        }
        
        private void AddTrackChunk(List<byte> midiData, List<byte> trackData)
        {
            midiData.AddRange(System.Text.Encoding.ASCII.GetBytes("MTrk"));
            midiData.AddRange(BitConverter.GetBytes(SwapEndian(trackData.Count)));
            midiData.AddRange(trackData);
        }
        
        private void WriteVariableLength(List<byte> data, int value)
        {
            var bytes = new List<byte> { (byte)(value & 0x7F) };
            
            value >>= 7;
            while (value > 0)
            {
                bytes.Insert(0, (byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            
            data.AddRange(bytes);
        }
        
        private int SwapEndian(int value) =>
            ((value & 0xFF) << 24) | (((value >> 8) & 0xFF) << 16) | 
            (((value >> 16) & 0xFF) << 8) | ((value >> 24) & 0xFF);
        
        private short SwapEndian(short value) =>
            (short)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));
    }
}