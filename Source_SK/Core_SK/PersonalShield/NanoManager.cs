using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Core_SK.PersonalShields.Nano
{
    static class NanoManager
    {
        public static int currentCharge = 0;
        private static int maxCharge = 100;
        private static int previousTickExecuted = 0;


        //private static List<NanoConnector> connections = new List<NanoConnector>();


        public static int getCurrentCharge()
        {
            return currentCharge;
        }

        public static int getMaxCharge()
        {
            return maxCharge;
        }

        //TODO: Move to rare tick?
        public static void tick()
        {
            int currentTick = Find.TickManager.TicksGame;

            //Only every 20 ticks
            if (currentTick % 20 == 0)
            {
                //Check to not have multiple ticks at the same time
                if (currentTick != previousTickExecuted)
                {
                    //Record the previous tick
                    previousTickExecuted = currentTick;

                    //Recharge
                    NanoManager.addCharge();
                }
            }
        }

        public static void addCharge()
        {
            NanoManager.addCharge(1);
        }

        public static void addCharge(int chargeAmmount)
        {
            if (NanoManager.currentCharge < NanoManager.maxCharge)
            {
                NanoManager.currentCharge += chargeAmmount;
                if (NanoManager.currentCharge > NanoManager.maxCharge)
                {
                    NanoManager.currentCharge = NanoManager.maxCharge;
                }
            }
        }

        public static bool requestCharge(int chargeAmmount)
        {
            if (chargeAmmount <= NanoManager.currentCharge)
            {
                NanoManager.currentCharge -= chargeAmmount;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
