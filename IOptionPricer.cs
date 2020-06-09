using System.Threading;

namespace OptionPricing
{
    interface IOptionPricer
    {
        double PricePut(Option option, CancellationTokenSource ct);
        double PriceCall(Option option, CancellationTokenSource ct);
    }
}