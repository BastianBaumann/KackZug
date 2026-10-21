namespace KackZug.Core.Planning;

/// <summary>Was an einem eingegebenen Datum einer Einzelfahrt nicht passt</summary>
public enum TripDateProblem
{
    None,
    Invalid,
    Past,
    AlreadyOver
}
