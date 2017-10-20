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
        private readonly BubbleFactory _bubbleFactory;
        private readonly BubbleGroup _bubbleGroup;
        private readonly Stack<int> _bubbleGroupSizeStack;
        private readonly ObservableCollection<BubbleViewModel> _bubblesInternal;
        private bool _isIdle;

        /// <summary>Initializes a new instance of the <see cref="BubbleMatrixViewModel"/> class.</summary>
        internal BubbleMatrixViewModel()
        {
            _bubblesInternal = new ObservableCollection<BubbleViewModel>();
            Bubbles = new ReadOnlyObservableCollection<BubbleViewModel>(_bubblesInternal);

            TaskManager = new BubblesTaskManager(this);

            _bubbleFactory = new BubbleFactory(this);

            _bubbleGroup = new BubbleGroup(Bubbles);

            _bubbleGroupSizeStack = new Stack<int>();

            _isIdle = true;
        }

        /// <summary>Raised when there are no more bubble groups left to burst.</summary>
        public event EventHandler GameEnded;

        #region Properties

        /// <summary>Returns a read-only collection of all bubbles in the bubble matrix.</summary>
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

                RaisePropertyChanged("IsIdle");
            }
        }

        /// <summary>Returns the object that creates and publishes tasks for a bubble matrix.</summary>
        public BubblesTaskManager TaskManager { get; private set; }

        /// <summary>Gets a value indicating whether action can be undone.</summary>
        internal bool CanUndo => IsIdle && TaskManager.CanUndo;

        /// <summary>Gets or sets the column count.</summary>
        internal int ColumnCount { get; set; }

        /// <summary>Gets or sets the row count.</summary>
        internal int RowCount { get; set; }

        /// <summary>Gets the most bubbles popped at once.</summary>
        internal int MostBubblesPoppedAtOnce => _bubbleGroupSizeStack.Max();

        #endregion // Properties

        #region Methods

        #region Public

        /// <summary>
        /// Removes all bubbles from the matrix.
        /// </summary>
        public void ClearBubbles()
        {
            if (!IsIdle)
                throw new InvalidOperationException("Cannot clear bubbles when matrix is not idle.");

            _bubblesInternal.Clear();
        }

        /// <summary>Updates the number of rows and columns that the matrix should contain.</summary>
        /// <param name="rowCount">The number of bubble rows.</param>
        /// <param name="columnCount">The number of bubble columns.</param>
        public void SetDimensions(int rowCount, int columnCount)
        {
            if (!IsIdle)
                throw new InvalidOperationException("Cannot set matrix dimensions is not idle.");

            if (rowCount < 1)
                throw new ArgumentOutOfRangeException("rowCount", rowCount, "Must be greater than zero.");

            if (columnCount < 1)
                throw new ArgumentOutOfRangeException("columnCount", columnCount, "Must be greater than zero.");

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        /// <summary>
        /// Begins a new game of BubbleBurst with a new set of bubbles.
        /// </summary>
        public void StartNewGame()
        {
            IsIdle = true;
            ResetBubbleGroup();
            _bubbleGroupSizeStack.Clear();
            TaskManager.Reset();

            // Create a new matrix of bubbles.
            ClearBubbles();
            _bubbleFactory.CreateBubblesAsync();
        }

        /// <summary>
        /// Reverts the game state to how it was before 
        /// the most recent group of bubbles was burst.
        /// </summary>
        public void Undo()
        {
            if (!IsIdle)
                throw new InvalidOperationException("Cannot undo when not idle.");

            if (CanUndo)
            {
                // Throw away the last bubble group size, 
                // since that burst is about to be undone.
                _bubbleGroupSizeStack.Pop();

                TaskManager.Undo();
            }
        }

        #endregion // Public

        #region Internal

        /// <summary>Adds the bubble.</summary>
        /// <param name="bubble">The bubble.</param>
        /// <exception cref="System.ArgumentNullException">bubble</exception>
        internal void AddBubble(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            _bubblesInternal.Add(bubble);
        }

        /// <summary>Bursts the bubble group.</summary>
        /// <exception cref="System.InvalidOperationException">Cannot burst a bubble group when not idle.</exception>
        internal void BurstBubbleGroup()
        {
            if (!IsIdle)
                throw new InvalidOperationException("Cannot burst a bubble group when not idle.");

            var bubblesInGroup = _bubbleGroup.BubblesInGroup.ToArray();
            if (!bubblesInGroup.Any())
                return;

            _bubbleGroupSizeStack.Push(bubblesInGroup.Length);

            TaskManager.PublishTasks(bubblesInGroup);
        }

        /// <summary>Resets the bubble group.</summary>
        internal void ResetBubbleGroup()
        {
            _bubbleGroup.Reset();
        }

        /// <summary>Removes the bubble.</summary>
        /// <param name="bubble">The bubble.</param>
        /// <exception cref="System.ArgumentNullException">bubble</exception>
        internal void RemoveBubble(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            _bubblesInternal.Remove(bubble);
        }

        /// <summary>Tries to end game.</summary>
        internal void TryToEndGame()
        {
            bool groupExists = Bubbles.Any(b => IsInBubbleGroup(b));

            if (!groupExists)
            {
                IsIdle = false;
                RaiseGameEnded();
            }
        }

        /// <summary>Verifies the group membership.</summary>
        /// <param name="bubble">The bubble.</param>
        internal void VerifyGroupMembership(BubbleViewModel bubble)
        {
            _bubbleGroup.Deactivate();

            if (bubble != null)
            {
                _bubbleGroup.FindBubbleGroup(bubble).Activate();
            }
        }

        #endregion // Internal

        private bool IsInBubbleGroup(BubbleViewModel bubble)
        {
            return new BubbleGroup(Bubbles).FindBubbleGroup(bubble).HasBubbles;
        }

        private void RaiseGameEnded()
        {
            GameEnded?.Invoke(this, EventArgs.Empty);
        }

        #endregion // Methods
    }
}