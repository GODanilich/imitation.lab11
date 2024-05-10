using System;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics.Distributions;

namespace IMLab11
{
    public partial class MainForm : Form
    {
        double[] statistics;
        double[] relativeStatistics;
        double[] expectedProbability;
        int N = 0;
        double empyricalVariance = 0;
        double empyricalMean = 0;
        double realVariance = 0;
        double realMean = 0;
        double criteriaChi = 16.919; //m = 9, a = 0,05
        double Chi = 0;
        double meanError = 0;
        double varianceError = 0;
        double[] value;
        double valueMax;
        double valueMin;
        double step;
        int intervalCount = 10;
        Random random = new Random();
        public MainForm()
        {
            InitializeComponent();
        }
        double GenerateCLT(double mean, double variance)
        {

            int n = 1000; // Количество случайных величин, которые будут суммироваться

            // Суммирование n случайных чисел с равномерным распределением на интервале [0, 1]
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += random.NextDouble();
            }

            // Преобразование суммы к нужному математическому ожиданию и дисперсии
            double transformedSum = mean + Math.Sqrt(variance / n) * (sum - n / 2.0);

            return transformedSum; // Возвращаем сгенерированное случайное число
        }
        void Clear()
        {
            chart1.ChartAreas[0].AxisX.CustomLabels.Clear();
            if (statistics != null)
            {
                for (int i = 0; i < intervalCount; i++)
                {
                    statistics[i] = 0;
                }
                for (int i = 0; i < N; i++)
                {
                    value[i] = 0;
                }
            }
            empyricalMean = 0;
            empyricalVariance = 0;
            realMean = 0;
            realVariance = 0;
            Chi = 0;
            meanError = 0;
            varianceError = 0;
        }
       
        void Write()
        {
            MeanLabel.Text = "Выборочное среднее: " + Math.Round(realMean,2).ToString() + " Погрешность: " + (Math.Round(meanError,2)*100).ToString() + "%";
            VarianceLabel.Text = "Дисперсия: " + Math.Round(realVariance,2).ToString() + " Погрешность: " + Math.Round(varianceError*100,2).ToString() + "%";
            if (Chi>criteriaChi)
            {
                ChiLabel.Text = "Критерий хи-квадрат: " + Math.Round(Chi,2).ToString() + " > " + criteriaChi.ToString() + " FALSE";
            }
            else
            {
                ChiLabel.Text = "Критерий хи-квадрат: " + Math.Round(Chi, 2).ToString() + " < " + criteriaChi.ToString() + " TRUE";
            }
        }
        void DrawChart()
        {
            chart1.Series[0].Points.Clear();
            for (int i = 0; i < intervalCount; i++)
            {
                chart1.Series[0].Points.AddXY(i, relativeStatistics[i]);
                chart1.ChartAreas[0].AxisX.CustomLabels.Add(new CustomLabel(i, i + 1, "(" + Math.Round((valueMin + step * i), 2).ToString() + "; " + Math.Round((valueMin + step * (i + 1)), 2).ToString() + "]", 0, LabelMarkStyle.None));
            }
        }

        void GenerateData()
        {
            empyricalMean = Double.Parse(MeanBox.Text);
            empyricalVariance = Double.Parse(VarianceBox.Text);
            value = new double[N];

            for (int i = 0; i < N; i++)
            {
                value[i] = GenerateCLT(empyricalMean, empyricalVariance);
            }

            valueMax = value.Max();
            valueMin = value.Min();
            expectedProbability = new double[intervalCount];
            statistics = new double[intervalCount];
            relativeStatistics = new double[intervalCount];

            for (int i = 0; i < intervalCount; i++)
            {
                statistics[i] = 0;
                relativeStatistics[i] = 0;
                expectedProbability[i] = 0;
            }
            step = (valueMax - valueMin) / intervalCount;
            double stepLow = valueMin;
            double stepHigh = valueMin + step;
            for (int i = 0; i < intervalCount; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if ((value[j] <= stepHigh) && (value[j] > stepLow))
                    {
                        statistics[i]++;
                    }
                }
                relativeStatistics[i] = statistics[i] / N;
                stepLow += step;
                stepHigh += step;
            }
            realMean = 0;
            realVariance = 0;
            
            for (int i = 0; i < N; i++)
            {
                realMean += value[i] / N;
            }
            for (int i = 0; i < N; i++)
            {
                realVariance += Math.Pow(value[i] - realMean, 2)/N;
            }
            
            meanError = Math.Abs(realMean - empyricalMean) / Math.Abs(realMean);
            varianceError = Math.Abs(realVariance - empyricalVariance) / Math.Abs(realVariance);

            double a = 0;
            double b = 0;

            Normal normalDist = new Normal(empyricalMean, Math.Sqrt(empyricalVariance));

            for (int i = 0; i < intervalCount; i++)
            {
                a = valueMin + step * i;
                b = valueMin + step * (i + 1);
                expectedProbability[i] = Normal.CDF(empyricalMean, Math.Sqrt(empyricalVariance), b) - Normal.CDF(empyricalVariance, Math.Sqrt(empyricalVariance), a);
            }
            Chi = 0;
            for (int i = 0; i < intervalCount; i++)
            {
                Chi += Math.Pow( statistics[i] - expectedProbability[i] * N, 2) / (expectedProbability[i] * N);
            }

        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            Clear();
            N = Int32.Parse(NumberOfTrialsBox.Text);
            GenerateData();
            DrawChart();
            Write();
        }
    }
}
