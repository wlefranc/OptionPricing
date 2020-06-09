using System.Threading;
using System;

namespace OptionPricing
{
    public class MonteCarloPricer : IOptionPricer
    {
        public int NIter { get; set; }

        public MonteCarloPricer(int num_sims)
        {
            this.NIter = num_sims;
        }
        public double PricePut(Option option, CancellationTokenSource ct)
        {
            return Price_helper(option, ct, (x) => Math.Max(0, option.ExercicePrice - x));   
        }
        public double PriceCall(Option option, CancellationTokenSource ct)
        {
            return Price_helper(option, ct, (x) => Math.Max(0, x - option.ExercicePrice));
        }

        private static double BoxMullerTransform()
        {
            Random rand = new Random();
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        private double Price_helper(Option option, CancellationTokenSource ct, Func<double, double> getPayoff)
        {
            var S = option.StockPrice;
            var T = option.TimeToExpiration;
            var r = option.InterestRate;
            var sigma = option.Volatility;
            var K = option.ExercicePrice;

            var S_adjust = S * Math.Exp(T*(r-0.5*Math.Pow(sigma,2)));
            var S_cur = 0.0;
            var payoff_sum = 0.0;

            for (int i = 0; i < this.NIter; i++)
            {
                if (ct.IsCancellationRequested)
                    ct.Token.ThrowIfCancellationRequested();

                var rand = BoxMullerTransform();
                S_cur = S_adjust * Math.Exp(sigma * Math.Sqrt(T) * rand);
                payoff_sum += getPayoff(S_cur);
            }

            return (payoff_sum / this.NIter) * Math.Exp(-r*T);    
        }
    }
}