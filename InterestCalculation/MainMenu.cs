using System.Globalization;
using InterestCalculation.Utilities;
using Spectre.Console;


namespace InterestCalculation;
public static class MenuHandler
{
    // Define the minimum allowed start date (21/08/1946)
    private static readonly DateOnly MinStartDate = new DateOnly(1946, 8, 21);
    // Define the URL of the Bank of Greece page
    private static readonly  string url = @"https://www.bankofgreece.gr/statistika/xrhmatopistwtikes-agores/ekswtrapezika-epitokia";
    public static void Conductor()
    {
        Dictionary<int, (DateOnly start, DateOnly end, (decimal legal, decimal delay))>? tableRows = HtmlDataExtractor.ExtractFromWebPage(url);

        //var input = GetUserInput();
        var input = (StartDate: DateOnly.Parse("1979-09-22"), EndDate: DateOnly.Parse("1979-10-19"), amount: 100);

        List<(int year, int days, decimal legal, decimal delay, DateOnly startDate, DateOnly endDate)>? daysAndRatesList = PerformCalculations.CalculateYearlyDaysAndInterests(tableRows, input.StartDate, input.EndDate);

        var interestRates = PerformCalculations.CalculateInterestPerDateRange(daysAndRatesList);

        var interestAmounts = ComputeInterestAmounts(input, interestRates);

        RenderResultTables(input, daysAndRatesList, interestAmounts);
    }

    private static void RenderResultTables((DateOnly StartDate, DateOnly EndDate, decimal amount) input, List<(int year, int days, decimal legal, decimal delay, DateOnly startDate, DateOnly endDate)> daysAndInterests, List<(decimal, decimal)> interestAmounts)
    {
        Table resultsTable = new();
        resultsTable.AddColumn("Start Date");
        resultsTable.AddColumn("End Date");
        resultsTable.AddColumn("Days");
        resultsTable.AddColumn("Legal-Interest Rate");
        resultsTable.AddColumn("Legal-Interest");
        resultsTable.AddColumn("Delay-Interest Rate");
        resultsTable.AddColumn("Delay-Interest");

        for (int i = 0; i < interestAmounts.Count; i++)
        {
            (decimal, decimal) item = interestAmounts[i];
            resultsTable.AddRow(
                daysAndInterests[i].startDate.ToString(),
                daysAndInterests[i].endDate.ToString(),
                daysAndInterests[i].days.ToString().TrimEnd('0'),
                daysAndInterests[i].legal.ToString() + "%",
                item.Item1.ToString().TrimEnd('0'),
                daysAndInterests[i].delay.ToString() + "%",
                item.Item2.ToString().TrimEnd('0'));
        }
        AnsiConsole.Write(resultsTable);

        decimal totalLegalInterest = interestAmounts.Sum(x => (decimal)x.Item1);
        decimal totalDelayInterest = interestAmounts.Sum(x => (decimal)x.Item2);

        Table resultSummaryTable = new();

        resultSummaryTable.AddColumn("Initial Capital");
        resultSummaryTable.AddColumn("Legal Interest");
        resultSummaryTable.AddColumn("Legal Total");
        resultSummaryTable.AddColumn("Delay Interest");
        resultSummaryTable.AddColumn("Delay Total");

        resultSummaryTable.AddRow(
            input.amount.ToString(),
            totalLegalInterest.ToString().TrimEnd('0'),
            (input.amount + totalLegalInterest).ToString().Trim('0'),
            totalDelayInterest.ToString().TrimEnd('0'),
            (input.amount + totalDelayInterest).ToString().Trim('0'));

        AnsiConsole.Write(resultSummaryTable);
    }

    private static List<(decimal, decimal)> ComputeInterestAmounts((DateOnly StartDate, DateOnly EndDate, decimal amount) input, List<(decimal, decimal)> result)
    {
        decimal legal = 0;
        decimal delay = 0;

        List<(decimal, decimal)> interestAmounts = new();

        foreach (var item in result)
        {
            legal = item.Item1 * input.amount;
            delay = item.Item2 * input.amount;

            interestAmounts.Add((legal, delay));
        }
        return interestAmounts;
    }

    public static (DateOnly StartDate, DateOnly EndDate, decimal Amount) GetUserInput()
    {
        DateOnly startDate = GetDateInput("Enter the start date (YYYY-MM-DD): ", isStartDate: true);
        DateOnly endDate = GetDateInput("Enter the end date (YYYY-MM-DD): ", isStartDate: false, startDate);
        decimal amount = GetAmountInput("Enter the amount (e.g., 123.45): ");

        return (startDate, endDate, amount);
    }

    private static DateOnly GetDateInput(string prompt, bool isStartDate, DateOnly? startDate = null)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (DateOnly.TryParseExact(input, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            {
                if (isStartDate && date < MinStartDate)
                {
                    Console.WriteLine($"Start date cannot be earlier than {MinStartDate:yyyy-MM-dd}.");
                    continue;
                }

                if (!isStartDate && date < startDate)
                {
                    Console.WriteLine($"End date cannot be earlier than the start date ({startDate:yyyy-MM-dd}).");
                    continue;
                }

                return date;
            }
            Console.WriteLine("Invalid date format. Please use YYYY-MM-DD.");
        }
    }

    private static decimal GetAmountInput(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (decimal.TryParse(input, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal amount))
            {
                if (amount <= 0)
                {
                    Console.WriteLine("Amount must be a positive number.");
                    continue;
                }

                return amount;
            }
            Console.WriteLine("Invalid amount. Please enter a valid positive number.");
        }
    }
}