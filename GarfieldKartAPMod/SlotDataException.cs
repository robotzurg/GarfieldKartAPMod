using System;

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
