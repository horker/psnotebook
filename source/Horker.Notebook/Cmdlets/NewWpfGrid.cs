using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("New", "WpfGrid")]
    [OutputType(typeof(Grid))]
    public class NewWpfGrid : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipeline = true)]
        public UIElement InputObject;

        [Parameter(Position = 1, Mandatory = false)]
        public string[] Columns;

        [Parameter(Position = 2, Mandatory = false)]
        public string[] Rows;

        [Parameter(Position = 3, Mandatory = false)]
        public int ColumnCount;

        [Parameter(Position = 4, Mandatory = false)]
        public int RowCount;

        [Parameter(Position = 5, Mandatory = false)]
        public double Width = double.NaN;

        [Parameter(Position = 6, Mandatory = false)]
        public double Height = double.NaN;

        private List<UIElement> _children;

        private GridLength GetGridLength(string obj)
        {
            var converter = new GridLengthConverter();
            return (GridLength)converter.ConvertFromString(obj);
        }

        private IEnumerable<GridLength> GetOneGridLengths(int count)
        {
            for (var i = 0; i < count; ++i)
                yield return new GridLength(1.0, GridUnitType.Star);
        }

        protected override void BeginProcessing()
        {
            _children = new List<UIElement>();
        }

        protected override void ProcessRecord()
        {
            if (InputObject == null)
                return;

            _children.Add(InputObject);
        }

        protected override void EndProcessing()
        {
            List<GridLength> widths = null;
            List<GridLength> heights = null;

            if (MyInvocation.BoundParameters.ContainsKey("Columns"))
                widths = Columns.Select(x => GetGridLength(x)).ToList();

            if (MyInvocation.BoundParameters.ContainsKey("Rows"))
                heights = Rows.Select(x => GetGridLength(x)).ToList();

            if (MyInvocation.BoundParameters.ContainsKey("ColumnCount"))
                widths = GetOneGridLengths(ColumnCount).ToList();

            if (MyInvocation.BoundParameters.ContainsKey("RowCount"))
                heights = GetOneGridLengths(RowCount).ToList();

            // Fill widths and heights when they are not specified.

            if (widths == null || widths.Count == 0)
            {
                if (heights == null || heights.Count == 0)
                {
                    var wc = (int)Math.Ceiling(Math.Sqrt(_children.Count));
                    var hc = (int)Math.Ceiling((double)_children.Count / wc);

                    widths = GetOneGridLengths(wc).ToList();
                    heights = GetOneGridLengths(hc).ToList();
                }
                else
                {
                    var wc = (int)Math.Ceiling((double)_children.Count / heights.Count);
                    widths = GetOneGridLengths(wc).ToList();
                }
            }
            else
            {
                if (heights == null || heights.Count == 0)
                {
                    var hc = (int)Math.Ceiling((double)_children.Count / widths.Count);
                    heights = GetOneGridLengths(hc).ToList();
                }
            }

            // Create a Grid object.

            Grid grid = null;

            Models.ApplicationInstance.Dispatcher.Invoke(() => {
                grid = new Grid()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                if (MyInvocation.BoundParameters.ContainsKey("Width"))
                    grid.Width = Width;

                if (MyInvocation.BoundParameters.ContainsKey("Height"))
                    grid.Height = Height;

                for (var i = 0; i < heights.Count; ++i)
                {
                    var rowDef = new RowDefinition() { Height = heights[i] };
                    grid.RowDefinitions.Add(rowDef);
                }

                for (var i = 0; i < widths.Count; ++i)
                {
                    var columnDef = new ColumnDefinition() { Width = widths[i] };
                    grid.ColumnDefinitions.Add(columnDef);
                }

                var count = 0;
                var exit = false;
                for (var h = 0; h < heights.Count; ++h)
                {
                    if (exit)
                        break;
                    for (var w = 0; w < widths.Count; ++w)
                    {
                        if (count >= _children.Count)
                        {
                            exit = true;
                            break;
                        }

                        var child = _children[count++];
                        grid.Children.Add(child);
                        Grid.SetRow(child, h);
                        Grid.SetColumn(child, w);
                    }
                }
            });

            WriteObject(grid);
        }
    }
}
