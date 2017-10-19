using System;
using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents some work that BubbleMatrixView must 
    /// process for a given set of BubbleViewModels.
    /// </summary>
    public class BubblesTask
    {
        #region Constructor

        internal BubblesTask(BubblesTaskType taskType, bool isUndo, Func<IEnumerable<BubbleViewModel>> getBubbles, Action complete)
        {
            this.TaskType = taskType;
            this.IsUndo = isUndo;
            _getBubbles = getBubbles;
            this.Complete = complete;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Returns the bubbles associated with this task.
        /// </summary>
        public IEnumerable<BubbleViewModel> Bubbles
        {
            get
            {
                if (_bubbles == null)
                {
                    // The list of bubbles associated with this task is
                    // retrieved once, on demand, because retrieving the 
                    // list can have side effects.
                    _bubbles = _getBubbles().ToArray();
                }
                return _bubbles;
            }
        }

        /// <summary>
        /// Invoked immediately after the task has been performed.
        /// </summary>
        public Action Complete { get; private set; }

        /// <summary>
        /// Returns true if this task is undoing the effects of a previously performed task.
        /// </summary>
        public bool IsUndo { get; private set; }

        /// <summary>
        /// Returns the kind of task this object represents.
        /// </summary>
        public BubblesTaskType TaskType { get; private set; }

        #endregion // Properties

        #region Fields

        BubbleViewModel[] _bubbles;
        readonly Func<IEnumerable<BubbleViewModel>> _getBubbles;

        #endregion // Fields
    }
}