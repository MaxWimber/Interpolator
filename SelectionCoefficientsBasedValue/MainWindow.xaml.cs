﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SelectionCoefficientsBasedValue
{
    /// <summary>
    /// Программа просчитывает через DataTable значения по формуле и записывает его в другую таблицу. А! еще выводит красивые графики
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InputData inputData = new InputData();
        private Brush brush2 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 43, 53, 65)); // Цвет возврата после креста

        public DataTable Dt =                       new DataTable();
        public DataTable DtForKandAFromAngle =      new DataTable();
        public DataTable DtSearchable =             new DataTable();
        public DataTable DataTableNewValue =        new DataTable();
        public DataTable DataTableNewValueFirst =   new DataTable();
        public DataTable DtRevers =                 new DataTable();
        public DataTable DtReversNewValue =         new DataTable();

        public static List<string> columnName = new List<string>() { "DN", "GN", "LZ", "angle1" };

        public List<string> columnsForKandAFromAngle = new List<string>() { "angle", "k", "a" };
        public List<string> columnsSearchable = new List<string>() { "isk", "isa", "M", "N", "B", "C" };
        public List<string> columnsRevers = new List<string>() { columnName[0], columnName[1], columnName[2] };


        private bool isCalculate = false;
        private int indexSelectedColumn = 0; 
        private int displayIndexSelectedColumn = 0;

        public MainWindow()
        {
            InitializeComponent();

            DefaultData(Dt, columnName[0]);
            DefaultData(Dt, columnName[1]);
            AddDefaultDataGrid(DataGrid, Dt, columnName[0], 0);
            AddDefaultDataGrid(DataGrid, Dt, columnName[1], 1);


            DefaultData(DataTableNewValue, columnName[0]);
            DefaultData(DataTableNewValue, columnName[1]);
            AddDefaultDataGrid(DataGridSearchedValues, DataTableNewValue, columnName[0], 0);
            AddDefaultDataGrid(DataGridSearchedValues, DataTableNewValue, columnName[1], 1);

            AddColumns(DtForKandAFromAngle, columnsForKandAFromAngle);
            DefaultData(DtForKandAFromAngle, columnName[0]);
            DefaultData(DtForKandAFromAngle, columnName[1]);

            AddColumns(DtSearchable, columnsSearchable);
            DefaultData(DtSearchable, columnName[0]);
            DefaultData(DtSearchable, columnName[1]);

            //AddColumns(DtRevers, columnsRevers);
            //AddColumns(DtReversNewValue, columnsRevers);
            for (int i = 0; i < 3; i++)
            {
                DefaultData(DtRevers, columnName[i]);
                DefaultData(DtReversNewValue, columnName[i]);
                AddDefaultDataGrid(DataGridReverse, DtRevers, columnName[i], i);
                AddDefaultDataGrid(DataGridSearchedValuesRevers, DtReversNewValue, columnName[i], i);
            }
            
            

            DefaultData(DataTableNewValueFirst, columnName[0]);
            DefaultData(DataTableNewValueFirst, columnName[1]);
            ButtonColumn.DisplayIndex = DataGrid.Columns.Count-1;

        }
        /// <summary>
        /// Добавление колонок по умолчанию
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
        /// Добавление значений по умолчанию
        /// </summary>
        /// <param name="dataTable">Таблица которой указывается значения по умолчанию</param>
        private void DefaultData(DataTable dataTable, string columnName)
        {
            InterfaceDataGrid.iforAngel = 1;
            dataTable.Columns.Add(columnName);
            if (dataTable.Rows.Count == 0)
            {
                dataTable.Rows.Add();
                dataTable.Rows[0][columnName] = "";
            }
        }
        /// <summary>
        /// Добавление колонок DataGrid по умолчанию
        /// </summary>
        /// <param name="DataGrid">DataGrid для которого создается колонки</param>
        /// <param name="dataTable">таблица с которой связывается dataGrid</param>
        private void AddDefaultDataGrid(DataGrid DataGrid, DataTable dataTable, string columnName, int displayIndex)
        {
            var newDataGridTextColumn = new DataGridTextColumn
            {
                Header = columnName,
                DisplayIndex = displayIndex,
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
        /// <param name="DataGrid">datagrid с которого берутся данные о выбранной ячейки</param>
        private void PaintGraphs(DataGrid DataGrid)
        {
            DataGridCellInfo currentCell = DataGrid.CurrentCell;
            if (currentCell.Column != null && currentCell.Column.Header.ToString() != "btn")
            {
                int columnIndex = currentCell.Column.DisplayIndex;
                if(columnIndex <= (Dt.Columns.Count - 1) && columnIndex >= 0)
                {
                    if (Dt.Rows.Count == 0) { Dt.Rows.Add(); }
                    
                    string nameGraph = inputData.GetFirst(Dt, columnIndex); 
                    PlotViewer.Model = ChartCoordinatesWindow.MainViewModel(Dt, DataTableNewValueFirst, 
                            DataTableNewValue, columnName[0], columnIndex, 
                             $"График искомых значений {columnName[1]}: {nameGraph}"  );
                }
                if (columnIndex - 1 >= 0)
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

                if (DataGrid.SelectedCells.Count > 1)
                {
                    int indexSelectedRow = DataGrid.SelectedIndex;
                    if (columnIndex > 1 && columnIndex < DataGrid.Columns.Count)
                    {
                        if (columnDisplayIndex > 1 && columnDisplayIndex < Dt.Columns.Count
                            && DataGrid.Columns[columnIndex].Header.ToString() != "btn")
                        {
                            Dt.Columns.RemoveAt(columnDisplayIndex);
                            DataGrid.Columns.RemoveAt(columnIndex);
                        }
                    }
                    if (indexSelectedRow != -1)
                    {

                        if (DataGrid.SelectedCells.Count >= Dt.Columns.Count + Dt.Rows.Count)
                        {
                            while (DataGrid.Columns.Count != 3)
                            {
                                int indexDt = Dt.Columns.Count - 1;
                                int indexDG = DataGrid.Columns.Count - 1;
                                Dt.Columns.RemoveAt(indexDt);
                                DataGrid.Columns.RemoveAt(indexDG);
                            }
                            while (Dt.Rows.Count != 1)
                            {
                                int indexRow = Dt.Rows.Count - 1;
                                Dt.Rows.RemoveAt(indexRow);
                            }
                            Dt.Rows[0][0] = "";
                            Dt.Rows[0][1] = "";
                        }
                        else
                        {
                            Dt.Rows.RemoveAt(indexSelectedRow);
                        }
                    }
                    
                }
                else if (DataGrid.SelectedCells.Count == 1)
                {
                    DataGridCellInfo currentCell = DataGrid.CurrentCell;
                    int columnIndexOne = currentCell.Column.DisplayIndex;
                    int currentRowIndex = DataGrid.Items.IndexOf(DataGrid.CurrentItem);

                    if (currentRowIndex < 0 || currentRowIndex >= Dt.Rows.Count || columnIndexOne >= Dt.Columns.Count)
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
        #region Кнопки закрытия и перетаскивания
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
       
        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainGrides.Margin = new Thickness(6);
                Border_maximized.Content = "🗗";
            }
            else
            {
                MainGrides.Margin = new Thickness(0);
                Border_maximized.Content = "🗖";
            }
        }
        #region Кнопки закрытия и перетаскивания
        private void Border_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    DragMove();
                }

            }
        }


        #endregion

        #region максимайз
        private void Border_maximized_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        #endregion


        private void DataGridReverse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                DataGridCellInfo currentCell = DataGridReverse.CurrentCell;
                int currentRowIndex = DataGridReverse.Items.IndexOf(DataGridReverse.CurrentItem);
                int columnIndex = currentCell.Column.DisplayIndex;
                
                inputData.InstanceForRevers(DataGridReverse, DtRevers, currentRowIndex, columnIndex);

                InterfaceDataGrid.BindingDataGridByName(DataGrid, Dt, true, true);

                inputData.DeReverse(DtRevers, Dt, DataGrid);
                ButtonColumn.DisplayIndex = DataGrid.Columns.Count - 1;

                isCalculate = false;
                

            }
        }

        private void DataGridReverse_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Delete == e.Key)
            {
                if (DtRevers.Rows != null)
                {
                    return;
                }

                DtRevers.Rows.Add("");
            }
        }
    }
}
