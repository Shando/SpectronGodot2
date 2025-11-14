using System;

public class AYChip : IAYChip
{
    private AYSignalGenerator[] _channels = new AYSignalGenerator[3];
    private byte[] registers = new byte[16];
    private float[] volumes = new float[16]; // Precomputed volume table
    public int clockDivider = 40; // Adjust for your CPU clock vs audio rate

    public AYChip()
    {
        InitVolumeTable();

        for (int i = 0; i < 3; i++)
            _channels[i] = new AYSignalGenerator(AYSignalGenerator.AYSignalGeneratorType.Square, 0.5f);
    }

    private void InitVolumeTable()
    {
        for (int i = 0; i < 16; i++)
            volumes[i] = (float)Math.Pow(2, i / 2.0) / 255f;
    }

    public void WriteRegister(byte reg, byte value)
    {
        if (reg >= 16) return;
        registers[reg] = value;

        switch (reg)
        {
            case 0:
            case 1: // Channel A tone
                UpdateFrequency(0);
                break;
            case 2:
            case 3: // Channel B tone
                UpdateFrequency(1);
                break;
            case 4:
            case 5: // Channel C tone
                UpdateFrequency(2);
                break;
            case 8:
            case 9:
            case 10: // Volume
                UpdateVolume(reg - 8);
                break;
            case 11: // Envelope period low
            case 12: // Envelope period high
            case 13: // Envelope shape
                UpdateEnvelope();
                break;
        }
    }

    public byte ReadRegister(byte reg)
    {
        return reg < 16 ? registers[reg] : (byte)0xFF;
    }

    private void UpdateFrequency(int channel)
    {
        int coarse = registers[channel * 2 + 1] & 0x0F;
        int fine = registers[channel * 2];
        int period = (coarse << 8) | fine;
        float freq = period > 0 ? 1_750_000f / (16f * period) : 0f;
        _channels[channel].Frequency = freq;
    }

    private void UpdateVolume(int channel)
    {
        byte vol = registers[8 + channel];
        bool useEnvelope = (vol & 0x10) != 0;
        _channels[channel].Gain = useEnvelope ? 0f : ((vol & 0x0F) / 15f);
        _channels[channel].Mix = true;
    }

    private void UpdateEnvelope()
    {
        int period = (registers[12] << 8) | registers[11];
        byte shape = registers[13];

        foreach (var ch in _channels)
        {
            ch.EnvelopePeriod = period / 1_750_000f; // Convert to seconds
            ch.EnvelopeShape = shape;
            ch.EnvelopeStart();
        }
    }

    public void Tick()
    {
        // No-op: all handled in GenerateSample()
    }

    public float MixSample()
    {
        float sum = 0f;

        foreach (var ch in _channels)
            sum += ch.GenerateSample();

        return sum / 3f;
    }
}
