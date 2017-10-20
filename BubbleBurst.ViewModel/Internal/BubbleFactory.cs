using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace BubbleBurst.ViewModel.Internal
{
    /// <summary>Creates BubbleViewModel objects and adds them to the bubble matrix.</summary>
    internal class BubbleFactory
    {
        private readonly BubbleMatrixViewModel _bubbleMatrix;
        private readonly List<BubbleViewModel> _bubbleStagingArea;
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private readonly DispatcherTimer _timer;

        /// <summary>Initializes a new instance of the <see cref="BubbleFactory"/> class.</summary>
        /// <param name="bubbleMatrix">The bubble matrix.</param>
        /// <exception cref="System.ArgumentNullException">bubbleMatrix</exception>
        internal BubbleFactory(BubbleMatrixViewModel bubbleMatrix)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            _bubbleMatrix = bubbleMatrix;

            _bubbleStagingArea = new List<BubbleViewModel>();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            _timer.Tick += HandleTimerTick;
        }

        /// <summary>Populates the bubble matrix with new bubbles over time.</summary>
        internal void CreateBubblesAsync()
        {
            _timer.Stop();

            _bubbleStagingArea.Clear();
            _bubbleStagingArea.AddRange(
                from row in Enumerable.Range(0, _bubbleMatrix.RowCount)
                from col in Enumerable.Range(0, _bubbleMatrix.ColumnCount)
                select new BubbleViewModel(_bubbleMatrix, row, col));

            _bubbleMatrix.IsIdle = false;

            _timer.Start();
        }

        private void HandleTimerTick(object sender, EventArgs e)
        {
            if (!_timer.IsEnabled)
                return;

            for (int i = 0; i < 4 && _bubbleStagingArea.Any(); ++i)
            {
                // Get a random bubble from the staging area.
                int index = _random.Next(0, _bubbleStagingArea.Count);
                var bubble = _bubbleStagingArea[index];
                _bubbleStagingArea.RemoveAt(index);

                // Add the bubble to the bubble matrix.
                _bubbleMatrix.AddBubble(bubble);

                if (!_bubbleStagingArea.Any())
                {
                    _timer.Stop();
                    _bubbleMatrix.IsIdle = true;
                }
            }
        }
    }
}