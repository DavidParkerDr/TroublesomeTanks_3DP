﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tankontroller.Controller
{
    public class PortMapping
    {
        public static int getPortForPlayer(int pJackNum) // Eww. Sorry
        {

            if (pJackNum == 0) // As the ports dont line up in code/IRL, I've had to hard code these - JD
                return 5;
            else if (pJackNum == 1)
                return 4;
            else if (pJackNum == 2)
                return 3;
            else if (pJackNum == 3)
                return 2;
            else if (pJackNum == 4)
                return 1;
            else if (pJackNum == 5)
                return 0;
            else if (pJackNum == 6)
                return 6;

            return -1;
        }
    }
}
