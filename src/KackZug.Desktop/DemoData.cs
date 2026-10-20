#if DEBUG
using KackZug.Core.Models;
using KackZug.Core.Storage;

namespace KackZug.Desktop;

/// <summary>Schreibt erfundene Beispieldaten in den Datenordner, nur fuer die Screenshot-Tour</summary>
internal static class DemoData
{
    public static void WriteIfRequested()
    {
        var directory = Environment.GetEnvironmentVariable("KACKZUG_DATA_DIR");
        var wanted = Environment.GetEnvironmentVariable("KACKZUG_DEMO") == "1";
        if (!wanted || string.IsNullOrEmpty(directory))
        {
            return;
        }

        var data = new AppData();
        var early = new Shift { Name = "Frühdienst", ColorHex = "#1E9E8B" };
        early.Outbound = Connection(
            Ride("RE7", "NRE", "4711", "Musterstadt Hbf", "8000001", "Beispielhausen", "8000002", 5, 42, 6, 10, "2"),
            Walk("Beispielhausen", "8000002", "Markt, Beispielhausen", "900001", 6, 10, 6, 15),
            Ride("STR 2", "STR", "2", "Markt, Beispielhausen", "900001", "Werkstor, Beispielhausen", "900002", 6, 20, 6, 28, null));
        early.Return = Connection(
            Ride("RE7", "NRE", "4728", "Beispielhausen", "8000002", "Musterstadt Hbf", "8000001", 14, 48, 15, 16, "5"));
        var late = new Shift { Name = "Spätdienst", ColorHex = "#8B5A2B" };
        late.Outbound = Connection(
            Ride("RB21", "DRB", "80664", "Musterstadt Hbf", "8000001", "Beispielhausen", "8000002", 13, 7, 13, 43, "1"));
        data.Shifts.Add(early);
        data.Shifts.Add(late);

        var today = DateOnly.FromDateTime(DateTime.Today);
        data.CurrentWeek = WeekPlan.CreateEmpty(WeekPlan.MondayOf(today));
        data.CurrentWeek.GetDay(DayOfWeek.Monday).ShiftId = early.Id;
        data.CurrentWeek.GetDay(DayOfWeek.Tuesday).ShiftId = early.Id;
        data.CurrentWeek.GetDay(DayOfWeek.Wednesday).ShiftId = late.Id;
        data.CurrentWeek.GetDay(DayOfWeek.Thursday).ShiftId = late.Id;
        data.CurrentWeek.GetDay(DayOfWeek.Thursday).ReturnEnabled = false;
        data.CurrentWeek.GetDay(DayOfWeek.Friday).ShiftId = early.Id;

        new AppDataStore(directory).Save(data);
    }

    private static SavedConnection Connection(params Leg[] legs)
    {
        var connection = new SavedConnection { ImportedAt = DateTimeOffset.Now };
        connection.Legs.AddRange(legs);
        return connection;
    }

    private static Leg Ride(string name, string category, string number, string from, string fromEva, string to, string toEva, int departureHour, int departureMinute, int arrivalHour, int arrivalMinute, string? platform)
    {
        return new Leg
        {
            Name = name,
            Category = category,
            TrainNumber = number,
            From = new Station { Name = from, EvaNumber = fromEva },
            To = new Station { Name = to, EvaNumber = toEva },
            PlannedDeparture = new TimeOnly(departureHour, departureMinute),
            PlannedArrival = new TimeOnly(arrivalHour, arrivalMinute),
            PlannedPlatform = platform
        };
    }

    private static Leg Walk(string from, string fromEva, string to, string toEva, int departureHour, int departureMinute, int arrivalHour, int arrivalMinute)
    {
        return new Leg
        {
            Kind = LegKind.Walk,
            Name = "Fussweg",
            From = new Station { Name = from, EvaNumber = fromEva },
            To = new Station { Name = to, EvaNumber = toEva },
            PlannedDeparture = new TimeOnly(departureHour, departureMinute),
            PlannedArrival = new TimeOnly(arrivalHour, arrivalMinute)
        };
    }
}
#endif
