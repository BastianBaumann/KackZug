using System.Text.Json.Nodes;

namespace KackZug.Core.Tests.Checking;

/// <summary>Baut erfundene Abfahrtstafeln und Fahrtdetails im Format von bahn.de</summary>
public static class BahnJsonBuilder
{
    public static JsonObject BoardEntry(string time, string name, string category, string number, string platform)
    {
        var journeyId = "2|#VN#1#ST#1#PI#0#ZI#1#TA#0#DA#201026#1S#1#1T#1#LS#2#LT#2#PU#80#RT#1#CA#" + category + "#ZE#" + number + "#ZB#" + name + "#PC#3#";
        return new JsonObject
        {
            ["bahnhofsId"] = "8000001",
            ["zeit"] = time,
            ["gleis"] = platform,
            ["journeyId"] = journeyId,
            ["meldungen"] = new JsonArray(),
            ["verkehrmittel"] = new JsonObject
            {
                ["name"] = number,
                ["mittelText"] = name,
                ["produktGattung"] = "REGIONAL"
            }
        };
    }

    public static string Board(params JsonObject[] entries)
    {
        var array = new JsonArray();
        foreach (var entry in entries)
        {
            array.Add(entry);
        }

        return new JsonObject { ["entries"] = array }.ToJsonString();
    }

    public static JsonObject Stop(string eva, string name, string? arrival, string? liveArrival, string? departure, string? liveDeparture, string? platform)
    {
        var stop = new JsonObject
        {
            ["extId"] = eva,
            ["name"] = name,
            ["risNotizen"] = new JsonArray()
        };
        if (platform is not null)
        {
            stop["gleis"] = platform;
        }

        if (arrival is not null)
        {
            stop["ankunft"] = Times(arrival, liveArrival);
        }

        if (departure is not null)
        {
            stop["abfahrt"] = Times(departure, liveDeparture);
        }

        return stop;
    }

    public static string Trip(bool cancelled, params JsonObject[] stops)
    {
        var array = new JsonArray();
        foreach (var stop in stops)
        {
            array.Add(stop);
        }

        return new JsonObject
        {
            ["zugName"] = "erfunden",
            ["cancelled"] = cancelled,
            ["halte"] = array
        }.ToJsonString();
    }

    private static JsonObject Times(string planned, string? live)
    {
        var times = new JsonObject { ["sollzeit"] = planned };
        if (live is not null)
        {
            times["echtzeit"] = live;
        }

        return times;
    }
}
