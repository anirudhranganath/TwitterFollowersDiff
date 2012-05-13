using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterFollowersDiff.Exceptions
{
    /// <summary>
    /// Exception to be used when a userhandle does not exist
    /// </summary>
    public class UserNotFoundException : System.Exception
    {
        public UserNotFoundException():base() {}
        public UserNotFoundException(string message) : base(message) { }
        public UserNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    }
}
