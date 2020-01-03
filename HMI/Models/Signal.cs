using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMI.Models
{
    public class Signal
    {
        public string Key;
        public string Name;
        public string Type;
        public string Controll;

        public string stringValue;
        public bool boolValue;
        public ushort numValue;
        public int intValue;
        public int Sequence;
        public bool Devider;
        public string Label;
        public int Size;

        public int signalGroup;

        public List<KeyValuePair<string, string>> AllowedValues;
    }
}
