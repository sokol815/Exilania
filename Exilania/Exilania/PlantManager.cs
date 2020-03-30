using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    public class GrowthEvent
    {
        public Point loc;
        public int growth_id;

        public GrowthEvent(Point p, int pgrowth_id)
        {
            loc = p;
            growth_id = pgrowth_id;
        }

        public GrowthEvent(Lidgren.Network.NetIncomingMessage m)
        {
            loc = new Point(m.ReadUInt16(), m.ReadUInt16());
            growth_id = m.ReadByte();
        }

        public void send_growth_event(Lidgren.Network.NetOutgoingMessage m)
        {
            m.Write((ushort)loc.X);
            m.Write((ushort)loc.Y);
            m.Write((byte)growth_id);
        }

    }
    public class Growth
    {
        public int chance_grow;
        public int total_chance;
        public string mature_plant_name;

        public Growth()
        {
        }

        public Growth(string qs)
        {
            string[] items = qs.Split('=');
            string[] chances = items[0].Split('/');
            chance_grow = int.Parse(chances[0]);
            total_chance = int.Parse(chances[1]);
            mature_plant_name = items[1];
        }
    }
    public class PlantPiece
    {
        public string name;
        public int width_min;
        public int width_max;
        public int height_min;
        public int height_max;
        public int[] images;
        public bool random_images;

        public PlantPiece()
        {

        }

        public PlantPiece(string vals)
        {
            string[] halves = vals.Split('|');
            halves[0] = halves[0].Trim();
            halves[1] = halves[1].Trim();
            string[] pair = halves[0].Split('=');
            name = pair[0];
            pair = pair[1].Split(',');
            if (pair[0].Contains('-'))
            { //variable width
                string[] temp = pair[0].Split('-');
                width_min = int.Parse(temp[0]);
                width_max = int.Parse(temp[1]);
                if (width_max < width_min)
                {
                    int t2 = width_min;
                    width_min = width_max;
                    width_max = t2;
                }
            }
            else
            { //non variable width
                width_max = int.Parse(pair[0]);
                width_min = width_max;
            }
            if (pair[1].Contains('-'))
            { //variable height
                string[] temp = pair[1].Split('-');
                height_min = int.Parse(temp[0]);
                height_max = int.Parse(temp[1]);
                if (height_max < height_min)
                {
                    int t2 = height_min;
                    height_min = height_max;
                    height_max = t2;
                }
            }
            else
            { //non variable height
                height_max = int.Parse(pair[1]);
                height_min = height_max;
            }
            //interpret half 1 now!, the images.
            if (Acc.script_remove_content_of_outer_parenthesis(halves[1]).ToLower() == "rand")
            {
                random_images = true;
            }
            halves[1] = Acc.script_remove_outer_parentheses(halves[1]);
            halves = halves[1].Split(',');
            images = new int[halves.Length];
            for (int x = 0; x < halves.Length; x++)
            {
                images[x] = int.Parse(halves[x]);
            }
        }

        public ushort get_an_image()
        {
            if (random_images)
            {
                return (ushort)images[Exilania.rand.Next(0, images.Length)];
            }
            return (ushort)images[0];
        }
    }

    public class PlantData
    {
        public ushort plant_id;
        public string name;
        public Dictionary<string, byte> drops;
        public string[] anatomy;
        public List<string> grows_on;
        public string behavior_cut;
        public List<PlantPiece> pieces;
        public List<Growth> growth_possibilities;
        public bool passable;
        public bool break_below;
        public bool below_block_remove;
        public bool auto_spawn;
        public bool grows_in_day;
        public int damaging;
        public int min_distance;
        public int density;
        public string material;
        public int max_height;

        public PlantData()
        {
            plant_id = ushort.MaxValue;
            name = "";
            drops = new Dictionary<string, byte>();
            anatomy = new string[0];
            grows_on = new List<string>();
            behavior_cut = "";
            pieces = new List<PlantPiece>();
            growth_possibilities = new List<Growth>();
            passable = true;
            break_below = true;
            below_block_remove = false;
            auto_spawn = true;
            grows_in_day = true;
            damaging = 0;
            min_distance = 24;
            density = 1;
            material = "SINGLE";
            max_height = -1;
        }

        public int get_part_id_by_name(string pname)
        {
            pname = pname.ToLower();
            for (int x = 0; x < pieces.Count; x++)
            {
                if (pieces[x].name.ToLower() == pname)
                    return x;
            }
            return 0;
        }

        public int[] get_height_range()
        {
            
            int[] dheights = new int[2];
            int use_id = 0;
            for (int x = 0; x < anatomy.Length; x++)
            {
                use_id = get_part_id_by_name(anatomy[x]);
                dheights[0] += pieces[use_id].height_min;
                dheights[1] += pieces[use_id].height_max;
            }

            if (max_height < 0)
                max_height = dheights[1];
            return dheights;
        }

        public int get_width()
        {
            int p = 0;
            int use_id = 0;
            for (int x = 0; x < anatomy.Length; x++)
            {
                use_id = get_part_id_by_name(anatomy[x]);
                if (pieces[use_id].width_min > p)
                    p = pieces[use_id].width_min;
            }
            return p;
        }

        public void hover_over(Point mouse_loc, SpriteBatch s, Display d, World w)
        {
            Vector2 size = d.small_font.MeasureString(name);
            Vector2 size2 = new Vector2();//d.small_font.MeasureString("Owned by: " + owner);
            //d.draw_bounding_box(s, new Rectangle(mouse_loc.X + 35, mouse_loc.Y - 50, (int)Math.Max(size.X, size2.X) + 14, 10));
            d.draw_text(s, d.small_font, "@05" + name, mouse_loc.X + 42 + (int)Math.Max(size.X / 2, size2.X / 2) - (int)(size.X / 2), mouse_loc.Y, 500);
            //d.draw_text(s, d.small_font, "@00Owned by: " + owner, mouse_loc.X + 42 + (int)Math.Max(size.X / 2, size2.X / 2) - (int)(size2.X / 2), mouse_loc.Y - 26, 500);
        }

    }
    public class PlantManager
    {
        public List<PlantData> plants;

        public PlantManager()
        {
            plants = new List<PlantData>();
            if (System.IO.File.Exists(@"plants.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"plants.txt");
                string line = "";
                PlantData p = new PlantData();
                bool cont = true;
                while (cont)
                {
                    line = r.ReadLine();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        //skip this line
                    }
                    else
                    {
                        string[] items = line.Split(':');
                        switch (items[0].ToLower())
                        {
                            case "name":
                                if (p.name == "")
                                {
                                    p.name = items[1].Trim();
                                    p.plant_id = 0;
                                }
                                else
                                {
                                    p.plant_id = (ushort)plants.Count;
                                    p.get_height_range();
                                    plants.Add(p);
                                    Exilania.text_stream.WriteLine("Plant Item '" + p.name + "' Loaded.");
                                    p = new PlantData();
                                    p.name = items[1].Trim();
                                }
                                break;
                            case "drops":
                                string[] pieces = items[1].Trim().Split(',');
                                if (items[1].Trim() != "")
                                {
                                    for (int x = 0; x < pieces.Length; x++)
                                    {
                                        pieces[x] = pieces[x].Trim();
                                        string key = Acc.script_remove_content_of_outer_parenthesis(pieces[x]);
                                        byte val = byte.Parse(Acc.script_remove_outer_parentheses(pieces[x]));
                                        p.drops.Add(key, val);
                                    }
                                }
                                break;
                            case "piece":
                                p.pieces.Add(new PlantPiece(items[1]));
                                break;
                            case "anatomy":
                                p.anatomy = items[1].Trim().Split(',');
                                break;
                            case "growson":
                                items = items[1].Trim().Split(',');
                                for (int x = 0; x < items.Length; x++)
                                {
                                    if(items[x].Length > 0)
                                        p.grows_on.Add(items[x].Trim().ToLower());
                                }
                                break;
                            case "behavior-cut":
                                p.behavior_cut = items[1].Trim();
                                break;
                            case "min-distance":
                                p.min_distance = Int32.Parse(items[1]);
                                break;
                            case "density":
                                p.density = Int32.Parse(items[1]);
                                break;
                            case "break-below":
                                p.break_below = bool.Parse(items[1]);
                                break;
                            case "below-block-remove":
                                p.below_block_remove = bool.Parse(items[1]);
                                break;
                            case "material":
                                p.material = items[1].Trim().ToUpper();
                                break;
                            case "auto-spawn":
                                p.auto_spawn = bool.Parse(items[1]);
                                break;
                            case "growth":
                                string[] parts = items[1].Split('|');
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    p.growth_possibilities.Add(new Growth(parts[i]));
                                }
                                break;
                            case "extreme-height":
                                p.max_height = int.Parse(items[1]);
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        p.plant_id = (ushort)plants.Count;
                        p.get_height_range();
                        plants.Add(p);
                        Exilania.text_stream.WriteLine("Plant Item '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
            }
            else
            {
                Exilania.text_stream.Write("ERROR! No plants.txt file.");
            }
        }

        public void run_growth_batch(GrowthEvent[] growers, World w)
        {
            for (int i = 0; i < growers.Length; i++)
            {
                run_growth(growers[i].loc, growers[i].growth_id, w);
            }
        }

        public bool run_growth(Point top_left, int growth_num, World w)
        {
            int i = w.map[top_left.X, top_left.Y].plant_index;
            if (i < 0 || plants[w.plants[i].plant_index].growth_possibilities.Count <= 0)
                return false;
            int p_index = w.plants[i].plant_index; 
            switch (plants[p_index].growth_possibilities[growth_num].mature_plant_name.ToLower().Trim())
            {
                case "height": //this plant is supposed to grow taller!
                    if (w.plants[i].height < plants[p_index].max_height && tree_new_position_ok(w.plants[i], 1, w, i))
                    {
                        w.plants[i].unset_map(w);
                        w.plants[i].top_left.Y--;
                        w.plants[i].height++;
                        for (int h = 0; h < w.plants[i].pieces.Count; h++)
                        {
                            if (w.plants[i].pieces[h].piece_type == "Leaves")
                            {
                                w.plants[i].pieces[h].top_left.Y--;
                            }
                            else if (w.plants[i].pieces[h].piece_type == "Trunk")
                            {
                                w.plants[i].pieces[h].top_left.Y--;
                                w.plants[i].pieces[h].height++;
                                ushort[] temp = new ushort[w.plants[i].pieces[h].images.Length + w.plants[i].pieces[h].width];
                                w.plants[i].pieces[h].images.CopyTo(temp, w.plants[i].pieces[h].width);
                                int piece_id = -1;
                                for (int c = 0; c < plants[p_index].pieces.Count; c++)
                                {
                                    if (plants[p_index].pieces[c].name == w.plants[i].pieces[h].piece_type)
                                        piece_id = c;
                                }
                                for (int c = 0; c < w.plants[i].pieces[h].width; c++)
                                {
                                    temp[c] = plants[p_index].pieces[piece_id].get_an_image();
                                }
                                w.plants[i].pieces[h].images = new ushort[temp.Length];
                                temp.CopyTo(w.plants[i].pieces[h].images, 0);
                            }
                        }
                        w.plants[i].reset_map(w, i);
                        return true;
                    }
                    break;
                default: //replace plant with other plant
                    int plant_need_id = get_plant_id_from_name(plants[p_index].growth_possibilities[growth_num].mature_plant_name);
                    return try_replace_plant(w.plants[i].top_left, plant_need_id, w);
                    //  Exilania.display.add_message("@05Trying to grow up! " + plants[w.plants[i].plant_index].growth_possibilities[j].mature_plant_name);
                    //break;
            }
            return false;
        }

        public bool try_replace_plant(Point top_left, int new_plant_id, World w)
        {
            int plant_index = w.map[top_left.X, top_left.Y].plant_index;
            Plant temp = new Plant(w.plants[plant_index]);
            w.plants[plant_index].unset_map(w);

            int cur_width = w.plants[plant_index].width;
            int cur_height = w.plants[plant_index].height;
            //try placing the new plant where the old one was.
            if (try_place_plant(new Point(top_left.X + cur_width / 2, top_left.Y + cur_height), new_plant_id, w))
            {
                Exilania.network_server.send_destroy_plant(w, w.plants[plant_index].top_left);
                Exilania.network_server.send_placed_plant(w, w.plants.Count - 1);
                //it worked, discard the old plant, add the new one.
                for (int x = plant_index + 1; x < w.plants.Count; x++)
                {
                    w.plants[x].unset_map(w);
                    w.plants[x].reset_map(w, x - 1);
                }
                w.plants.RemoveAt(plant_index);
                return true;
            }
            else
            {
                //didn't work.. restore the old plant.
                w.plants[plant_index].reset_map(w, plant_index);
                return false;
            }
        }

        /// <summary>
        /// server is checking to see if trees or anything else is growing.
        /// </summary>
        /// <param name="w"></param>
        public void update_growing_plants(World w)
        {
            List<GrowthEvent> growers = new List<GrowthEvent>();
            int p_index = -1;
            for (int i = 0; i < w.plants.Count; i++)
            {
                p_index = w.plants[i].plant_index;
                if (plants[p_index].growth_possibilities.Count > 0)
                {
                    for (int j = 0; j < plants[p_index].growth_possibilities.Count; j++)
                    {
                        if (Exilania.rand.Next(0, plants[p_index].growth_possibilities[j].total_chance) <= plants[p_index].growth_possibilities[j].chance_grow)
                        { //we are doing something! hop to it!
                            if (run_growth(w.plants[i].top_left, j, w))
                            {
                                growers.Add(new GrowthEvent(w.plants[i].top_left, j));
                                j = plants[p_index].growth_possibilities.Count;
                            }
                        }
                    }
                }
            }
            if (growers.Count > 0)
            {
                Exilania.network_server.send_grow_plants(growers);
            }
        }


        public bool tree_new_position_ok(Plant p, int height_increase, World w, int plant_id)
        {
            for (int i = 0; i < p.pieces.Count; i++)
            {
                for (int x = p.pieces[i].top_left.X; x < p.pieces[i].top_left.X + p.pieces[i].width; x++)
                {
                    for (int y = p.pieces[i].top_left.Y - height_increase; y < p.pieces[i].top_left.Y - height_increase + p.pieces[i].height; y++)
                    {
                        if (y < 0 || (w.map[w.wraparound_x(x), y].plant_index > -1 && w.map[w.wraparound_x(x), y].plant_index != plant_id) ||
                            w.map[w.wraparound_x(x), y].fgd_block != -1 || w.map[w.wraparound_x(x), y].furniture_index!=-1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// this function is called in 2 ways... when the player uses it to plant a plant, and when the game uses it to try to grow a plant. 
        /// the user will click above the spot they want the plant to grow from, the game will pass the spot upon (or beneath) which the plant will grow.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="loc"></param>
        /// <param name="plant_name"></param>
        public bool spawn_plant_in_world(World w, Point loc, string plant_name)
        {
            int index_use = -1;

            string block_name = "";
            if (w.map[loc.X, loc.Y].fgd_block > -1)
            {
                block_name = Exilania.block_types.blocks[w.map[loc.X, loc.Y].fgd_block].name.ToLower();
            }
            else if (loc.Y + 1 < w.map.GetLength(1) && w.map[loc.X, loc.Y + 1].fgd_block > -1)
            {
                block_name = Exilania.block_types.blocks[w.map[loc.X, loc.Y + 1].fgd_block].name.ToLower();
                loc.Y++;
            }
            else
                return false;

            //Exilania.display.add_message("@05spawn: " + plant_name + " at location " + loc.ToString() + " block name was: " + block_name);
            if (plant_name != "random")
            {
                index_use = get_plant_id_from_name(plant_name);
            }
            else
            {
                index_use = get_random_plant_can_grow_on(block_name);
            }
            if (index_use == -1)
            {
                //Exilania.display.add_message("@08Not Allowed.");
                return false;
            }
            bool res = try_place_plant(loc, index_use, w);
            if (res && Exilania.game_server)
            {
                Exilania.network_server.send_placed_plant(w, w.plants.Count - 1);
            }
            return res;
        }

        public int get_random_plant_can_grow_on(string block_Type)
        {
            Dictionary<int, int> pairs = new Dictionary<int, int>();
            bool add = false;
            int tot_val = 0;
            for (int i = 0; i < plants.Count; i++)
            {
                add = false;
                for (int j = 0; j < plants[i].grows_on.Count; j++)
                {
                    if (plants[i].grows_on[j].ToLower() == block_Type && plants[i].auto_spawn)
                    {
                        
                        add = true;
                    }
                }
                if (add)
                {
                    pairs.Add(i, plants[i].density);
                    tot_val += plants[i].density;
                }
            }
            if (pairs.Count == 0)
                return -1;
            int chose_val = Exilania.rand.Next(0, tot_val);
            tot_val = 0;
            int id_chose = -1;
            foreach(var t in pairs)
            {
                tot_val += t.Value;
                if (tot_val >= chose_val)
                {
                    id_chose = t.Key;
                    break;
                }
            }
            return id_chose;
        }

        public int get_plant_id_from_name(string name)
        {
            for (int i = 0; i < plants.Count; i++)
            {
                if (plants[i].name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }
            return -1;
        }

        public void spawn_plants_in_world(World w)
        {
            int width = w.map.GetLength(0);
            List<Point> grass_tiles = new List<Point>();
            List<Point> sand_tiles = new List<Point>();
            sbyte sand_tile_id = (sbyte)Exilania.block_types.get_block_by_name("Sand");
            sbyte grass_tile_id = (sbyte)Exilania.block_types.get_block_by_name("Grass");
            for (int x = 0; x < w.map.GetLength(0); x++)
            {
                for (int y = 0; y < w.map.GetLength(1); y++)
                {
                    if (w.map[x, y].bkd_block == -1)
                    {
                        if (w.map[x, y].fgd_block == grass_tile_id)
                        {
                            grass_tiles.Add(new Point(x, y));
                        }
                        else if (w.map[x, y].fgd_block == sand_tile_id)
                        {
                            sand_tiles.Add(new Point(x, y));
                        }
                    }
                }
            }
            for (int j = 0; j < plants.Count; j++)
            {
                int[] plant_range = plants[j].get_height_range();
                int num_make = (int)((float)plants[j].density / 100f * (float)width);
                Exilania.display.add_message("Adding " + num_make.ToString() + " " + plants[j].name + "s.");
                int fails_left = w.map.GetLength(0);
                Point top_left = new Point();
                int spot_choice = 0;
                if (plants[j].grows_on.Contains("grass"))
                {
                    for (int i = 0; i < num_make && grass_tiles.Count > 0; i++)
                    {
                        spot_choice = Exilania.rand.Next(0, grass_tiles.Count);
                        //the below 2 would be unused if they don't work.
                        try_place_plant(grass_tiles[spot_choice], j, w);
                        grass_tiles.RemoveAt(spot_choice);
                        /*top_left = grass_tiles[spot_choice];
                        int uheight = Exilania.rand.Next(plant_range[0], plant_range[1] + 1);
                        int uwidth = plants[j].get_width();
                        top_left.Y -= uheight;
                        if (uwidth % 2 == 0)
                        {
                            top_left.X -= uwidth / 2;
                            top_left.X = w.wraparound_x(top_left.X);
                        }
                        else
                        {
                            top_left.X -= (uwidth - 1) / 2;
                            top_left.X = w.wraparound_x(top_left.X);
                        }

                        grass_tiles.RemoveAt(spot_choice);
                        if (top_left.Y < 0)
                        {

                        }
                        else
                        {
                            bool good = true;
                            for (int x = top_left.X; x < top_left.X + uwidth; x++)
                            {
                                for (int y = top_left.Y; y < top_left.Y + uheight; y++)
                                {
                                    if (w.map[w.wraparound_x(x), y].plant_index != -1 || w.map[w.wraparound_x(x), y].fgd_block_type != -1)
                                    {
                                        good = false;
                                    }
                                }
                            }
                            if (good)
                            {
                                w.plants.Add(new Plant(plants[j], w, top_left, uheight, w.plants.Count));
                            }
                        }*/
                    }
                }
                else if (plants[j].grows_on.Contains("sand"))
                {
                    for (int i = 0; i < num_make && sand_tiles.Count > 0; i++)
                    {
                        spot_choice = Exilania.rand.Next(0, sand_tiles.Count);
                        top_left = sand_tiles[spot_choice];
                        int uheight = Exilania.rand.Next(plant_range[0], plant_range[1] + 1);
                        int uwidth = plants[j].get_width();
                        top_left.Y -= uheight;
                        if (uwidth % 2 == 0)
                        {
                            top_left.X -= uwidth / 2;
                        }
                        else
                        {
                            top_left.X -= (uwidth - 1) / 2;
                        }

                        sand_tiles.RemoveAt(spot_choice);
                        if (top_left.Y < 0)
                        {

                        }
                        else
                        {
                            bool good = true;
                            for (int x = top_left.X; x < top_left.X + uwidth; x++)
                            {
                                for (int y = top_left.Y; y < top_left.Y + uheight; y++)
                                {
                                    if (w.map[w.wraparound_x(x), y].plant_index != -1 || w.map[w.wraparound_x(x), y].fgd_block_type != -1)
                                    {
                                        good = false;
                                    }
                                }
                            }
                            if (good)
                            {
                                w.plants.Add(new Plant(plants[j], w, top_left, uheight, w.plants.Count));
                            }
                        }
                    }
                } //end sand else if
            } //end loop
        } //end function

        public bool try_place_plant(Point spot_choice, int plant_id, World w)
        {
            int[] plant_range = plants[plant_id].get_height_range();
            Point top_left = spot_choice;
            int uheight = Exilania.rand.Next(plant_range[0], plant_range[1] + 1);
            int uwidth = plants[plant_id].get_width();
            top_left.Y -= uheight;
            if (uwidth % 2 == 0)
            {
                top_left.X -= uwidth / 2;
                top_left.X = w.wraparound_x(top_left.X);
            }
            else
            {
                top_left.X -= (uwidth - 1) / 2;
                top_left.X = w.wraparound_x(top_left.X);
            }

           
            if (top_left.Y < 0)
            {

            }
            else
            {
                bool good = true;
                for (int x = top_left.X; x < top_left.X + uwidth; x++)
                {
                    for (int y = top_left.Y; y < top_left.Y + uheight; y++)
                    {
                        if (w.map[w.wraparound_x(x), y].plant_index != -1 || w.map[w.wraparound_x(x), y].fgd_block_type != -1 ||
                            w.map[w.wraparound_x(x),y].furniture_index != -1)
                        {
                            good = false;
                        }
                    }
                }
                if (good)
                {
                    w.plants.Add(new Plant(plants[plant_id], w, top_left, uheight, w.plants.Count));
                    return true;
                }
            }
            return false;
        }
    }
}
