using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{

    public class HashCell
    {
        public List<int> furniture_ids;
        public List<int> player_ids;
        public List<int> npc_ids;
        public List<int> projectile_ids;
        public Rectangle bounding_box;

        public HashCell(int x, int y)
        {
            bounding_box = new Rectangle(x * CollisionHashTable.world_size_cell, y * CollisionHashTable.world_size_cell, CollisionHashTable.world_size_cell, CollisionHashTable.world_size_cell);
            furniture_ids = new List<int>();
            player_ids = new List<int>();
            npc_ids = new List<int>();
            projectile_ids = new List<int>();
        }

        public bool obj_in_table(int id, TargetType t)
        {
            switch (t)
            {
                case TargetType.Furniture:
                    return furniture_ids.Contains(id);
                case TargetType.Player:
                    return player_ids.Contains(id);
                case TargetType.NPC:
                    return npc_ids.Contains(id);
                case TargetType.Projectile:
                    return projectile_ids.Contains(id);
            }
            return false;
        }

        public void add_obj_to_table(int id, TargetType t)
        {
            switch (t)
            {
                case TargetType.Furniture:
                    furniture_ids.Add(id);
                    break;
                case TargetType.Player:
                    player_ids.Add(id);
                    break;
                case TargetType.NPC:
                    npc_ids.Add(id);
                    break;
                case TargetType.Projectile:
                    projectile_ids.Add(id);
                    break;
            }
        }

        public void remove_obj_from_table(int id, TargetType t)
        {
            switch (t)
            {
                case TargetType.Furniture:
                    furniture_ids.Remove(id);
                    break;
                case TargetType.Player:
                    player_ids.Remove(id);
                    break;
                case TargetType.NPC:
                    npc_ids.Remove(id);
                    break;
                case TargetType.Projectile:
                    projectile_ids.Remove(id);
                    break;
            }
        }


        public override string ToString()
        {
            string build = "";
            if (furniture_ids.Count > 0)
            {
                build += "F: ";
                for (int x = 0; x < furniture_ids.Count; x++)
                {
                    if(x == 0)
                    build += furniture_ids[x].ToString();
                    else
                        build += ", "+furniture_ids[x].ToString();
                }
            }
            if (player_ids.Count > 0)
            {
                if (build != "")
                    build += "; ";
                build += "P: ";
                for (int x = 0; x < player_ids.Count; x++)
                {
                    if (x == 0)
                        build += player_ids[x].ToString();
                    else
                        build += ", " + player_ids[x].ToString();
                }
            }
            if (npc_ids.Count > 0)
            {
                if (build != "")
                    build += "; ";
                build += "N: ";
                for (int x = 0; x < npc_ids.Count; x++)
                {
                    if (x == 0)
                        build += npc_ids[x].ToString();
                    else
                        build += ", " + npc_ids[x].ToString();
                }
            }
            return build;
        }
    }

    public class CollisionHashTable
    {
        public static int block_size_cell = 40;
        public static int world_size_cell = 24 * block_size_cell;
        public HashCell[,] cells;

        public CollisionHashTable(World w)
        {
            cells = new HashCell[(w.map.GetLength(0) / block_size_cell) + (w.map.GetLength(0) % block_size_cell > 0 ? 1 : 0), (w.map.GetLength(1) / block_size_cell) + (w.map.GetLength(1) % block_size_cell > 0 ? 1 : 0)];
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    cells[x, y] = new HashCell(x, y);
                }
            }
        }

        public void add_all_furniture_to_table(List<Furniture> f)
        {
            for (int x = 0; x < f.Count; x++)
            {
                Rectangle test = f[x].get_rect();
                object_point_to_table(new Point(test.Left, test.Top), x, TargetType.Furniture, true);
                object_point_to_table(new Point(test.Right, test.Top), x, TargetType.Furniture, true);
                object_point_to_table(new Point(test.Left, test.Bottom), x, TargetType.Furniture, true);
                object_point_to_table(new Point(test.Right, test.Bottom), x, TargetType.Furniture, true);
            }
        }

        public void remove_furniture_from_table(List<Furniture> f, int use)
        {
            Rectangle test = f[use].get_rect();
            object_point_to_table(new Point(test.Left, test.Top), use, TargetType.Furniture, false);
            object_point_to_table(new Point(test.Right, test.Top), use, TargetType.Furniture, false);
            object_point_to_table(new Point(test.Left, test.Bottom), use, TargetType.Furniture, false);
            object_point_to_table(new Point(test.Right, test.Bottom), use, TargetType.Furniture, false);
        }

        public void add_furniture_to_table(List<Furniture> f, int use)
        {
            Rectangle test = f[use].get_rect();
            object_point_to_table(new Point(test.Left, test.Top), use, TargetType.Furniture, true);
            object_point_to_table(new Point(test.Right, test.Top), use, TargetType.Furniture, true);
            object_point_to_table(new Point(test.Left, test.Bottom), use, TargetType.Furniture, true);
            object_point_to_table(new Point(test.Right, test.Bottom), use, TargetType.Furniture, true);
        }

        public void modify_actor_in_table(Actor a, bool is_player, int id, bool adding)
        {
            Rectangle test = new Rectangle((int)a.world_loc.X - (a.bounding_box.Width / 2), (int)a.world_loc.Y - (a.bounding_box.Height / 2), a.bounding_box.Width, a.bounding_box.Height);
            object_point_to_table(new Point(test.Left, test.Top), id, (is_player ? TargetType.Player : TargetType.NPC), adding);
            object_point_to_table(new Point(test.Right, test.Top), id, (is_player ? TargetType.Player : TargetType.NPC), adding);
            object_point_to_table(new Point(test.Left, test.Bottom), id, (is_player ? TargetType.Player : TargetType.NPC), adding);
            object_point_to_table(new Point(test.Right, test.Bottom), id, (is_player ? TargetType.Player : TargetType.NPC), adding);
        }


        public void object_point_to_table(Point loc, int id, TargetType t, bool adding)
        {
            loc.X /= 24*block_size_cell;
            if (loc.X >= cells.GetLength(0))
                loc.X -= cells.GetLength(0);
            else if (loc.X < 0)
                loc.X += cells.GetLength(0);
            loc.Y /= 24*block_size_cell;
            if (loc.Y < 0)
                loc.Y = 0;
            if (loc.Y >= cells.GetLength(1))
                loc.Y = cells.GetLength(1) - 1;
            if (adding)
            {
                if (!cells[loc.X, loc.Y].obj_in_table(id, t))
                {
                    cells[loc.X, loc.Y].add_obj_to_table(id, t);
                }
            }
            else
            {
                cells[loc.X, loc.Y].remove_obj_from_table(id, t);
            }
        }

        public List<int> get_power_furniture_in_range(int my_furniture_id, Point loc, int range, World w)
        {
            List<int> my_ret = new List<int>();
            Point center = new Point(loc.X, loc.Y);
            loc.X /= 24 * block_size_cell;
            if (loc.X >= cells.GetLength(0))
                loc.X -= cells.GetLength(0);
            else if (loc.X < 0)
                loc.X += cells.GetLength(0);
            loc.Y /= 24 * block_size_cell;
            Point loc_use = new Point();
            for (int x = loc.X - 1; x <= loc.X + 1; x++)
            {
                for (int y = loc.Y - 1; y <= loc.Y + 1; y++)
                {
                    loc_use = new Point(x, y);
                    if (loc_use.X >= cells.GetLength(0))
                        loc_use.X -= cells.GetLength(0);
                    else if (loc_use.X < 0)
                        loc_use.X += cells.GetLength(0);
                    if (loc_use.Y > -1 && loc_use.Y < cells.GetLength(1))
                    {
                        for (int i = 0; i < cells[loc_use.X, loc_use.Y].furniture_ids.Count; i++)
                        {
                            if (cells[loc_use.X, loc_use.Y].furniture_ids[i] != my_furniture_id &&
                                w.furniture[cells[loc_use.X, loc_use.Y].furniture_ids[i]].share_power != 255 &&
                                (w.furniture[my_furniture_id].share_power == 255 || w.furniture[cells[loc_use.X, loc_use.Y].furniture_ids[i]].share_power > w.furniture[my_furniture_id].share_power) &&
                                !my_ret.Contains(cells[loc_use.X, loc_use.Y].furniture_ids[i]) && 
                                !w.furniture[cells[loc_use.X, loc_use.Y].furniture_ids[i]].flags[(int)FFLAGS.EMPTY])
                                my_ret.Add(cells[loc_use.X, loc_use.Y].furniture_ids[i]);
                        }
                    }
                }
            }

            for (int x = 0; x < my_ret.Count; x++)
            {
                if (Acc.get_distance(center, w.furniture[my_ret[x]].get_rect().Center) > range)
                {
                    my_ret.RemoveAt(x);
                    x--;
                }
            }
            return my_ret;
        }

        public List<int> get_furniture_in_range(Point loc, int range, World w)
        {
            List<int> my_ret = new List<int>();
            Point center = new Point(loc.X, loc.Y);
            loc.X /= 24 * block_size_cell;
            if (loc.X >= cells.GetLength(0))
                loc.X -= cells.GetLength(0);
            else if (loc.X < 0)
                loc.X += cells.GetLength(0);
            loc.Y /= 24 * block_size_cell;
            Point loc_use = new Point();
            for (int x = loc.X - 1; x <= loc.X + 1; x++)
            {
                for (int y = loc.Y - 1; y <= loc.Y + 1; y++)
                {
                    loc_use = new Point(x, y);
                    if (loc_use.X >= cells.GetLength(0))
                        loc_use.X -= cells.GetLength(0);
                    else if (loc_use.X < 0)
                        loc_use.X += cells.GetLength(0);
                    if (loc_use.Y > -1 && loc_use.Y < cells.GetLength(1))
                    {
                        for (int i = 0; i < cells[loc_use.X, loc_use.Y].furniture_ids.Count; i++)
                        {
                            if (!my_ret.Contains(cells[loc_use.X, loc_use.Y].furniture_ids[i]) && !w.furniture[cells[loc_use.X, loc_use.Y].furniture_ids[i]].flags[(int)FFLAGS.EMPTY])
                                my_ret.Add(cells[loc_use.X, loc_use.Y].furniture_ids[i]);
                        }
                    }
                }
            }

            for (int x = 0; x < my_ret.Count; x++)
            {
                if (Acc.get_distance(center, w.furniture[my_ret[x]].get_rect().Center) > range)
                {
                    my_ret.RemoveAt(x);
                    x--;
                }
            }
            return my_ret;
        }

        public List<int> get_plants_in_range(Point loc, World w, int range, bool hit_nonbreakablebelow, bool facing_right)
        {
            List<int> hit_plants = new List<int>();
            int srange = (int)Math.Ceiling((double)range / 24);
            
            int min_x = srange;
            int max_x = srange;
            if (facing_right)
                min_x = 1;
            else
                max_x = 0;
            for (int x = (loc.X/24) - min_x; x < (loc.X/24) + max_x + 1; x++)
            {
                for (int y = (loc.Y/24) - srange; y < (loc.Y/24) + srange; y++)
                {
                    w.areas_checked.Add(new Point(x, y));
                    if (y > -1 && y < w.map.GetLength(1) && w.map[w.wraparound_x(x), y].plant_index != -1 &&
                        (Acc.get_distance(new Point(loc.X, loc.Y), new Point(x * 24, y * 24)) <= range
                        || Acc.get_distance(new Point(loc.X, loc.Y), new Point(x * 24 + 24, y * 24)) <= range
                        || Acc.get_distance(new Point(loc.X, loc.Y), new Point(x * 24 + 24, y * 24 + 24)) <= range
                        || Acc.get_distance(new Point(loc.X, loc.Y), new Point(x * 24, y * 24 + 24)) <= range))
                    {
                        //we have a plant!
                        int plant_id = w.map[w.wraparound_x(x), y].plant_index;
                        if (!hit_plants.Contains(plant_id) && (hit_nonbreakablebelow == true || Exilania.plant_manager.plants[w.plants[plant_id].plant_index].break_below))
                            hit_plants.Add(plant_id);
                    }

                }
            }
            return hit_plants;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc">top left corner in the map array</param>
        /// <param name="dx">pixels wide to check</param>
        /// <param name="dy">pixels tall to check</param>
        /// <param name="w">world to check</param>
        /// <returns>returns true if the block can be placed.</returns>
        public bool can_place_at(Point loc, int dx, int dy, World w, Rectangle placing_player)
        {
           
            Point cell_loc = new Point(loc.X / block_size_cell, loc.Y / block_size_cell);
            if (cell_loc.X >= cells.GetLength(0))
                cell_loc.X -= cells.GetLength(0);
            else if (cell_loc.X < 0)
                cell_loc.X += cells.GetLength(0);
           
            Rectangle place_at = new Rectangle(loc.X * 24, loc.Y * 24, dx, dy);
            Rectangle compare;
            if (place_at.Intersects(placing_player) )
                return false;
            if (place_at.Intersects(w.spawn_protect))
            {
                Exilania.display.add_message("@08Cannot add foreground blocks to the spawn zone.");
                return false;
            }
            if (cell_loc.Y > -1 && cell_loc.Y < cells.GetLength(1))
            {
                for (int i = 0; i < cells[cell_loc.X, cell_loc.Y].player_ids.Count; i++)
                {

                    compare = new Rectangle((int)w.players[cells[cell_loc.X, cell_loc.Y].player_ids[i]].avatar.world_loc.X + w.players[cells[cell_loc.X, cell_loc.Y].player_ids[i]].avatar.bounding_box.X,
                            (int)w.players[cells[cell_loc.X, cell_loc.Y].player_ids[i]].avatar.world_loc.Y + w.players[cells[cell_loc.X, cell_loc.Y].player_ids[i]].avatar.bounding_box.Y,
                            w.players[cells[cell_loc.X, cell_loc.Y].player_ids[i]].avatar.bounding_box.Width,
                            w.players[cells[cell_loc.X, cell_loc.Y].player_ids[i]].avatar.bounding_box.Height);
                    if (place_at.Intersects(compare))
                        return false;
                }
                for (int i = 0; i < cells[cell_loc.X, cell_loc.Y].npc_ids.Count; i++)
                {
                    compare = new Rectangle((int)w.npcs[cells[cell_loc.X, cell_loc.Y].npc_ids[i]].world_loc.X + w.npcs[cells[cell_loc.X, cell_loc.Y].npc_ids[i]].bounding_box.X,
                            (int)w.npcs[cells[cell_loc.X, cell_loc.Y].npc_ids[i]].world_loc.Y + w.npcs[cells[cell_loc.X, cell_loc.Y].npc_ids[i]].bounding_box.Y,
                            w.npcs[cells[cell_loc.X, cell_loc.Y].npc_ids[i]].bounding_box.Width,
                            w.npcs[cells[cell_loc.X, cell_loc.Y].npc_ids[i]].bounding_box.Height);
                    if (place_at.Intersects(compare))
                        return false;
                }
                for (int i = 0; i < cells[cell_loc.X, cell_loc.Y].furniture_ids.Count; i++)
                {
                    compare = w.furniture[cells[cell_loc.X,cell_loc.Y].furniture_ids[i]].get_rect();
                    if (place_at.Intersects(compare))
                        return false;
                }
            }
            return true;
        }

        public List<int> get_players_in_range(Point loc, int range, World w)
        {
            List<int> my_ret = new List<int>();
            Point center = new Point(loc.X, loc.Y);
            loc.X /= 24 * block_size_cell;
            if (loc.X >= cells.GetLength(0))
                loc.X -= cells.GetLength(0);
            else if (loc.X < 0)
                loc.X += cells.GetLength(0);
            loc.Y /= 24 * block_size_cell;
            Point loc_use = new Point();
            for (int x = loc.X - 1; x <= loc.X + 1; x++)
            {
                for (int y = loc.Y - 1; y <= loc.Y + 1; y++)
                {
                    loc_use = new Point(x, y);
                    if (loc_use.X >= cells.GetLength(0))
                        loc_use.X -= cells.GetLength(0);
                    else if (loc_use.X < 0)
                        loc_use.X += cells.GetLength(0);
                    if (loc_use.Y > -1 && loc_use.Y < cells.GetLength(1))
                    {
                        for (int i = 0; i < cells[loc_use.X, loc_use.Y].player_ids.Count; i++)
                        {
                            if (!my_ret.Contains(cells[loc_use.X, loc_use.Y].player_ids[i]))
                                my_ret.Add(cells[loc_use.X, loc_use.Y].player_ids[i]);
                        }
                    }
                }
            }

            for (int x = 0; x < my_ret.Count; x++)
            {
                if (w.players[my_ret[x]].is_player_empty || Acc.get_distance(w.players[my_ret[x]].avatar.world_loc, center) > range)
                {
                    my_ret.RemoveAt(x);
                    x--;
                }
            }
            return my_ret;
        }

        public List<int> get_npcs_in_range(Point loc, int range, World w, bool facing_right)
        {
            List<int> my_ret = new List<int>();
            Point center = new Point(loc.X, loc.Y);
            loc.X /= 24 * block_size_cell;
            if (loc.X >= cells.GetLength(0))
                loc.X -= cells.GetLength(0);
            else if (loc.X < 0)
                loc.X += cells.GetLength(0);
            loc.Y /= 24 * block_size_cell;
            Point loc_use = new Point();
            for (int x = loc.X - 1; x <= loc.X + 1; x++)
            {
                for (int y = loc.Y - 1; y <= loc.Y + 1; y++)
                {
                    loc_use = new Point(x, y);
                    if (loc_use.X >= cells.GetLength(0))
                        loc_use.X -= cells.GetLength(0);
                    else if (loc_use.X < 0)
                        loc_use.X += cells.GetLength(0);
                    if (loc_use.Y > -1 && loc_use.Y < cells.GetLength(1))
                    {
                        for (int i = 0; i < cells[loc_use.X, loc_use.Y].npc_ids.Count; i++)
                        {
                            if (!my_ret.Contains(cells[loc_use.X, loc_use.Y].npc_ids[i]))
                                my_ret.Add(cells[loc_use.X, loc_use.Y].npc_ids[i]);
                        }
                    }
                }
            }
            Circle t = new Circle(center, range);
            
            for (int x = 0; x < my_ret.Count; x++)
            {
                if (w.npcs[my_ret[x]].empty || !t.intersects_rectangle(new Rectangle((int)w.npcs[my_ret[x]].world_loc.X-w.npcs[my_ret[x]].bounding_box.Width/2,
                    (int)w.npcs[my_ret[x]].world_loc.Y-w.npcs[my_ret[x]].bounding_box.Height/2,
                    w.npcs[my_ret[x]].bounding_box.Width, w.npcs[my_ret[x]].bounding_box.Height)) ||
                    (facing_right && w.npcs[my_ret[x]].world_loc.X + (w.npcs[my_ret[x]].bounding_box.Width/2) <= center.X) ||
                    (!facing_right && w.npcs[my_ret[x]].world_loc.X - (w.npcs[my_ret[x]].bounding_box.Width / 2) >= center.X))
                {
                    my_ret.RemoveAt(x);
                    x--;
                }
            }
            return my_ret;
        }

    }
}
