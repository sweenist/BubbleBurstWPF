using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel.Internal
{
    /// <summary>Keeps track of a bubble's current location and its location history.</summary>
    internal class BubbleLocationManager
    {

        private BubbleLocation? _currentLocation;
        private readonly Stack<BubbleLocation> _previousLocations;

        /// <summary>Initializes a new instance of the <see cref="BubbleLocationManager"/> class.</summary>
        internal BubbleLocationManager()
        {
            _previousLocations = new Stack<BubbleLocation>();
        }

        /// <summary>Gets the column.</summary>
        internal int Column => _currentLocation.HasValue ? _currentLocation.Value.Column : -1;

        /// <summary>Gets the row.</summary>
        internal int Row => _currentLocation.HasValue ? _currentLocation.Value.Row : -1;

        /// <summary>Gets the previous column.</summary>
        internal int PreviousColumn => _previousLocations.Any() ? _previousLocations.Peek().Column : -1;

        /// <summary>Gets the previous row.</summary>
        internal int PreviousRow => _previousLocations.Any() ? _previousLocations.Peek().Row : -1;

        /// <summary>Moves to new location.</summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        internal void MoveTo(int row, int column)
        {
            if (_currentLocation.HasValue)
            {
                _previousLocations.Push(_currentLocation.Value);
            }

            _currentLocation = new BubbleLocation(row, column);
        }

        /// <summary>Moves to previous location.</summary>
        internal void MoveToPreviousLocation()
        {
            if (_previousLocations.Any())
            {
                _currentLocation = _previousLocations.Pop();
            }
        }

        private struct BubbleLocation
        {
            public BubbleLocation(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public readonly int Column;
            public readonly int Row;
        }
    }
}