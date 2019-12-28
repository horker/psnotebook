using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("New", "WpfDataGrid")]
    [OutputType(typeof(DataGrid))]
    public class NewWpfDataGrid : PSCmdlet
    {
        private DataGrid _dataGrid;
        private ObservableCollection<List<object>> _data;
        private Dictionary<string, int> _indexMap;
        private Style _numericCellStyle;
        private Style _stringCellStyle;

        [Parameter(Position = 0, Mandatory = false)]
        public double Width = double.NaN;

        [Parameter(Position = 1, Mandatory = false)]
        public double Height = double.NaN;

        [Parameter(Position = 2, Mandatory = false)]
        public double MaxWidth = double.NaN;

        [Parameter(Position = 3, Mandatory = false)]
        public double MaxHeight = -1;

        [Parameter(Position = 4, Mandatory = false)]
        public string FloatingNumberFormat = null;

        [Parameter(Position = 5, Mandatory = true, ValueFromPipeline = true)]
        public PSObject InputObject;

        protected override void BeginProcessing()
        {
            Models.Application.Dispatcher.Invoke(() =>
            {
                _data = new ObservableCollection<List<object>>();
                _dataGrid = new DataGrid()
                {
                    AutoGenerateColumns = false,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    CanUserReorderColumns = true,
                    CanUserResizeColumns = true,
                    CanUserResizeRows = true,
                    CanUserSortColumns = true,
                    RowHeaderWidth = 0,
                    BorderThickness = new Thickness(0),
                    GridLinesVisibility = DataGridGridLinesVisibility.None,
                    AlternatingRowBackground = Brushes.WhiteSmoke
                };

                _numericCellStyle = new Style();
                _numericCellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Right));

                _stringCellStyle = new Style();
                _stringCellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Left));

                if (!double.IsNaN(Width)) _dataGrid.Width = Width;
                if (!double.IsNaN(Height)) _dataGrid.Height = Height;
                if (!double.IsNaN(MaxWidth)) _dataGrid.MaxWidth = MaxWidth;

                if (MaxHeight == -1)
                    _dataGrid.MaxHeight = Models.Application.Session.Configuration.MaxOutputHeight;
                else if (!double.IsNaN(MaxHeight))
                    _dataGrid.MaxHeight = MaxHeight;

                var style = new Style();
                style.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0, 0, 1, 0))); ;
                style.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, Brushes.LightGray));
                style.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Right));

                _dataGrid.Columns.Add(new DataGridTextColumn()
                {
                    DisplayIndex = 0,
                    Header = "#",
                    IsReadOnly = true,
                    Binding = new Binding("[0]"),
                    CellStyle = style
                });
            });

            _indexMap = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            _indexMap.Add("#", 0);

            WriteObject(_dataGrid);
        }

        private static bool isNumeric(object value)
        {
            return value is byte || value is sbyte || value is short || value is int || value is long ||
                value is float || value is double || value is decimal;
        }

        protected override void ProcessRecord()
        {
            if (InputObject == null)
                return;

            var row = new List<object>(Math.Max(_indexMap.Count, 3));

            row.Add(_data.Count);

            foreach (var p in InputObject.Properties)
            {
                var name = p.Name;
                var value = p.Value;
                if (!_indexMap.TryGetValue(p.Name, out var index))
                {
                    index = _indexMap.Count;
                    _indexMap.Add(name, index);

                    Models.Application.Dispatcher.Invoke(() =>
                    {
                        Style style;
                        if (isNumeric(value))
                            style = _numericCellStyle;
                        else
                            style = _stringCellStyle;

                        _dataGrid.Columns.Add(new DataGridTextColumn()
                        {
                            DisplayIndex = index,
                            Header = "_" + name,
                            IsReadOnly = true,
                            Binding = new Binding("[" + index + "]"),
                            CellStyle = style
                        });
                    });
                }

                while (index >= row.Count)
                    row.Add(null);

                if (FloatingNumberFormat != null)
                {
                    if (value is double d)
                        value = d.ToString(FloatingNumberFormat);
                    else if (value is float f)
                        value = f.ToString(FloatingNumberFormat);
                    else if (value is decimal dec)
                        value = dec.ToString(FloatingNumberFormat);
                }

                row[index] = value;
            }

            Models.Application.Dispatcher.Invoke(() =>
            {
                _data.Add(row);
                _dataGrid.ItemsSource = _data;
            });
        }

        protected override void EndProcessing()
        {
            _indexMap = null;
        }
    }
}
