using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace OptionPricing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IOptionPricer m_pricer;
        private bool m_computeCall;
        private bool m_computePut;
        private ITaskCanceler m_taskCanceler;
        public MainWindow()
        {
            m_taskCanceler = new DictTaskCanceler();
            InitializeComponent();
        }

        Tuple<double?,double?> Price(Option option, CancellationTokenSource cancellationToken)
        {
            double? putPrice = null;
            double? callPrice = null;
            if (m_computePut)
                putPrice = m_pricer.PricePut(option, cancellationToken);

            if (m_computeCall)
                callPrice = m_pricer.PriceCall(option, cancellationToken);

            return Tuple.Create(putPrice, callPrice);
        }

        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            m_taskCanceler.CancelTask();

            ResetResultTextBoxes();
            Option? option = CreateOptionFromInputs();
            if (option == null)
                return;

            Tuple<double?,double?> res;
            Task<Tuple<double?,double?>> t = null;
            try
            {
                t = Task.Run(() => 
                {
                    var taskId = Task.CurrentId.GetValueOrDefault();
                    var cancellationToken = m_taskCanceler.RegisterTask(taskId);
                    return Price(option.Value, cancellationToken);
                });                                                                                                                                                                                                                                                                                                                                                                                                      
                
                res = await t;
                var putPrice = res.Item1;
                var callPrice = res.Item2;
                WriteResults(putPrice, callPrice);

                m_taskCanceler.NotifyTaskFinished();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                m_taskCanceler.DisposeTask(t.Id);
            }
        }

        public void HandleCheckPut(object sender, RoutedEventArgs e)
        {
            m_computePut = true;
        }
        public void HandleUncheckedPut(object sender, RoutedEventArgs e)
        {
            m_computePut = false;
        }
        public void HandleCheckCall(object sender, RoutedEventArgs e)
        {
            m_computeCall = true;
        }
        public void HandleUncheckedCall(object sender, RoutedEventArgs e)
        {
            m_computeCall = false;
        }

        private void ResetResultTextBoxes()
        {
            var putTextBox = (TextBox)this.FindName("PutResult");
            putTextBox.Text = String.Empty;
            var callTextBox = (TextBox)this.FindName("CallResult");
            callTextBox.Text = String.Empty;
        }

        private Option? CreateOptionFromInputs()
        {
            var stockPriceTextBox = (TextBox)this.FindName("StockPrice");
            var exercicePriceTextBox = (TextBox)this.FindName("ExercicePrice");
            var volatilityTextBox = (TextBox)this.FindName("Volatility");
            var interestRateTextBox = (TextBox)this.FindName("InterestRate");
            var timeToExpirationTextBox = (TextBox)this.FindName("TimeToExpiration");

            double stockPrice = 0;
            double exercicePrice = 0;
            double volatility = 0;
            double interestRate = 0;
            double timeToExpiration = 0;

            try 
            {
                    stockPrice = Double.Parse(stockPriceTextBox.Text, CultureInfo.InvariantCulture);
                    exercicePrice = Double.Parse(exercicePriceTextBox.Text, CultureInfo.InvariantCulture);
                    volatility = Double.Parse(volatilityTextBox.Text, CultureInfo.InvariantCulture)/100;
                    interestRate = Double.Parse(interestRateTextBox.Text, CultureInfo.InvariantCulture)/100;
                    timeToExpiration = Double.Parse(timeToExpirationTextBox.Text, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please fill all the inputs with numbers.");
                return null;
            }
        
            return new Option
            {
                StockPrice = stockPrice,
                ExercicePrice = exercicePrice,
                Volatility = volatility,
                InterestRate = interestRate,
                TimeToExpiration = timeToExpiration,
            };
        }

        void WriteResults(double? putPrice, double? callPrice)
        {
                if (putPrice != null)
                {
                    var putTextBox = (TextBox)this.FindName("PutResult");
                    putTextBox.Text = putPrice.Value.ToString("#.000");
                }
                
                if (callPrice != null)
                {
                    var callTextBox = (TextBox)this.FindName("CallResult");
                    callTextBox.Text = callPrice.Value.ToString("#.000");
                }
        }

        void HandleChooseMonteCarlo(object sender, RoutedEventArgs e)
        {
            var niterTextBox = (TextBox)this.FindName("NIter");
            niterTextBox.Visibility = Visibility.Visible;
            var niter = Int32.Parse(niterTextBox.Text);
            m_pricer = new MonteCarloPricer(niter);
        }

        void HandleChooseBlackScholes(object sender, RoutedEventArgs e)
        {
            var niterTextBox = (TextBox)this.FindName("NIter");
            niterTextBox.Visibility = Visibility.Hidden;
            m_pricer = new BlackScholesPricer();
        }
        void HandleChangeNIter(object sender, RoutedEventArgs e)
        {
            var niter = Int32.Parse((sender as TextBox).Text);
            if (m_pricer != null)
                (m_pricer as MonteCarloPricer).NIter = niter;
        }
    }
}
