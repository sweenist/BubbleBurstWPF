using System;
using System.Windows;
using System.Windows.Controls;
using BubbleBurst.ViewModel;

namespace BubbleBurst.View
{
    /// <summary>
    /// A Canvas panel that arranges bubbles into a matrix layout.
    /// </summary>
    public class BubbleCanvas : Canvas
    {
        #region Constructor

        public BubbleCanvas()
        {
        }

        #endregion // Constructor

        #region Properties

        internal static int BubbleSize
        {
            get { return 42; }
        }

        internal int ColumnCount
        {
            get { return (int)Math.Floor(base.ActualWidth / BubbleSize); }
        }

        internal int RowCount
        {
            get { return (int)Math.Floor(base.ActualHeight / BubbleSize); }
        }

        #endregion // Properties

        #region Methods

        #region Internal

        internal double CalculateLeft(FrameworkElement bubbleContainer)
        {
            if (bubbleContainer == null)
                throw new ArgumentNullException("bubbleContainer");

            var bubble = bubbleContainer.DataContext as BubbleViewModel;
            if (bubble == null)
                throw new ArgumentException("Element does not have a BubbleViewModel as its DataContext.", "bubbleContainer");

            return this.CalculateLeft(bubble.Column);
        }

        internal double CalculateTop(FrameworkElement bubbleContainer)
        {
            if (bubbleContainer == null)
                throw new ArgumentNullException("bubbleContainer");

            var bubble = bubbleContainer.DataContext as BubbleViewModel;
            if (bubble == null)
                throw new ArgumentException("Element does not have a BubbleViewModel as its DataContext.", "bubbleContainer");

            return this.CalculateTop(bubble.Row);
        }

        #endregion // Internal

        #region Protected

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            var contentPresenter = visualAdded as ContentPresenter;
            if (contentPresenter != null)
            {
                var bubble = contentPresenter.DataContext as BubbleViewModel;
                if (bubble != null)
                {
                    Canvas.SetLeft(contentPresenter, CalculateLeft(bubble.Column));
                    Canvas.SetTop(contentPresenter, CalculateTop(bubble.Row));

                    contentPresenter.Width = BubbleSize;
                    contentPresenter.Height = BubbleSize;
                }
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        #endregion // Protected

        #region Private

        double CalculateLeft(int column)
        {
            double bubblesWidth = BubbleSize * this.ColumnCount;
            double horizOffset = (base.ActualWidth - bubblesWidth) / 2;
            return column * BubbleSize + horizOffset;
        }

        double CalculateTop(int row)
        {
            double bubblesHeight = BubbleSize * this.RowCount;
            double vertOffset = (base.ActualHeight - bubblesHeight) / 2;
            return row * BubbleSize + vertOffset;
        }

        #endregion // Private

        #endregion // Methods
    }
}