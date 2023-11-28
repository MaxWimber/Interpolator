using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows;


namespace SelectionCoefficientsBasedValue
{
    public class InputData
    {



        /// <summary>
        /// Вставка текста
        /// </summary>
        /// <param name="DataGrid">DataGrid к которому привязывается таблица</param>
        /// <param name="Dt">Таблица в которую вставляются значения</param>
        /// <param name="currentRowIndex">текущий индекс строки выделенной ячейки</param>
        /// <param name="columnIndex">текущий индекс колонки выделенной ячейки</param>
        public void Instance(DataGrid DataGrid, DataTable Dt, int currentRowIndex, int columnIndex)
        {
            var copyingData = Clipboard.GetData("Text");
            if (copyingData == null) 
            { 
                return; 
            }

            string[] dataNumbers = copyingData.ToString().Split('\n');

            dataNumbers = dataNumbers.Reverse().Skip(1).Reverse().ToArray();
            int DataTableCountRow = currentRowIndex;
            for (int i = 0; i < dataNumbers.Length; i++)
            {
                int dataTableCountColumn = columnIndex;
                string[] dataNumberCount = dataNumbers[i].Split('\t');
                if (Dt.Rows.Count < dataNumbers.Length + currentRowIndex) 
                { 
                    Dt.Rows.Add(); 
                }
                for (int j = 0; j < dataNumberCount.Length; j++)
                {
                    if (Dt.Columns.Count < dataNumberCount.Length + columnIndex && Dt.Columns[$"angle{dataTableCountColumn}"] == null)
                    {
                            InterfaceDataGrid.iforAngel++;
                            InterfaceDataGrid.CreateDataGridColumns(DataGrid, $"angle{dataTableCountColumn}",
                                                                    typeof(TextBlock), $"angle{dataTableCountColumn}", 40.0);
                            Dt.Columns.Add($"angle{dataTableCountColumn}");
                    }
                    Dt.Rows[DataTableCountRow][dataTableCountColumn] = dataNumberCount[j].Replace("\r", "");
                    dataTableCountColumn++;
                }
                DataTableCountRow++;
            }
            ClearSymbol(Dt);
            InterfaceDataGrid.BindingDataGridByName(DataGrid, Dt, false, false);
        }

        /// <summary>
        /// Конвертер из string в double
        /// </summary>
        /// <param name="stringForConvert">Строка которую нужно конвертировать</param>
        /// <returns>возвращает число с плавающей точкой, которое прошло конвертацию</returns>
        public static double Converter(string stringForConvert) 
        {
            string inputString = stringForConvert ?? "0";
            if (!double.TryParse(inputString.Replace(",", "."), out double outputValue))
            {
                double.TryParse(inputString.Replace(".", ","), out outputValue);
            }  
            return outputValue;
        }

        private void ClearSymbol(DataTable Dt)
        {
            for (int i = 0; i < Dt.Columns.Count; i++)
            {
                Dt.AsEnumerable()
                    .Where(row => (row.Field<string>(Dt.Columns[i])?.Any(symbol => char.IsLetter(symbol)) ?? true))
                    .ToList()
                    .ForEach(row => row.SetField<string>(Dt.Columns[i], ""));

            }
        }

        /// <summary>
        /// Поиск значений для первых и последних
        /// </summary>
        /// <param name="Dt">Таблица из которой берутся значения</param>
        /// <param name="nameFirstColumn">Имя первой колонки</param>
        /// <param name="indexSecondColumn">Индекс второй колонки</param>
        /// <returns>возвращет коллекцию значений которые не имеют пустых ячеек</returns>
        private List<(string FirstColumn, string SecondColumn)> GetStringValuesFromTable(DataTable Dt, string nameFirstColumn, 
                                                                                         int indexSecondColumn)  
        {
            List<(string FirstColumn, string SecondColumn)> rows = Dt.AsEnumerable()
            .Select(row => (FirstColumn: row.Field<string>(nameFirstColumn), SecondColumn: row.Field<string>(indexSecondColumn)))
            .Where(row => !string.IsNullOrEmpty(row.FirstColumn)
                            && row.FirstColumn != "0" 
                            && !string.IsNullOrEmpty(row.SecondColumn)
                            && row.SecondColumn != "0").ToList();
            return rows;
        }
        /// <summary>
        /// Поиск первого значения начиная с указаной колонки
        /// </summary>
        /// <param name="dataTable">Таблица из которой берутся значения</param>
        /// <param name="columnIndex">индекс колонки</param>
        /// <returns>возвращает первое значение начиная с указаной колонки</returns>
        public string GetFirst(DataTable dataTable, int columnIndex)
        {
            string rows = dataTable.AsEnumerable()
            .Select(row => row.Field<string>(columnIndex))
            .Where(row => !string.IsNullOrEmpty(row) && row != "0").ToList().FirstOrDefault();
            return rows;
        }

        /// <summary>
        /// Формула расчета всех коэфициентов
        /// </summary>
        /// <param name="firstValue">Первый найденный элемент из двух колонок</param>
        /// <param name="lastValue">Последний найденный элемент из двух колонок</param>
        /// <returns>Возвращает первый и второй коэффициент</returns>
        private (double firstСoefficient, double SecondСoefficient) СoefficientCalculationformula((string dN, string lZ) firstValue,
                                                                                                    (string dN, string lZ) lastValue)
        {
            (double firstСoefficient, double SecondСoefficient) coefficient = (firstСoefficient: 0, SecondСoefficient: 0);

            double k = 1;
            double a = 1;

            double lastResultlz = Converter(lastValue.lZ);
            double lastResultdN = Converter(lastValue.dN);
            double firstResultlz = Converter(firstValue.lZ);
            double firstResultdN = Converter(firstValue.dN);

            while (coefficient.firstСoefficient != k)
            {
                if (lastResultdN < firstResultdN || lastResultlz < firstResultlz || firstResultlz == lastResultdN || firstResultdN == lastResultlz)
                {
                    break;
                }
                if (lastValue != (null, null) && firstValue != (null, null))
                {
                    k = (lastResultlz - coefficient.SecondСoefficient) / lastResultdN;
                    coefficient.firstСoefficient = k;
                    while (coefficient.SecondСoefficient != a)
                    {
                        a = firstResultlz - firstResultdN * coefficient.firstСoefficient;
                        coefficient.SecondСoefficient = a;
                        k = (lastResultlz - coefficient.SecondСoefficient) / lastResultdN;
                    }
                    a = firstResultlz - firstResultdN * k;
                }
                else
                {
                    break;
                }
            }
            return coefficient;
        }

        /// <summary>
        /// Заполнение пустых значений исходной таблицы
        /// </summary>
        /// <param name="Dt">Главная таблица</param>
        /// <param name="DtForKandAFromAngle">Таблица из которой берутся коэффициенты для подсчета</param>
        /// <param name="iAngle">i из цикла который выступает вкачестве индекса</param>
        public void FillingEmptyRows(DataTable Dt, DataTable DtForKandAFromAngle, int iAngle)
        {
            ClearSymbol(Dt);
            for (int i = 0; i < Dt.Columns.Count; i++)
            {
                Dt.AsEnumerable()
                    .Where(row => !(row.Field<string>(Dt.Columns[i])?.Any(symbol => char.IsDigit(symbol)) ?? true))
                    .ToList()
                    .ForEach(row => row.SetField<string>(Dt.Columns[i], ""));
            }

            List<(string FirstColumn, string SecondColumn)> stringValuesFromTable = GetStringValuesFromTable(Dt, MainWindow.columnName[0], iAngle);
            (string dN, string lZ) firstValue = stringValuesFromTable.FirstOrDefault();
            (string dN, string lZ) lastValue = stringValuesFromTable.LastOrDefault();

            (double coefficientK, double coefficientA) = СoefficientCalculationformula(firstValue, lastValue);

            if (DtForKandAFromAngle.Rows.Count < iAngle) 
            { 
                DtForKandAFromAngle.Rows.Add(); 
            }
            if (GetFirst(Dt, iAngle) != "" && Converter(GetFirst(Dt, iAngle)) != 0)
            {
                DataRow KAndA = DtForKandAFromAngle.Rows[iAngle - 1];
                KAndA["angle"] = GetFirst(Dt, iAngle);
                KAndA["k"] = coefficientK;
                KAndA["a"] = coefficientA;
            }

            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                string rowsDt = Dt.Rows[i][MainWindow.columnName[0]].ToString();
                if (rowsDt != "")
                {
                    double result = Converter(rowsDt);
                    if (result > 0 && (Dt.Rows[i][iAngle].ToString() == "" || result == 0))
                    {
                        double valueKZ = result * coefficientK + coefficientA;
                        if (valueKZ != 0)
                        {
                            Dt.Rows[i][iAngle] = Math.Round(valueKZ, 3);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Расчет и заполнение таблицы по упращенной формуле
        /// </summary>
        /// <param name="Dt">Главная таблица из которой берутся значения</param>
        /// <param name="DataTableNewValueFirst">Таблица из которой берутся коэффициенты для подсчета</param>
        /// <param name="iAngle">i из цикла который выступает вкачестве индекса</param>
        public void Searchables(DataTable Dt, DataTable DataTableNewValueFirst, int iAngle)
        {
            List<(string FirstColumn, string SecondColumn)> stringValuesFromTable = GetStringValuesFromTable(Dt, MainWindow.columnName[0], iAngle);
            (string dN, string lZ) firstValue = stringValuesFromTable.FirstOrDefault();
            (string dN, string lZ) lastValue = stringValuesFromTable.LastOrDefault();

            (double coefficientK, double coefficientA) = СoefficientCalculationformula(firstValue, lastValue);

            DataRowCollection rows = DataTableNewValueFirst.Rows;
            DataColumnCollection columns = DataTableNewValueFirst.Columns;
            if (Dt.Rows.Count <= 1 || Dt.Columns.Count <= 1)
            {
                return;
            }
            for (int i = 0; i < Dt.Rows.Count; i++)
            {

                if (rows.Count < Dt.Rows.Count) 
                {
                    rows.Add(); 
                }
                else if (rows.Count > Dt.Rows.Count) 
                { 
                    rows.RemoveAt(rows.Count - 1); 
                }
                for (int j = 1; j < Dt.Columns.Count; j++)
                {
                    if (columns.Count < Dt.Columns.Count && columns[$"angle{j}"] == null)
                    {
                        columns.Add($"angle{j}");
                    }
                    if (columns.Count > Dt.Columns.Count)
                    {
                        columns.RemoveAt(columns.Count - 1);
                    }
                }
            }
            for (int i = 0; i < rows.Count; i++) 
            {
                object itemDt = Dt.Rows[i][MainWindow.columnName[0]];
                if (Dt.Rows == null || rows.Count > Dt.Rows.Count)
                {
                    continue;
                }
                if (itemDt.ToString() != "" && Converter(itemDt.ToString()) != 0)
                {
                    rows[i][MainWindow.columnName[0]] = itemDt;
                }
                rows[i][iAngle] = Dt.Rows[i][iAngle];
                
            }
            for (int i = 1; i < rows.Count; i++)
            {
                object itemDt = Dt.Rows[i][MainWindow.columnName[0]];
                if (Dt.Rows == null || rows.Count > Dt.Rows.Count)
                {
                    continue;
                }
                if (itemDt.ToString() != "" && Converter(itemDt.ToString()) != 0 && iAngle < columns.Count)
                {
                    double valueKZ = Converter(itemDt.ToString()) * coefficientK + coefficientA;
                    rows[i][iAngle] = valueKZ;
                }
            }
                
            
        }
        /// <summary>
        /// Расчет таблицы искомых значений по усложненной формуле
        /// </summary>
        /// <param name="DtForKandAFromAngle">Таблица из которой берутся коэффициенты для подсчета</param>
        /// <param name="DtSearchable">Таблица искомых коэфициентов</param>
        /// <param name="Dt">Главная таблица из которых берутся значения</param>
        /// <param name="DataTableNewValue">Таблица новых значений, которые расчитываются по услажненой формуле</param>
        /// <param name="DataGrid">DataGrid который связывается с DataTableNewValue</param>
        public void HardingAngle(DataTable DtForKandAFromAngle, DataTable DtSearchable, DataTable Dt, DataTable DataTableNewValue, DataGrid DataGrid)
        {

            if (Dt.Rows.Count == 0) 
            { 
                Dt.Rows.Add(); 
            }
            if (Dt.Rows.Count <= 1 || Dt.Columns.Count <= 1)
            {
                return;
            }
            List<(string FirstColumn, string SecondColumn)> stringValuesFromTable = GetStringValuesFromTable(DtForKandAFromAngle, "angle", 1);
            (string Angle, string k) first = stringValuesFromTable.FirstOrDefault();
            (string Angle, string k) last = stringValuesFromTable.LastOrDefault();

            List<(string FirstColumn, string SecondColumn)> stringValuesFromTableBC = GetStringValuesFromTable(DtForKandAFromAngle, "angle", 2);
            (string Angle, string a) firstBC = stringValuesFromTableBC.FirstOrDefault();
            (string Angle, string a) lastBC = stringValuesFromTableBC.LastOrDefault();

            (double coefficientM, double coefficientN) = СoefficientCalculationformula(first, last);
            (double coefficientB, double coefficientC) = СoefficientCalculationformula(firstBC, lastBC);


            for (int i = 0; i < DtForKandAFromAngle.Rows.Count; i++)
            {
                if (DtSearchable.Rows.Count < DtForKandAFromAngle.Rows.Count) 
                { 
                    DtSearchable.Rows.Add(); 
                }
                if (DtForKandAFromAngle.Rows[i]["angle"].ToString() != "")
                {
                    DataRow dataRow = DtSearchable.Rows[i];
                    double angle = Converter(DtForKandAFromAngle.Rows[i]["angle"].ToString());
                    dataRow["isk"] = angle * coefficientM + coefficientN;
                    dataRow["isa"] = angle * coefficientB + coefficientC;
                    dataRow["M"] = Math.Round(coefficientM, 3);
                    dataRow["N"] = Math.Round(coefficientN, 3);
                    dataRow["B"] = Math.Round(coefficientB, 3);
                    dataRow["C"] = Math.Round(coefficientC, 3);
                }
            }

            DataRowCollection rows = DataTableNewValue.Rows;
            DataColumnCollection columns = DataTableNewValue.Columns;

            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                if (rows.Count < Dt.Rows.Count) 
                {
                    rows.Add(); 
                }
                else if (rows.Count > Dt.Rows.Count) 
                { 
                    rows.RemoveAt(rows.Count - 1); 
                }
                for (int j = 1; j < Dt.Columns.Count; j++)
                {
                    int countEmptyColumns = Dt.AsEnumerable()
                                             .Select(row => row.Field<string>(j))
                                             .Where(row => !string.IsNullOrEmpty(row) && row != "0")
                                             .ToList().Count;

                    if (columns.Count < Dt.Columns.Count && columns[$"angle{j}"] == null)
                    {
                        InterfaceDataGrid.CreateDataGridColumns(DataGrid, $"angle{j}", typeof(TextBlock), $"angle{j}", 40.0);
                        columns.Add($"angle{j}");
                    }
                    
                }
                if (columns.Count > Dt.Columns.Count)
                {
                    columns.RemoveAt(columns.Count - 1);
                    DataGrid.Columns.RemoveAt(DataGrid.Columns.Count - 1);
                }
            }
            for (int i = 0; i < rows.Count; i++)
            {
                object itemDt = Dt.Rows[i][MainWindow.columnName[0]];
                if (Dt.Rows == null || rows.Count > Dt.Rows.Count)
                {
                    continue;
                }
                if (itemDt.ToString() != "" && Converter(itemDt.ToString()) != 0)
                {
                    rows[i][MainWindow.columnName[0]] = itemDt;
                }
                for (int j = 1; j < columns.Count; j++)
                {
                    if (Dt.Columns.Count < columns.Count)
                    {
                        continue;
                    }
                    rows[i][j] = Dt.Rows[i][j];
                }
            }
            for (int i = 1; i < rows.Count; i++)
            {
                object itemDt = Dt.Rows[i][MainWindow.columnName[0]];
                if (Dt.Rows == null || rows.Count > Dt.Rows.Count)
                {
                    continue;
                }
                if (itemDt.ToString() != "" && Converter(itemDt.ToString()) != 0)
                {
                    for (int j = 1; j < columns.Count; j++)
                    {
                        DataRow searchableRows = DtSearchable.Rows[j - 1];
                        if (searchableRows[0].ToString() != "")
                        {
                            rows[i][j] = Math.Round(Converter(itemDt.ToString())
                                                            * Converter(searchableRows["isk"].ToString())
                                                            + Converter(searchableRows["isa"].ToString()), 3);
                        }
                    }
                }
            }
            InterfaceDataGrid.BindingDataGridByName(DataGrid, DataTableNewValue, false, true);
        }

        /// <summary>
        /// Разварот таблиц по вертикали
        /// </summary>
        /// <param name="Dt">Таблица которую нужно развернуть по вертикали</param>
        /// <param name="DtReverse">Таблица которая будет хранить значения по вертикале</param>
        /// <param name="DataGrid">DataGrid которая будет связываться с DtReverse</param>
        public void Reverse(DataTable Dt, DataTable DtReverse, DataGrid DataGrid)
        {
            DataRowCollection rowsReverse = DtReverse.Rows;
            DataRowCollection rowsDt = Dt.Rows;
            DataColumnCollection columnsDt = Dt.Columns;

            while (rowsReverse.Count < (rowsDt.Count - 1) * (columnsDt.Count - 1)) 
            {
                rowsReverse.Add(); 
            }
            while (rowsReverse.Count > (rowsDt.Count - 1) * (columnsDt.Count - 1)) 
            {
                rowsReverse.RemoveAt(rowsReverse.Count - 1); 
            }

            int k = 1;
            int l = 1;

            for (int i = 0; i < rowsReverse.Count; i++)
            {
                rowsReverse[i][MainWindow.columnName[0]] = rowsDt[k][MainWindow.columnName[0]];
                rowsReverse[i][MainWindow.columnName[1]] = rowsDt[0][l];
                rowsReverse[i][MainWindow.columnName[2]] = rowsDt[k][l];
                k++;
                if (k == rowsDt.Count)
                {
                    k = 1;
                    if (l < columnsDt.Count - 1)
                    {
                        l++;
                    }
                }
            }
            InterfaceDataGrid.BindingDataGridByName(DataGrid, DtReverse, true, true);
        }
    }
}
