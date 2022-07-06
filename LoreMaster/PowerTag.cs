using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster;

public enum PowerTag
{
    /// <summary>
    /// The power is only available at the zone where it is found (or globally, if all in the zone are collected).
    /// Is default state.
    /// </summary>
    Local,

    /// <summary>
    /// The power is available at all times.
    /// </summary>
    Global,

    /// <summary>
    /// Functions normally, but is not required to globally activate the other ones in the zone.
    /// </summary>
    Exclude,

    /// <summary>
    /// Doesn't work, but it's collection behaves normal.
    /// </summary>
    Disabled,

    /// <summary>
    /// Doesn't work and is not required to globally activate the other ones in the zone.
    /// </summary>
    Removed
}
