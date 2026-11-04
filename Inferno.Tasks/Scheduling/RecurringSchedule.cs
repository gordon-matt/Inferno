namespace Inferno.Tasks.Scheduling;

/// <summary>
/// Turns an <see cref="ITask.DefaultInterval"/> into a cron expression. Hangfire cannot run more often
/// than once a minute, and a minute step has to divide the hour or the job drifts.
/// </summary>
public static class RecurringSchedule
{
    public static string FromSeconds(int seconds)
    {
        if (seconds <= 60)
        {
            return "* * * * *";
        }

        int minutes = (int)Math.Round(seconds / 60d, MidpointRounding.AwayFromZero);

        if (minutes >= 1440 && minutes % 1440 == 0)
        {
            int days = minutes / 1440;
            return days == 1 ? "0 0 * * *" : $"0 0 */{Math.Min(days, 28)} * *";
        }

        if (minutes >= 60 && minutes % 60 == 0)
        {
            int hours = minutes / 60;
            return hours == 1 ? "0 * * * *" : $"0 */{hours} * * *";
        }

        if (minutes >= 60)
        {
            int hours = Math.Max(1, (int)Math.Round(minutes / 60d, MidpointRounding.AwayFromZero));
            return hours == 1 ? "0 * * * *" : $"0 */{Math.Min(hours, 23)} * * *";
        }

        int step = LargestDivisorAtMost(60, minutes);
        return step <= 1 ? "* * * * *" : $"*/{step} * * * *";
    }

    /// <summary>
    /// The interval, in minutes, of a cron expression <see cref="FromSeconds"/> would write.
    /// Anything else, including a cron typed by hand in Hangfire, returns null.
    /// </summary>
    public static int? TryToMinutes(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return null;
        }

        string[] parts = cron.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 5)
        {
            return null;
        }

        if (parts is ["*", "*", "*", "*", "*"])
        {
            return 1;
        }

        if (parts[1] == "*" && parts[2] == "*" && parts[3] == "*" && parts[4] == "*"
            && TryStep(parts[0], 2, 59, out int minuteStep) && 60 % minuteStep == 0)
        {
            return minuteStep;
        }

        if (parts is ["0", "*", "*", "*", "*"])
        {
            return 60;
        }

        if (parts[0] == "0" && parts[2] == "*" && parts[3] == "*" && parts[4] == "*"
            && TryStep(parts[1], 2, 23, out int hourStep))
        {
            return hourStep * 60;
        }

        if (parts is ["0", "0", "*", "*", "*"])
        {
            return 1440;
        }

        if (parts[0] == "0" && parts[1] == "0" && parts[3] == "*" && parts[4] == "*"
            && TryStep(parts[2], 2, 28, out int dayStep))
        {
            return dayStep * 1440;
        }

        return null;
    }

    private static bool TryStep(string field, int minimum, int maximum, out int step)
    {
        step = 0;
        return field.StartsWith("*/", StringComparison.Ordinal)
            && int.TryParse(field.AsSpan(2), out step)
            && step >= minimum
            && step <= maximum;
    }

    private static int LargestDivisorAtMost(int of, int maximum)
    {
        for (int candidate = Math.Min(of, maximum); candidate > 1; candidate--)
        {
            if (of % candidate == 0)
            {
                return candidate;
            }
        }

        return 1;
    }
}
