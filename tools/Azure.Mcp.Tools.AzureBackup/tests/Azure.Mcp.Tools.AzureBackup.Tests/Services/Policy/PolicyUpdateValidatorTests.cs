// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services.Policy;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services.Policy;

public class PolicyUpdateValidatorTests
{
    private static PolicyUpdateOptions BaseOptions() => new()
    {
        Subscription = "sub",
        ResourceGroup = "rg",
        Vault = "v",
        Policy = "p",
    };

    [Fact]
    public void Validate_LegacyOptionsOnly_Passes()
    {
        var options = BaseOptions();
        options.ScheduleTime = "04:00";
        options.DailyRetentionDays = "30";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyOptions_Passes()
    {
        var result = PolicyUpdateValidator.Validate(BaseOptions());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WeeklyWithoutDaysOfWeek_Fails()
    {
        var options = BaseOptions();
        options.ScheduleFrequency = "Weekly";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--schedule-days-of-week");
    }

    [Fact]
    public void Validate_WeeklyWithDaysOfWeek_Passes()
    {
        var options = BaseOptions();
        options.ScheduleFrequency = "Weekly";
        options.ScheduleDaysOfWeek = "Sunday";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Hourly")]
    [InlineData("Monthly")]
    [InlineData("Bogus")]
    public void Validate_UnsupportedFrequency_Fails(string freq)
    {
        var options = BaseOptions();
        options.ScheduleFrequency = freq;

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--schedule-frequency");
    }

    [Theory]
    [InlineData(4, null)]
    [InlineData(0, "Sunday")]
    public void Validate_PartialWeeklyRetention_Fails(int weeks, string? days)
    {
        var options = BaseOptions();
        options.WeeklyRetentionWeeks = weeks;
        options.WeeklyRetentionDaysOfWeek = days;

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--weekly-retention-weeks");
    }

    [Fact]
    public void Validate_FullWeeklyRetention_Passes()
    {
        var options = BaseOptions();
        options.WeeklyRetentionWeeks = 4;
        options.WeeklyRetentionDaysOfWeek = "Sunday";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MonthlyFlagsWithoutMonths_Fails()
    {
        var options = BaseOptions();
        options.MonthlyRetentionDaysOfMonth = "1";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-months");
    }

    [Fact]
    public void Validate_MonthlyBothSchemes_Fails()
    {
        var options = BaseOptions();
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionDaysOfMonth = "1";
        options.MonthlyRetentionWeekOfMonth = "First";
        options.MonthlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-days-of-month");
    }

    [Fact]
    public void Validate_MonthlyRelativeIncomplete_Fails()
    {
        var options = BaseOptions();
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionWeekOfMonth = "First";
        // missing --monthly-retention-days-of-week

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--monthly-retention-week-of-month");
    }

    [Fact]
    public void Validate_MonthlyAbsolute_Passes()
    {
        var options = BaseOptions();
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionDaysOfMonth = "1,15";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MonthlyRelative_Passes()
    {
        var options = BaseOptions();
        options.MonthlyRetentionMonths = 12;
        options.MonthlyRetentionWeekOfMonth = "First";
        options.MonthlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_YearlyFlagsWithoutYears_Fails()
    {
        var options = BaseOptions();
        options.YearlyRetentionMonths = "January";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--yearly-retention-years");
    }

    [Fact]
    public void Validate_YearlyWithoutMonths_Fails()
    {
        var options = BaseOptions();
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionDaysOfMonth = "1";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--yearly-retention-months");
    }

    [Fact]
    public void Validate_YearlyBothSchemes_Fails()
    {
        var options = BaseOptions();
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionDaysOfMonth = "1";
        options.YearlyRetentionWeekOfMonth = "First";
        options.YearlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--yearly-retention-days-of-month");
    }

    [Fact]
    public void Validate_YearlyRelativeIncomplete_Fails()
    {
        var options = BaseOptions();
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionWeekOfMonth = "First";
        // missing --yearly-retention-days-of-week

        var result = PolicyUpdateValidator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Flag == "--yearly-retention-week-of-month");
    }

    [Fact]
    public void Validate_YearlyAbsolute_Passes()
    {
        var options = BaseOptions();
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionDaysOfMonth = "1";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_YearlyRelative_Passes()
    {
        var options = BaseOptions();
        options.YearlyRetentionYears = 5;
        options.YearlyRetentionMonths = "January";
        options.YearlyRetentionWeekOfMonth = "First";
        options.YearlyRetentionDaysOfWeek = "Sunday";

        var result = PolicyUpdateValidator.Validate(options);

        Assert.True(result.IsValid);
    }
}
