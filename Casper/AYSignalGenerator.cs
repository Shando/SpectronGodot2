using Godot;
using System;

public class AYSignalGenerator
{
    public enum AYSignalGeneratorType
    {
        White,
        Square
    }

    private AYSignalGeneratorType _type;
    private Random _random = new Random();

    private float _gain;
    private float _frequency;
    private bool _mix;
    private bool _muted;

    private float _maxGain;
    private float _initialGain;
    private float _envelopePeriod;
    private byte _envelopeShape;
    private float _envelopeStep;
    private bool _envelopeOn;

    private float _phaseAngle;
    private float _noiseValue1;
    private float _noiseValue2;

    private int _sampleRate;

    public AYSignalGenerator(AYSignalGeneratorType type, float maxGain, int sampleRate = 44100)
    {
        _type = type;
        _maxGain = maxGain;
        _sampleRate = sampleRate;
        _frequency = 1f;
        _gain = 0f;
        _initialGain = 0f;
        _envelopeOn = false;
        _muted = false;
        _mix = false;
        _phaseAngle = 0f;
        _noiseValue1 = (float)_random.NextDouble();
        _noiseValue2 = (float)_random.NextDouble();
    }

    public bool Muted
    {
        get => _muted;
        set => _muted = value;
    }

    public float Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            EnvelopeReset();
        }
    }

    public byte EnvelopeShape
    {
        get => _envelopeShape;
        set
        {
            _envelopeShape = value;
            EnvelopeReset();
        }
    }

    public float EnvelopePeriod
    {
        get => _envelopePeriod;
        set
        {
            _envelopePeriod = value;
            EnvelopeReset();
        }
    }

    public float Gain
    {
        get => _gain;
        set => _gain = value;
    }

    public bool Mix
    {
        get => _mix;
        set
        {
            _mix = value;
            if (_mix)
            {
                EnvelopeReset();
                _noiseValue1 = (float)_random.NextDouble();
                _noiseValue2 = (float)_random.NextDouble();
            }
        }
    }

    public void EnvelopeStart()
    {
        _envelopeOn = true;
        EnvelopeReset();
    }

    public void EnvelopeStop()
    {
        _envelopeOn = false;
    }

    private void EnvelopeReset()
    {
        _envelopeStep = _maxGain / (_envelopePeriod * _sampleRate);

        switch (_envelopeShape)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 8:
            case 9:
            case 10:
            case 11:
                _initialGain = _maxGain;
                _envelopeStep = -_envelopeStep;
                break;
            default:
                _initialGain = 0f;
                break;
        }

        if (_envelopeOn)
            _gain = _initialGain;
    }

    public float GenerateSample()
    {
        if (!_mix || _muted)
            return 0f;

        float sample = 0f;
        float phaseAngleIncrement = 2f * Mathf.Pi * _frequency / _sampleRate;

        switch (_type)
        {
            case AYSignalGeneratorType.White:
                sample = (_phaseAngle < Mathf.Pi ? _noiseValue1 : _noiseValue2) * _gain;
                break;
            case AYSignalGeneratorType.Square:
                sample = (_phaseAngle < Mathf.Pi ? 1f : -1f) * _gain;
                break;
        }

        if (_envelopeOn)
        {
            _gain += _envelopeStep;

            switch (_envelopeShape)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 9:
                    if (_gain < 0f) { _gain = 0f; _envelopeStep = 0f; }
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 15:
                    if (_gain > _maxGain) { _gain = 0f; _envelopeStep = 0f; }
                    break;
                case 8:
                    if (_gain < 0f) _gain = _maxGain;
                    break;
                case 10:
                case 14:
                    if (_gain < 0f || _gain > _maxGain) _envelopeStep = -_envelopeStep;
                    _gain = Mathf.Clamp(_gain, 0f, _maxGain);
                    break;
                case 11:
                    if (_gain < 0f) { _gain = _maxGain; _envelopeStep = 0f; }
                    break;
                case 12:
                    if (_gain > _maxGain) _gain = 0f;
                    break;
                case 13:
                    if (_gain > _maxGain) { _gain = _maxGain; _envelopeStep = 0f; }
                    break;
            }
        }

        _phaseAngle += phaseAngleIncrement;
        if (_phaseAngle > 2f * Mathf.Pi)
        {
            _phaseAngle -= 2f * Mathf.Pi;
            _noiseValue1 = (float)_random.NextDouble();
            _noiseValue2 = (float)_random.NextDouble();
        }

        return sample;
    }
}
