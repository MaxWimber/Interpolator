using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace SelectionCoefficientsBasedValue
{
    public class InterfaceDataGrid
    {
        public static int iforAngel = 0;


        /// <summary>
        /// Создание колонки DataGrid
        /// </summary>
        /// <param name="DataGrid">DataGrid для которого создается колонка</param>
        /// <param name="columnName">Имя header колонки</param>
        /// <param name="typeChildElements">Тип элемента который будет храниться в header колонки</param>
        /// <param name="nameChildElements">Имя элемента который будет храниться в header колонки</param>
        /// <param name="widthChildElements">Размер элемента который будет храниться в header колонки</param>
        public static void CreateDataGridColumns(DataGrid DataGrid, string columnName, Type typeChildElements, 
                                                 string nameChildElements, double widthChildElements)
        {
            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.AppendChild(TextBoxTemplate(typeChildElements, nameChildElements, widthChildElements));
            // Создание нового столбца
            var newDataGridTextColumn = new DataGridTextColumn
            {
                Header = columnName,
                DisplayIndex = DataGrid.Columns.Count,
                HeaderTemplate = new DataTemplate
                {
                    VisualTree = stackPanel
                },
                MinWidth = 50
            };
            // Добавление столбца в DataGrid
            DataGrid.Columns.Add(newDataGridTextColumn);
        }


        /// <summary>
        /// Создание элемента для stackPanel, который будет содержаться в header колонки
        /// </summary>
        /// <param name="typeCreateObjects">Тип элемента который будет храниться в header колонки</param>
        /// <param name="name">Имя элемента который будет храниться в header колонки</param>
        /// <param name="width">Размер элемента который будет храниться в header колонки</param>
        /// <returns>Возвращает созданный элемент</returns>
        public static FrameworkElementFactory TextBoxTemplate(Type typeCreateObjects, string name, double width)
        {
            var element = new FrameworkElementFactory(typeCreateObjects);
            element.SetValue(FrameworkElement.WidthProperty, width);
            element.SetValue(FrameworkElement.NameProperty, name);
            return element;
        }


        /// <summary>
        /// Привязка DataTeble с DataGrid
        /// </summary>
        /// <param name="DataGrid">DataGrid с которой будет связываться dataTable</param>
        /// <param name="dataTable">dataTable с которой будет связываться DataGrid</param>
        /// <param name="bindingName">Если true, то связывание будет по имени. Если false, то связывание будет по индексу</param>
        /// <param name="dataTableDefaultView">Будет ли использоваться defaulView</param>
        public static void BindingDataGridByName(DataGrid DataGrid, DataTable dataTable,    
                                                 bool bindingName, bool dataTableDefaultView)
        {
            if (bindingName)
            {
                if (dataTableDefaultView) 
                { 
                    DataGrid.ItemsSource = dataTable.DefaultView; 
                }

                for (int i = 0; i < DataGrid.Columns.Count; i++)
                {
                    string ju = ((DataGridTextColumn)DataGrid.Columns[i]).Header.ToString();
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        if (ju != dataTable.Columns[j].ColumnName) 
                        { 
                            continue; 
                        }
                        ((DataGridTextColumn)DataGrid.Columns[i]).Binding = new Binding(dataTable.Columns[j].ColumnName);
                        
                    }
                }
            }
            else
            {
                DataGrid.ItemsSource = dataTable.DefaultView;
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (((DataGridTextColumn)DataGrid.Columns[j]).Header.ToString() == "btn") 
                    { 
                        continue;
                    }
                    ((DataGridTextColumn)DataGrid.Columns[j]).Binding = new Binding(dataTable.Columns[j].ColumnName);
                }
            }
            
        }
    }
}
