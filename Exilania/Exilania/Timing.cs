using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public class Timing
    {
        List<int> time_slot;
        string time_name;
        long cumulative_time;
        long last_time_step;
        long msec_step;
        public float average_time;
        long last_sto_cum_time;
        static int steps_keep = 100;

        public Timing()
        {
            time_slot = new List<int>();
            time_name = "Default Timer";
            cumulative_time = 0;
            last_time_step = System.DateTime.Now.Ticks / 10000;
            msec_step = 0;
            average_time = 0;
            last_sto_cum_time = 0;
        }

        public Timing(string name)
        {
            time_slot = new List<int>();
            time_name = name;
            cumulative_time = 0;
            last_time_step = System.DateTime.Now.Ticks / 10000;
            msec_step = 0;
            average_time = 0;
            last_sto_cum_time = 0;
        }

        public void start()
        {
            last_time_step = System.DateTime.Now.Ticks / 10000;
        }

        public void stop()
        {
            long cur_time = System.DateTime.Now.Ticks / 10000;
            msec_step = cur_time - last_time_step;
            time_slot.Add((int)msec_step);
            if (time_slot.Count > steps_keep)
            {
                cumulative_time -= time_slot[0];
                time_slot.RemoveAt(0);
            }
            
            cumulative_time += msec_step;
            last_sto_cum_time += msec_step;
            last_time_step = cur_time;
            if(time_slot.Count > 0)
            average_time = (float)((float)cumulative_time / (float)time_slot.Count);
        }

        public override string ToString()
        {
            return time_name + " AVG: " + Math.Round(average_time,2).ToString().PadLeft(6,' ') + " MSEC. ";
        }
    }
}
