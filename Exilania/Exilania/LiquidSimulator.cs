using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace Exilania
{
    public class LiquidSimulator
    {
        public List<LiquidCell> liquid_cells;
        HashSet<Point> water = new HashSet<Point>();
        HashSet<Point> overflow_water = new HashSet<Point>();
        public List<Point> cells_need_update;
        public List<Point> cells_before_need_update;
        //public Point[] all_pts_on_last = new Point[0];
        public List<int> removed_cell_stack;
        public sbyte water_block_id = Exilania.block_types.get_block_by_name("Water");
        public float water_update_interval = 1f/60f;
        public float default_water_update_interval = 1f / 60f;
        public float elapsed_water_update_time = 0;
        public int frames_reset_pressure = 120;
        public bool run_liquid_rtl = false;
        public int water_frame = 0;
        public static byte max_liquid = 100;
        public static int pic_pressurized = 955;
        public static int pic_unpressurized = 956;
        public static int pic_look_at = 1021;
        public static int cell_size = 10;
        public static int magic_stack_size_num = 3000;
        int called = 0;

        public int cur_liquid = 0;

        public LiquidSimulator()
        {
            water = new HashSet<Point>();
            liquid_cells = new List<LiquidCell>();
            cells_need_update = new List<Point>();
            cells_before_need_update = new List<Point>();
            removed_cell_stack = new List<int>();
        }

        public void set_update_time(World w)
        {
            water_update_interval = Math.Max(w.water_update_time.average_time, default_water_update_interval);
        }

        public void move_overflow()
        {
            Point[] t;
            if (water.Count + overflow_water.Count > magic_stack_size_num)
            {
                t = new Point[Math.Min(overflow_water.Count, magic_stack_size_num - water.Count)];
                overflow_water.CopyTo(t, 0, magic_stack_size_num - water.Count);
            }
            else
            {
                t = new Point[overflow_water.Count];
                overflow_water.CopyTo(t, 0);
            }
            
            int t_iterator = 0;
            Point temp = new Point();
            while (t_iterator < t.Length && water.Count < magic_stack_size_num)
            {
                temp = t[t_iterator];
                if (!water.Contains(temp))
                {
                    water.Add(temp);
                }
                overflow_water.Remove(temp);
                t_iterator++;
            }
        }

        public int update(World w, float elapsed_time, bool just_do_one)
        {
            //if (water_frame % 100 == 0)
              //  set_update_time(w);
            move_overflow();
            if (just_do_one)
            {
                water_frame++;
                run_liquid_rtl = !run_liquid_rtl;
                if (water_frame % 3 == 0)
                {
                    //do oil and lava updates
                }
                //do water updates.
                do_pre_cell_refactoring(w);
                run_liquid(1, w);
                do_cell_refactoring(w);
                check_cell_integrity(w);
                if (water_frame % frames_reset_pressure == 0)
                {
                    reset_pressure();
                }
                update_pressure(w);
            }
            else
            {
                elapsed_water_update_time += elapsed_time;
                while (elapsed_water_update_time > water_update_interval)
                {
                    water_frame++;
                    run_liquid_rtl = !run_liquid_rtl;
                    if (water_frame % 3 == 0)
                    {
                        //do oil and lava updates
                    }
                    //do water updates.
                    do_pre_cell_refactoring(w);
                    run_liquid(1, w);
                    do_cell_refactoring(w);
                    check_cell_integrity(w);
                    if (water_frame % frames_reset_pressure == 0)
                    {
                        reset_pressure();
                    }
                    update_pressure(w);
                    //if(Exilania.debug == "")
                    //Exilania.debug = "@05Liquid: " + water.Count.ToString() + " liquid-overflow: " + overflow_water.Count;
                    //else
                    //    Exilania.debug += " @05Liquid: " + water.Count.ToString() + " liquid-overflow: " + overflow_water.Count;
                    elapsed_water_update_time -= water_update_interval;
                }
            }
            return water.Count + overflow_water.Count;
        }

        public void add_to_before_cell_update(Point loc)
        {
            loc.X -= loc.X % cell_size;
            loc.Y -= loc.Y % cell_size;
            if (!cells_before_need_update.Contains(loc))
                cells_before_need_update.Add(loc);
        }


        public void add_liquid_of_interest(Point loc, int liquid_id)
        {
            switch (liquid_id)
            {
                case 1:
                    if (water.Count < magic_stack_size_num)
                    {
                        if (!water.Contains(loc))
                            water.Add(loc);
                    }
                    else if(!overflow_water.Contains(loc))
                    {
                        overflow_water.Add(loc);
                    }
                    break;
                default: //-1 or 0 will do this... add to all different types.
                    if (water.Count < magic_stack_size_num)
                    {
                        if (!water.Contains(loc))
                            water.Add(loc);
                    }
                    else if (!overflow_water.Contains(loc))
                    {
                        overflow_water.Add(loc);
                    }
                    break;
            }
        }

        public void reset_pressure()
        {
            for (int i = 0; i < liquid_cells.Count; i++)
            {
                liquid_cells[i].pressure = 0;
                //liquid_cells[i].pressure = Math.Max(0,liquid_cells[i].pressure-100);
                liquid_cells[i].cell_above = -1;
            }
        }

        public void update_pressure(World w)
        {
            int cell_id = 0;
            int width_accounted_for = 0;
            for (int i = 0; i < liquid_cells.Count; i++)
            {
                if (!liquid_cells[i].empty && liquid_cells[i].can_transfer)
                {
                    width_accounted_for = 0;
                    for (int j = 0; j < liquid_cells[i].touching_ids.Count; j++)
                    {
                        //i is currently updating it's neighbor, j; hereafter "cell_id"
                        cell_id = liquid_cells[i].touching_ids[j];
                        //the cell it is touching can transfer
                        if (liquid_cells[cell_id].can_transfer && liquid_cells[cell_id].pressurized)
                        {
                            if ((liquid_cells[cell_id].bounds.Y < liquid_cells[i].bounds.Y ||
                                (liquid_cells[cell_id].cell_above != -1 && liquid_cells[liquid_cells[cell_id].cell_above].bounds.Y < liquid_cells[i].bounds.Y))
                                && liquid_cells[i].cell_above == -1)
                            {
                                liquid_cells[i].cell_above = (liquid_cells[cell_id].cell_above != -1 ? liquid_cells[cell_id].cell_above : cell_id);
                                liquid_cells[i].pressure = (liquid_cells[i].bounds.Y - liquid_cells[liquid_cells[i].cell_above].bounds.Y) * 100;
                            }
                            else if (liquid_cells[cell_id].cell_above != -1 && liquid_cells[i].cell_above != -1 &&
                                liquid_cells[liquid_cells[i].cell_above].bounds.Y > liquid_cells[liquid_cells[cell_id].cell_above].bounds.Y)
                            {
                                liquid_cells[i].cell_above = liquid_cells[cell_id].cell_above;
                                liquid_cells[i].pressure = (liquid_cells[i].bounds.Y - liquid_cells[liquid_cells[i].cell_above].bounds.Y) * 100;
                            }
                        }
                    }
                    if (water_frame % 31 == 0 && width_accounted_for < liquid_cells[i].bounds.Width && liquid_cells[i].bounds.Y > 0 && 
                        liquid_cells[i].pressure > 0)
                    {
                        if (max_liquid - w.map[liquid_cells[i].bounds.X, liquid_cells[i].bounds.Y].liquid_level == 0)
                        {
                            for (int x = liquid_cells[i].bounds.Left; x < liquid_cells[i].bounds.Right; x++)
                            {
                                add_liquid_of_interest(new Point(x, liquid_cells[i].bounds.Y - 1), liquid_cells[i].liquid_id);
                            }
                        }
                        else
                        {
                            for (int x = liquid_cells[i].bounds.Left; x < liquid_cells[i].bounds.Right; x++)
                            {
                                add_liquid_of_interest(new Point(x, liquid_cells[i].bounds.Bottom-1), liquid_cells[i].liquid_id);
                            }
                        }
                    }
                }
            }
        }

        public void decrement_list(HashSet<Point> t)
        {
            t.Clear();
            /*List<Point> keys = new List<Point>(t.Keys);
            foreach (var p in keys)
            {
                if (t[p] <= 0)
                {
                    t.Remove(p);
                }
                else
                    t[p]--;
            }*/
        }

        public void do_cell_refactoring(World w)
        {
            for (int x = 0; x < cells_need_update.Count; x++)
            {
                redo_cells_at_loc(cells_need_update[x], w);
            }
            cells_need_update.Clear();
        }

        public void do_pre_cell_refactoring(World w)
        {

            for (int x = 0; x < cells_before_need_update.Count; x++)
            {
                redo_cells_at_loc(cells_before_need_update[x], w);
            }
            cells_before_need_update.Clear();
        }

        public void run_liquid(int liquid_id, World w)
        {
           
            IEnumerable<Point> get_list;

            Point[] temp = new Point[water.Count];
            water.CopyTo(temp, 0);
            cur_liquid = liquid_id;
            switch (liquid_id)
            {
                case 1:
                    if (run_liquid_rtl)
                    {
                        get_list = temp.OrderByDescending(s => s.Y).ThenBy(s => s.X);
                    }
                    else
                    {
                        get_list = temp.OrderByDescending(s => s.Y).ThenByDescending(s => s.X);
                    }
                    temp = get_list.ToArray<Point>();
                    decrement_list(water);
                    break;
                default:
                    Exilania.display.add_message("@08You need to add logic for other types of liquids.");
                    break;
            }
            for (int i = 0; i < temp.Length; i++)
            {
                if (is_valid_transfer_target(temp[i], w))
                {
                    if (w.map[temp[i].X, temp[i].Y].liquid_cell_id == -1)
                    {
                        fill_with_liquid(temp[i], w);
                    }
                    else
                    {
                        fill_cell_with_liquid(temp[i], w);
                    }
                }
            }
            
        }


        /// <summary>
        /// for use with cells that are not completely full, so they can have some liquids added to them.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="w"></param>
        public void fill_cell_with_liquid(Point loc, World w)
        {
            //this box is part of a cell, try to make it's liquid increased from that of its neighbors.
            int this_cell_id = w.map[loc.X,loc.Y].liquid_cell_id;
            bool val_changed = false;
            int donate_amt = 0;
            int liquid_needed = liquid_cells[this_cell_id].bounds.Width * liquid_cells[this_cell_id].bounds.Height * max_liquid;
            liquid_needed -= liquid_cells[this_cell_id].tot_liquid_level;
            if (liquid_needed <= 0)
            {
                return;
            }
            //try to get liquid from above first
            donate_amt = is_valid_donor(new Point(loc.X, loc.Y - 1), loc, w);
            if (donate_amt > 0)
            { //the cell above is willing to transfer liquid to this cell.
                Point t = new Point(loc.X, loc.Y - 1);
                donate_amt = Math.Min(donate_amt, liquid_needed);
                liquid_needed -= donate_amt;
                add_liquid_to_cell(this_cell_id, donate_amt, w, true);
                take_liquid_from_cell_at_loc(t, donate_amt, w, true);
                val_changed = true;
            }

            //try get from below
            if (!val_changed && liquid_needed > 0 && loc.Y < w.map.GetLength(1) - 1 && w.map[loc.X, loc.Y + 1].liquid_cell_id != w.map[loc.X, loc.Y].liquid_cell_id)
            {
                int id_below = w.map[loc.X, loc.Y + 1].liquid_cell_id;
                if (id_below != -1 && liquid_cells[id_below].pressure > 0)
                {
                    int take_from_id = -1;
                    if (liquid_cells[id_below].cell_above != -1 &&
                        liquid_cells[liquid_cells[id_below].cell_above].can_transfer && !liquid_cells[liquid_cells[id_below].cell_above].empty)
                        take_from_id = liquid_cells[id_below].cell_above;
                    else
                    {
                        // List<int> t = new List<int>();
                        // t.Add(this_cell_id);
                        // take_from_id = liquid_cells[id_below].get_higher_connected(loc.Y, w, ref t);
                    }
                    if (take_from_id > -1)
                    {
                        donate_amt = is_valid_donor(new Point(liquid_cells[take_from_id].bounds.X, liquid_cells[take_from_id].bounds.Y), loc, w);
                        if (donate_amt > 0)
                        {
                            donate_amt = Math.Min(donate_amt, liquid_needed);
                            add_liquid_to_cell(this_cell_id, donate_amt, w, true);
                            take_liquid_from_cell_at_loc(new Point(liquid_cells[take_from_id].bounds.X, liquid_cells[take_from_id].bounds.Y), donate_amt, w, true);
                            liquid_needed -= donate_amt;
                            val_changed = true;
                        }
                    }
                }
            }
           
            if (!val_changed)
            {
                //try get from left and right
                Point right = new Point(0, -1);
                Point left = new Point(0, -1);
                int left_outside = w.wraparound_x(liquid_cells[this_cell_id].bounds.Left - 1);
                int right_outside = w.wraparound_x(liquid_cells[this_cell_id].bounds.Right);
                for (int y = liquid_cells[this_cell_id].bounds.Bottom - 1; y > liquid_cells[this_cell_id].bounds.Top - 1; y--)
                {
                    if (w.map[left_outside, y].liquid_cell_id != -1 && w.map[left_outside, y].liquid_id == cur_liquid
                        && liquid_cells[w.map[left_outside, y].liquid_cell_id].pressure >= liquid_cells[this_cell_id].pressure)
                    {
                        left = new Point(left_outside, y);
                    }
                    if (w.map[right_outside, y].liquid_cell_id != -1 && w.map[right_outside, y].liquid_id == cur_liquid
                        && liquid_cells[w.map[right_outside, y].liquid_cell_id].pressure >= liquid_cells[this_cell_id].pressure)
                    {
                        right = new Point(right_outside, y);
                    }
                }
                int donate_right = is_valid_donor(right, loc, w);
                int donate_left = is_valid_donor(left, loc, w);
                if ((donate_right > 0 || donate_left > 0))
                {
                    bool odd = false;
                    int remain = 0;
                    int diff = 0;
                    if (donate_left > donate_right)
                    {
                        diff = donate_left - donate_right;
                        if (diff >= liquid_needed) //the left has all the liquid needed to satisfy the requirements.
                        {
                            donate_right = 0;
                            donate_left = Math.Min(donate_left, liquid_needed);
                        }
                        else
                        {
                            odd = ((liquid_needed - diff) % 2) == 1;
                            remain = (liquid_needed - diff) / 2;
                            donate_right = Math.Min(donate_right, remain);
                            donate_left = Math.Min(donate_left, remain);
                        }
                    }
                    else if (donate_right > donate_left)
                    {
                        diff = donate_right - donate_left;
                        if (diff >= liquid_needed) //the right has all the liquid needed to satisfy the requirements.
                        {
                            donate_left = 0;
                            donate_right = Math.Min(donate_right, liquid_needed);
                        }
                        else
                        {
                            odd = ((liquid_needed - diff) % 2) == 1;
                            remain = (liquid_needed - diff) / 2;
                            donate_left = Math.Min(donate_left, remain);// +(water_frame % 2 == 0 && odd ? 1 : 0));
                            donate_right = Math.Min(donate_right, remain);// + (water_frame % 2 == 1 && odd ? 1 : 0));
                        }
                    }
                    else
                    { //the two are equal.. weird.
                        remain = liquid_needed / 2;
                        donate_left = Math.Min(remain, donate_left);
                        donate_right = Math.Min(donate_right, liquid_needed - donate_left);
                    }
                    donate_amt = 0;
                    if (donate_left > 0)
                        donate_amt += donate_left;
                    if (donate_right > 0)
                        donate_amt += donate_right;
                    liquid_needed -= donate_amt;

                    add_liquid_to_cell(this_cell_id, donate_amt, w, false);
                    if (donate_left > 0)
                    {
                        take_liquid_from_cell_at_loc(left, donate_left, w, false);
                    }
                    if (donate_right > 0)
                    {
                        take_liquid_from_cell_at_loc(right, donate_right, w, false);
                    }
                    val_changed = true;
                }
            }

           

            

            if (liquid_needed < 0)
                Exilania.display.add_message("@07The liquid needed is " + liquid_needed + " something bad happened!!!");
            
           
            
            if (val_changed)
            {
                add_liquid_of_interest(loc, cur_liquid);
            }
        }

        /// <summary>
        /// for use on areas that probably have no liquid, but definately have no cell id... this means they are about to get some liquid added to them.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="w"></param>
        public void fill_with_liquid(Point loc, World w)
        {
            bool val_changed = false;
            int donate_amt = 0;
            int liquid_needed = max_liquid - w.map[loc.X, loc.Y].liquid_level;
            if (liquid_needed <= 0) //already full of liquid, don't worry about it, brah!
                return;
            //try to get liquid from above first.
            donate_amt = is_valid_donor(new Point(loc.X, loc.Y - 1), loc, w);
            if (donate_amt > 0)
            {
                donate_amt = Math.Min(liquid_needed, donate_amt);
                liquid_needed -= donate_amt;
                w.map[loc.X, loc.Y].liquid_level += (byte)donate_amt;
                w.map[loc.X, loc.Y].liquid_id = (byte)cur_liquid;
                take_liquid_from_cell_at_loc(new Point(loc.X, loc.Y - 1), donate_amt, w, true);
                val_changed = true;
            }

            if (!val_changed)
            {
                //try to get liquid from left and right (equally) next
                int donate_right = is_valid_donor(new Point(w.wraparound_x(loc.X + 1), loc.Y), loc, w);
                int donate_left = is_valid_donor(new Point(w.wraparound_x(loc.X - 1), loc.Y), loc, w);
                if ((donate_right > 0 || donate_left > 0))
                {
                    bool odd = false;
                    int remain = 0;
                    int diff = 0;
                    if (donate_left > donate_right)
                    {
                        diff = donate_left - donate_right;
                        if (diff >= liquid_needed) //the left has all the liquid needed to satisfy the requirements.
                        {
                            donate_right = 0;
                            donate_left = Math.Min(liquid_needed, donate_left);
                        }
                        else
                        {
                            odd = ((liquid_needed - diff) % 2) == 1;
                            remain = (liquid_needed - diff) / 2;
                            donate_right = Math.Min(donate_right, remain);
                            donate_left = Math.Min(donate_left, remain);
                        }
                    }
                    else if (donate_right > donate_left)
                    {
                        diff = donate_right - donate_left;
                        if (diff >= liquid_needed) //the right has all the liquid needed to satisfy the requirements.
                        {
                            donate_left = 0;
                            donate_right = Math.Min(liquid_needed, donate_right);
                        }
                        else
                        {
                            odd = ((liquid_needed - diff) % 2) == 1;
                            remain = (liquid_needed - diff) / 2;
                            donate_left = Math.Min(donate_left, remain);
                            donate_right = Math.Min(donate_right, remain);
                        }
                    }
                    else
                    { //the two are equal.. weird.

                        remain = liquid_needed / 2;
                        donate_left = Math.Min(remain, donate_left);
                        donate_right = Math.Min(donate_right, remain);
                    }
                    donate_amt = 0;
                    if (donate_left > 0)
                        donate_amt += donate_left;
                    if (donate_right > 0)
                        donate_amt += donate_right;
                    liquid_needed -= donate_amt;
                    w.map[loc.X, loc.Y].liquid_level += (byte)donate_amt;
                    w.map[loc.X, loc.Y].liquid_id = (byte)cur_liquid;
                    if (donate_left > 0)
                        take_liquid_from_cell_at_loc(new Point(w.wraparound_x(loc.X - 1), loc.Y), donate_left, w, false);
                    if (donate_right > 0)
                        take_liquid_from_cell_at_loc(new Point(w.wraparound_x(loc.X + 1), loc.Y), donate_right, w, false);
                    val_changed = true;
                }
            }

            //http://onlinesequencer.net/3536

            //try to get liquid from below last (if you can!) AT LEAST PARTIAL CULPRITE....
            if (!val_changed && liquid_needed > 0 && loc.Y < w.map.GetLength(1) - 1 && w.map[loc.X, loc.Y + 1].liquid_cell_id != w.map[loc.X, loc.Y].liquid_cell_id
                && max_liquid - w.map[loc.X, loc.Y + 1].liquid_level < 5)
            {
                int id_below = w.map[loc.X, loc.Y + 1].liquid_cell_id;
                if (id_below != -1 && liquid_cells[id_below].pressure > 0)
                {
                    donate_amt = is_valid_donor(new Point(liquid_cells[id_below].bounds.X, liquid_cells[id_below].bounds.Y), loc, w);
                    if (donate_amt > 0)
                    {
                        int take_from_id = -1;
                        if (liquid_cells[id_below].cell_above != -1 &&
                            liquid_cells[liquid_cells[id_below].cell_above].can_transfer && !liquid_cells[liquid_cells[id_below].cell_above].empty)
                            take_from_id = liquid_cells[id_below].cell_above;
                        else
                        {
                            //   List<int> t = new List<int>();
                            //   t.Add(id_below);
                            //   take_from_id = liquid_cells[id_below].get_higher_connected(loc.Y, w, ref t);
                        }
                        if (take_from_id > -1)
                        {
                            donate_amt = is_valid_donor(new Point(liquid_cells[take_from_id].bounds.X, liquid_cells[take_from_id].bounds.Y), loc, w);
                            if (donate_amt > 0)
                            {
                                donate_amt = Math.Min(liquid_needed, donate_amt);
                                liquid_needed -= donate_amt;
                                w.map[loc.X, loc.Y].liquid_level += (byte)donate_amt;
                                w.map[loc.X, loc.Y].liquid_id = (byte)cur_liquid;
                                take_liquid_from_cell_at_loc(new Point(liquid_cells[take_from_id].bounds.X, liquid_cells[take_from_id].bounds.Y), donate_amt, w, true);
                                val_changed = true;
                            }
                        }
                    }
                }
            }


            //add to update stack!
            if (val_changed)
            {
                add_liquid_of_interest(loc, cur_liquid);
                //add children to stack
                Point t = new Point();
                for (int i = 0; i < World.lightxes.Length; i++)
                {
                    t = new Point(w.wraparound_x(loc.X + World.lightxes[i]), loc.Y + World.lightyes[i]);
                    add_liquid_of_interest(t, cur_liquid);
                }
                loc.X -= loc.X % cell_size;
                loc.Y -= loc.Y % cell_size;
                if (!cells_need_update.Contains(loc))
                    cells_need_update.Add(loc);
            }
        }

        public int add_liquid_to_cell(int this_cell_id, int amt, World w, bool force)
        {
            liquid_cells[this_cell_id].tot_liquid_level += amt;
            if (amt > liquid_cells[this_cell_id].bounds.Width * liquid_cells[this_cell_id].bounds.Height * max_liquid)
            {
                Exilania.display.add_message("@08Cannot be over!! WHAT HAVE YOU DONE!!?!?");
            }
            int left_outside = w.wraparound_x(liquid_cells[this_cell_id].bounds.Left - 1);
            int right_outside = w.wraparound_x(liquid_cells[this_cell_id].bounds.Right);
            if (amt > 5 || !liquid_cells[this_cell_id].can_transfer || force || (liquid_cells[this_cell_id].bounds.Width < 3 && liquid_cells[this_cell_id].bounds.Height == 1))
            {
                for (int y = liquid_cells[this_cell_id].bounds.Bottom - 1; y > liquid_cells[this_cell_id].bounds.Top - 1; y--)
                {
                    add_liquid_of_interest(new Point(left_outside, y), cur_liquid);
                    add_liquid_of_interest(new Point(right_outside, y), cur_liquid);
                    //add_liquid_of_interest(new Point(w.wraparound_x(left_outside+1), y), cur_liquid);
                    //add_liquid_of_interest(new Point(w.wraparound_x(right_outside-1), y), cur_liquid);
                }
                for (int x = liquid_cells[this_cell_id].bounds.Left; x < liquid_cells[this_cell_id].bounds.Right; x++)
                {
                    add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Top - 1), cur_liquid);
                    add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Top), cur_liquid);
                    //add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Bottom-1), cur_liquid);
                    add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Bottom), cur_liquid);
                }
            }
            Point loc = new Point(liquid_cells[this_cell_id].bounds.X, liquid_cells[this_cell_id].bounds.Y);
            loc.X -= loc.X % cell_size;
            loc.Y -= loc.Y % cell_size;
            if (!cells_need_update.Contains(loc))
                cells_need_update.Add(loc);
            /*for (int i = 0; i < World.xes.Length; i++)
            {
                Point t = new Point(loc.X + (World.xes[i] * cell_size), loc.Y + (World.yes[i] * cell_size));
                if (!cells_need_update.Contains(t))
                    cells_need_update.Add(t);
            }*/
            return liquid_cells[this_cell_id].tot_liquid_level;
        }

        public int take_liquid_from_cell_at_loc(Point loc, int amt, World w, bool force)
        {
            int this_cell_id = w.map[loc.X, loc.Y].liquid_cell_id;
            if (!w.world_ending)
            {
                liquid_cells[this_cell_id].tot_liquid_level -= amt;
                int left_outside = w.wraparound_x(liquid_cells[this_cell_id].bounds.Left - 1);
                int right_outside = w.wraparound_x(liquid_cells[this_cell_id].bounds.Right);
                //if (amt > 5 || !liquid_cells[this_cell_id].can_transfer || force || (liquid_cells[this_cell_id].bounds.Width < 3 && liquid_cells[this_cell_id].bounds.Height == 1))
                {
                    for (int y = liquid_cells[this_cell_id].bounds.Bottom - 1; y > liquid_cells[this_cell_id].bounds.Top - 1; y--)
                    {
                        add_liquid_of_interest(new Point(left_outside, y), cur_liquid);
                        add_liquid_of_interest(new Point(right_outside, y), cur_liquid);
                        //add_liquid_of_interest(new Point(w.wraparound_x(left_outside+1), y), cur_liquid);
                        //add_liquid_of_interest(new Point(w.wraparound_x(right_outside-1), y), cur_liquid);
                    }
                    for (int x = liquid_cells[this_cell_id].bounds.Left; x < liquid_cells[this_cell_id].bounds.Right; x++)
                    {
                        add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Top - 1), cur_liquid);
                        add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Top), cur_liquid);
                        //add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Bottom - 1), cur_liquid);
                        add_liquid_of_interest(new Point(x, liquid_cells[this_cell_id].bounds.Bottom), cur_liquid);
                    }

                }
                loc.X -= loc.X % cell_size;
                loc.Y -= loc.Y % cell_size;
                if (!cells_need_update.Contains(loc))
                    cells_need_update.Add(loc);
                /*for (int i = 0; i < World.xes.Length; i++)
                {
                    Point t = new Point(loc.X + (World.xes[i] * cell_size), loc.Y + (World.yes[i] * cell_size));
                    if (!cells_need_update.Contains(t))
                        cells_need_update.Add(t);
                }*/
            }
            return liquid_cells[this_cell_id].tot_liquid_level;
        }

        public bool has_more_liquid(Point from, Point to, World w)
        {
            if (w.map[from.X, from.Y].liquid_level > w.map[to.X, to.Y].liquid_level)
                return true;
            return false;
        }

        public bool is_valid_transfer_target(Point to, World w)
        {
            to.X = w.wraparound_x(to.X);
            if (to.Y >= w.map.GetLength(1) || to.Y < 0 || !w.map[to.X, to.Y].passable ||
                (w.map[to.X, to.Y].liquid_id != 0 && (w.map[to.X, to.Y].liquid_id != cur_liquid || w.map[to.X, to.Y].liquid_level == max_liquid)) ||
                (w.map[to.X, to.Y].furniture_index != -1 && !w.furniture[w.map[to.X, to.Y].furniture_index].flags[(int)FFLAGS.PASSABLE]) ||
                (w.map[to.X, to.Y].plant_index != -1 && !w.plants[w.map[to.X, to.Y].plant_index].passable))
                return false;
            return true;
        }

        public int is_valid_donor(Point from, Point to, World w)
        {
            if (from.Y > -1 && from.Y < w.map.GetLength(1) && w.map[from.X, from.Y].liquid_id == cur_liquid &&
                w.map[from.X, from.Y].liquid_cell_id != -1 && w.map[from.X, from.Y].liquid_cell_id != w.map[to.X, to.Y].liquid_cell_id)
            {
                int amt = liquid_cells[w.map[from.X, from.Y].liquid_cell_id].get_quantity_visible_at_level(to.Y, w.map[to.X, to.Y].liquid_level, w);
                if (amt > 0)
                    return amt;
            }
            return -1;
        }

        public void init_liquid_cells(World w)
        {
            liquid_cells = new List<LiquidCell>();
            for (int i = 0; i < w.map.GetLength(0) / cell_size; i++)
            {
                for (int j = 0; j < w.map.GetLength(1) / cell_size; j++)
                {
                    create_cells_in_area(w, i * cell_size, j * cell_size);
                }
            }
            for (int x = 0; x < liquid_cells.Count; x++)
            {
                link_cell(x, w);
                add_liquid_to_cell(x, 0, w, true);
            }
        }

        public List<int> create_cells_in_area(World w, int xstart, int ystart)
        {
            List<int> cells_made = new List<int>();
            int width = Math.Min(w.map.GetLength(0) - xstart, cell_size);
            int height = Math.Min(w.map.GetLength(1) - ystart, cell_size);
            Point grid_max = new Point(width + xstart, height + ystart);
            Point grid_minimum = new Point(xstart,ystart);
            
            bool[,] selector = new bool[width,height];
            bool can_transfer = true;
            for (int y = ystart + height - 1; y > ystart - 1; y--)
            {
                for (int x = xstart; x < xstart + width; x++)
                {
                    if (selector[x - xstart, y - ystart] == false && w.map[x, y].liquid_level > 0)
                    {
                        if (w.map[x, y].liquid_id == 0)
                        {
                            if (w.map[x, y].liquid_cell_id != -1 && !liquid_cells[w.map[x,y].liquid_cell_id].empty)
                                w.map[x, y].liquid_id = (byte)liquid_cells[w.map[x, y].liquid_cell_id].liquid_id;
                            else
                            {
                                w.map[x, y].liquid_level = 0;
                                selector[x - xstart, y - ystart] = true;
                                continue;
                            }
                        }
                        can_transfer = true;
                        Rectangle use = form_largest_cell(new Point(x, y), grid_minimum, grid_max, w, ref selector, ref can_transfer);
                        int next_id = get_next_cell_id();
                        if (next_id == liquid_cells.Count)
                            liquid_cells.Add(new LiquidCell(use, w, liquid_cells.Count,can_transfer));
                        else
                        {
                            liquid_cells[next_id] = new LiquidCell(use, w, next_id,can_transfer);
                        }
                        cells_made.Add(next_id);
                    }
                    else if(selector[x - xstart, y - ystart] == false)
                    {
                        if (w.map[x, y].liquid_cell_id != -1)
                            liquid_cells[w.map[x, y].liquid_cell_id].empty = true;
                        w.map[x, y].liquid_cell_id = -1;
                        w.map[x, y].liquid_id = 0;
                        selector[x - xstart, y - ystart] = true;
                    }
                }
            }
            return cells_made;
        }

        public void redo_cells_at_loc(Point clicked, World w)
        {
            called++;
            clicked.X = w.wraparound_x(clicked.X);
            if (clicked.Y < 0 || clicked.Y > w.map.GetLength(1))
                return;
            int xstart = (clicked.X / cell_size) * cell_size;
            int ystart = (clicked.Y / cell_size) * cell_size;
            List<int> cells_affected = new List<int>();
            for (int x = xstart; x < xstart + cell_size; x++)
            {
                for (int y = ystart; y < ystart + cell_size; y++)
                {
                    if (w.map[x, y].liquid_cell_id != -1 && !cells_affected.Contains(w.map[x, y].liquid_cell_id))
                    {
                        
                        cells_affected.Add(w.map[x, y].liquid_cell_id);
                    }
                }
            }
            for (int i = 0; i < cells_affected.Count; i++)
            {
                remove_link_between_cells(cells_affected[i]);
                liquid_cells[cells_affected[i]].distribute_liquid(w,true);
                liquid_cells[cells_affected[i]].empty = true;
            }
            List<int> new_cells = create_cells_in_area(w, xstart, ystart);
            for (int x = 0; x < new_cells.Count; x++)
            {
                link_cell(new_cells[x],w);
            }
        }

        public void check_cell_integrity(World w)
        {
            for (int i = 0; i < removed_cell_stack.Count; i++)
            {
                if (w.map[liquid_cells[removed_cell_stack[i]].bounds.X, liquid_cells[removed_cell_stack[i]].bounds.Bottom-1].liquid_cell_id != removed_cell_stack[i])
                    liquid_cells[removed_cell_stack[i]].empty = true;
            }
            removed_cell_stack.Clear();
        }

        public void link_cell(int id,World w)
        {
            int use_cell_id = 0;
            for (int x = liquid_cells[id].bounds.Left; x < liquid_cells[id].bounds.Right; x++)
            {
                use_cell_id = w.map[x, liquid_cells[id].bounds.Top - 1].liquid_cell_id;
                if (use_cell_id != -1 && liquid_cells[use_cell_id].can_transfer && !liquid_cells[id].touching_ids.Contains(use_cell_id))
                {
                    liquid_cells[id].touching_ids.Add(use_cell_id);
                    liquid_cells[use_cell_id].touching_ids.Add(id);
                }
                if (liquid_cells[id].bounds.Bottom < w.map.GetLength(1) )
                {
                    use_cell_id = w.map[x, liquid_cells[id].bounds.Bottom].liquid_cell_id;
                    if (use_cell_id != -1 && liquid_cells[use_cell_id].can_transfer && !liquid_cells[id].touching_ids.Contains(use_cell_id))
                    {
                        liquid_cells[id].touching_ids.Add(use_cell_id);
                        liquid_cells[use_cell_id].touching_ids.Add(id);
                    }
                }
            }
            for (int y = liquid_cells[id].bounds.Top; y < liquid_cells[id].bounds.Bottom; y++)
            {
                use_cell_id = w.map[w.wraparound_x(liquid_cells[id].bounds.Left-1),y].liquid_cell_id;
                if (use_cell_id != -1 && liquid_cells[use_cell_id].can_transfer && !liquid_cells[id].touching_ids.Contains(use_cell_id))
                {
                    liquid_cells[id].touching_ids.Add(use_cell_id);
                    liquid_cells[use_cell_id].touching_ids.Add(id);
                }
                use_cell_id = w.map[w.wraparound_x(liquid_cells[id].bounds.Right), y].liquid_cell_id;
                if (use_cell_id != -1 && liquid_cells[use_cell_id].can_transfer && !liquid_cells[id].touching_ids.Contains(use_cell_id))
                {
                    liquid_cells[id].touching_ids.Add(use_cell_id);
                    liquid_cells[use_cell_id].touching_ids.Add(id);
                }
            }
        }

        public void remove_link_between_cells(int id)
        {
            for (int x = 0; x < liquid_cells[id].touching_ids.Count; x++)
            {
                liquid_cells[liquid_cells[id].touching_ids[x]].touching_ids.Remove(id);
            }
        }

        /// <summary>
        /// expand horizontally, then vertically
        /// </summary>
        /// <param name="bottom_left"></param>
        /// <param name="max_x"></param>
        /// <param name="min_y"></param>
        /// <param name="w"></param>
        /// <param name="sel"></param>
        public Rectangle form_largest_cell(Point bottom_left, Point grid_minimum, Point grid_maximum, World w, ref bool[,] sel, ref bool transferrable)
        {
            int liq_id_use = w.map[bottom_left.X,bottom_left.Y].liquid_id;
            bool pressurized = w.map[bottom_left.X,bottom_left.Y].liquid_level == 100;
            Rectangle contain = new Rectangle(bottom_left.X, bottom_left.Y, 1, 1);
            sel[bottom_left.X - grid_minimum.X, bottom_left.Y - grid_minimum.Y] = true;
            if(!w.map[contain.X,contain.Y].passable || (w.map[contain.X,contain.Y].furniture_index!= -1 && 
                !w.furniture[w.map[contain.X,contain.Y].furniture_index].flags[(int)FFLAGS.PASSABLE]))
            {
               transferrable = false;
                return contain;
            }
            bool cont = true;
            while (cont) //expand right
            {
                if (contain.Right >= grid_maximum.X || sel[contain.Right - grid_minimum.X,bottom_left.Y - grid_minimum.Y] == true || w.map[contain.Right, contain.Top].liquid_id != liq_id_use ||
                    w.map[contain.Right, contain.Top].liquid_level == 100 != pressurized || w.map[contain.Right, contain.Top].liquid_level == 0 || !w.map[contain.Right, contain.Top].passable
                    || (w.map[contain.Right, contain.Top].furniture_index != -1 && !w.furniture[w.map[contain.Right, contain.Top].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                {
                    cont = false;
                }
                if (cont)
                {
                    contain.Width++;
                    sel[contain.Right - 1 - grid_minimum.X,bottom_left.Y - grid_minimum.Y] = true;
                }
            }
            cont = true;
            while (cont && pressurized) //expand up
            {
                for (int x = contain.Left; x < contain.Right; x++)
                {
                    if (contain.Top <= grid_minimum.Y || sel[x - grid_minimum.X, contain.Top - grid_minimum.Y - 1] == true || w.map[x, contain.Top - 1].liquid_id != liq_id_use ||
                        w.map[x, contain.Top - 1].liquid_level == 100 != pressurized || w.map[x, contain.Top - 1].liquid_level == 0 || !w.map[x, contain.Top - 1].passable ||
                        (w.map[x, contain.Top - 1].furniture_index!= -1 && !w.furniture[w.map[x, contain.Top - 1].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    {
                        cont = false;
                    }
                }
                if (cont)
                {
                    for (int x = contain.Left; x < contain.Right; x++)
                    {
                        sel[x - grid_minimum.X, contain.Top - grid_minimum.Y-1] = true;
                    }
                    contain.Y--;
                    contain.Height++;
                }
            }
            return contain;
        }

        public int get_next_cell_id()
        {
            for (int x = 0; x < liquid_cells.Count; x++)
            {
                if (liquid_cells[x].empty)
                {
                    liquid_cells[x].pressure = 0;
                    return x;
                }
            }
            return liquid_cells.Count;
        }

        public void draw_liquid_cells(SpriteBatch s, Display d, World w, int x_offset, int y_offset, Vector2 top_left_corner)
        {
            Rectangle on_screen = new Rectangle(0,0, Exilania.screen_size.X, Exilania.screen_size.Y);
            Rectangle temp = new Rectangle();
            Vector2 temp_v = new Vector2();
            Point center_s = new Point();
            for (int i = 0; i < liquid_cells.Count; i++)
            {
                if (!liquid_cells[i].empty)
                {
                    temp = new Rectangle(liquid_cells[i].bounds.X * 24 - (int)top_left_corner.X,
                        liquid_cells[i].bounds.Y * 24 - (int)top_left_corner.Y, liquid_cells[i].bounds.Width * 24, liquid_cells[i].bounds.Height * 24);
                    if (on_screen.Intersects(temp))
                    {
                        if (liquid_cells[i].pressurized)
                            s.Draw(d.sprites, temp, d.frames[pic_pressurized], Color.White);
                        else
                            s.Draw(d.sprites, temp, d.frames[pic_unpressurized], Color.White);
                        center_s = temp.Center;
                        string words = liquid_cells[i].id.ToString();
                        temp_v = d.small_font.MeasureString(words);
                        center_s.X -= (int)(temp_v.X / 2);
                        center_s.Y -= (int)(temp_v.Y / 2);
                        d.draw_text(s, d.small_font, "@00" + words, center_s.X, center_s.Y, 500);
                    }
                }
            }
            foreach(var p in water)
            {
                temp = new Rectangle(((Point)p).X * 24 - (int)top_left_corner.X, ((Point)p).Y * 24 - (int)top_left_corner.Y, 24, 24);
                if (on_screen.Intersects(temp))
                {
                    s.Draw(d.sprites, temp, d.frames[pic_look_at], Color.White);
                }
            }
        }
    }
}
