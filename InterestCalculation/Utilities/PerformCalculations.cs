namespace InterestCalculation.Utilities
{
    internal class PerformCalculations
    {
        static MidpointRounding roundingStrategy = MidpointRounding.ToPositiveInfinity;

        public static List<(int, decimal, decimal)> CalculateDaysAndInterestsInRange(Dictionary<int, (DateOnly, DateOnly, (decimal, decimal))> tableRecords, DateOnly startdate, DateOnly endDate)
        {
            bool startDateControl = true;
            if (startdate < tableRecords[0].Item1)
            {
                Console.WriteLine("Input Date out of range");
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
                        //only 1 date in this record, and  keep going
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

        public static List<(decimal, decimal)> CalculateInterestPerDateRange(List<(int, decimal, decimal)> daysAndInterests)
        {
            List<(decimal, decimal)> interests = new();

            foreach (var daysAndInterest in daysAndInterests)
            {
                decimal legalInterest, delayInterest;
                DetermineInterest(daysAndInterest, out legalInterest, out delayInterest);

                interests.Add((legalInterest, delayInterest));
            }
            return interests;
        }

        private static void DetermineInterest((int, decimal, decimal) daysAndInterest, out decimal legalInterest, out decimal delayInterest)
        {
            var daysFraction = (decimal)Math.Round(daysAndInterest.Item1 / 360.0, 9, roundingStrategy);

            var legalInterestPercentage = daysAndInterest.Item2 / 100;
            var delayInterestPercentage = daysAndInterest.Item3 / 100;

            legalInterest = Math.Round(daysFraction * legalInterestPercentage, 4, roundingStrategy);
            delayInterest = Math.Round(daysFraction * delayInterestPercentage, 4, roundingStrategy);
        }
    }
}
