using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BubbleBurst.ViewModel;
using Thriple.Easing;

namespace BubbleBurst.View
{
    /// <summary>
    /// Creates Storyboards used to provide animated 
    /// transitions when a bubble group bursts or un-bursts.
    /// </summary>
    internal class BubblesTaskStoryboardFactory
    {
        private readonly BubbleCanvas _bubbleCanvas;

        /// <summary>Initializes a new instance of the <see cref="BubblesTaskStoryboardFactory"/> class.</summary>
        /// <param name="bubbleCanvas">The bubble canvas.</param>
        /// <exception cref="System.ArgumentNullException">bubbleCanvas</exception>
        internal BubblesTaskStoryboardFactory(BubbleCanvas bubbleCanvas)
        {
            if (bubbleCanvas == null)
                throw new ArgumentNullException("bubbleCanvas");

            _bubbleCanvas = bubbleCanvas;
        }

        internal Storyboard CreateStoryboard(BubblesTask task)
        {
            int millisecondsPerUnit;
            Func<ContentPresenter, double> getTo;
            DependencyProperty animatedProperty;
            IEnumerable<BubbleViewModel> bubbles;

            GetStoryboardCreationData(
                task,
                out millisecondsPerUnit,
                out getTo,
                out animatedProperty,
                out bubbles);

            return CreateStoryboard(
                task,
                millisecondsPerUnit,
                getTo,
                animatedProperty,
                bubbles.ToArray());
        }

        private void GetStoryboardCreationData(
            BubblesTask task,
            out int millisecondsPerUnit,
            out Func<ContentPresenter, double> getTo,
            out DependencyProperty animatedProperty,
            out IEnumerable<BubbleViewModel> bubbles)
        {
            switch (task.TaskType)
            {
                case BubblesTaskType.Burst:
                    millisecondsPerUnit = 250;
                    getTo = cp => (task.IsUndo ? 1.0 : 0.0);
                    animatedProperty = UIElement.OpacityProperty;
                    bubbles = task.Bubbles;
                    break;

                case BubblesTaskType.MoveDown:
                    millisecondsPerUnit = 115;
                    getTo = _bubbleCanvas.CalculateTop;
                    animatedProperty = Canvas.TopProperty;

                    // Sort the bubbles to ensure that the columns move 
                    // in sync with each other in an appealing way.
                    bubbles =
                        from bubble in task.Bubbles
                        orderby bubble.PreviousColumn
                        orderby bubble.PreviousRow descending
                        select bubble;
                    break;

                case BubblesTaskType.MoveRight:
                    millisecondsPerUnit = 115;
                    getTo = _bubbleCanvas.CalculateLeft;
                    animatedProperty = Canvas.LeftProperty;

                    // Sort the bubbles to ensure that the rows move 
                    // in sync with each other in an appealing way.
                    bubbles =
                        from bubble in task.Bubbles
                        orderby bubble.PreviousRow descending
                        orderby bubble.PreviousColumn descending
                        select bubble;
                    break;

                default:
                    throw new ArgumentException("Unrecognized BubblesTaskType: " + task.TaskType);
            }

            if (task.IsUndo)
            {
                bubbles = bubbles.Reverse();
            }
        }

        private Storyboard CreateStoryboard(
            BubblesTask task,
            int millisecondsPerUnit,
            Func<ContentPresenter, double> getTo,
            DependencyProperty animatedProperty,
            BubbleViewModel[] bubbles)
        {
            if (!bubbles.Any())
                return null;

            var storyboard = new Storyboard();
            var targetProperty = new PropertyPath(animatedProperty);
            var beginTime = TimeSpan.FromMilliseconds(0);
            var beginTimeIncrement = TimeSpan.FromMilliseconds(millisecondsPerUnit / bubbles.Count());

            foreach (var presenter in GetBubblePresenters(bubbles))
            {
                var bubble = presenter.DataContext as BubbleViewModel;
                var duration = CalculateDuration(task.TaskType, bubble, millisecondsPerUnit);
                var to = getTo(presenter);
                var anim = new EasingDoubleAnimation
                {
                    BeginTime = beginTime,
                    Duration = duration,
                    Equation = EasingEquation.CubicEaseIn,
                    To = to,
                };

                Storyboard.SetTarget(anim, presenter);
                Storyboard.SetTargetProperty(anim, targetProperty);

                if (IsTaskStaggered(task.TaskType))
                {
                    beginTime = beginTime.Add(beginTimeIncrement);
                }

                storyboard.Children.Add(anim);
            }

            return storyboard;
        }

        IEnumerable<ContentPresenter> GetBubblePresenters(IEnumerable<BubbleViewModel> bubbles)
        {
            var bubblePresenters = new List<ContentPresenter>();
            var contentPresenters = _bubbleCanvas.Children.Cast<ContentPresenter>().ToArray();
            foreach (BubbleViewModel bubble in bubbles)
            {
                var bubblePresenter = contentPresenters.FirstOrDefault(cp => cp.DataContext == bubble);
                if (bubblePresenter != null)
                {
                    bubblePresenters.Add(bubblePresenter);
                }
            }

            return bubblePresenters;
        }

        private static Duration CalculateDuration(
            BubblesTaskType taskType,
            BubbleViewModel bubble,
            int millisecondsPerUnit)
        {
            int totalMilliseconds;
            switch (taskType)
            {
                case BubblesTaskType.Burst:
                    totalMilliseconds = millisecondsPerUnit;
                    break;

                case BubblesTaskType.MoveDown:
                    totalMilliseconds = millisecondsPerUnit * Math.Abs(bubble.Row - bubble.PreviousRow);
                    break;

                case BubblesTaskType.MoveRight:
                    totalMilliseconds = millisecondsPerUnit * Math.Abs(bubble.Column - bubble.PreviousColumn);
                    break;

                default:
                    throw new ArgumentException("Unrecognized BubblesTaskType value: " + taskType, "taskType");
            }

            return new Duration(TimeSpan.FromMilliseconds(totalMilliseconds));
        }

        private static bool IsTaskStaggered(BubblesTaskType taskType)
        {
            return taskType != BubblesTaskType.Burst;
        }
    }
}