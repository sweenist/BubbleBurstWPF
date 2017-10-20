using System;
using System.Windows;
using System.Windows.Controls;
using BubbleBurst.ViewModel;

namespace BubbleBurst.View
{
    /// <summary>A Canvas panel that arranges bubbles into a matrix layout.</summary>
    /// <seealso cref="System.Windows.Controls.Canvas" />
    public class BubbleCanvas : Canvas
    {
        /// <summary>Gets the size of the bubble.</summary>
        internal static int BubbleSize => 42;

        /// <summary>Gets the column count.</summary>
        internal int ColumnCount => (int)Math.Floor(ActualWidth / BubbleSize);

        /// <summary>Gets the row count.</summary>
        internal int RowCount => (int)Math.Floor(ActualHeight / BubbleSize);

        #region Methods

        /// <summary>Calculates the left point of a bubble.</summary>
        /// <param name="bubbleContainer">The bubble container.</param>
        /// <returns>Left point of a bubble's column.</returns>
        /// <exception cref="System.ArgumentNullException">bubbleContainer</exception>
        /// <exception cref="System.ArgumentException">Element does not have a BubbleViewModel as its DataContext.;bubbleContainer</exception>
        internal double CalculateLeft(FrameworkElement bubbleContainer)
        {
            if (bubbleContainer == null)
                throw new ArgumentNullException("bubbleContainer");

            var bubble = bubbleContainer.DataContext as BubbleViewModel;
            if (bubble == null)
                throw new ArgumentException("Element does not have a BubbleViewModel as its DataContext.", "bubbleContainer");

            return CalculateLeft(bubble.Column);
        }

        /// <summary>Calculates the top point of a bubble.</summary>
        /// <param name="bubbleContainer">The bubble container.</param>
        /// <returns>Top point of a bubble's row.</returns>
        /// <exception cref="System.ArgumentNullException">bubbleContainer</exception>
        /// <exception cref="System.ArgumentException">Element does not have a BubbleViewModel as its DataContext.;bubbleContainer</exception>
        internal double CalculateTop(FrameworkElement bubbleContainer)
        {
            if (bubbleContainer == null)
                throw new ArgumentNullException("bubbleContainer");

            var bubble = bubbleContainer.DataContext as BubbleViewModel;
            if (bubble == null)
                throw new ArgumentException("Element does not have a BubbleViewModel as its DataContext.", "bubbleContainer");

            return CalculateTop(bubble.Row);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            var contentPresenter = visualAdded as ContentPresenter;
            if (contentPresenter != null)
            {
                var bubble = contentPresenter.DataContext as BubbleViewModel;
                if (bubble != null)
                {
                    SetLeft(contentPresenter, CalculateLeft(bubble.Column));
                    SetTop(contentPresenter, CalculateTop(bubble.Row));

                    contentPresenter.Width = BubbleSize;
                    contentPresenter.Height = BubbleSize;
                }
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private double CalculateLeft(int column)
        {
            double bubblesWidth = BubbleSize * ColumnCount;
            var horizOffset = (ActualWidth - bubblesWidth) / 2L;

            return column * BubbleSize + horizOffset;
        }

        private double CalculateTop(int row)
        {
            double bubblesHeight = BubbleSize * RowCount;
            var vertOffset = (ActualHeight - bubblesHeight) / 2L;

            return row * BubbleSize + vertOffset;
        }

        #endregion // Methods
    }
}