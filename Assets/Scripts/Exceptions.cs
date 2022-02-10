using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingRoom.Exceptions
{
	// Thrown when the media attached to a narrative object is invalid.
	public class InvalidMediaException : Exception { public InvalidMediaException(string message) : base(message) { } }

	// Thrown when atomic narrative object has a duration of zero.
	public class InvalidDurationException : Exception { public InvalidDurationException(string message) : base(message) { } }

	// Thrown when an object has an invalid root narrative object.
	public class InvalidRootNarrativeObjectException : Exception { public InvalidRootNarrativeObjectException(string message) : base(message) { } }

	/// <summary>
	/// Thrown when a method attached to a constraint is implemented with a parameter which is not supported by our implementation.
	/// </summary>
	public class InvalidConstraintParameterException : Exception { public InvalidConstraintParameterException(string message) : base(message) { } }
}
