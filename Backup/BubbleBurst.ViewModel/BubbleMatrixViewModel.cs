using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BubbleBurst.ViewModel.Internal;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents the matrix of bubbles and contains 
    /// logic that drives a game to completion.
    /// </summary>
    public class BubbleMatrixViewModel : ObservableObject
    {
        #region Constructor

        internal BubbleMatrixViewModel()
        {
            _bubblesInternal = new ObservableCollection<BubbleViewModel>();
            this.Bubbles = new ReadOnlyObservableCollection<BubbleViewModel>(_bubblesInternal);

            this.TaskManager = new BubblesTaskManager(this);

            _bubbleFactory = new BubbleFactory(this);

            _bubbleGroup = new BubbleGroup(this.Bubbles);

            _bubbleGroupSizeStack = new Stack<int>();

            _isIdle = true;
        }

        #endregion // Constructor

        #region Events

        /// <summary>
        /// Raised when there are no more bubble groups left to burst.
        /// </summary>
        public event EventHandler GameEnded;

        #endregion // Events

        #region Properties

        #region Public

        /// <summary>
        /// Returns a read-only collection of all bubbles in the bubble matrix.
        /// </summary>
        public ReadOnlyObservableCollection<BubbleViewModel> Bubbles { get; private set; }

        /// <summary>
        /// Represents whether the application is currently processing something that
        /// requires the user interface to ignore user interactions until it finishes.
        /// </summary>
        public bool IsIdle
        {
            get { return _isIdle; }
            internal set
            {
                if (value.Equals(_isIdle))
                    return;

                _isIdle = value;

                base.RaisePropertyChanged("IsIdle");
            }
        }

        /// <summary>
        /// Returns the object that creates and publishes tasks for a bubble matrix.
        /// </summary>
        public BubblesTaskManager TaskManager { get; private set; }

        #endregion // Public

        #region Internal

        internal bool CanUndo
        {
            get { return this.IsIdle && this.TaskManager.CanUndo; }
        }

        internal int ColumnCount
        {
            get { return _columnCount; }
        }

        internal int MostBubblesPoppedAtOnce
        {
            get { return _bubbleGroupSizeStack.Max(); }
        }

        internal int RowCount
        {
            get { return _rowCount; }
        }

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Public

        /// <summary>
        /// Removes all bubbles from the matrix.
        /// </summary>
        public void ClearBubbles()
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot clear bubbles when matrix is not idle.");

            _bubblesInternal.Clear();
        }

        /// <summary>
        /// Updates the number of rows and columns that 
        /// the matrix should contain.
        /// </summary>
        /// <param name="rowCount">The number of bubble rows.</param>
        /// <param name="columnCount">The number of bubble columns.</param>
        public void SetDimensions(int rowCount, int columnCount)
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot set matrix dimensions is not idle.");

            if (rowCount < 1)
                throw new ArgumentOutOfRangeException("rowCount", rowCount, "Must be greater than zero.");

            if (columnCount < 1)
                throw new ArgumentOutOfRangeException("columnCount", columnCount, "Must be greater than zero.");

            _rowCount = rowCount;
            _columnCount = columnCount;
        }

        /// <summary>
        /// Begins a new game of BubbleBurst with a new set of bubbles.
        /// </summary>
        public void StartNewGame()
        {
            // Reset game state.
            this.IsIdle = true;
            this.ResetBubbleGroup();
            _bubbleGroupSizeStack.Clear();
            this.TaskManager.Reset();

            // Create a new matrix of bubbles.
            this.ClearBubbles();
            _bubbleFactory.CreateBubblesAsync();
        }

        /// <summary>
        /// Reverts the game state to how it was before 
        /// the most recent group of bubbles was burst.
        /// </summary>
        public void Undo()
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot undo when not idle.");

            if (this.CanUndo)
            {
                // Throw away the last bubble group size, 
                // since that burst is about to be undone.
                _bubbleGroupSizeStack.Pop();

                this.TaskManager.Undo();
            }
        }

        #endregion // Public

        #region Internal

        internal void AddBubble(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            _bubblesInternal.Add(bubble);
        }

        internal void BurstBubbleGroup()
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot burst a bubble group when not idle.");

            var bubblesInGroup = _bubbleGroup.BubblesInGroup.ToArray();
            if (!bubblesInGroup.Any())
                return;

            _bubbleGroupSizeStack.Push(bubblesInGroup.Length);

            this.TaskManager.PublishTasks(bubblesInGroup);
        }

        internal void ResetBubbleGroup()
        {
            _bubbleGroup.Reset();
        }

        internal void RemoveBubble(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            _bubblesInternal.Remove(bubble);
        }

        internal void TryToEndGame()
        {
            bool groupExists = this.Bubbles.Any(b => this.IsInBubbleGroup(b));
            if (!groupExists)
            {
                this.IsIdle = false;
                this.RaiseGameEnded();
            }
        }

        internal void VerifyGroupMembership(BubbleViewModel bubble)
        {
            _bubbleGroup.Deactivate();
            if (bubble != null)
            {
                _bubbleGroup.FindBubbleGroup(bubble).Activate();
            }
        }

        #endregion // Internal

        #region Private

        bool IsInBubbleGroup(BubbleViewModel bubble)
        {
            return new BubbleGroup(this.Bubbles).FindBubbleGroup(bubble).HasBubbles;
        }

        void RaiseGameEnded()
        {
            var handler = this.GameEnded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion // Private

        #endregion // Methods

        #region Fields

        readonly BubbleFactory _bubbleFactory;
        readonly BubbleGroup _bubbleGroup;
        readonly Stack<int> _bubbleGroupSizeStack;
        readonly ObservableCollection<BubbleViewModel> _bubblesInternal;

        int _columnCount, _rowCount;
        bool _isIdle;

        #endregion // Fields
    }
}