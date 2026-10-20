namespace KackZug.Core.Models;

/// <summary>Eine Schicht mit Hin- und Rueckfahrt</summary>
public sealed class Shift
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public string ColorHex { get; set; } = "#1E9E8B";

    public SavedConnection? Outbound { get; set; }

    public SavedConnection? Return { get; set; }

    public SavedConnection? GetConnection(TripDirection direction)
    {
        if (direction == TripDirection.Outbound)
        {
            return Outbound;
        }

        return Return;
    }

    public void SetConnection(TripDirection direction, SavedConnection? connection)
    {
        if (direction == TripDirection.Outbound)
        {
            Outbound = connection;
        }
        else
        {
            Return = connection;
        }
    }
}
