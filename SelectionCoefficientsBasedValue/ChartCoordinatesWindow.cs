using System;
using System.Data;
using OxyPlot;
using OxyPlot.Series;

namespace SelectionCoefficientsBasedValue
{
    public class ChartCoordinatesWindow
    {
        public static PlotModel MyModel { get; private set; }



        /// <summary>
        /// График
        /// </summary>
        /// <param name="Dt">Отвечает за x и линию исходных значений</param>
        /// <param name="DtSimp">Отвечает за y линии по упращенной формуле</param>
        /// <param name="DtisK">Отвечает за y линии по усложненной формуле</param>
        /// <param name="DnAndGn">Указывается имя колонки для главной таблице</param>
        /// <param name="columnIndex">индекс колонки которая будет использоваться как y</param>
        /// <param name="nameTitel">Имя графика</param>
        /// <returns></returns>
        public static PlotModel MainViewModel(DataTable Dt, DataTable DtSimp, DataTable DtisK, 
                                              string DnAndGn, int columnIndex, string nameTitel)
        {
            MyModel = new PlotModel { Title = nameTitel };

            var line1 = new LineSeries()
            {
                Title = $"Исходные значения",
                Color = OxyColors.Red,
                StrokeThickness = 2,
            };
            var line2 = new LineSeries()
            {
                Title = $"По упрощенной формуле",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
            };
            var line3 = new LineSeries()
            {
                Title = $"По усложненной формуле",
                Color = OxyColors.Green,
                StrokeThickness = 2,
            };


            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                if (columnIndex >= DtisK.Columns.Count)
                {
                    break;
                }
                string value = Dt.Rows[i][DnAndGn]?.ToString();
                string value2 = Dt.Rows[i][columnIndex]?.ToString();
                if (DtisK.Rows.Count < Dt.Rows.Count || DtSimp.Rows.Count < Dt.Rows.Count ||
                    DtisK.Columns.Count < Dt.Columns.Count || DtSimp.Columns.Count < Dt.Columns.Count)
                    {
                        break;
                    }
                    
                string value3 = DtisK.Rows[i][columnIndex]?.ToString();
                string value4 = DtSimp.Rows[i][columnIndex]?.ToString();

                if (!string.IsNullOrEmpty(value)
                    && !string.IsNullOrEmpty(value2)
                    && !string.IsNullOrEmpty(value3)
                    && !string.IsNullOrEmpty(value4))
                    {
                        double x = InputData.Converter(Dt.Rows[i][DnAndGn].ToString());
                        double yl = InputData.Converter(Dt.Rows[i][columnIndex].ToString());
                        double ySimplified = InputData.Converter(DtSimp.Rows[i][columnIndex].ToString());
                        double yHarding = InputData.Converter(DtisK.Rows[i][columnIndex].ToString());
                        line1.Points.Add(new DataPoint(x, yl));
                        line2.Points.Add(new DataPoint(x, ySimplified));
                        line3.Points.Add(new DataPoint(x, yHarding));
                    }
            }

            MyModel.Series.Add(line1);
            MyModel.Series.Add(line2);
            MyModel.Series.Add(line3);
            return MyModel;
        }


    }
}
