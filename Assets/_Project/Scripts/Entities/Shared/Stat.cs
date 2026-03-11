using System;
using System.Collections.Generic;

public enum StatModType
{
    Flat,
    PercentAdd,
    PercentMult
}

public class StatModifier
{
    public readonly float Value;
    public readonly StatModType Type;
    public readonly object Source;

    public StatModifier(float value, StatModType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}

[Serializable]
public class Stat
{
    private float _baseValue;
    public float BaseValue
    {
        get => _baseValue;
        set
        {
            if (Math.Abs(_baseValue - value) > 0.0001f)
            {
                _baseValue = value;
                _isDirty = true;
            }
        }
    }

    private bool _isDirty = true;
    private float _lastCalculatedValue;
    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    public Stat(float baseValue)
    {
        _baseValue = baseValue;
    }

    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _lastCalculatedValue = CalculateFinalValue();
                _isDirty = false;
            }
            return _lastCalculatedValue;
        }
    }

    public void AddModifier(StatModifier mod)
    {
        _isDirty = true;
        _modifiers.Add(mod);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (_modifiers.Remove(mod))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            if (_modifiers[i].Source == source)
            {
                _isDirty = true;
                _modifiers.RemoveAt(i);
            }
        }
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            StatModifier mod = _modifiers[i];

            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= (1 + mod.Value);
            }
        }

        finalValue *= (1 + sumPercentAdd);
        return (float)Math.Round(finalValue, 4);
    }

    public override string ToString()
    {
        return Value.ToString("F2");
    }
}