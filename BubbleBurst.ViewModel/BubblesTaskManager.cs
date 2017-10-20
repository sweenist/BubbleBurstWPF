using System;
using System.Collections.Generic;
using System.Linq;
using BubbleBurst.ViewModel.Internal;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Responsible for the tasks used to burst and un-burst bubble groups.  
    /// </summary>
    public class BubblesTaskManager
    {
        #region Constructor

        internal BubblesTaskManager(BubbleMatrixViewModel bubbleMatrix)
        {
            _bubblesTaskFactory = new BubblesTaskFactory(bubbleMatrix);
            _pendingTasks = new Queue<BubblesTask>();
            _undoStack = new Stack<IEnumerable<BubblesTask>>();
        }

        #endregion // Constructor

        #region Events

        /// <summary>
        /// Raised when tasks are available to be performed.
        /// </summary>
        public event EventHandler PendingTasksAvailable;

        #endregion // Events

        #region Properties

        /// <summary>
        /// Returns true if an undo operation can be performed at this time.
        /// </summary>
        internal bool CanUndo
        {
            get { return _undoStack.Any(); }
        }

        #endregion // Properties

        #region Methods

        #region Public

        /// <summary>
        /// Returns the next pending task if one exists, or null.
        /// </summary>
        public BubblesTask GetPendingTask()
        {
            return _pendingTasks.Any() ? _pendingTasks.Dequeue() : null;
        }

        #endregion // Public

        #region Internal

        /// <summary>
        /// Publishs a set of tasks that will burst a bubble group.
        /// </summary>
        /// <param name="bubblesInGroup">The bubbles to burst.</param>
        internal void PublishTasks(BubbleViewModel[] bubblesInGroup)
        {
            var tasks = _bubblesTaskFactory.CreateTasks(bubblesInGroup);
            ArchiveTasks(tasks);
            PublishTasks(tasks);
        }

        /// <summary>
        /// Publishs a set of tasks that will undo the previous bubble burst.
        /// </summary>
        internal void Undo()
        {
            var originalTasks = _undoStack.Pop();
            var undoTasks = _bubblesTaskFactory.CreateUndoTasks(originalTasks);
            PublishTasks(undoTasks);
        }

        /// <summary>
        /// Initializes this object back to its original state.
        /// </summary>
        internal void Reset()
        {
            _pendingTasks.Clear();
            _undoStack.Clear();
        }

        #endregion // Internal

        #region Private

        void ArchiveTasks(IEnumerable<BubblesTask> tasks)
        {
            _undoStack.Push(tasks);
        }

        void PublishTasks(IEnumerable<BubblesTask> tasks)
        {
            foreach (BubblesTask task in tasks)
            {
                _pendingTasks.Enqueue(task);
            }

            RaisePendingTasksAvailable();
        }

        void RaisePendingTasksAvailable()
        {
            var handler = PendingTasksAvailable;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion // Private

        #endregion // Methods

        #region Fields

        readonly BubblesTaskFactory _bubblesTaskFactory;
        readonly Queue<BubblesTask> _pendingTasks;
        readonly Stack<IEnumerable<BubblesTask>> _undoStack;

        #endregion // Fields
    }
}