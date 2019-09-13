using System.ComponentModel;

namespace Superdoctor.Exceptions
{
    class WrongPathException : WarningException
    {
        public WrongPathException(string message) : base(message)
        {

        }
    }
}
