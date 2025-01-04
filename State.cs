using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF1006_tp1
{
    
    public class State
    {
        public bool IsFinal {get; private set;}
        public string Name { get; private set; }
        public List<Transition> Transitions { get; private set; }

        public State (string name, bool isFinal)
        {
            Name = name;
            IsFinal = isFinal;
            Transitions = new List<Transition>();    
        }

        public override string ToString()
        {
            return IsFinal ? $"({Name})" : Name;
        }
    }
}
