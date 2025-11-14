public interface IAYChip
{
    void WriteRegister(byte reg, byte value);
    byte ReadRegister(byte reg);
    void Tick(); // Advance one clock cycle
    float MixSample(); // Return mixed audio sample [-1.0, 1.0]
}
