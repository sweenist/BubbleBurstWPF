using System;
using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel.Internal
{
    /// <summary>
    /// Locates and stores a set contiguous bubbles of the same color.
    /// Exposes methods to activate and deactivate a group of bubbles,
    /// which is used to highlight them in the UI.
    /// </summary>
    internal class BubbleGroup
    {
        private readonly IEnumerable<BubbleViewModel> _allBubbles;

        /// <summary>Initializes a new instance of the <see cref="BubbleGroup"/> class.</summary>
        /// <param name="allBubbles">All bubbles.</param>
        /// <exception cref="System.ArgumentNullException">allBubbles</exception>
        internal BubbleGroup(IEnumerable<BubbleViewModel> allBubbles)
        {
            if (allBubbles == null)
                throw new ArgumentNullException("allBubbles");

            _allBubbles = allBubbles;
            BubblesInGroup = new List<BubbleViewModel>();
        }

        /// <summary>Returns the list of bubbles in the bubble group.</summary>
        internal IList<BubbleViewModel> BubblesInGroup { get; }

        /// <summary>Returns true if there are any bubbles in the group.</summary>
        internal bool HasBubbles => BubblesInGroup.Any();


        #region Methods

        #region Internal

        /// <summary>Informs each bubble in the group that it is in the active bubble group.</summary>
        internal void Activate()
        {
            foreach (var member in BubblesInGroup)
            {
                member.IsInBubbleGroup = true;
            }
        }

        /// <summary>Informs each bubble in the group that it is not in the active bubble group.</summary>
        internal void Deactivate()
        {
            foreach (var member in BubblesInGroup)
            {
                member.IsInBubbleGroup = false;
            }
        }

        /// <summary>
        /// Searches for a bubble group in which the specified bubble
        /// is a member.  If a group is found, this object's BubblesInGroup
        /// collection will contain the bubbles in that group afterwards.
        /// </summary>
        /// <param name="bubble">The bubble with which to begin searching for a group.</param>
        /// <returns>Returns this object, enabling a fluid-style API usage.</returns>
        internal BubbleGroup FindBubbleGroup(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            var isBubbleInCurrentGroup = BubblesInGroup.Contains(bubble);

            if (!isBubbleInCurrentGroup)
            {
                BubblesInGroup.Clear();
                SearchForGroup(bubble);

                var addOriginalBubble = HasBubbles
                                     && !BubblesInGroup.Contains(bubble);

                if (addOriginalBubble)
                {
                    BubblesInGroup.Add(bubble);
                }
            }
            return this;
        }

        internal void Reset()
        {
            Deactivate();
            BubblesInGroup.Clear();
        }

        #endregion // Internal

        #region Private

        void SearchForGroup(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            foreach (var groupMember in FindMatchingNeighbors(bubble))
            {
                if (!BubblesInGroup.Contains(groupMember))
                {
                    BubblesInGroup.Add(groupMember);
                    SearchForGroup(groupMember);
                }
            }
        }

        IEnumerable<BubbleViewModel> FindMatchingNeighbors(BubbleViewModel bubble)
        {
            var matches = new List<BubbleViewModel>();

            // Check above.
            var match = TryFindMatch(bubble.Row - 1, bubble.Column, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            // Check below.
            match = TryFindMatch(bubble.Row + 1, bubble.Column, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            // Check left.
            match = TryFindMatch(bubble.Row, bubble.Column - 1, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            // Check right.
            match = TryFindMatch(bubble.Row, bubble.Column + 1, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            return matches;
        }

        BubbleViewModel TryFindMatch(int row, int column, BubbleType bubbleType)
        {
            return _allBubbles.SingleOrDefault(b => b.Row == row
                                                 && b.Column == column
                                                 && b.BubbleType == bubbleType);
        }

        #endregion // Private

        #endregion // Methods

    }
}