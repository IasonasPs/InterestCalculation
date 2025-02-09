using HtmlAgilityPack;

namespace InterestCalculation.Utilities
{
    internal class HtmlDataExtractor
    {
        static Dictionary<int, (DateOnly, DateOnly, (decimal, decimal))>? keys;

        public static Dictionary<int, (DateOnly startDate, DateOnly endDate, (decimal legal, decimal delay))> ExtractFromWebPage(string html)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc;

            try
            {
                htmlDoc = web.Load(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            var tableRows = htmlDoc.DocumentNode.SelectNodes("//table//tbody//tr");
            keys = [];

            ExtractDataFromNodeCollection(tableRows, keys);
            return keys;
        }
        private static void ExtractDataFromNodeCollection(HtmlNodeCollection tableRows, Dictionary<int, (DateOnly, DateOnly, (decimal, decimal))> keys)
        {
            for (int i = 0; i < tableRows.Count; i++)
            {
                HtmlNode? tableRow = tableRows[i];
                var startDateString = tableRow.ChildNodes[1].InnerText;
                var endDateString = tableRow.ChildNodes[3].InnerText;
                var legalString = tableRow.ChildNodes[9].InnerText.Trim('%');
                var delayString = tableRow.ChildNodes[11].InnerText.Trim('%');
                DateOnly startDate = DataInputUtilities.ConvertToDateOnly(startDateString);
                DateOnly endDate = DataInputUtilities.ConvertToDateOnly(endDateString);
                decimal legal = DataInputUtilities.ConvertToDecimal(legalString);
                decimal delay = DataInputUtilities.ConvertToDecimal(delayString);

                keys.Add(i, (startDate, endDate, (legal, delay)));
            }
        }
    }
}
