using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// The ViewModel responsible for supplying data and behavior for the game-over dialog.
    /// </summary>
    public class GameOverViewModel
    {
        #region Constructor

        internal GameOverViewModel(BubbleMatrixViewModel bubbleMatrix)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            _bubbleMatrix = bubbleMatrix;

            if (bubbleMatrix.Bubbles.Count == 0)
            {
                this.Title = "CONGRATULATIONS!";
            }
            else
            {
                string theLetterS = bubbleMatrix.Bubbles.Count == 1 ? string.Empty : "S";
                this.Title = string.Format(CultureInfo.CurrentCulture, "{0} BUBBLE{1} LEFT", bubbleMatrix.Bubbles.Count, theLetterS);
            }

            this.Subtitle = "Most bubbles popped at once: " + bubbleMatrix.MostBubblesPoppedAtOnce;

            this.QuitCommand = new RelayCommand(Application.Current.Shutdown);
        }

        #endregion // Constructor

        #region Events

        /// <summary>
        /// Raised when the game-over dialog should be closed.
        /// </summary>
        public event EventHandler RequestClose;

        #endregion // Events

        #region Properties

        /// <summary>
        /// Returns the command that exits the application.
        /// </summary>
        public ICommand QuitCommand { get; private set; }

        /// <summary>
        /// Returns the subtitle of the game-over dialog.
        /// </summary>
        public string Subtitle { get; private set; }

        /// <summary>
        /// Returns the title of the game-over dialog.
        /// </summary>
        public string Title { get; private set; }        

        #endregion // Properties

        #region Methods

        #region Public

        /// <summary>
        /// Closes the game-over dialog.
        /// </summary>
        public void Close()
        {
            this.RaiseRequestClose();
        }

        /// <summary>
        /// Starts a new round of the BubbleBurst game.
        /// </summary>
        public void StartNewGame()
        {
            _bubbleMatrix.StartNewGame();
        }

        #endregion // Public

        #region Private

        void RaiseRequestClose()
        {
            var handler = this.RequestClose;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion // Private

        #endregion // Methods

        #region Fields

        readonly BubbleMatrixViewModel _bubbleMatrix;

        #endregion // Fields
    }
}