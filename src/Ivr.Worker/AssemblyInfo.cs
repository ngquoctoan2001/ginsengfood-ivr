using System.Runtime.CompilerServices;

// The worker's job hosts and their shared loop are internal on purpose: nothing outside this
// assembly constructs them, and a public base class would invite one. The unit tests reach the
// loop through here rather than by widening the surface to be testable.
[assembly: InternalsVisibleTo("Ivr.UnitTests")]
