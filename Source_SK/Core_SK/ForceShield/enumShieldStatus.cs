using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enhanced_Development.Shields
{

    public enum enumShieldStatus
    {
        //Disabled and offline
        Disabled,
        //Warming up
        Loading,
        //Online and gathering power
        Charging,
        //Charged and sustaining
        Sustaining
    }
}
