using KackZug.Core.Checking;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Checking;

/// <summary>Prueft die Bewertung von Verspaetung, Ausfall, Gleis und Umstieg</summary>
public class StatusEvaluatorTests
{
    private static readonly DateOnly Day = new DateOnly(2026, 10, 21);

    [Fact]
    public void EverythingOnTime()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.OnTime, status.Overall);
        var transfer = Assert.Single(status.Transfers);
        Assert.False(transfer.AtRisk);
        Assert.Equal(5, transfer.SpareMinutes);
    }

    [Fact]
    public void WithoutLiveDataItIsOnlyPlanned()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[0]!.HasLiveData = false;
        observations[2]!.HasLiveData = false;

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.PlannedOnly, status.Overall);
        Assert.False(status.HasLiveData);
    }

    [Fact]
    public void TramWithoutLiveDataDoesNotHideTrainData()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[2]!.HasLiveData = false;

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.OnTime, status.Overall);
    }

    [Fact]
    public void SmallDelayBelowThresholdIsOnTime()
    {
        var connection = Connection();
        var observations = Observations(connection, 3, 3);

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.OnTime, status.Overall);
        Assert.Equal(3, status.Legs[0].DelayMinutes);
    }

    [Fact]
    public void DelayThatBreaksTheTransferIsReported()
    {
        var connection = Connection();
        var observations = Observations(connection, 8, 0);

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.TransferAtRisk, status.Overall);
        Assert.Equal(LegStatus.Delayed, status.Legs[0].Status);
        Assert.True(status.Transfers[0].AtRisk);
        Assert.Equal(-3, status.Transfers[0].SpareMinutes);
    }

    [Fact]
    public void DelayedFollowingTramKeepsTheTransfer()
    {
        var connection = Connection();
        var observations = Observations(connection, 8, 8);

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.False(status.Transfers[0].AtRisk);
        Assert.Equal(LegStatus.Delayed, status.Overall);
    }

    [Fact]
    public void TightPlannedTransferGivesNoFalseAlarm()
    {
        var connection = Connection();
        connection.Legs[2].PlannedDeparture = new TimeOnly(7, 16);
        connection.Legs[2].PlannedArrival = new TimeOnly(7, 24);
        var observations = Observations(connection, 0, 0);

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings { MinTransferMinutes = 4 });

        Assert.False(status.Transfers[0].AtRisk);
    }

    [Fact]
    public void ArrivalDelayIsGuessedFromDepartureWhenDetailsAreMissing()
    {
        var connection = Connection();
        var observations = Observations(connection, 8, 0);
        observations[0]!.LiveArrival = null;
        observations[0]!.ArrivalKnown = false;

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.True(status.Transfers[0].AtRisk);
    }

    [Fact]
    public void CancellationIsWorstAndSkipsTransfer()
    {
        var connection = Connection();
        var observations = Observations(connection, 8, 0);
        observations[0]!.DepartureCancelled = true;

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.Cancelled, status.Overall);
        Assert.Empty(status.Transfers);
    }

    [Fact]
    public void PartlyCancelledAndNotFound()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[0]!.ArrivalCancelled = true;
        observations[2]!.Result = ObservationResult.NotFound;

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.PartlyCancelled, status.Overall);
        Assert.Equal(LegStatus.NotFound, status.Legs[2].Status);
    }

    [Fact]
    public void MissingDestinationWithoutLastStopIsNoData()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[0]!.ArrivalMissing = true;

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.Unknown, status.Legs[0].Status);
        Assert.Equal(LegStatus.Unknown, status.Overall);
        Assert.Equal(AlertSeverity.Info, AlertComposer.SeverityOf(status.Overall));
    }

    [Fact]
    public void TrainEndingEarlyIsAProblemWithItsLastStop()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[0]!.ArrivalMissing = true;
        observations[0]!.EndsAt = "Zwischenort";

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.PartlyCancelled, status.Overall);
        Assert.Equal(AlertSeverity.Problem, AlertComposer.SeverityOf(status.Overall));
        Assert.Equal("Nur bis Zwischenort", AlertComposer.BadgeText(status));
    }

    [Fact]
    public void PlatformChangeIsReported()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[0]!.CurrentPlatform = "4";

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.PlatformChanged, status.Overall);
        Assert.Equal("4", status.Legs[0].NewPlatform);
    }

    [Fact]
    public void UnknownWhenBahnCannotBeReached()
    {
        var connection = Connection();
        var observations = Observations(connection, 0, 0);
        observations[0] = LegObservation.Unknown(DateTime.MinValue, DateTime.MinValue, "offline");

        var status = StatusEvaluator.Evaluate(connection, observations, new Settings());

        Assert.Equal(LegStatus.Unknown, status.Overall);
        Assert.Empty(status.Transfers);
    }

    private static SavedConnection Connection()
    {
        return SampleData.CreateShift("Fruehdienst").Outbound!;
    }

    private static List<LegObservation?> Observations(SavedConnection connection, int trainDelay, int tramDelay)
    {
        var train = connection.Legs[0];
        var tram = connection.Legs[2];
        return new List<LegObservation?>
        {
            Found(train, trainDelay),
            null,
            Found(tram, tramDelay)
        };
    }

    private static LegObservation Found(Leg leg, int delayMinutes)
    {
        var plannedDeparture = leg.DepartureOn(Day);
        var plannedArrival = leg.ArrivalOn(Day);
        return new LegObservation
        {
            Result = ObservationResult.Found,
            PlannedDeparture = plannedDeparture,
            PlannedArrival = plannedArrival,
            LiveDeparture = plannedDeparture.AddMinutes(delayMinutes),
            LiveArrival = plannedArrival.AddMinutes(delayMinutes),
            ArrivalKnown = true,
            HasLiveData = true,
            CurrentPlatform = leg.PlannedPlatform
        };
    }
}
