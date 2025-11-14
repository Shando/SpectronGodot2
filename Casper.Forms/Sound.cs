using Godot;
using System;

namespace Casper.Forms
{
    public partial class Sound : Node
    {
        private AYChip _ayChip;
        private AYSignalGenerator _aySignalGenerator;
        private AudioStreamGenerator _stream;
        private AudioStreamGeneratorPlayback _playback;
        private float[] _samples;
        private double _bufferDuration = 0.02;
        private int _sampleRate = 44100;
        private int _numSamples;

        public override void _Ready()
        {
            _ayChip = new AYChip();
            _aySignalGenerator = new AYSignalGenerator(AYSignalGenerator.AYSignalGeneratorType.Square, 0.5f);
            _aySignalGenerator.Frequency = 440f;
            _aySignalGenerator.Mix = true;
            _aySignalGenerator.EnvelopePeriod = 0.5f;
            _aySignalGenerator.EnvelopeShape = 0;
            _aySignalGenerator.EnvelopeStart();

            _stream = new AudioStreamGenerator
            {
                MixRate = _sampleRate,
                BufferLength = (float)_bufferDuration
            };

            var player = new AudioStreamPlayer();
            player.Stream = _stream;
            AddChild(player);
            player.Play();

            _playback = (AudioStreamGeneratorPlayback)player.GetStreamPlayback();

            _numSamples = (int)(_bufferDuration * _sampleRate);
            _samples = new float[_numSamples];
        }

        public void ActivateSpeaker(double time)
        {
            if (_samples == null || _samples.Length == 0)
                return;

            var r = time / _bufferDuration;

            if (r >= 0 && r < 1)
            {
                var i = (int)(r * _samples.Length);
                _samples[i] = 1.0f; // Max amplitude for float PCM
            }
        }

        public void FlushBuffer()
        {
            if (_samples == null)
                return;

            for (int i = 0; i < _samples.Length; i++)
            {
                float aySample = _aySignalGenerator.GenerateSample();
                float beeperSample = _samples[i];
                float mixed = Math.Clamp(beeperSample + aySample, -1f, 1f);
                _playback.PushFrame(new Vector2(mixed, mixed));
            }

            Array.Clear(_samples, 0, _samples.Length);
        }

        public void WriteAYRegister(byte reg, byte value)
        {
            _ayChip.WriteRegister(reg, value);
        }

        public byte ReadAYRegister(byte reg)
        {
            return _ayChip.ReadRegister(reg);
        }
    }
}
