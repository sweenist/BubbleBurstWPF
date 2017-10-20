using System;
using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel.Internal
{
    /// <summary>
    /// Creates BubblesTask objects.  This class is responsible
    /// for updating the bubble matrix after the user has burst
    /// a bubble group, or performed an Undo action.  The tasks 
    /// created by this class are processed by BubbleMatrixView 
    /// after being added to BubbleMatrixViewModel's TaskManager.
    /// </summary>
    internal class BubblesTaskFactory
    {
        private readonly BubbleMatrixViewModel _bubbleMatrix;

        /// <summary>Initializes a new instance of the <see cref="BubblesTaskFactory"/> class.</summary>
        /// <param name="bubbleMatrix">The bubble matrix.</param>
        /// <exception cref="System.ArgumentNullException">bubbleMatrix</exception>
        internal BubblesTaskFactory(BubbleMatrixViewModel bubbleMatrix)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            _bubbleMatrix = bubbleMatrix;
        }

        #region Methods

        #region Internal

        /// <summary>
        /// Creates a sequence of tasks that must be performed for the 
        /// specified collection of bubbles.
        /// </summary>
        /// <param name="bubblesInGroup">The bubbles for which tasks are created.</param>
        internal IEnumerable<BubblesTask> CreateTasks(BubbleViewModel[] bubblesInGroup)
        {
            var taskTypes = new BubblesTaskType[]
            {
                BubblesTaskType.Burst,
                BubblesTaskType.MoveDown,
                BubblesTaskType.MoveRight
            };

            // Dump the tasks into an array so that the query is not executed twice.
            return
                (from taskType in taskTypes
                 select CreateTask(taskType, bubblesInGroup))
                .ToArray();
        }

        /// <summary>
        /// Creates tasks used to undo the effects of the specified tasks.
        /// </summary>
        /// <param name="originalTasks">
        /// The tasks used to perform the bubble burst about to be undone.
        /// </param>
        internal IEnumerable<BubblesTask> CreateUndoTasks(IEnumerable<BubblesTask> originalTasks)
        {
            // Dump the tasks into an array so that the query is not executed twice.
            return
                (from originalTask in originalTasks.Reverse()
                 select CreateUndoTask(originalTask))
                .ToArray();
        }

        #endregion // Internal

        #region Private

        BubblesTask CreateUndoTask(BubblesTask originalTask)
        {
            Func<IEnumerable<BubbleViewModel>> getBubbles;
            Action complete;

            var bubbles = originalTask.Bubbles.ToList();

            switch (originalTask.TaskType)
            {
                case BubblesTaskType.MoveRight:
                    getBubbles = delegate
                    {
                        _bubbleMatrix.IsIdle = false;
                        _bubbleMatrix.ResetBubbleGroup();
                        bubbles.ForEach(b => b.BeginUndo());
                        return bubbles;
                    };
                    complete = delegate
                    {
                        bubbles.ForEach(b => b.EndUndo());
                    };
                    break;

                case BubblesTaskType.MoveDown:
                    getBubbles = delegate
                    {
                        bubbles.ForEach(b => b.BeginUndo());
                        return bubbles;
                    };
                    complete = delegate
                    {
                        bubbles.ForEach(b => b.EndUndo());
                    };
                    break;

                case BubblesTaskType.Burst:
                    getBubbles = delegate
                    {
                        bubbles.ForEach(b => _bubbleMatrix.AddBubble(b));
                        return bubbles;
                    };
                    complete = delegate
                    {
                        _bubbleMatrix.IsIdle = true;
                    };
                    break;

                default:
                    throw new ArgumentException("Unrecognized task type: " + originalTask.TaskType);
            }

            return new BubblesTask(originalTask.TaskType, true, getBubbles, complete);
        }

        BubblesTask CreateTask(BubblesTaskType taskType, BubbleViewModel[] bubblesInGroup)
        {
            Func<IEnumerable<BubbleViewModel>> getBubbles;
            Action complete;

            switch (taskType)
            {
                case BubblesTaskType.Burst:
                    getBubbles = delegate
                    {
                        _bubbleMatrix.IsIdle = false;
                        return bubblesInGroup;
                    };
                    complete = delegate
                    {
                        foreach (BubbleViewModel bubble in bubblesInGroup)
                        {
                            _bubbleMatrix.RemoveBubble(bubble);
                        }
                    };
                    break;

                case BubblesTaskType.MoveDown:
                    getBubbles = delegate
                    {
                        return MoveNeighboringBubblesDown(bubblesInGroup);
                    };
                    complete = delegate
                    {
                        /* Nothing to do here. */
                    };
                    break;

                case BubblesTaskType.MoveRight:
                    getBubbles = delegate
                    {
                        return MoveBubblesRight();
                    };
                    complete = delegate
                    {
                        _bubbleMatrix.IsIdle = true;
                        _bubbleMatrix.TryToEndGame();
                    };
                    break;

                default:
                    throw new ArgumentException("Unrecognized task type: " + taskType);
            }

            return new BubblesTask(taskType, false, getBubbles, complete);
        }

        IEnumerable<BubbleViewModel> MoveBubblesRight()
        {
            var movedBubbles = new List<BubbleViewModel>();

            for (var rowIndex = 0; rowIndex < _bubbleMatrix.RowCount; ++rowIndex)
            {
                var bubblesInRow =
                    _bubbleMatrix.Bubbles.Where(b => b.Row == rowIndex).ToArray();

                // Skip empty rows and full rows.
                if (bubblesInRow.Length == 0 ||
                    bubblesInRow.Length == _bubbleMatrix.ColumnCount)
                    continue;

                for (var colIndex = _bubbleMatrix.ColumnCount - 1; colIndex > -1; --colIndex)
                {
                    var bubble = bubblesInRow.SingleOrDefault(b => b.Column == colIndex);
                    if (bubble != null)
                    {
                        // Find out how many cells between the bubble and the last column have bubbles in them.
                        var occupied = bubblesInRow.Where(b => bubble.Column < b.Column).Count();

                        // Now determine how many of the cells do not have a bubble in them.
                        var empty = _bubbleMatrix.ColumnCount - 1 - bubble.Column - occupied;
                        if (empty != 0)
                        {
                            bubble.MoveTo(bubble.Row, bubble.Column + empty);
                            movedBubbles.Add(bubble);
                        }
                    }
                }
            }
            return movedBubbles;
        }

        IEnumerable<BubbleViewModel> MoveNeighboringBubblesDown(BubbleViewModel[] bubblesInGroup)
        {
            var movedBubbles = new List<BubbleViewModel>();

            var affectedColumns = bubblesInGroup.Select(b => b.Column).Distinct().ToArray();

            foreach (var affectedColumn in affectedColumns)
            {
                var bubblesInColumn = _bubbleMatrix.Bubbles.Where(b => b.Column == affectedColumn).ToArray();
                if (bubblesInColumn.Length == 0)
                    continue;

                while (true)
                {
                    var bubbleRowIndexes = bubblesInColumn.Select(b => b.Row).ToArray();

                    var emptyIndexes =
                        (from rowIndex in Enumerable.Range(0, _bubbleMatrix.RowCount)
                         where !bubbleRowIndexes.Contains(rowIndex)
                         select rowIndex)
                        .ToArray();

                    var emptyIndexCount = emptyIndexes.Count();
                    if (emptyIndexCount == 0 || emptyIndexCount == _bubbleMatrix.RowCount)
                        break;

                    var occupiedIndexes = bubblesInColumn.Select(b => b.Row).ToArray();
                    var occupiedIndexCount = occupiedIndexes.Count();
                    if (occupiedIndexCount == 0 || occupiedIndexCount == _bubbleMatrix.RowCount)
                        break;

                    var bottomEmptyIndex = emptyIndexes.Max();
                    var topOccupiedIndex = occupiedIndexes.Min();
                    if (bottomEmptyIndex < topOccupiedIndex)
                        break;

                    var closestBubbleIndex = bubblesInColumn.Where(b => b.Row < bottomEmptyIndex).Max(b => b.Row);
                    var closestBubble = bubblesInColumn.Single(b => b.Row == closestBubbleIndex);
                    closestBubble.MoveTo(bottomEmptyIndex, closestBubble.Column);

                    movedBubbles.Add(closestBubble);
                }
            }
            return movedBubbles;
        }

        #endregion // Private

        #endregion // Methods
    }
}