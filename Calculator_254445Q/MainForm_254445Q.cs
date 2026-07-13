using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator_254445Q
{
    public partial class MainForm_254445Q : Form
    {
        SpeechSynthesizer syn = new SpeechSynthesizer();
        SoundPlayer clickPlayer = null;

        string opr = "";
        double operand = 0;
        bool flagOpPressed = false;

        string equationText = "";
        double ans = 0;

        bool isDegreeMode = true;
        bool justPressedEqual = false;
        bool soundOn = true;

        // Start with STANDARD mode first
        bool scientificMode = false;

        List<string> historyEquation = new List<string>();
        List<string> historyResult = new List<string>();
        int historyIndex = -1;

        string editableEquation = "";
        int equationCursor = 0;
        bool showCursor = false;

        public MainForm_254445Q()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += MainForm_254445Q_KeyDown;
        }

        private void MainForm_254445Q_Load(object sender, EventArgs e)
        {
            txtResult.Text = "0";

            lblEquation.Text = "";
            lblEquation.TextAlign = ContentAlignment.MiddleLeft;
            lblEquation.ForeColor = Color.DimGray;
            lblEquation.Font = new Font(lblEquation.Font.FontFamily, 12.0f, FontStyle.Regular);

            txtResult.TextAlign = ContentAlignment.MiddleRight;
            txtResult.AutoSize = false;

            label1.Text = "STD";
            label2.Text = "DEG";
            label3.Text = "History";
            label4.Text = "Sound ON";

            label1.TextAlign = ContentAlignment.MiddleCenter;
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label3.TextAlign = ContentAlignment.MiddleCenter;
            label4.TextAlign = ContentAlignment.MiddleCenter;

            label1.ForeColor = Color.Black;
            label2.ForeColor = Color.Black;
            label3.ForeColor = Color.Black;
            label4.ForeColor = Color.Black;

            syn.Rate = 0;
            syn.Volume = 100;

            LoadClickSound();

            // Start in STD mode
            ShowScientificButtons(false);
            ChangeFirstButtonText(this, "SCI", "STD");
        }

        private void LoadClickSound()
        {
            try
            {
                clickPlayer = new SoundPlayer(Properties.Resources.ClickSound);
                clickPlayer.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Click sound error: " + ex.Message);
                clickPlayer = null;
            }
        }

        private void PlayClickSound()
        {
            if (soundOn == false)
            {
                return;
            }

            try
            {
                if (clickPlayer != null)
                {
                    clickPlayer.Stop();
                    clickPlayer.Play();
                }
            }
            catch
            {
                // Ignore sound error
            }
        }

        private async void SpeakEquation(string equation, string result)
        {
            if (soundOn == false)
            {
                return;
            }

            await Task.Delay(250);

            if (soundOn == false)
            {
                return;
            }

            string speechText = equation;

            speechText = speechText.Replace("+", " plus ");
            speechText = speechText.Replace("-", " minus ");
            speechText = speechText.Replace("x", " times ");
            speechText = speechText.Replace("X", " times ");
            speechText = speechText.Replace("÷", " divided by ");
            speechText = speechText.Replace("/", " divided by ");
            speechText = speechText.Replace("%", " modulus ");
            speechText = speechText.Replace("π", " pi ");
            speechText = speechText.Replace("(", " open bracket ");
            speechText = speechText.Replace(")", " close bracket ");

            speechText = speechText + " equals " + result;

            syn.SpeakAsyncCancelAll();
            syn.SpeakAsync(speechText);
        }

        private async void SpeakUnaryResult(string equation, string result)
        {
            if (soundOn == false)
            {
                return;
            }

            await Task.Delay(250);

            if (soundOn == false)
            {
                return;
            }

            string speechText = equation;

            speechText = speechText.Replace("³√", " cube root ");
            speechText = speechText.Replace("√", " square root ");
            speechText = speechText.Replace("²", " squared ");
            speechText = speechText.Replace("³", " cubed ");
            speechText = speechText.Replace("^", " power ");
            speechText = speechText.Replace("log", " log ");
            speechText = speechText.Replace("ln", " natural log ");
            speechText = speechText.Replace("sin", " sine ");
            speechText = speechText.Replace("cos", " cosine ");
            speechText = speechText.Replace("tan", " tangent ");
            speechText = speechText.Replace("π", " pi ");

            speechText = speechText + " equals " + result;

            syn.SpeakAsyncCancelAll();
            syn.SpeakAsync(speechText);
        }

        private void lblID_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            Clipboard.SetText(attribute.Value.ToString());
        }

        private void txtResult_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblEquation_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void numPad_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            Button btn = sender as Button;

            if (btn == null)
            {
                return;
            }

            InputNumber(btn.Text);
        }

        private void InputNumber(string num)
        {
            if (justPressedEqual == true && opr == "")
            {
                ClearAll();
            }

            string temp = txtResult.Text;

            if (flagOpPressed == true)
            {
                temp = "";
                flagOpPressed = false;
            }

            if (num == ".")
            {
                if (!temp.Contains("."))
                {
                    if (temp == "")
                    {
                        temp = "0";
                    }

                    temp += ".";
                }
            }
            else
            {
                if (temp == "0" || temp == "NaN")
                {
                    temp = "";
                }

                temp += num;
            }

            txtResult.Text = temp;
            UpdateEquationPreview();
            justPressedEqual = false;
        }

        private void operator_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            Button btn = sender as Button;

            if (btn == null)
            {
                return;
            }

            string selectedOpr = GetOperatorFromButton(btn);
            InputOperator(selectedOpr);
        }

        private void InputOperator(string selectedOpr)
        {
            if (selectedOpr == "")
            {
                return;
            }

            double currentValue = GetDisplayValue();
            string symbol = GetOperatorSymbol(selectedOpr);
            string currentEquation = lblEquation.Text.Replace("|", "").TrimEnd();

            if (opr == "")
            {
                operand = currentValue;

                if (currentEquation.EndsWith(")"))
                {
                    equationText = currentEquation + " " + symbol + " ";
                }
                else if (equationText.TrimEnd().EndsWith("("))
                {
                    equationText = equationText + FormatNumber(currentValue) + " " + symbol + " ";
                }
                else if (currentEquation == "π")
                {
                    equationText = "π " + symbol + " ";
                }
                else if (equationText == "")
                {
                    equationText = FormatNumber(currentValue) + " " + symbol + " ";
                }
                else
                {
                    equationText = equationText + FormatNumber(currentValue) + " " + symbol + " ";
                }
            }
            else
            {
                if (flagOpPressed == true)
                {
                    if (currentEquation.EndsWith(")"))
                    {
                        equationText = currentEquation + " " + symbol + " ";
                    }
                    else
                    {
                        equationText = ReplaceLastOperator(equationText, symbol);
                    }
                }
                else
                {
                    if (equationText.Contains("(") || equationText.Contains("π"))
                    {
                        equationText += FormatNumber(currentValue) + " " + symbol + " ";
                    }
                    else
                    {
                        operand = Calculate(operand, currentValue, opr);
                        txtResult.Text = FormatNumber(operand);
                        equationText += FormatNumber(currentValue) + " " + symbol + " ";
                    }
                }
            }

            opr = selectedOpr;
            flagOpPressed = true;
            justPressedEqual = false;

            lblEquation.Text = equationText;
            editableEquation = lblEquation.Text;
            equationCursor = editableEquation.Length;
            showCursor = false;
        }

        private void btnEqual_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            string fullEquation = lblEquation.Text.Replace("|", "").Trim();

            if (fullEquation == "")
            {
                return;
            }

            if (EndsWithOperator(fullEquation) || fullEquation.EndsWith("("))
            {
                return;
            }

            double result;

            try
            {
                result = EvaluateEquationLeftToRight(fullEquation);
            }
            catch
            {
                result = double.NaN;
            }

            string resultText = FormatNumber(result);

            lblEquation.Text = fullEquation;
            txtResult.Text = resultText;

            editableEquation = fullEquation;
            equationCursor = editableEquation.Length;
            showCursor = false;

            AddHistory(fullEquation, resultText);
            SpeakEquation(fullEquation, resultText);

            if (!double.IsNaN(result) && !double.IsInfinity(result))
            {
                ans = result;
            }

            operand = result;
            opr = "";
            equationText = "";
            flagOpPressed = true;
            justPressedEqual = true;
        }

        private double Calculate(double num1, double num2, string selectedOpr)
        {
            switch (selectedOpr)
            {
                case "Add":
                    return num1 + num2;

                case "Subtract":
                    return num1 - num2;

                case "Multiply":
                    return num1 * num2;

                case "Divide":
                    if (num2 == 0)
                    {
                        return double.NaN;
                    }
                    return num1 / num2;

                case "Modulus":
                    if (num2 == 0)
                    {
                        return double.NaN;
                    }
                    return num1 % num2;

                default:
                    return num2;
            }
        }

        private string GetOperatorFromButton(Button btn)
        {
            if (btn.Tag != null && btn.Tag.ToString() != "")
            {
                return btn.Tag.ToString();
            }

            switch (btn.Text)
            {
                case "+":
                    return "Add";

                case "-":
                    return "Subtract";

                case "X":
                case "x":
                case "×":
                case "*":
                    return "Multiply";

                case "÷":
                case "/":
                    return "Divide";

                case "%":
                    return "Modulus";

                default:
                    return "";
            }
        }

        private string GetOperatorSymbol(string selectedOpr)
        {
            switch (selectedOpr)
            {
                case "Add":
                    return "+";

                case "Subtract":
                    return "-";

                case "Multiply":
                    return "x";

                case "Divide":
                    return "÷";

                case "Modulus":
                    return "%";

                default:
                    return "";
            }
        }

        private string ReplaceLastOperator(string text, string newSymbol)
        {
            text = text.TrimEnd();

            if (text.Length == 0)
            {
                return "";
            }

            int lastSpace = text.LastIndexOf(' ');

            if (lastSpace >= 0)
            {
                text = text.Substring(0, lastSpace + 1);
                text += newSymbol + " ";
            }

            return text;
        }

        private void uOperator_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            Button btn = sender as Button;

            if (btn == null)
            {
                return;
            }

            string u_opr = GetUnaryFromButton(btn);
            InputUnary(u_opr);
        }

        private void InputUnary(string u_opr)
        {
            if (u_opr == "")
            {
                return;
            }

            double value = GetDisplayValue();
            double result = 0;
            string displayEquation = "";

            switch (u_opr)
            {
                case "SqrtRoot":
                    if (value < 0)
                    {
                        result = double.NaN;
                    }
                    else
                    {
                        result = Math.Sqrt(value);
                    }

                    displayEquation = "√(" + FormatNumber(value) + ")";
                    break;

                case "Square":
                    result = Math.Pow(value, 2);
                    displayEquation = "(" + FormatNumber(value) + ")²";
                    break;

                case "Cube":
                    result = Math.Pow(value, 3);
                    displayEquation = "(" + FormatNumber(value) + ")³";
                    break;

                case "CubeRoot":
                    result = Math.Pow(value, 1.0 / 3.0);
                    displayEquation = "³√(" + FormatNumber(value) + ")";
                    break;

                case "Reciprocal":
                    if (value == 0)
                    {
                        result = double.NaN;
                    }
                    else
                    {
                        result = 1 / value;
                    }

                    displayEquation = "1/(" + FormatNumber(value) + ")";
                    break;

                case "PlusMinus":
                    result = -value;
                    displayEquation = "-(" + FormatNumber(value) + ")";
                    break;

                case "Log10":
                    if (value <= 0)
                    {
                        result = double.NaN;
                    }
                    else
                    {
                        result = Math.Log10(value);
                    }

                    displayEquation = "log(" + FormatNumber(value) + ")";
                    break;

                case "Ln":
                    if (value <= 0)
                    {
                        result = double.NaN;
                    }
                    else
                    {
                        result = Math.Log(value);
                    }

                    displayEquation = "ln(" + FormatNumber(value) + ")";
                    break;

                case "Pow10":
                    result = Math.Pow(10, value);
                    displayEquation = "10^(" + FormatNumber(value) + ")";
                    break;

                case "Exp":
                    result = Math.Exp(value);
                    displayEquation = "e^(" + FormatNumber(value) + ")";
                    break;

                case "Sin":
                    result = Math.Sin(ConvertAngle(value));
                    displayEquation = "sin(" + FormatNumber(value) + ")";
                    break;

                case "Cos":
                    result = Math.Cos(ConvertAngle(value));
                    displayEquation = "cos(" + FormatNumber(value) + ")";
                    break;

                case "Tan":
                    result = Math.Tan(ConvertAngle(value));
                    displayEquation = "tan(" + FormatNumber(value) + ")";
                    break;

                case "Pi":
                    result = Math.PI;
                    displayEquation = "π";
                    break;

                default:
                    return;
            }

            txtResult.Text = FormatNumber(result);

            if (!double.IsNaN(result) && !double.IsInfinity(result))
            {
                ans = result;
            }

            if (u_opr == "Pi")
            {
                if (equationText != "" || lblEquation.Text.Replace("|", "").EndsWith("("))
                {
                    lblEquation.Text = equationText + "π";
                }
                else
                {
                    lblEquation.Text = "π";
                }
            }
            else
            {
                if (opr == "")
                {
                    lblEquation.Text = displayEquation;
                }
                else
                {
                    lblEquation.Text = equationText + displayEquation;
                }
            }

            editableEquation = lblEquation.Text;
            equationCursor = editableEquation.Length;
            showCursor = false;

            flagOpPressed = false;
            justPressedEqual = false;

            SpeakUnaryResult(displayEquation, txtResult.Text);
        }

        private string GetUnaryFromButton(Button btn)
        {
            if (btn.Tag != null && btn.Tag.ToString() != "")
            {
                return btn.Tag.ToString();
            }

            switch (btn.Text)
            {
                case "√":
                    return "SqrtRoot";

                case "x²":
                case "X²":
                    return "Square";

                case "x³":
                case "X³":
                    return "Cube";

                case "3√":
                case "³√":
                    return "CubeRoot";

                case "1/x":
                case "1/X":
                    return "Reciprocal";

                case "±":
                    return "PlusMinus";

                case "log":
                case "log₁₀":
                    return "Log10";

                case "ln":
                    return "Ln";

                case "10ˣ":
                case "10x":
                case "10^x":
                    return "Pow10";

                case "eˣ":
                case "ex":
                case "e^x":
                    return "Exp";

                case "sin":
                    return "Sin";

                case "cos":
                    return "Cos";

                case "tan":
                    return "Tan";

                case "π":
                    return "Pi";

                default:
                    return "";
            }
        }

        private double ConvertAngle(double value)
        {
            if (isDegreeMode == true)
            {
                return value * Math.PI / 180;
            }
            else
            {
                return value;
            }
        }

        private void btnOpenBracket_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            if (justPressedEqual == true)
            {
                ClearAll();
            }

            string currentEquation = lblEquation.Text.Replace("|", "").TrimEnd();

            if (currentEquation == "")
            {
                equationText = "(";
            }
            else if (char.IsDigit(currentEquation[currentEquation.Length - 1]) ||
                     currentEquation.EndsWith(")") ||
                     currentEquation.EndsWith("π"))
            {
                // Example: 9(9) becomes 9 x (9)
                equationText = currentEquation + " x (";
            }
            else
            {
                equationText = currentEquation + "(";
            }

            lblEquation.Text = equationText;

            editableEquation = lblEquation.Text;
            equationCursor = editableEquation.Length;
            showCursor = false;

            flagOpPressed = true;
            justPressedEqual = false;
        }

        private void btnCloseBracket_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            string currentEquation = lblEquation.Text.Replace("|", "").TrimEnd();

            if (currentEquation == "")
            {
                return;
            }

            lblEquation.Text = currentEquation + ")";
            equationText = lblEquation.Text;

            editableEquation = lblEquation.Text;
            equationCursor = editableEquation.Length;
            showCursor = false;

            flagOpPressed = true;
            justPressedEqual = false;
        }

        private double EvaluateEquationLeftToRight(string expression)
        {
            int index = 0;
            return ParseExpression(expression, ref index);
        }

        private double ParseExpression(string expression, ref int index)
        {
            double value = ParseValue(expression, ref index);

            while (index < expression.Length)
            {
                SkipSpaces(expression, ref index);

                if (index >= expression.Length || expression[index] == ')')
                {
                    break;
                }

                char op = expression[index];
                index++;

                double nextValue = ParseValue(expression, ref index);

                switch (op)
                {
                    case '+':
                        value = value + nextValue;
                        break;

                    case '-':
                        value = value - nextValue;
                        break;

                    case 'x':
                    case 'X':
                    case '*':
                    case '×':
                        value = value * nextValue;
                        break;

                    case '÷':
                    case '/':
                        if (nextValue == 0)
                        {
                            return double.NaN;
                        }
                        value = value / nextValue;
                        break;

                    case '%':
                        if (nextValue == 0)
                        {
                            return double.NaN;
                        }
                        value = value % nextValue;
                        break;
                }
            }

            return value;
        }

        private double ParseValue(string expression, ref int index)
        {
            SkipSpaces(expression, ref index);

            if (index < expression.Length && expression[index] == '(')
            {
                index++;
                double bracketValue = ParseExpression(expression, ref index);

                if (index < expression.Length && expression[index] == ')')
                {
                    index++;
                }

                return bracketValue;
            }

            if (index < expression.Length && expression[index] == 'π')
            {
                index++;
                return Math.PI;
            }

            string numberText = "";

            if (index < expression.Length && expression[index] == '-')
            {
                numberText += "-";
                index++;
            }

            while (index < expression.Length &&
                  (char.IsDigit(expression[index]) || expression[index] == '.'))
            {
                numberText += expression[index];
                index++;
            }

            double number;

            if (double.TryParse(numberText, out number))
            {
                return number;
            }

            return 0;
        }

        private void SkipSpaces(string expression, ref int index)
        {
            while (index < expression.Length && expression[index] == ' ')
            {
                index++;
            }
        }

        private bool EndsWithOperator(string expression)
        {
            expression = expression.TrimEnd();

            if (expression.Length == 0)
            {
                return false;
            }

            char last = expression[expression.Length - 1];

            return last == '+' || last == '-' || last == 'x' || last == 'X' ||
                   last == '×' || last == '*' || last == '÷' || last == '/' ||
                   last == '%';
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            ClearAll();
        }

        private void ClearAll()
        {
            opr = "";
            operand = 0;
            flagOpPressed = false;
            equationText = "";
            txtResult.Text = "0";
            lblEquation.Text = "";
            justPressedEqual = false;

            editableEquation = "";
            equationCursor = 0;
            showCursor = false;

            syn.SpeakAsyncCancelAll();
        }

        private void btnAns_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            txtResult.Text = FormatNumber(ans);
            flagOpPressed = false;
            justPressedEqual = false;
            UpdateEquationPreview();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            if (txtResult.Text != "")
            {
                Clipboard.SetText(txtResult.Text);
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            DeleteOneCharacter();
        }

        private void DeleteOneCharacter()
        {
            editableEquation = lblEquation.Text.Replace("|", "");

            if (editableEquation.Length > 0)
            {
                if (showCursor == true)
                {
                    if (equationCursor > 0)
                    {
                        editableEquation = editableEquation.Remove(equationCursor - 1, 1);
                        equationCursor--;
                    }
                }
                else
                {
                    editableEquation = editableEquation.Substring(0, editableEquation.Length - 1);
                    equationCursor = editableEquation.Length;
                }

                if (editableEquation.Length == 0)
                {
                    ClearAll();
                    return;
                }

                UpdateEquationWithCursor();
                return;
            }

            ClearAll();
        }

        private void UpdateEquationWithCursor()
        {
            if (editableEquation == "")
            {
                lblEquation.Text = "";
                return;
            }

            if (equationCursor < 0)
            {
                equationCursor = 0;
            }

            if (equationCursor > editableEquation.Length)
            {
                equationCursor = editableEquation.Length;
            }

            if (showCursor == true)
            {
                lblEquation.Text = editableEquation.Insert(equationCursor, "|");
            }
            else
            {
                lblEquation.Text = editableEquation;
            }
        }

        private void MoveCursorLeft()
        {
            showCursor = true;

            editableEquation = lblEquation.Text.Replace("|", "");

            if (equationCursor == 0)
            {
                equationCursor = editableEquation.Length;
            }

            if (equationCursor > 0)
            {
                equationCursor--;
            }

            UpdateEquationWithCursor();
        }

        private void MoveCursorRight()
        {
            showCursor = true;

            editableEquation = lblEquation.Text.Replace("|", "");

            if (equationCursor < editableEquation.Length)
            {
                equationCursor++;
            }

            UpdateEquationWithCursor();
        }

        private void btnDegRad_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            isDegreeMode = !isDegreeMode;

            Button btn = sender as Button;

            if (btn != null)
            {
                if (isDegreeMode == true)
                {
                    btn.Text = "DEG";
                    label2.Text = "DEG";
                }
                else
                {
                    btn.Text = "RAD";
                    label2.Text = "RAD";
                }
            }
        }

        private void btnSound_Click(object sender, EventArgs e)
        {
            soundOn = !soundOn;

            Button btn = sender as Button;

            if (btn != null)
            {
                if (soundOn == true)
                {
                    btn.Text = "Sound";
                    label4.Text = "Sound ON";
                    PlayClickSound();
                }
                else
                {
                    btn.Text = "Mute";
                    label4.Text = "Sound OFF";

                    syn.SpeakAsyncCancelAll();

                    if (clickPlayer != null)
                    {
                        clickPlayer.Stop();
                    }
                }
            }
        }

        private void btnSci_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            scientificMode = !scientificMode;

            Button btn = sender as Button;

            if (scientificMode == true)
            {
                if (btn != null)
                {
                    btn.Text = "SCI";
                }

                label1.Text = "SCI";
                ShowScientificButtons(true);
            }
            else
            {
                if (btn != null)
                {
                    btn.Text = "STD";
                }

                label1.Text = "STD";
                ShowScientificButtons(false);
            }
        }

        private void ShowScientificButtons(bool show)
        {
            SetScientificButtonsVisible(this, show);
        }

        private void SetScientificButtonsVisible(Control parent, bool show)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button)
                {
                    Button btn = (Button)ctrl;

                    switch (btn.Text)
                    {
                        case "10ˣ":
                        case "10x":
                        case "10^x":
                        case "π":
                        case "𝝅":
                        case "pi":
                        case "PI":
                        case "(":
                        case ")":
                        case "3√":
                        case "³√":
                        case "x³":
                        case "X³":
                        case "x^3":
                        case "sin":
                        case "cos":
                        case "tan":
                        case "ln":
                        case "log":
                        case "log₁₀":
                        case "eˣ":
                        case "ex":
                        case "e^x":
                            btn.Visible = show;
                            break;
                    }
                }

                if (ctrl.HasChildren)
                {
                    SetScientificButtonsVisible(ctrl, show);
                }
            }
        }

        private void ChangeFirstButtonText(Control parent, string oldText, string newText)
        {
            foreach (Control ctrl in parent.Controls)
            {
                Button btn = ctrl as Button;

                if (btn != null && btn.Text == oldText)
                {
                    btn.Text = newText;
                    return;
                }

                if (ctrl.HasChildren)
                {
                    ChangeFirstButtonText(ctrl, oldText, newText);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            PlayClickSound();

            syn.SpeakAsyncCancelAll();

            if (clickPlayer != null)
            {
                clickPlayer.Stop();
            }

            this.Close();
        }

        private void AddHistory(string equation, string result)
        {
            historyEquation.Add(equation);
            historyResult.Add(result);
            historyIndex = historyEquation.Count;
        }

        private void ShowHistory(int direction)
        {
            if (historyEquation.Count == 0)
            {
                label3.Text = "No History";
                return;
            }

            historyIndex += direction;

            if (historyIndex < 0)
            {
                historyIndex = 0;
            }

            if (historyIndex >= historyEquation.Count)
            {
                historyIndex = historyEquation.Count - 1;
            }

            lblEquation.Text = historyEquation[historyIndex];
            txtResult.Text = historyResult[historyIndex];

            editableEquation = lblEquation.Text;
            equationCursor = editableEquation.Length;
            showCursor = false;

            opr = "";
            equationText = "";
            flagOpPressed = true;
            justPressedEqual = true;

            if (direction < 0)
            {
                label3.Text = "History Up";
            }
            else
            {
                label3.Text = "History Down";
            }
        }

        private void btnHistoryUp_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            ShowHistory(-1);
        }

        private void btnHistoryDown_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            ShowHistory(1);
        }

        private void btnHistoryLeft_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            MoveCursorLeft();
        }

        private void btnHistoryRight_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            MoveCursorRight();
        }

        private void btnNotImplemented_Click(object sender, EventArgs e)
        {
            PlayClickSound();
            lblEquation.Text = "Not used";
        }

        private void MainForm_254445Q_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                ShowHistory(-1);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                ShowHistory(1);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                MoveCursorLeft();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Right)
            {
                MoveCursorRight();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 && e.Shift == false)
            {
                int num = e.KeyCode - Keys.D0;
                InputNumber(num.ToString());
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                int num = e.KeyCode - Keys.NumPad0;
                InputNumber(num.ToString());
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
            {
                InputNumber(".");
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Add || (e.KeyCode == Keys.Oemplus && e.Shift == true))
            {
                InputOperator("Add");
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                InputOperator("Subtract");
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Multiply || (e.KeyCode == Keys.D8 && e.Shift == true))
            {
                InputOperator("Multiply");
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion)
            {
                InputOperator("Divide");
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.D5 && e.Shift == true)
            {
                InputOperator("Modulus");
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Enter || (e.KeyCode == Keys.Oemplus && e.Shift == false))
            {
                btnEqual_Click(sender, e);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.C)
            {
                ClearAll();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                DeleteOneCharacter();
                e.SuppressKeyPress = true;
                return;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                MoveCursorLeft();
                return true;
            }

            if (keyData == Keys.Right)
            {
                MoveCursorRight();
                return true;
            }

            if (keyData == Keys.Up)
            {
                ShowHistory(-1);
                return true;
            }

            if (keyData == Keys.Down)
            {
                ShowHistory(1);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private double GetDisplayValue()
        {
            double value;

            if (double.TryParse(txtResult.Text, out value))
            {
                return value;
            }

            return 0;
        }

        private string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "NaN";
            }

            string result = Math.Round(value, 6).ToString("0.######");

            if (result == "-0")
            {
                result = "0";
            }

            return result;
        }

        private void UpdateEquationPreview()
        {
            if (opr != "" && flagOpPressed == false)
            {
                lblEquation.Text = equationText + txtResult.Text;
            }
            else if (opr == "" && equationText == "")
            {
                if (txtResult.Text == "0")
                {
                    lblEquation.Text = "";
                }
                else
                {
                    lblEquation.Text = txtResult.Text;
                }
            }
            else if (opr == "" && equationText.EndsWith("("))
            {
                lblEquation.Text = equationText + txtResult.Text;
            }
            else
            {
                lblEquation.Text = equationText;
            }

            editableEquation = lblEquation.Text;
            equationCursor = editableEquation.Length;
            showCursor = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            InputOperator("Add");
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblBrand_Click(object sender, EventArgs e)
        {

        }
    }
}