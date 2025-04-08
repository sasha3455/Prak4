using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace ScientificCalculator
{
    public partial class MainWindow : Window
    {
        private const string InitialDisplayValue = "0";
        private const string ErrorDisplayText = "Error";

        private readonly StringBuilder _expressionBuilder = new();
        private double _lastResult;
        private string _lastOperation = string.Empty;
        private double _powerBase;
        private bool _newInputExpected;
        private bool _displayError;
        private bool _isPowerOperation;
        private bool _isBracketOpen;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCalculator();
        }

        private void InitializeCalculator()
        {
            ResetAll();
        }

        private void ResetAll()
        {
            _expressionBuilder.Clear();
            _lastResult = 0;
            _lastOperation = string.Empty;
            _powerBase = 0;
            _newInputExpected = false;
            _displayError = false;
            _isPowerOperation = false;
            _isBracketOpen = false;

            UpdateDisplay(InitialDisplayValue);
            ClearExpression();
        }

        private void ResetEntry()
        {
            UpdateDisplay(InitialDisplayValue);
            _newInputExpected = false;
        }

        private void DigitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_displayError) ResetAll();

            var digit = ((Button)sender).Content.ToString() ?? string.Empty;

            if (_newInputExpected)
            {
                if (ExpressionTextBlock.Text.EndsWith("="))
                {
                    ClearExpression();
                }
                UpdateDisplay(digit);
                _newInputExpected = false;
            }
            else
            {
                UpdateDisplay(DisplayText == InitialDisplayValue ? digit : DisplayText + digit);
            }
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_displayError) ResetAll();

            var operation = ((Button)sender).Content.ToString() ?? string.Empty;

            if (operation == "(" || operation == ")")
            {
                HandleBrackets(operation);
                return;
            }

            try
            {
                var currentValue = ParseDisplayValue();

                if (ExpressionTextBlock.Text.EndsWith("="))
                {
                    _expressionBuilder.Clear();
                    _expressionBuilder.Append(_lastResult);
                }

                if (!string.IsNullOrEmpty(_lastOperation))
                {
                    CalculateResult(currentValue);
                }
                else
                {
                    _lastResult = currentValue;
                }

                _lastOperation = operation;
                UpdateExpression($"{_lastResult} {operation} ");
                _newInputExpected = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_displayError) ResetAll();

            var function = ((Button)sender).Content.ToString() ?? string.Empty;

try
            {
                switch (function)
                {
                    case "x^y":
                        HandlePowerOperation();
                        return;
                    case "n!":
                        HandleFactorial();
                        return;
                    default:
                        HandleMathFunction(function);
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isPowerOperation)
                {
                    CompletePowerOperation();
                    return;
                }

                if (!string.IsNullOrEmpty(_lastOperation))
                {
                    CompleteStandardOperation();
                }
                else if (_isBracketOpen)
                {
                    CompleteBracketOperation();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ResetAll();
        private void ClearEntryButton_Click(object sender, RoutedEventArgs e) => ResetEntry();

        private void DecimalPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (_newInputExpected)
            {
                UpdateDisplay("0.");
                _newInputExpected = false;
            }
            else if (!DisplayText.Contains("."))
            {
                UpdateDisplay(DisplayText + ".");
            }
        }

        private void SignChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayText != InitialDisplayValue)
            {
                UpdateDisplay(DisplayText.StartsWith("-")
                    ? DisplayText[1..]
                    : "-" + DisplayText);
            }
        }
        private void HandleBrackets(string bracket)
        {
            if (bracket == "(")
            {
                _isBracketOpen = true;
                UpdateExpression("(");
            }
            else if (_isBracketOpen)
            {
                UpdateExpression(DisplayText + ")");
                _isBracketOpen = false;
            }
            _newInputExpected = true;
        }

        private void HandlePowerOperation()
        {
            _powerBase = ParseDisplayValue();
            _isPowerOperation = true;
            UpdateExpression($"{_powerBase}^");
            _newInputExpected = true;
        }

        private void HandleFactorial()
        {
            var value = (int)ParseDisplayValue();
            var result = CalculateFactorial(value);
            UpdateDisplay(result.ToString(CultureInfo.InvariantCulture));
            UpdateExpression($"{value}!");
            _newInputExpected = true;
        }

        private void HandleMathFunction(string function)
        {
            var value = ParseDisplayValue();
            var result = CalculateMathFunction(value, function);
            UpdateDisplay(result.ToString(CultureInfo.InvariantCulture));
            UpdateExpression($"{function}({value})");
            _newInputExpected = true;
        }

        private void CompletePowerOperation()
        {
            var exponent = ParseDisplayValue();
            var result = Math.Pow(_powerBase, exponent);
            UpdateExpression($"{_powerBase}^{exponent} =");
            UpdateDisplay(result.ToString(CultureInfo.InvariantCulture));
            _isPowerOperation = false;
            _powerBase = 0;
            _newInputExpected = true;
            _lastResult = result;
        }


private void CompleteStandardOperation()
        {
            var expression = _expressionBuilder + DisplayText;
            CalculateResult(ParseDisplayValue());
            UpdateExpression($"{expression} =");
            _newInputExpected = true;
        }

        private void CompleteBracketOperation()
        {
            UpdateExpression(_expressionBuilder + DisplayText + ")");
            _isBracketOpen = false;
            _newInputExpected = true;
        }
        
        private void CalculateResult(double currentValue)
        {
            _lastResult = _lastOperation switch
            {
                "+" => _lastResult + currentValue,
                "-" => _lastResult - currentValue,
                "*" => _lastResult * currentValue,
                "/" => currentValue == 0 ? throw new DivideByZeroException() : _lastResult / currentValue,
                "^" => Math.Pow(_lastResult, currentValue),
                _ => _lastResult
            };

            UpdateDisplay(_lastResult.ToString(CultureInfo.InvariantCulture));
        }

        private double CalculateMathFunction(double value, string function)
        {
            return function switch
            {
                "sin" => Math.Sin(value * Math.PI / 180),
                "cos" => Math.Cos(value * Math.PI / 180),
                "tg" => Math.Tan(value * Math.PI / 180),
                "x^2" => Math.Pow(value, 2),
                "1/x" => value == 0 ? throw new DivideByZeroException() : 1 / value,
                "|x|" => Math.Abs(value),
                "2√x" => Math.Sqrt(value),
                "π" => Math.PI,
                "e" => Math.E,
                "10^x" => Math.Pow(10, value),
                "log" => Math.Log10(value),
                "ln" => Math.Log(value),
                _ => value
            };
        }

        private static double CalculateFactorial(int n)
        {
            if (n < 0) throw new ArgumentException("Factorial of negative number is undefined");
            if (n > 170) throw new ArgumentException("Value too large for factorial calculation");

            double result = 1;
            for (int i = 2; i <= n; i++) result *= i;
            return result;
        }
        
        private double ParseDisplayValue()
        {
            return double.Parse(DisplayText, CultureInfo.InvariantCulture);
        }

        private void UpdateDisplay(string value)
        {
            ResultTextBlock.Text = value;
        }

        private void UpdateExpression(string value)
        {
            if (_displayError)
            {
                _expressionBuilder.Clear();
                _displayError = false;
            }
            _expressionBuilder.Append(value);
            ExpressionTextBlock.Text = _expressionBuilder.ToString();
        }

        private void ClearExpression()
        {
            ExpressionTextBlock.Text = string.Empty;
        }

        private void ShowError(string message)
        {
            UpdateDisplay(ErrorDisplayText);
            ExpressionTextBlock.Text = message;
            _displayError = true;
        }

        private string DisplayText => ResultTextBlock.Text;
    }
}
