using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarfieldKartAPMod
{
    [Serializable]
    public class SlotDataException : ApplicationException
    {
        public SlotDataException() { }

        public SlotDataException(string message) : base(message) 
        {
        }

        public SlotDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
