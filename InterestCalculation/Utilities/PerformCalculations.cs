using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InterestCalculation.Utilities
{
    internal class PerformCalculations
    {
        static MidpointRounding roundingStrategy = MidpointRounding.ToPositiveInfinity;

        public static List<(int days, decimal legal, decimal delay)> CalculateDaysAndInterestsInRange(Dictionary<int, (DateOnly startDate, DateOnly endDate, (decimal legal, decimal delay))> tableRecords, DateOnly startdate, DateOnly endDate)
        {
            bool startDateControl = true;
            if (startdate < tableRecords[0].Item1)
            {
                throw new ArgumentOutOfRangeException("Input Date out of range");
            }
            List<(int, decimal, decimal)> daysAndInterests = new();

            //Calculate  the number of days between the start and end date and populate the daysAndInterests list with the days and the interest rates
            foreach (var tableRecord in tableRecords)
            {
                int days = 0;

                //Calculate  the number of days between the start date and the end date of the data record
                if (startdate < tableRecord.Value.Item2 && startDateControl)
                {
                    startDateControl = false;
                    if (startdate == tableRecord.Value.Item2)
                    {
                        //only 1 date in this record
                        days = 1;
                    }
                    else if (endDate <= tableRecord.Value.Item2)
                    {
                        //The whole date range is in this record
                        days = (endDate.ToDateTime(TimeOnly.MinValue) - startdate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                        break;
                    }
                    else
                    {
                        //The start date is in this record, but the end date is not
                        days = (tableRecord.Value.Item2.ToDateTime(TimeOnly.MinValue) - startdate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                    }
                    daysAndInterests.Add((days, tableRecord.Value.Item3.Item1, tableRecord.Value.Item3.Item2));
                }
                else if (!startDateControl && endDate > tableRecord.Value.Item2)
                {
                    //The endDate is not in this record , so we calculate the days from the start date of the record to the end date of the record
                    days = (tableRecord.Value.Item2.ToDateTime(TimeOnly.MinValue) - tableRecord.Value.Item1.ToDateTime(TimeOnly.MinValue)).Days + 1;
                    daysAndInterests.Add((days, tableRecord.Value.Item3.Item1, tableRecord.Value.Item3.Item2));
                    //var days = (endDate.ToDateTime(TimeOnly.MinValue) - record.Value.Item1.ToDateTime(TimeOnly.MinValue)).Days;
                }
                else if (!startDateControl && endDate <= tableRecord.Value.Item2)
                {
                    //The endDate is in this record , so we calculate the days from the start date of the record to the endDate of the input
                    days = (endDate.ToDateTime(TimeOnly.MinValue) - tableRecord.Value.Item1.ToDateTime(TimeOnly.MinValue)).Days + 1;
                    daysAndInterests.Add((days, tableRecord.Value.Item3.Item1, tableRecord.Value.Item3.Item2));
                    break;
                }
            }
            return daysAndInterests;
        }

        public static List<(int year, int days, decimal legalInterest, decimal delayInterest)> CalculateYearlyDaysAndInterests(
                Dictionary<int, (DateOnly start, DateOnly end, (decimal legal, decimal delay) rates)> tableRecords,
                        DateOnly startDate, DateOnly endDate)
        {
            List<(int year, int days, decimal legal, decimal delay)> daysAndInterests = new();

            foreach (var record in tableRecords)
            {
                if (startDate > record.Value.end)
                    continue;

                int days = 0;

                // start date is exactly on the end of the record 
                if (startDate == record.Value.end)
                {
                    //only 1 day in this record
                    days = 1;
                    daysAndInterests.Add((record.Value.end.Year, days, record.Value.rates.legal, record.Value.rates.delay));
                    break;
                }
                else
                {
                    //Define the actual end date for this record 
                    DateOnly effectiveEndDate = (endDate <= record.Value.end) ? endDate : record.Value.end;

                    // Calculate days for this record
                    days = CalculateDaysForDifferentYears(startDate, effectiveEndDate, daysAndInterests, record, days);

                    // If we have processed the full date range, exit loop
                    if (endDate <= record.Value.end)
                        break;

                    // Move startDate to the next possible record
                    startDate = record.Value.end.AddDays(1);
                }
            }

            return daysAndInterests;
        }

        private static int CalculateDaysForDifferentYears(
            DateOnly startDate, DateOnly endDate, List<(int year, int days, decimal legal, decimal delay)> daysAndInterests, KeyValuePair<int, (DateOnly Start, DateOnly End, (decimal Legal, decimal Delay) Rates)> record, int days)
        {
            DateOnly tempStartDate = startDate;

            if (startDate.Year == endDate.Year)
            {
                // If the range is within the same year, calculate normally
                days = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                daysAndInterests.Add((startDate.Year, days, record.Value.Rates.Legal, record.Value.Rates.Delay));
            }
            else
            {
                // First year: from startDate to the end of that year
                DateOnly endOfYear = new DateOnly(tempStartDate.Year, 12, 31);
                int firstYearDays = (endOfYear.ToDateTime(TimeOnly.MinValue) - tempStartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                daysAndInterests.Add((tempStartDate.Year, firstYearDays, record.Value.Rates.Legal, record.Value.Rates.Delay));

                // Move tempStartDate to the next year
                tempStartDate = new DateOnly(tempStartDate.Year + 1, 1, 1);

                // Middle years: full years between start and end
                while (tempStartDate.Year < endDate.Year)
                {
                    int fullYearDays = DateTime.IsLeapYear(tempStartDate.Year) ? 366 : 365;
                    daysAndInterests.Add((tempStartDate.Year, fullYearDays, record.Value.Rates.Legal, record.Value.Rates.Delay));
                    tempStartDate = new DateOnly(tempStartDate.Year + 1, 1, 1);
                }

                // Last year: from start of the last year to endDate
                int lastYearDays = (endDate.ToDateTime(TimeOnly.MinValue) - tempStartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
                daysAndInterests.Add((tempStartDate.Year, lastYearDays, record.Value.Rates.Legal, record.Value.Rates.Delay));
            }

            return days;
        }

        public static List<(decimal, decimal)> CalculateInterestPerDateRange(List<(int year, int days, decimal legal, decimal delay)> daysAndInterests)
        {
            List<(decimal, decimal)> interests = new();

            foreach (var daysAndInterest in daysAndInterests)
            {
                DetermineInterest(daysAndInterest, out decimal legalInterest, out decimal delayInterest);

                interests.Add((legalInterest, delayInterest));
            }
            return interests;
        }

        private static void DetermineInterest((int year, int days, decimal legal, decimal delay) daysAndInterest, out decimal legalInterest, out decimal delayInterest)
        {
            var isLeapYear = DateTime.IsLeapYear(daysAndInterest.year);
            decimal daysFraction;

            if (isLeapYear)
            {
                daysFraction = (decimal)Math.Round(daysAndInterest.days / 366.0, 9, roundingStrategy);
            }
            else
            {
                daysFraction = (decimal)Math.Round(daysAndInterest.days / 365.0, 9, roundingStrategy);
            }
            var legalInterestPercentage = daysAndInterest.legal / 100;
            var delayInterestPercentage = daysAndInterest.delay / 100;

            legalInterest = Math.Round(daysFraction * legalInterestPercentage, 4, roundingStrategy);
            delayInterest = Math.Round(daysFraction * delayInterestPercentage, 4, roundingStrategy);
        }
    }
}
