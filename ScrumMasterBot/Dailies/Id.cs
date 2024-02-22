using System;

namespace ScrumMasterBot.Dailies;

public readonly struct Id : IEquatable<Id>
{
    public readonly string Date => _dateString;

    public Id(DateTime date)
    {
        _dateString = $"{date:d}";
    }

    readonly string _dateString;

    public bool Equals(Id other)
    {
        return Date == other.Date;
    }
}
