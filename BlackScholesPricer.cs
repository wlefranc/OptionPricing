using System.Threading;
using System;

namespace OptionPricing
{
    public class BlackScholesPricer : IOptionPricer
    {
        public static double NormalDistribution(double x)
        {
            var a1 =  0.254829592;
            var a2 = -0.284496736;
            var a3 =  1.421413741;
            var a4 = -1.453152027;
            var a5 =  1.061405429;
            var p  =  0.3275911;

            var sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x)/Math.Sqrt(2.0);

            var t = 1.0/(1.0 + p*x);
            var y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);

            return 0.5*(1.0 + sign*y);
        }

        private static Tuple<double,double> compute_d1_and_d2(Option option)
        {
            var S = option.StockPrice;
            var K = option.ExercicePrice;
            var T = option.TimeToExpiration;
            var sigma = option.Volatility;
            var r = option.InterestRate;

            double numerator_d1 = Math.Log(S/K) + (r+0.5*Math.Pow(sigma,2))*T;
            double denominator_d1 = sigma * Math.Sqrt(T);

            var d1 = numerator_d1 / denominator_d1;
            var d2 = d1 - sigma * Math.Sqrt(T);
            return Tuple.Create(d1, d2);
        }

        public double PricePut(Option option, CancellationTokenSource ct)
        {
            var t = compute_d1_and_d2(option);
            var d1 = t.Item1;
            var d2 = t.Item2;

            return -option.StockPrice * NormalDistribution(-d1)
                    + option.ExercicePrice * Math.Exp(-option.InterestRate*option.TimeToExpiration)*NormalDistribution(-d2);
        }
        public double PriceCall(Option option, CancellationTokenSource ct)
        {
            var t = compute_d1_and_d2(option);
            var d1 = t.Item1;
            var d2 = t.Item2;
            
            return option.StockPrice * NormalDistribution(d1)
                    - option.ExercicePrice * Math.Exp(-option.InterestRate*option.TimeToExpiration)*NormalDistribution(d2);
        }
    }
}