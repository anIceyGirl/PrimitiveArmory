using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace PrimitiveArmory
{
    public class CreatureHooks
    {
        public static Dictionary<Creature, CreatureState> critterState;

        public struct CreatureState
        {
            public bool blank;
        }
    }
}
