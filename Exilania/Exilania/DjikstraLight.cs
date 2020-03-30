using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Exilania
{
    
    public class DjikstraLight
    {
        List<Point> active_nodes;
        int node_it = 0;
        public int num_visits = 0;


        public DjikstraLight()
        {
            active_nodes = new List<Point>();
        }

        public void run_djikstra(World w, byte[] ambient_light, Rectangle area_run)
        {
            num_visits = 0;
            if (area_run.Y < 0)
            {
                int offset = 0 - area_run.Y;
                area_run.Y = 0;
                area_run.Height -= offset;
            }
            if (area_run.Bottom >= w.map.GetLength(1))
            {
                area_run.Height = w.map.GetLength(1)  - area_run.Y;
            }
            clear_light(w, ambient_light, area_run);
            //the board has been set ready to run.
            complete_djikstra(w, area_run);
            update_light(w, area_run,ambient_light);
        }

        public void complete_djikstra(World w, Rectangle area_run)
        {
            node_it = 0;
            while (active_nodes.Count > 0)
            {
                //remove nodes from list that are not active anymore.
                while (active_nodes.Count > 0 && !w.map[active_nodes[node_it].X, active_nodes[node_it].Y].djikstra_node)
                {
                    active_nodes.RemoveAt(node_it);
                    /*if (node_it > 0)
                        node_it--;
                    if (node_it >= active_nodes.Count)
                        node_it = 0;*/
                }
                //node currently selected is an active node.. do something with it!
                if (active_nodes.Count > 0)
                update_neighbors(w, area_run);
                //node_it++;
                //if (node_it >= active_nodes.Count)
                //    node_it = 0;
            }
        }

        public void update_neighbors(World w,Rectangle area_run)
        {
           
            byte count_changed = 0;
            for (int i = 0; i < World.lightxes.Length; i++)
            {

                //if (active_nodes[node_it].X + World.lightxes[i] >= area_run.X && active_nodes[node_it].X + World.lightxes[i] < area_run.Right &&
                //    active_nodes[node_it].Y + World.lightyes[i] >= area_run.Y && active_nodes[node_it].Y + World.lightyes[i] < area_run.Bottom)
                    count_changed += compare_neighbors(w, active_nodes[node_it], new Point(active_nodes[node_it].X + World.lightxes[i], active_nodes[node_it].Y + World.lightyes[i]), (i > 3));
            }
            //if (count_changed == 0)
           // {
                w.map[active_nodes[node_it].X, active_nodes[node_it].Y].djikstra_node = false;
                //active_nodes.RemoveAt(node_it);
            //}
        }

        public byte compare_neighbors(World w, Point from, Point to, bool diag)
        {
            if (to.Y < 0 || to.Y >= w.map.GetLength(1))
                return 0;
            to.X = w.wraparound_x(to.X);
            int diag_l = 21;
            int norm_l = 15;
            if( w.map[to.X, to.Y].transparent)
            {
                diag_l = 14;
                norm_l = 10;
            }
            num_visits++;
            bool changed = false;
            if (diag)
            { // change by 14
                if (w.map[from.X, from.Y].light_level.R > w.map[to.X, to.Y].light_level.R + diag_l)
                {
                    w.map[to.X, to.Y].light_level.R = (byte)(w.map[from.X, from.Y].light_level.R - diag_l);
                    changed = true;
                }
                if (w.map[from.X, from.Y].light_level.G > w.map[to.X, to.Y].light_level.G + diag_l)
                {
                    w.map[to.X, to.Y].light_level.G = (byte)(w.map[from.X, from.Y].light_level.G - diag_l);
                    changed = true;
                }
                if (w.map[from.X, from.Y].light_level.B > w.map[to.X, to.Y].light_level.B + diag_l)
                {
                    w.map[to.X, to.Y].light_level.B = (byte)(w.map[from.X, from.Y].light_level.B - diag_l);
                    changed = true;
                }
            }
            else
            { //change by 10
                if (w.map[from.X, from.Y].light_level.R > w.map[to.X, to.Y].light_level.R + norm_l)
                {
                    w.map[to.X, to.Y].light_level.R = (byte)(w.map[from.X, from.Y].light_level.R - norm_l);
                    changed = true;
                }
                if (w.map[from.X, from.Y].light_level.G > w.map[to.X, to.Y].light_level.G + norm_l)
                {
                    w.map[to.X, to.Y].light_level.G = (byte)(w.map[from.X, from.Y].light_level.G - norm_l);
                    changed = true;
                }
                if (w.map[from.X, from.Y].light_level.B > w.map[to.X, to.Y].light_level.B + norm_l)
                {
                    w.map[to.X, to.Y].light_level.B = (byte)(w.map[from.X, from.Y].light_level.B - norm_l);
                    changed = true;
                }
            }
            if (changed)
            {
                if (!w.map[to.X, to.Y].djikstra_node)
                {
                    w.map[to.X, to.Y].djikstra_node = true;
                    active_nodes.Add(to);
                }
                return 1;
            }
            return 0;
        }


        public byte light_level_into_byte(byte max, byte light_source)
        {
            return (byte)((float)light_source / (float)max * 255f);
        }

        public void clear_light(World w, byte[] ambient_light, Rectangle area)
        {
            active_nodes.Clear();
            int use_x = 0;
            for (int x = area.X; x < area.Right; x++)
            {
                use_x = w.wraparound_x(x);
                for (int y = area.Y; y < area.Bottom; y++)
                {
                    if (w.map[use_x, y].furniture_index != -1 && w.furniture[w.map[use_x, y].furniture_index].light_source != null)
                    {
                        w.map[use_x, y].light_level.R = w.furniture[w.map[use_x, y].furniture_index].light_source[0];
                        w.map[use_x, y].light_level.G = w.furniture[w.map[use_x, y].furniture_index].light_source[1];
                        w.map[use_x, y].light_level.B = w.furniture[w.map[use_x, y].furniture_index].light_source[2];
                        active_nodes.Add(new Point(use_x, y));
                        w.map[use_x, y].djikstra_node = true;
                    }
                    else if (w.map[use_x, y].light_source != null)
                    { //populate light_level with the light_source
                        w.map[use_x, y].light_level.R = w.map[use_x, y].light_source[0];
                        w.map[use_x, y].light_level.G = w.map[use_x, y].light_source[1];
                        w.map[use_x, y].light_level.B = w.map[use_x, y].light_source[2];
                        active_nodes.Add(new Point(use_x, y));
                        w.map[use_x, y].djikstra_node = true;
                    }
                    else if (w.map[w.wraparound_x(x), y].transparent && w.map[w.wraparound_x(x), y].bkd_transparent && 
                        w.has_non_transparent_neighbors(new Point(use_x,y)))
                    { //in the open, give ambient light
                        if (w.map[use_x, y].liquid_cell_id != -1)
                        {
                            int pressure = w.liquid_simulator.liquid_cells[w.map[use_x, y].liquid_cell_id].pressure;
                            pressure /= 20;
                            w.map[use_x, y].light_level.R = (byte)Math.Max(ambient_light[0] - pressure,0);
                            w.map[use_x, y].light_level.G = (byte)Math.Max(ambient_light[1] - pressure, 0);
                            w.map[use_x, y].light_level.B = (byte)Math.Max(ambient_light[2] - pressure, 0);
                        }
                        else
                        {
                            w.map[use_x, y].light_level.R = ambient_light[0];
                            w.map[use_x, y].light_level.G = ambient_light[1];
                            w.map[use_x, y].light_level.B = ambient_light[2];
                        }
                        active_nodes.Add(new Point(use_x, y));
                        w.map[use_x, y].djikstra_node = true;
                    }
                    else
                    {
                        w.map[use_x, y].djikstra_node = false;
                        w.map[use_x, y].light_level = Color.Black;
                    }
                }
            }
            Point voxel_loc = new Point();
            for (int i = 0; i < w.players.Count; i++)
            {
                if ( w.players[i].avatar.screen_loc.X > -100 && w.players[i].avatar.screen_loc.X < Exilania.screen_size.X + 100 &&
                w.players[i].avatar.screen_loc.Y > -100 && w.players[i].avatar.screen_loc.Y < Exilania.screen_size.Y + 100)
                {
                    voxel_loc = new Point((int)w.players[i].avatar.world_loc.X / 24, (int)w.players[i].avatar.world_loc.Y / 24);
                    voxel_loc.X = w.wraparound_x(voxel_loc.X);
                    if (w.players[i].avatar.light_source[0] > w.map[voxel_loc.X, voxel_loc.Y].light_level.R)
                        w.map[voxel_loc.X, voxel_loc.Y].light_level.R = w.players[i].avatar.light_source[0];
                    if (w.players[i].avatar.light_source[1] > w.map[voxel_loc.X, voxel_loc.Y].light_level.G)
                        w.map[voxel_loc.X, voxel_loc.Y].light_level.G = w.players[i].avatar.light_source[1];
                    if (w.players[i].avatar.light_source[2] > w.map[voxel_loc.X, voxel_loc.Y].light_level.B)
                        w.map[voxel_loc.X, voxel_loc.Y].light_level.B = w.players[i].avatar.light_source[2];
                    active_nodes.Add(voxel_loc);
                    w.map[voxel_loc.X, voxel_loc.Y].djikstra_node = true;
                }
            }

            for (int i = 0; i < w.npcs.Count; i++)
            {
                if (!w.npcs[i].empty && w.npcs[i].screen_loc.X > -100 && w.npcs[i].screen_loc.X < Exilania.screen_size.X + 100 &&
                w.npcs[i].screen_loc.Y > -100 && w.npcs[i].screen_loc.Y < Exilania.screen_size.Y + 100)
                {
                    voxel_loc = new Point((int)w.npcs[i].world_loc.X / 24, (int)w.npcs[i].world_loc.Y / 24);
                    voxel_loc.X = w.wraparound_x(voxel_loc.X);
                    if (w.npcs[i].light_source[0] > w.map[voxel_loc.X, voxel_loc.Y].light_level.R)
                        w.map[voxel_loc.X, voxel_loc.Y].light_level.R = w.npcs[i].light_source[0];
                    if (w.npcs[i].light_source[1] > w.map[voxel_loc.X, voxel_loc.Y].light_level.G)
                        w.map[voxel_loc.X, voxel_loc.Y].light_level.G = w.npcs[i].light_source[1];
                    if (w.npcs[i].light_source[2] > w.map[voxel_loc.X, voxel_loc.Y].light_level.B)
                        w.map[voxel_loc.X, voxel_loc.Y].light_level.B = w.npcs[i].light_source[2];
                    active_nodes.Add(voxel_loc);
                    w.map[voxel_loc.X, voxel_loc.Y].djikstra_node = true;
                }
            }
        }

        public void update_light(World w, Rectangle area, byte[] ambient_light)
        {
            int use_x = 0;
            Color ambient_now = Color.FromNonPremultiplied(light_level_into_byte(100, ambient_light[0]), light_level_into_byte(100, ambient_light[1]), light_level_into_byte(100, ambient_light[2]),255);
            
            for (int x = area.X; x < area.Right; x++)
            {
                use_x = w.wraparound_x(x);
                for (int y = area.Y; y < area.Bottom; y++)
                {

                    if (w.map[use_x, y].light_level != Color.Black)
                    {
                        byte highest = w.map[use_x, y].light_level.R;
                        if (w.map[use_x, y].light_level.G > highest)
                            highest = w.map[use_x, y].light_level.G;
                        if (w.map[use_x, y].light_level.B > highest)
                            highest = w.map[use_x, y].light_level.B;
                        if (highest < 100)
                            highest = 100;
                        w.map[use_x, y].light_level.R = light_level_into_byte(highest, w.map[use_x, y].light_level.R);
                        w.map[use_x, y].light_level.G = light_level_into_byte(highest, w.map[use_x, y].light_level.G);
                        w.map[use_x, y].light_level.B = light_level_into_byte(highest, w.map[use_x, y].light_level.B);
                    }
                    if(w.map[use_x,y].transparent && w.map[use_x,y].bkd_transparent && w.map[use_x,y].plant_index == -1 && 
                        w.map[use_x,y].furniture_index == -1 && w.map[use_x,y].liquid_id == 0)
                    {
                        w.map[use_x, y].light_level.R = Math.Max(ambient_now.R, w.map[use_x, y].light_level.R);
                        w.map[use_x, y].light_level.G = Math.Max(ambient_now.G, w.map[use_x, y].light_level.G);
                        w.map[use_x, y].light_level.B = Math.Max(ambient_now.B, w.map[use_x, y].light_level.B);
                    }
                }
            }
        }
    }
}
