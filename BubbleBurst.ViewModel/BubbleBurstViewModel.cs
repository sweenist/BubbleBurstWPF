using System.Windows.Input;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// This is the top-level view model class.
    /// </summary>
    public class BubbleBurstViewModel : ObservableObject
    {
        #region Constructor

        public BubbleBurstViewModel()
        {
            BubbleMatrix = new BubbleMatrixViewModel();
            BubbleMatrix.GameEnded += delegate
            {
                GameOver = new GameOverViewModel(BubbleMatrix);
                GameOver.RequestClose += HandleGameOverRequestClose;
            };
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Returns the ViewModel responsible for managing the matrix of bubbles.
        /// </summary>
        public BubbleMatrixViewModel BubbleMatrix { get; private set; }

        /// <summary>
        /// Returns true if the application can currently perform an undo operation.
        /// </summary>
        public bool CanUndo
        {
            get { return GameOver == null && BubbleMatrix.CanUndo; }
        }

        /// <summary>
        /// Returns the ViewModel used by the game-over dialog.
        /// </summary>
        public GameOverViewModel GameOver
        {
            get { return _gameOver; }
            private set
            {
                if (value == _gameOver)
                    return;

                _gameOver = value;

                base.RaisePropertyChanged(nameof(GameOver));
            }
        }

        /// <summary>
        /// Returns the command that starts a new game of BubbleBurst.
        /// </summary>
        public ICommand RestartCommand
        {
            get { return new RelayCommand(BubbleMatrix.StartNewGame); }
        }

        /// <summary>
        /// Returns the command that un-bursts the previously burst bubble group.
        /// </summary>
        public ICommand UndoCommand
        {
            get { return new RelayCommand(BubbleMatrix.Undo, () => CanUndo); }
        }

        #endregion // Properties

        #region Methods

        void HandleGameOverRequestClose(object sender, EventArgs e)
        {
            GameOver.RequestClose -= HandleGameOverRequestClose;
            GameOver = null;
        }

        #endregion // Methods

        #region Fields

        GameOverViewModel _gameOver;

        #endregion // Fields
    }
}