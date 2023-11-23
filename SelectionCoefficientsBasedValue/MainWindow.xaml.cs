using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace SelectionCoefficientsBasedValue
{
    /// <summary>
    /// Программа просчитывает через DataTable значения по формуле и записывает его в другую таблицу. А! еще выводит красивые графики
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InputData inputData = new InputData();

        public DataTable Dt =                       new DataTable();
        public DataTable DtForKandAFromAngle =      new DataTable();
        public DataTable DtSearchable =             new DataTable();
        public DataTable DataTableNewValue =        new DataTable();
        public DataTable DataTableNewValueFirst =   new DataTable();
        public DataTable DtRevers =                 new DataTable();
        public DataTable DtReversNewValue =         new DataTable();

        public static List<string> columnName = new List<string>() { "DN", "GN", "LZ" };

        public List<string> columnsForKandAFromAngle = new List<string>() { "angle", "k", "a" };
        public List<string> columnsSearchable = new List<string>() { "isk", "isa", "M", "N", "B", "C" };
        public List<string> columnsRevers = new List<string>() { columnName[0], columnName[1], columnName[2] };


        private bool isCalculate = false;
        private int indexSelectedColumn = 0; 
        private int displayIndexSelectedColumn = 0;

        public MainWindow()
        {
            InitializeComponent();

            DefaultData(Dt);
            AddDefaultDataGrid(DataGrid, Dt);

            DefaultData(DataTableNewValue);
            AddDefaultDataGrid(DataGridSearchedValues, DataTableNewValue);

            AddColumns(DtForKandAFromAngle, columnsForKandAFromAngle);
            DefaultData(DtForKandAFromAngle);

            AddColumns(DtSearchable, columnsSearchable);
            DefaultData(DtSearchable);

            AddColumns(DtRevers, columnsRevers);
            AddColumns(DtReversNewValue, columnsRevers);

            DefaultData(DataTableNewValueFirst);
            ButtonColumn.DisplayIndex = DataGrid.Columns.Count-1;

        }
        /// <summary>
        /// Добавление колонок поумолчанию
        /// </summary>
        /// <param name="dataTable">Таблица которой нужно добавить колонки</param>
        /// <param name="columns">Список имен колонок</param>
        private void AddColumns(DataTable dataTable, List<string> columns)
        {
            foreach(string column in columns)
            {
                dataTable.Columns.Add(column);
            }
        }
        /// <summary>
        /// Добавление значений поумолчанию
        /// </summary>
        /// <param name="dataTable">Таблица которой указывается значения поумолчанию</param>
        private void DefaultData(DataTable dataTable)
        {
            InterfaceDataGrid.iforAngel = 0;
            dataTable.Columns.Add(columnName[0]);
            if (dataTable.Rows.Count == 0)
            {
                dataTable.Rows.Add();
                dataTable.Rows[0][columnName[0]] = "";
            }
        }
        /// <summary>
        /// Добавление колонок DataGrid по умолчанию
        /// </summary>
        /// <param name="DataGrid">DataGrid для которого создается колонки</param>
        /// <param name="dataTable">таблица с которой связывается dataGrid</param>
        private void AddDefaultDataGrid(DataGrid DataGrid, DataTable dataTable)
        {
            var newDataGridTextColumn = new DataGridTextColumn
            {
                Header = columnName[0],
                DisplayIndex = 0,
                MinWidth = 50
            };

            DataGrid.Columns.Add(newDataGridTextColumn);

            InterfaceDataGrid.BindingDataGridByName(DataGrid, dataTable, true, true);
        }
        /// <summary>
        /// Метод, который расчитывает все значения по формулам и добавляет их в таблицы данных
        /// </summary>
        private void Calculate()
        {
            for (int j = 0; j < DtForKandAFromAngle.Columns.Count; j++)
            {
                for (int i = 0; i < DtForKandAFromAngle.Rows.Count; i++)
                {
                    DtForKandAFromAngle.Rows[i][j] = "";
                }
            }
            for (int j = 0; j < DtSearchable.Columns.Count; j++)
            {
                for (int i = 0; i < DtSearchable.Rows.Count; i++)
                {
                    DtSearchable.Rows[i][j] = "";
                }
            }

            for (int i = 1; i < Dt.Columns.Count; i++)
            {
                inputData.FillingEmptyRows(Dt, DtForKandAFromAngle, i);
                inputData.Searchables(Dt, DataTableNewValueFirst, i);
            }
            
            inputData.HardingAngle(DtForKandAFromAngle, DtSearchable, Dt, DataTableNewValue, DataGridSearchedValues);
            inputData.Reverse(Dt, DtRevers, DataGridReverse);
            inputData.Reverse(DataTableNewValue, DtReversNewValue, DataGridSearchedValuesRevers);
            
        }
        /// <summary>
        /// Метод вывода графиков и формул
        /// </summary>
        /// <param name="DataGrid">datagrid с которого бирутся данные о выбраной ячейки</param>
        private void PaintGraphs(DataGrid DataGrid)
        {
            DataGridCellInfo currentCell = DataGrid.CurrentCell;
            if (currentCell.Column != null && currentCell.Column.Header.ToString() != "btn")
            {
                int columIndex = currentCell.Column.DisplayIndex;
                if(columIndex <= (Dt.Columns.Count - 1) && columIndex >= 0)
                {
                    if (Dt.Rows.Count == 0) { Dt.Rows.Add(); }
                    
                    string nameGraph = inputData.GetFirst(Dt, columIndex); 
                    PlotViewer.Model = ChartCoordinatesWindow.MainViewModel(Dt, DataTableNewValueFirst, 
                            DataTableNewValue, columnName[0], columIndex, 
                             $"График искомых значений {columnName[1]}: {nameGraph}"  );
                }
                if (columIndex - 1 >= 0)
                {
                    return;
                }
            }
            if (Dt.Columns.Count == 2)
            {
                if (DtForKandAFromAngle.Rows[0]["k"].ToString() != "" && DtForKandAFromAngle.Rows[0]["a"].ToString() != "")
                {
                    textBoxFormula.Text = $"{columnName[0]} * " +
                                          $"{InputData.Converter(DtForKandAFromAngle.Rows[0]["k"].ToString()): 0.000; (-0.000)}" +
                                          $"{InputData.Converter(DtForKandAFromAngle.Rows[0]["a"].ToString()): + 0.000; - 0.000}" ;
                    return;
                }

            }
            else if (Dt.Columns.Count > 2)
            {
                textBoxFormula.Text = $"{columnName[0]} * " +
                                      $"({columnName[1]} * {InputData.Converter(DtSearchable.Rows[0]["M"].ToString()): 0.000; (-0.000)}" +
                                      $"{InputData.Converter(DtSearchable.Rows[0]["N"].ToString()): + 0.000; - 0.000}) + " +
                                      $"({columnName[1]} * {InputData.Converter(DtSearchable.Rows[0]["B"].ToString()): 0.000; (-0.000)}" +
                                      $"{InputData.Converter(DtSearchable.Rows[0]["C"].ToString()): + 0.000; - 0.000})";
                              
            }
        }
        /// <summary>
        /// Клик кнопки "Построить и расчитать"
        /// </summary>
        /// <param name="sender">кнопка</param>
        /// <param name="e">RoutedEventArgs</param>
        private void BuildAndCalculate_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
            PaintGraphs(DataGrid);
            isCalculate = true;

            if (DtForKandAFromAngle.Rows != null && DtSearchable.Rows != null) 
            {
                return; 
            }
        
        }
        /// <summary>
        /// Событие выбраной ячейки. При выборе любой ячейки, рисуется график если кнопка просчета была нажата
        /// </summary>
        /// <param name="sender">ячейка</param>
        /// <param name="e">RoutedEventArgs</param>
        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (isCalculate)
            {
                PaintGraphs(DataGrid);
            }
        }
        /// <summary>
        /// Событие клик зеленой кнопки, которое добавляет новую колонку
        /// </summary>
        /// <param name="sender">кнопка</param>
        /// <param name="e">RoutedEventArgs</param>
        private void PlussColumn_Click(object sender, RoutedEventArgs e)
        {
            InterfaceDataGrid.iforAngel++;
            InterfaceDataGrid.CreateDataGridColumns(DataGrid, $"angle{InterfaceDataGrid.iforAngel}", 
                                                    typeof(TextBlock), $"angle{InterfaceDataGrid.iforAngel}", 40.0);

            ButtonColumn.DisplayIndex = DataGrid.Columns.Count - 1;

            Dt.Columns.Add($"angle{InterfaceDataGrid.iforAngel}");


            if (Dt.Rows.Count == 0)
            {
                Dt.Rows.Add();
                Dt.Rows[0][$"angle{InterfaceDataGrid.iforAngel}"] = "";
            }
            InterfaceDataGrid.BindingDataGridByName(DataGrid, Dt, true, false);

        }
        /// <summary>
        /// Событие DataGrid исходных значений, при нажатии комбинации кнопок Ctrl+V, таблица Dt заполняется
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">KeyEventArgs</param>
        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                DataGridCellInfo currentCell = DataGrid.CurrentCell;
                int currentRowIndex = DataGrid.Items.IndexOf(DataGrid.CurrentItem);
                int columnIndex = currentCell.Column.DisplayIndex;
                if (columnIndex != ButtonColumn.DisplayIndex)
                {
                    inputData.Instance(DataGrid, Dt, currentRowIndex, columnIndex);

                    InterfaceDataGrid.BindingDataGridByName(DataGrid, Dt, true, false);

                    ButtonColumn.DisplayIndex = DataGrid.Columns.Count - 1;
                    isCalculate = false;
                }
                
            }
        }
        /// <summary>
        /// Событие клик кнопки "Вставить"
        /// </summary>
        /// <param name="sender">кнопка</param>
        /// <param name="e">RoutedEventArgs</param>
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            inputData.Instance(DataGrid, Dt, 0, 0);

            InterfaceDataGrid.BindingDataGridByName(DataGrid, Dt, true, false);

            ButtonColumn.DisplayIndex = DataGrid.Columns.Count - 1;
            isCalculate = false;
        }
        /// <summary>
        /// создание графика
        /// </summary>
        /// <param name="sender">ячейка</param>
        /// <param name="e">EventArgs</param>
        private void DataGridSearchedValues_CurrentCellChanged(object sender, EventArgs e)
        {
            if (isCalculate)
            {
                PaintGraphs(DataGridSearchedValues);
            }
        }
        /// <summary>
        /// Событие клика по header колонки
        /// </summary>
        /// <param name="sender">Heder по которому вы нажали</param>
        /// <param name="e">RoutedEventArgs</param>
        private void ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            DataGridColumnHeader columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                DataGrid.SelectedCells?.Clear();
                foreach (var item in DataGrid.Items)
                {
                    DataGrid.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
                    displayIndexSelectedColumn = columnHeader.Column.DisplayIndex;
                    indexSelectedColumn = DataGrid.Columns.IndexOf(columnHeader.Column);
                }
                DataGrid.Focus();
            }
        }
        /// <summary>
        /// Событие клика по header строки
        /// </summary>
        /// <param name="sender">Heder по которому вы нажали</param>
        /// <param name="e">RoutedEventArgs</param>
        private void RowHeaderClick(object sender, RoutedEventArgs e)
        {
            displayIndexSelectedColumn = -1;
            indexSelectedColumn = -1;
        }
        /// <summary>
        /// Событие нажатия кнопки delete
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">KeyEventArgs</param>
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {       
            if (Key.Delete == e.Key)
            {
                int columnDisplayIndex = displayIndexSelectedColumn;
                int columnIndex = indexSelectedColumn;
                
                if(DataGrid.SelectedCells.Count > 1)
                {
                    if (columnIndex > 0 && columnIndex < DataGrid.Columns.Count)
                    {
                        if (columnDisplayIndex > 0 && columnDisplayIndex < Dt.Columns.Count
                            && DataGrid.Columns[columnIndex].Header.ToString() != "btn")
                        {
                            Dt.Columns.RemoveAt(columnDisplayIndex);
                            DataGrid.Columns.RemoveAt(columnIndex);
                        }
                    } 
                }
                else if (DataGrid.SelectedCells.Count == 1)
                {
                    DataGridCellInfo currentCell = DataGrid.CurrentCell;
                    int columnIndexOne = currentCell.Column.DisplayIndex;
                    int currentRowIndex = DataGrid.Items.IndexOf(DataGrid.CurrentItem);

                    if(currentRowIndex < 0 || currentRowIndex >= Dt.Rows.Count || columnIndexOne >= Dt.Columns.Count)
                    {
                        return;
                    }
                    if (Dt.Rows[currentRowIndex][columnIndexOne] == null)
                    {
                        return;
                    }

                    Dt.Rows[currentRowIndex][columnIndexOne] = "";
                }
            }
        }
    }
}
