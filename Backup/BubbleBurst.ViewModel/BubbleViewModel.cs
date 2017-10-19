using System;
using System.Windows.Input;
using BubbleBurst.ViewModel.Internal;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents a bubble in the bubble matrix.
    /// </summary>
    public class BubbleViewModel : ObservableObject
    {
        #region Constructor

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

            this.BubbleType = GetRandomBubbleType();
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Returns the kind of bubble this is.
        /// </summary>
        public BubbleType BubbleType { get; private set; }

        /// <summary>
        /// Returns true if this bubble is a member of the 
        /// currently active bubble group in the user interface.
        /// </summary>
        public bool IsInBubbleGroup
        {
            get { return _isInBubbleGroup; }
            internal set
            {
                if (value.Equals(_isInBubbleGroup))
                    return;

                _isInBubbleGroup = value;

                base.RaisePropertyChanged("IsInBubbleGroup");
            }
        }

        /// <summary>
        /// Returns the command used to burst the bubble group in which this bubble exists.
        /// </summary>
        public ICommand BurstBubbleGroupCommand
        {
            get { return new RelayCommand(_bubbleMatrix.BurstBubbleGroup); }
        }

        /// <summary>
        /// The column in which this bubble exists.
        /// </summary>
        public int Column
        {
            get { return _locationManager.Column; }
        }

        /// <summary>
        /// The column in which this bubble existed before it moved to its current column.
        /// </summary>
        public int PreviousColumn
        {
            get 
            {
                if (_prevColumnDuringUndo.HasValue)
                {
                    return _prevColumnDuringUndo.Value;
                }
                else
                {
                    return _locationManager.PreviousColumn;
                }
            }
        }

        /// <summary>
        /// The row in which this bubble existed before it moved to its current row.
        /// </summary>
        public int PreviousRow
        {
            get 
            {
                if (_prevRowDuringUndo.HasValue)
                {
                    return _prevRowDuringUndo.Value;
                }
                else
                {
                    return _locationManager.PreviousRow;
                }
            }
        }

        /// <summary>
        /// The row in which this bubble exists.
        /// </summary>
        public int Row
        {
            get { return _locationManager.Row; }
        }

        #endregion // Properties

        #region Methods

        #region Public

        /// <summary>
        /// Causes the bubble to evaluate whether or not it is in a bubble group.
        /// </summary>
        /// <param name="isMouseOver">
        /// True if the mouse cursor is currently over this bubble.
        /// </param>
        public void VerifyGroupMembership(bool isMouseOver)
        {
            _bubbleMatrix.VerifyGroupMembership(isMouseOver ? this : null);
        }

        #endregion // Public

        #region Internal

        internal void BeginUndo()
        {
            // During an Undo operation we need to treat the current row
            // and column as the previous row and column, since the bubble
            // will be moving from where it currently is to where it used
            // to be.  This logic is kept in the BubbleViewModel class in
            // order to keep BubbleLocationManager simple.
            _prevRowDuringUndo = this.Row;
            _prevColumnDuringUndo = this.Column;

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

        #region Private

        static BubbleType GetRandomBubbleType()
        {
            var bubbleTypeValues = Enum.GetValues(typeof(BubbleType)) as BubbleType[];
            int highestValue = bubbleTypeValues.Length - 1;
            return (BubbleType)_random.Next(0, highestValue + 1);
        }

        #endregion // Private

        #endregion // Methods

        #region Fields

        readonly BubbleMatrixViewModel _bubbleMatrix;
        readonly BubbleLocationManager _locationManager;

        bool _isInBubbleGroup;
        int? _prevColumnDuringUndo, _prevRowDuringUndo;

        static readonly Random _random = new Random(DateTime.Now.Millisecond);

        #endregion // Fields
    }
}