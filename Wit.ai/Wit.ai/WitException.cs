using System;

namespace Wit.ai
{
    /// <summary>
    /// Represents an exception thrown by Wit.AI
    /// </summary>
    class WitException : Exception
    {
        public WitException(string message) : base(message) { }
    }
}