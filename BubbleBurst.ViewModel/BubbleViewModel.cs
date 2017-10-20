using System;
using System.Windows.Input;
using BubbleBurst.ViewModel.Internal;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>Represents a bubble in the bubble matrix.</summary>
    public class BubbleViewModel : ObservableObject
    {
        static readonly Random _random = new Random(DateTime.Now.Millisecond);

        private readonly BubbleMatrixViewModel _bubbleMatrix;
        private readonly BubbleLocationManager _locationManager;

        private bool _isInBubbleGroup;
        private int? _prevColumnDuringUndo, _prevRowDuringUndo;

        /// <summary>Initializes a new instance of the <see cref="BubbleViewModel"/> class.</summary>
        /// <param name="bubbleMatrix">The bubble matrix.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <exception cref="System.ArgumentNullException">bubbleMatrix</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">row or column</exception>
        internal BubbleViewModel(BubbleMatrixViewModel bubbleMatrix, int row, int column)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            if (row < 0 || bubbleMatrix.RowCount <= row)
                throw new ArgumentOutOfRangeException("row");

            if (column < 0 || bubbleMatrix.ColumnCount <= column)
                throw new ArgumentOutOfRangeException("column");

            _bubbleMatrix = bubbleMatrix;

            _locationManager = new BubbleLocationManager();
            _locationManager.MoveTo(row, column);

            BubbleType = GetRandomBubbleType();
        }

        #region Properties

        /// <summary>Returns the kind of bubble this is.</summary>
        public BubbleType BubbleType { get; }

        /// <summary>Returns true if this bubble is a member of the currently active bubble group in the user interface.</summary>
        public bool IsInBubbleGroup
        {
            get { return _isInBubbleGroup; }
            internal set
            {
                if (value.Equals(_isInBubbleGroup))
                    return;

                _isInBubbleGroup = value;

                RaisePropertyChanged("IsInBubbleGroup");
            }
        }

        /// <summary>Returns the command used to burst the bubble group in which this bubble exists.</summary>
        public ICommand BurstBubbleGroupCommand => new RelayCommand(_bubbleMatrix.BurstBubbleGroup);

        /// <summary>The column in which this bubble exists.</summary>
        public int Column => _locationManager.Column;

        /// <summary>The column in which this bubble existed before it moved to its current column.</summary>
        public int PreviousColumn => _prevColumnDuringUndo.HasValue ? _prevColumnDuringUndo.Value : _locationManager.PreviousColumn;

        /// <summary>The row in which this bubble existed before it moved to its current row.</summary>
        public int PreviousRow => _prevRowDuringUndo.HasValue ? _prevRowDuringUndo.Value : _locationManager.PreviousRow;

        /// <summary>The row in which this bubble exists.</summary>
        public int Row => _locationManager.Row;

        #endregion // Properties

        #region Methods

        /// <summary>Causes the bubble to evaluate whether or not it is in a bubble group.</summary>
        /// <param name="isMouseOver">True if the mouse cursor is currently over this bubble.</param>
        public void VerifyGroupMembership(bool isMouseOver)
        {
            _bubbleMatrix.VerifyGroupMembership(isMouseOver ? this : null);
        }

        #region Internal

        internal void BeginUndo()
        {
            // During an Undo operation we need to treat the current row
            // and column as the previous row and column, since the bubble
            // will be moving from where it currently is to where it used
            // to be.  This logic is kept in the BubbleViewModel class in
            // order to keep BubbleLocationManager simple.
            _prevRowDuringUndo = Row;
            _prevColumnDuringUndo = Column;

            _locationManager.MoveToPreviousLocation();
        }

        internal void EndUndo()
        {
            // Now that the Undo operation is finished,
            // it's back to business as usual.
            _prevRowDuringUndo = null;
            _prevColumnDuringUndo = null;
        }

        /// <summary>
        /// Updates the logical matrix coordinate of this bubble.
        /// </summary>
        internal void MoveTo(int row, int column)
        {
            _locationManager.MoveTo(row, column);
        }

        #endregion // Internal

        private static BubbleType GetRandomBubbleType()
        {
            var bubbleTypeValues = Enum.GetValues(typeof(BubbleType)) as BubbleType[];
            var highestValue = bubbleTypeValues.Length - 1;

            return (BubbleType)_random.Next(0, highestValue + 1);
        }

        #endregion // Methods
    }
}