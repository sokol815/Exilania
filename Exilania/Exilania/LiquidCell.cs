using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    public class LiquidCell
    {
        public bool empty;
        public bool pressurized;
        public int pressure;
        public bool can_transfer;
        public Rectangle bounds; //top left corner of the world and width and height of rectangle
        public int id;
        public int tot_liquid_level;
        public int liquid_id;
        public int cell_above;
        public List<int> touching_ids;

        public LiquidCell()
        {

        }

        public LiquidCell(Rectangle b, World w, int pid,bool pcantransfer)
        {
            pressure = 0;
            cell_above = -1;
            can_transfer = pcantransfer;
            empty = false;
            id = pid;
            touching_ids = new List<int>();
            bounds = b;
            liquid_id = w.map[b.X, b.Y].liquid_id;
            //first expand down as far as possible, then expand sideways.
            if (w.map[b.X, b.Y].liquid_level < 100)
            {
                pressurized = false;
            }
            else
            {
                pressurized = true;
            }
            calculate_capacities(w);
            map_to_cell(w);
        }

        public void calculate_capacities(World w)
        {
            tot_liquid_level = 0;
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                for (int y = bounds.Top; y < bounds.Bottom; y++)
                {
                    tot_liquid_level += w.map[w.wraparound_x(x), y].liquid_level;
                }
            }
        }

        public int get_higher_connected(int orig_y, World w, ref List<int> t)
        {
            if (t.Count > 30 || !can_transfer)
                return -1;
            if (bounds.Y < orig_y)
                return id;
            int ret = -1;
            for (int i = 0; i < touching_ids.Count; i++)
            {
                if (!t.Contains(touching_ids[i]))
                {
                    t.Add(touching_ids[i]);
                    ret = w.liquid_simulator.liquid_cells[touching_ids[i]].get_higher_connected(orig_y, w, ref t);
                    if (ret != -1)
                        return ret;
                    else if (t.Count > 30)
                        break;
                }
            }
            return -1;
        }

        public int get_quantity_visible_at_level(int y_level, int quantity_greater, World w)
        {
            if (!can_transfer)
                return 0;
            if (y_level >= bounds.Bottom || (pressure > 0 /*&& LiquidSimulator.max_liquid - w.map[bounds.X,bounds.Y].liquid_level < 5*/))
                return tot_liquid_level;
            else
            {
                int cell_capacity = (tot_liquid_level / (bounds.Width * bounds.Height));
                int quant_return = 0;
                int layers_above_y = y_level - bounds.Top;
                quant_return += layers_above_y * bounds.Width * cell_capacity;
                quant_return += (cell_capacity - quantity_greater) * bounds.Width / (bounds.Width + 1);
                //quant_return = quant_return -Exilania.rand.Next(0, 5);
                if (quant_return > tot_liquid_level)
                {
                    Exilania.display.add_message("@05You need to fix the quant_Visible calc... it is whacky.");
                }
                return quant_return;
            }
        }

        /// <summary>
        /// calling this will spread the liquid out evenly among each level, with the top level having the least/none if it needs to. it will also add itself to the updater stack.
        /// </summary>
        /// <param name="w"></param>
        public void distribute_liquid(World w, bool add_unassigned_liquids)
        {
            int liquid_remaining = tot_liquid_level;
            for (int y = bounds.Bottom - 1; y > bounds.Top - 1; y--)
            {
                if (liquid_remaining >= bounds.Width * LiquidSimulator.max_liquid)
                { //full liquid for all these puppies
                    for (int x = bounds.Left; x < bounds.Right; x++)
                    {
                        if (w.map[x, y].liquid_level == 0)
                            w.minimap.update_loc(x, y, (sbyte)World.liquid_blocks[w.liquid_simulator.cur_liquid]);
                        w.map[x, y].liquid_level = 100;
                        liquid_remaining -= 100;
                        w.map[x, y].liquid_id = (byte)liquid_id;
                        if (!pressurized)
                        {
                            w.map[x, y].liquid_cell_id = -1;
                        }
                    }
                }
                else
                {
                    int unassigned_liquid = liquid_remaining;
                    //may need to use the code here that gets the pressure at level x...
                    int liquid_dole_out = liquid_remaining / bounds.Width;

                    unassigned_liquid -= (liquid_dole_out * bounds.Width);
                    for (int x = bounds.Left; x < bounds.Right; x++)
                    {
                        w.map[x, y].liquid_level = (byte)liquid_dole_out;
                        liquid_remaining -= liquid_dole_out;
                        if (w.map[x, y].liquid_level > 0)
                        {
                            w.map[x, y].liquid_id = (byte)liquid_id;
                            w.minimap.update_loc(x, y, (sbyte)World.liquid_blocks[w.liquid_simulator.cur_liquid]);
                        }
                        else
                        {
                            if (w.liquid_simulator.removed_cell_stack.Contains(id))
                                w.liquid_simulator.removed_cell_stack.Add(id);
                            w.map[x, y].liquid_cell_id = -1;
                            w.minimap.update_loc(x, y, -1);
                            //w.map[x, y].liquid_id = 0;
                            //empty = true;
                        }
                    }
                    if (add_unassigned_liquids)
                    {
                        int p_una = unassigned_liquid;
                        for (int x = bounds.Left; x < bounds.Right && p_una > 0; x++)
                        {
                            w.map[x, bounds.Top].liquid_level++;
                            p_una--;
                        }
                    }
                }
            }
            Point update_at = new Point(bounds.X, bounds.Y);
            update_at.X = (update_at.X / LiquidSimulator.cell_size) * LiquidSimulator.cell_size;
            update_at.Y = (update_at.Y / LiquidSimulator.cell_size) * LiquidSimulator.cell_size;
            if (!w.liquid_simulator.cells_need_update.Contains(update_at))
            {
                w.liquid_simulator.cells_need_update.Add(update_at);
            }
        }

        public void map_to_cell(World w)
        {
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                for (int y = bounds.Top; y < bounds.Bottom; y++)
                {
                    w.map[w.wraparound_x(x), y].liquid_cell_id = id;
                }
            }
        }

        public override string ToString()
        {
            string all = "";
            for (int i = 0; i < touching_ids.Count; i++)
            {
                all += touching_ids[i].ToString() + " ";
            }
            return "Level: " + tot_liquid_level + " " +  (pressurized?"@08":"") +  "pressure: " + pressure + " cell_above: " + cell_above + "\n{" + all + "}";
        }
    }
}
