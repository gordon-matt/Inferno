using Inferno.Tasks.Scheduling;
using Xunit;

namespace Inferno.Tests;

public class RecurringScheduleTests
{
    [Theory]
    [InlineData(30, "* * * * *")]
    [InlineData(60, "* * * * *")]
    [InlineData(600, "*/10 * * * *")]
    [InlineData(7 * 60, "*/6 * * * *")]
    [InlineData(3600, "0 * * * *")]
    [InlineData(2 * 3600, "0 */2 * * *")]
    [InlineData(86400, "0 0 * * *")]
    [InlineData(2 * 86400, "0 0 */2 * *")]
    public void FromSeconds_Interval_UsesACronStepHangfireCanKeep(int seconds, string cron)
    {
        Assert.Equal(cron, RecurringSchedule.FromSeconds(seconds));
    }

    [Theory]
    [InlineData(30, 1)]
    [InlineData(60, 1)]
    [InlineData(600, 10)]
    [InlineData(7 * 60, 6)]
    [InlineData(3600, 60)]
    [InlineData(2 * 3600, 120)]
    [InlineData(86400, 1440)]
    [InlineData(2 * 86400, 2880)]
    public void TryToMinutes_CronFromSeconds_ReadsTheEffectiveInterval(int seconds, int minutes)
    {
        Assert.Equal(minutes, RecurringSchedule.TryToMinutes(RecurringSchedule.FromSeconds(seconds)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("15 10 * * *")]
    [InlineData("*/7 * * * *")]
    public void TryToMinutes_UnknownCron_ReturnsNull(string cron)
    {
        Assert.Null(RecurringSchedule.TryToMinutes(cron));
    }
}
