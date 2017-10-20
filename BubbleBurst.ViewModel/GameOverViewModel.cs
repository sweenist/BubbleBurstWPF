using System;
using System.Windows;
using System.Windows.Input;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>The ViewModel responsible for supplying data and behavior for the game-over dialog.</summary>
    public class GameOverViewModel
    {
        readonly BubbleMatrixViewModel _bubbleMatrix;

        /// <summary>Initializes a new instance of the <see cref="GameOverViewModel"/> class.</summary>
        /// <param name="bubbleMatrix">The bubble matrix.</param>
        /// <exception cref="System.ArgumentNullException">bubbleMatrix</exception>
        internal GameOverViewModel(BubbleMatrixViewModel bubbleMatrix)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            _bubbleMatrix = bubbleMatrix;

            if (_bubbleMatrix.Bubbles.Count == 0)
            {
                Title = "CONGRATULATIONS!";
            }
            else
            {
                var pluralEnding = bubbleMatrix.Bubbles.Count == 1 ? string.Empty : "S";
                Title = $"{bubbleMatrix.Bubbles.Count} BUBBLE{pluralEnding} LEFT";
            }
        }

        /// <summary>Raised when the game-over dialog should be closed.</summary>
        public event EventHandler RequestClose;

        /// <summary>Returns the command that exits the application.</summary>
        public ICommand QuitCommand => new RelayCommand(Application.Current.Shutdown);

        /// <summary>Returns the subtitle of the game-over dialog.</summary>
        public string Subtitle => $"Most bubbles popped at once: {_bubbleMatrix.MostBubblesPoppedAtOnce}";

        /// <summary>Returns the title of the game-over dialog.</summary>
        public string Title { get; }

        /// <summary>Closes the game-over dialog.</summary>
        public void Close()
        {
            RaiseRequestClose();
        }

        /// <summary>Starts a new round of the BubbleBurst game.</summary>
        public void StartNewGame()
        {
            _bubbleMatrix.StartNewGame();
        }

        private void RaiseRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}