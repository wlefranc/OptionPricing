namespace OptionPricing
{
    public struct Option
    {
        public double StockPrice { get; set; }
        public double ExercicePrice { get; set; }
        public double InterestRate { get; set; }
        public double Volatility { get; set; }
        public double TimeToExpiration { get; set; }

        public static Option MockOption = new Option
        {
            StockPrice = 300,
            ExercicePrice = 250,
            InterestRate = 0.03,
            Volatility = 0.15,
            TimeToExpiration = 1,
        };
    }
}