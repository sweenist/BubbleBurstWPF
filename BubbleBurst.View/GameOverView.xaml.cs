using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BubbleBurst.ViewModel;

namespace BubbleBurst.View
{
    /// <summary>
    /// The modal dialog shown once a game has ended.
    /// </summary>
    public partial class GameOverView : UserControl
    {

        private GameOverViewModel _gameOver;
        private readonly Storyboard _outroStoryboard;

        /// <summary>Initializes a new instance of the <see cref="GameOverView"/> class.</summary>
        public GameOverView()
        {
            InitializeComponent();

            _outroStoryboard = _contentBorder.Resources["OutroStoryboard"] as Storyboard;

            DataContextChanged += HandleDataContextChanged;
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _gameOver = DataContext as GameOverViewModel;
        }

        private void HandlePlayAgainHyperlinkClick(object sender, RoutedEventArgs e)
        {
            _gameOver.StartNewGame();
            _outroStoryboard.Begin(this);
        }

        private void HandleOutroCompleted(object sender, EventArgs e)
        {
            _gameOver.Close();
        }
    }
}