using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ScientificCalculator
{
    public partial class MainWindow : Window
    {
        private StringBuilder expressionBuilder = new StringBuilder();
        private bool newInputExpected = false;
        private bool displayError = false;
        private double lastResult = 0;
        private string lastOperation = "";
        private bool isPowerOperation = false;
        private double powerBase = 0;
        private bool isBracketOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            ClearAll();
        }

        private void ClearAll()
        {
            expressionBuilder.Clear();
            newInputExpected = false;
            displayError = false;
            lastResult = 0;
            lastOperation = "";
            isPowerOperation = false;
            powerBase = 0;
            isBracketOpen = false;
            ExpressionTextBlock.Text = "";
            ResultTextBlock.Text = "0";
        }

        private void ClearEntry()
        {
            ResultTextBlock.Text = "0";
            newInputExpected = false;
        }

        private void AddToExpression(string value)
        {
            if (displayError)
            {
                expressionBuilder.Clear();
                displayError = false;
            }
            expressionBuilder.Append(value);
            ExpressionTextBlock.Text = expressionBuilder.ToString();
        }

        private void DigitButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayError) ClearAll();

            var button = (Button)sender;
            string digit = button.Content.ToString();

            if (newInputExpected)
            {
                if (ExpressionTextBlock.Text.EndsWith("="))
                {
                    expressionBuilder.Clear();
                    ExpressionTextBlock.Text = "";
                }
                ResultTextBlock.Text = digit;
                newInputExpected = false;
            }
            else
            {
                if (ResultTextBlock.Text == "0")
                    ResultTextBlock.Text = digit;
                else
                    ResultTextBlock.Text += digit;
            }
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayError) ClearAll();

            var button = (Button)sender;
            string operation = button.Content.ToString();

            if (operation == "(")
            {
                isBracketOpen = true;
                AddToExpression("(");
                newInputExpected = true;
                return;
            }
            else if (operation == ")")
            {
                if (!isBracketOpen) return;

                AddToExpression(ResultTextBlock.Text + ")");
                isBracketOpen = false;
                newInputExpected = true;
                return;
            }

            try
            {
                double currentValue = double.Parse(ResultTextBlock.Text, CultureInfo.InvariantCulture);

                if (ExpressionTextBlock.Text.EndsWith("="))
                {
                    expressionBuilder.Clear();
                    expressionBuilder.Append(lastResult);
                    lastOperation = operation;
                    AddToExpression($" {operation} ");
                    newInputExpected = true;
                    return;
                }

                if (!string.IsNullOrEmpty(lastOperation))
                {
                    CalculateResult();
                }
                else
                {
                    lastResult = currentValue;
                }

                lastOperation = operation;
                AddToExpression($"{lastResult} {operation} ");
                newInputExpected = true;
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
            }
        }

        private void CalculateResult()
        {
            try
            {
                double currentValue = double.Parse(ResultTextBlock.Text, CultureInfo.InvariantCulture);

                switch (lastOperation)
                {
                    case "+":
                        lastResult += currentValue;
                        break;
                    case "-":
                        lastResult -= currentValue;
                        break;
                    case "*":
                        lastResult *= currentValue;
                        break;
                    case "/":
                        if (currentValue == 0) throw new DivideByZeroException();
                        lastResult /= currentValue;
                        break;
                    case "^":
                        lastResult = Math.Pow(lastResult, currentValue);
                        break;
                }

                ResultTextBlock.Text = lastResult.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
            }
        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayError) ClearAll();

            var button = (Button)sender;
            string function = button.Content.ToString();

            if (function == "x^y")
            {
                try
                {
                    powerBase = double.Parse(ResultTextBlock.Text, CultureInfo.InvariantCulture);
                    isPowerOperation = true;
                    AddToExpression($"{powerBase}^");
                    newInputExpected = true;
                    return;
                }
                catch (Exception ex)
                {
                    HandleError(ex.Message);
                    return;
                }
            }

            if (function == "n!")
            {
                try
                {
                    int value = (int)double.Parse(ResultTextBlock.Text, CultureInfo.InvariantCulture);
                    double result = Factorial(value);
                    ResultTextBlock.Text = result.ToString(CultureInfo.InvariantCulture);
                    AddToExpression($"{value}!");
                    newInputExpected = true;
                    return;
                }
                catch (Exception ex)
                {
                    HandleError(ex.Message);
                    return;
                }
            }

            try
            {
                double value = double.Parse(ResultTextBlock.Text, CultureInfo.InvariantCulture);
                double result = CalculateFunction(value, function);

                ResultTextBlock.Text = result.ToString(CultureInfo.InvariantCulture);
                AddToExpression($"{function}({value})");
                newInputExpected = true;
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
            }
        }

        private double CalculateFunction(double value, string function)
        {
            switch (function)
            {
                case "sin": return Math.Sin(value * Math.PI / 180);
                case "cos": return Math.Cos(value * Math.PI / 180);
                case "tg": return Math.Tan(value * Math.PI / 180);
                case "x^2": return Math.Pow(value, 2);
                case "1/x": return value == 0 ? throw new DivideByZeroException() : 1 / value;
                case "|x|": return Math.Abs(value);
                case "2√x": return Math.Sqrt(value);
                case "π": return Math.PI;
                case "e": return Math.E;
                case "10^x": return Math.Pow(10, value);
                case "log": return Math.Log10(value);
                case "ln": return Math.Log(value);
                default: return value;
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isPowerOperation)
                {
                    double exponent = double.Parse(ResultTextBlock.Text, CultureInfo.InvariantCulture);
                    double result = Math.Pow(powerBase, exponent);
                    ExpressionTextBlock.Text = $"{powerBase}^{exponent} =";
                    ResultTextBlock.Text = result.ToString(CultureInfo.InvariantCulture);
                    isPowerOperation = false;
                    powerBase = 0;
                    newInputExpected = true;
                    lastResult = result;
                    return;
                }

                if (!string.IsNullOrEmpty(lastOperation))
                {
                    string expression = expressionBuilder.ToString() + ResultTextBlock.Text;
                    CalculateResult();
                    ExpressionTextBlock.Text = $"{expression} =";
                    newInputExpected = true;
                }
                else if (isBracketOpen)
                {
                    ExpressionTextBlock.Text = expressionBuilder.ToString() + ResultTextBlock.Text + ")";
                    isBracketOpen = false;
                    newInputExpected = true;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
            }
        }

        private double Factorial(int n)
        {
            if (n < 0) throw new ArgumentException("Factorial of negative number is undefined");
            if (n > 170) throw new ArgumentException("Value too large for factorial calculation");
            if (n == 0) return 1;

            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private void HandleError(string message)
        {
            ResultTextBlock.Text = "Error";
            ExpressionTextBlock.Text = message;
            displayError = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearAll();
        private void ClearEntryButton_Click(object sender, RoutedEventArgs e) => ClearEntry();

        private void DecimalPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (newInputExpected)
            {
                ResultTextBlock.Text = "0.";
                newInputExpected = false;
            }
            else if (!ResultTextBlock.Text.Contains("."))
            {
                ResultTextBlock.Text += ".";
            }
        }

        private void SignChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultTextBlock.Text != "0")
            {
                ResultTextBlock.Text = ResultTextBlock.Text.StartsWith("-")
                    ? ResultTextBlock.Text.Substring(1)
                    : "-" + ResultTextBlock.Text;
            }
        }
    }
}