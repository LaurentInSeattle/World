namespace Lyt.CoreMvvm
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    public static class GridAnimations
    {
        public static void AnimateRow(this Grid grid, int rowIndex, double to, int milliseconds)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            var rowDefinitions = grid.RowDefinitions;
            if ((rowIndex < 0) || (rowIndex > rowDefinitions.Count - 1))
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            var gridRow = rowDefinitions[rowIndex];
            var animation = new GridLengthAnimation(gridRow.Height, to, milliseconds);
            gridRow.BeginAnimation(RowDefinition.HeightProperty, animation, HandoffBehavior.Compose);
        }

        public static void AnimateColumn(this Grid grid, int columnIndex, double to, int milliseconds)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            var columnDefinitions = grid.ColumnDefinitions;
            if ((columnIndex < 0) || (columnIndex > columnDefinitions.Count - 1))
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            var gridColumn = columnDefinitions[columnIndex];
            var animation = new GridLengthAnimation(gridColumn.Width, to, milliseconds);
            gridColumn.BeginAnimation(ColumnDefinition.WidthProperty, animation, HandoffBehavior.Compose);
        }
    }
}
