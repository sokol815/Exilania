using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    public enum FACETTRACKERS
    {
        PROXIMITY =         1,
        ENEMY =             2,
        USERMOUSE =         3,
        ROTATECWSLOW =        4,
        ROTATECWMEDIUM=       5,
        ROTATECWFAST =        6,
        LEFTRIGHT =         7,
        USERDIRECTIONS =    8,
        ROTATEHOUR =        9,
        ROTATEMINUTE=       10,
        ROTATESPAWN =       11,
        ROTATECCWSLOW =     12,
        ROTATECCWMEDIUM =   13,
        ROTATECCWFAST =     14

    }
    enum FFLAGS
    {
        PASSABLE = 0,
        PLATFORM = 1,
        TRANSPARENT = 2,
        EMPTY = 3,
        BREAK_BELOW = 4,
        IS_CHEST = 5
    }
    public class FrameFacet
    {
        public int[] images;
        public byte width;
        public byte height;
        public FACETTRACKERS rotation_type;
        /// <summary>
        /// attach_point is in real world numbers, not indeces. so 24,36 would be 2feet right, 3 feet down.
        /// </summary>
        public Point attach_point;
        public Point center_of_rotation;

        public FrameFacet()
        {

        }

        /// <summary>
        /// pass in something like this: 2,2|(990,991,1022,1023)|24,24|ROTATEFAST
        /// </summary>
        /// <param name="info"></param>
        public FrameFacet(string info)
        {
            string[] pieces = info.Split('|');
            string[] subpiece = pieces[0].Split(',');
            width = byte.Parse(subpiece[0]);
            height = byte.Parse(subpiece[1]);
            subpiece = Acc.script_remove_outer_parentheses(pieces[1]).Split(',');
            images = new int[subpiece.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = int.Parse(subpiece[i]);
            }
            subpiece = pieces[2].Split(',');
            attach_point = new Point(int.Parse(subpiece[0]), int.Parse(subpiece[1]));
            if (pieces.Length == 4)
            {
                center_of_rotation = new Point(0,0);
                rotation_type = (FACETTRACKERS)Enum.Parse(typeof(FACETTRACKERS), pieces[3]);
            }
            else
            {
                subpiece = pieces[3].Split(',');
                center_of_rotation = new Point(int.Parse(subpiece[0]), int.Parse(subpiece[1]));
                center_of_rotation.X = width * 12 -center_of_rotation.X;
                center_of_rotation.Y = height * 12 -center_of_rotation.Y;

                rotation_type = (FACETTRACKERS)Enum.Parse(typeof(FACETTRACKERS), pieces[4]);
            }
        }
    }
    public class FrameFurniture
    {
        public int[] images;
        public byte width;
        public byte height;

        public FrameFurniture()
        {
            images = new int[0];
            width = 0;
            height = 0;
        }

        public FrameFurniture(FrameFurniture f)
        {
            f.images.CopyTo(images, 0);
            width = f.width;
            height = f.height;
        }

        public FrameFurniture(string read_in)
        {
            string[] split = read_in.Split('|');
            string[] size_split = split[0].Split(',');
            split = Acc.get_inner_parenthesis(split[1]).Split(',');
            width = byte.Parse(size_split[0]);
            height = byte.Parse(size_split[1]);
            images = new int[split.Length];
            for (int x = 0; x < images.Length; x++)
            {
                images[x] = int.Parse(split[x]);
            }
        }
    }

    public class FurnitureData
    {
        public string name;
        public ushort furniture_id;
        public int worth;
        public bool[] flags;
        public byte[] light_source;
        public string message;
        public string actions_proximity;
        public string actions_time;
        public string actions_click;
        public string actions_power;
        public Dictionary<int, FrameFurniture> image_frames;
        public List<FrameFacet> facets;
        public ushort[] state_power_production;
        public ushort[] state_power_usage;
        public int power_storage;
        public int max_power_storage;
        public Dictionary<string, byte> materials;
        public string[] craft_require;
        public int state_item_draw;
        public byte share_power;
        public int timeout;
        public int complexity;
        public ItemConnectionType default_connection_make;
        /// <summary>
        /// for things like wheels, jets, engines, drills
        /// </summary>
        public string vehicle_properties;

        public FurnitureData()
        {
            name = "";
            furniture_id = ushort.MaxValue;
            worth = 0;
            flags = new bool[6];
            flags[(int)FFLAGS.PASSABLE] = true;
            message = "";
            actions_proximity = "";
            actions_time = "";
            actions_click = "";
            actions_power = "";
            image_frames = new Dictionary<int, FrameFurniture>();
            facets = new List<FrameFacet>();
            //state power production and usage remain uninstantiated... null means there is none!
            power_storage = 0;
            max_power_storage = 0;
            materials = new Dictionary<string, byte>();
            //craft_require can also be uninstantiated... null means there is none!
            state_item_draw = 0;
            share_power = 255;
            timeout = 0;
            craft_require = new string[0];
            complexity = 0;
            default_connection_make = ItemConnectionType.Default;
            vehicle_properties = "";
        }

         public Rectangle get_source_rect(Display d)
         {
             Rectangle draw_at = d.frames[Exilania.furniture_manager.furniture[furniture_id].image_frames[Exilania.furniture_manager.furniture[furniture_id].state_item_draw].images[0]];
             draw_at.Width = Exilania.furniture_manager.furniture[furniture_id].image_frames[Exilania.furniture_manager.furniture[furniture_id].state_item_draw].width * 24;
             draw_at.Height = Exilania.furniture_manager.furniture[furniture_id].image_frames[Exilania.furniture_manager.furniture[furniture_id].state_item_draw].height * 24;
             return draw_at;
         }

         /// <summary>
         /// this is used in the inventory only... physically drawing an item on a person is done by the body class itself.
         /// </summary>
         /// <param name="s"></param>
         /// <param name="d"></param>
         /// <param name="location"></param>
         public void draw_item_furniture(SpriteBatch s, Display d, Point location)
         {
             Rectangle draw_at = d.frames[Exilania.furniture_manager.furniture[furniture_id].image_frames[Exilania.furniture_manager.furniture[furniture_id].state_item_draw].images[0]];
             float width = Exilania.furniture_manager.furniture[furniture_id].image_frames[Exilania.furniture_manager.furniture[furniture_id].state_item_draw].width * 24;
             float height = Exilania.furniture_manager.furniture[furniture_id].image_frames[Exilania.furniture_manager.furniture[furniture_id].state_item_draw].height * 24;
             Rectangle source = draw_at;
             source.Width = (int)width;
             source.Height = (int)height;
             
             float amt;

             if (width > height)
             { //29 > 7
                 amt = 44f / width;
             }
             else
             {
                 amt = 44f / height;
             }
             width *= amt;
             height *= amt;
             draw_at = new Rectangle(
                 location.X + 1 + (int)((44 - width) / 2),
                 location.Y + 1 + (int)((44 - height) / 2),
                 (int)width, (int)height);
             s.Draw(d.sprites, draw_at, source, Color.White);
         }

         public bool try_place(Actor a, Point world_place_loc, World w, Rectangle placer)
         {
             world_place_loc.Y += 1 - image_frames[0].height;
             //ok, interpret the world_place_loc as the bottom left corner of the item to be placed.
             bool open = false;
             if (!flags[(int)FFLAGS.PASSABLE] && !w.collision_table.can_place_at(world_place_loc, image_frames[0].width * 24, image_frames[0].height * 24, w, placer))
                 return false;

             for (int x = 0; x < image_frames[0].width; x++)
             {
                 for (int y = 0; y < image_frames[0].height; y++)
                 {
                     if (y + world_place_loc.Y < 0 || y + world_place_loc.Y >= w.map.GetLength(1))
                     {
                         return false;
                     }
                     else if ((w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].fgd_block_type == -1 ||
                         w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].fgd_block_type == 5) &&
                         w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].furniture_index == -1 &&
                         w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].plant_index == -1)
                     {
                        
                     }
                     else
                         return false;
                 }
             }
             for (int x = -1; x <= image_frames[0].width; x++)
             {
                 for (int y = -1; y <= image_frames[0].height; y++)
                 {
                     if (y + world_place_loc.Y < 0 || y + world_place_loc.Y >= w.map.GetLength(1))
                     {
                        
                     }
                     else if ((w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].fgd_block_type != -1 &&
                         w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].fgd_block_type != 5) || 
                         w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].bkd_block != -1)
                     {
                         open = true;
                     }
                 }
             }
             
             if (open)
             { //place said item!!!!
                 int insert_id = -1;
                 for (int x = 0; x < w.furniture.Count; x++)
                 {
                     if (w.furniture[x].flags[(int)FFLAGS.EMPTY])
                     {
                         insert_id = x;
                         x = w.furniture.Count;
                     }
                 }
                 if (insert_id > -1)
                 {
                     w.furniture[insert_id] = new Furniture(new Point(world_place_loc.X, world_place_loc.Y), a.name, this, w);
                 }
                 else
                 {
                     if (insert_id > short.MaxValue)
                         return false;
                     insert_id = w.furniture.Count;
                     w.furniture.Add(new Furniture(new Point(world_place_loc.X, world_place_loc.Y), a.name, this, w));
                 }
                 if (w.furniture[insert_id].flags[(int)FFLAGS.IS_CHEST] == true)
                 {
                     w.chests.Add(new ItemChest(25, true, "Default", "~ALL~", w.furniture[insert_id].top_left, insert_id));
                 }
                 w.collision_table.add_furniture_to_table(w.furniture, insert_id);
                 Point temp = new Point();
                 for (int x = 0; x < image_frames[0].width; x++)
                 {
                     for (int y = 0; y < image_frames[0].height; y++)
                     {
                         w.map[w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y].furniture_index = (short)insert_id;
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x + world_place_loc.X), world_place_loc.Y + y), -1);
                         temp.X = (x + world_place_loc.X) - ((x + world_place_loc.X) % LiquidSimulator.cell_size);
                         temp.Y = (world_place_loc.Y + y) - ((world_place_loc.Y + y) % LiquidSimulator.cell_size);
                         if (!w.liquid_simulator.cells_need_update.Contains(temp))
                             w.liquid_simulator.cells_need_update.Add(temp);
                         if (x == 0)
                         {
                             w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x + world_place_loc.X - 1), world_place_loc.Y + y), -1);
                             w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(image_frames[0].width + world_place_loc.X), world_place_loc.Y + y), -1);
                         }
                     }
                 }

                 return true;
             }
             return false;
         }

         public void remove_furniture(World w, Point loc)
         {
             int state = w.furniture[w.map[loc.X, loc.Y].furniture_index].state;
             int this_id = w.map[loc.X, loc.Y].furniture_index;
             w.furniture[this_id].flags[(int)FFLAGS.EMPTY] = true;
             w.collision_table.remove_furniture_from_table(w.furniture, this_id);
             loc = w.furniture[this_id].top_left;
             int width = image_frames[state].width;
             int height = image_frames[state].height;
             Point temp = new Point();
             if (flags[(int)FFLAGS.IS_CHEST])
             {
                 //find all players currently looking in the chest... close it.
                 
                 //find the id of this furniture, match it with the chest, obliterate the chest
                 for (int i = 0; i < w.chests.Count; i++)
                 {
                     if (w.chests[i].furniture_id == this_id)
                     {
                         for (int j = 0; j < w.players.Count; j++)
                         {
                             if (!w.players[j].is_player_empty && w.players[j].avatar.active_chest_id == i)
                                 w.players[j].avatar.active_chest_id = -1;
                         }
                         w.chests.RemoveAt(i);
                         i = w.chests.Count;
                     }
                 }
             }
             for (int x = loc.X; x < loc.X + width; x++)
             {
                 for (int y = loc.Y; y < loc.Y + height; y++)
                 {
                     w.map[w.wraparound_x(x), y].furniture_index = -1;
                     w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x), y), -1);
                     temp.X = x - (x % LiquidSimulator.cell_size);
                     temp.Y = y - (y % LiquidSimulator.cell_size);
                     if (!w.liquid_simulator.cells_need_update.Contains(temp))
                         w.liquid_simulator.cells_need_update.Add(temp);
                     if (x == loc.X)
                     {
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x - 1), y), -1);
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(loc.X + width), y), -1);
                     }
                 }
             }
         }

         public void clear_map_pointers(World w, Point loc)
         {
             int state = w.furniture[w.map[loc.X, loc.Y].furniture_index].state;
             w.collision_table.remove_furniture_from_table(w.furniture, w.map[loc.X, loc.Y].furniture_index);
             loc = w.furniture[w.map[loc.X, loc.Y].furniture_index].top_left;
             int width = image_frames[state].width;
             int height = image_frames[state].height;
             Point temp = new Point();
             for (int x = loc.X; x < loc.X + width; x++)
             {
                 for (int y = loc.Y; y < loc.Y + height; y++)
                 {
                     w.map[w.wraparound_x(x), y].furniture_index = -1;
                     w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x), y), -1);
                     temp.X = x - (x % LiquidSimulator.cell_size);
                     temp.Y = y - (y % LiquidSimulator.cell_size);
                     if (!w.liquid_simulator.cells_need_update.Contains(temp))
                         w.liquid_simulator.cells_need_update.Add(temp);
                     if (x == loc.X)
                     {
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x - 1), y), -1);
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(loc.X+width), y), -1);
                     }
                 }
             }
             w.liquid_simulator.do_cell_refactoring(w);
         }

         public void set_map_pointers(int furniture_num, World w, Point loc)
         {
             int state = w.furniture[furniture_num].state;
             loc = w.furniture[furniture_num].top_left;
             w.collision_table.add_furniture_to_table(w.furniture, furniture_num);
             int width = image_frames[state].width;
             int height = image_frames[state].height;
             Point temp = new Point();
             for (int x = loc.X; x < loc.X + width; x++)
             {
                 for (int y = loc.Y; y < loc.Y + height; y++)
                 {
                     w.map[w.wraparound_x(x), y].furniture_index = (short)furniture_num;
                     w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x), y), -1);
                     temp.X = x - (x % LiquidSimulator.cell_size);
                     temp.Y = y - (y % LiquidSimulator.cell_size);
                     if (!w.liquid_simulator.cells_need_update.Contains(temp))
                         w.liquid_simulator.cells_need_update.Add(temp);
                     if (x == loc.X)
                     {
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(x - 1), y), -1);
                         w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(loc.X + width), y), -1);
                     }
                 }
             }
             w.liquid_simulator.do_cell_refactoring(w);
         }
    }

    

    public class FurnitureManager
    {

        public List<FurnitureData> furniture;

        public FurnitureManager()
        {
            furniture = new List<FurnitureData>();

            if (System.IO.File.Exists(@"furniture.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"furniture.txt");
                string line = "";
                FurnitureData p = new FurnitureData();
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
                        string next = "";
                        switch (items[0].ToLower())
                        {
                            case "fpiece":
                                if (p.name == "")
                                {
                                    p.name = items[1].Trim();
                                    p.furniture_id = 0;
                                }
                                else
                                {
                                    p.furniture_id = (ushort)furniture.Count;
                                    furniture.Add(p);
                                    Exilania.text_stream.WriteLine("Furniture Item '" + p.name + "' Loaded.");
                                    p = new FurnitureData();
                                    p.name = items[1].Trim();
                                }
                                break;
                            case "click": p.actions_click = items[1].Trim();
                                next = r.ReadLine().Trim();
                                while (next.ToLower() != "EndClick".ToLower())
                                {
                                    if (next.Length == 0 || next[0] == '#')
                                    {

                                    }
                                    else
                                    {
                                        p.actions_click += " " + next;
                                    }
                                    next = r.ReadLine().Trim();
                                }
                                break;
                            case "proximity": p.actions_proximity = items[1].Trim();
                                next = r.ReadLine().Trim();
                                while (next.ToLower() != "EndProximity".ToLower())
                                {
                                    if (next.Length == 0 || next[0] == '#')
                                    {

                                    }
                                    else
                                    {
                                        p.actions_proximity += " " + next;
                                    }
                                    next = r.ReadLine().Trim();
                                }
                                break;
                            case "power": p.actions_power = items[1].Trim();
                                next = r.ReadLine().Trim();
                                while (next.ToLower() != "EndPower".ToLower())
                                {
                                    if (next.Length == 0 || next[0] == '#')
                                    {

                                    }
                                    else
                                    {
                                        p.actions_power += " " + next;
                                    }
                                    next = r.ReadLine().Trim();
                                }
                                break;
                            case "time": p.actions_time = items[1].Trim();
                                next = r.ReadLine().Trim();
                                while (next.ToLower() != "EndTime".ToLower())
                                {
                                    if (next.Length == 0 || next[0] == '#')
                                    {

                                    }
                                    else
                                    {
                                        p.actions_time += " " + next;
                                    }
                                    next = r.ReadLine().Trim();
                                }
                                break;
                           
                            case "worth": p.worth = int.Parse(items[1]);
                                break;
                            case "materials":
                                items = items[1].Split(';');
                                for (int x = 0; x < items.Length; x++)
                                {
                                    if (items[x].Length > 0)
                                    {
                                        string[] set = items[x].Split('=');
                                        p.materials.Add(set[0], byte.Parse(set[1]));
                                    }
                                }
                                break;
                            case "light_source":
                                items = items[1].Split(',');
                                p.light_source = new byte[] { byte.Parse(items[0]), byte.Parse(items[1]), byte.Parse(items[2]) };
                                break;
                            case "craft-require":
                                p.craft_require = items[1].Split(',');
                                break;
                            case "image":
                                items = items[1].Split(';');
                                for (int i = 0; i < items.Length; i++)
                                {
                                    string[] set = items[i].Split('=');
                                    p.image_frames.Add(int.Parse(set[0]), new FrameFurniture(set[1]));
                                }
                                break;
                            case "power_production":
                                items = items[1].Split(',');
                                p.state_power_production = new ushort[items.Length];
                                for (int i = 0; i < items.Length; i++)
                                {
                                    p.state_power_production[i] = ushort.Parse(items[i]);
                                }
                                break;
                            case "power_capacity":
                                p.max_power_storage = int.Parse(items[1]);
                                break;
                            case "cur_power":
                                p.power_storage = int.Parse(items[1]);
                                break;
                            case "power_usage":
                                items = items[1].Split(',');
                                p.state_power_usage = new ushort[items.Length];
                                for (int i = 0; i < items.Length; i++)
                                {
                                    p.state_power_usage[i] = ushort.Parse(items[i]);
                                }
                                break;
                            case "transparent": p.flags[(int)FFLAGS.TRANSPARENT] = bool.Parse(items[1]);
                                break;
                            case "platform": p.flags[(int)FFLAGS.PLATFORM] = bool.Parse(items[1]);
                                break;
                            case "passable": p.flags[(int)FFLAGS.PASSABLE] = bool.Parse(items[1]);
                                break;
                            case "break-below": p.flags[(int)FFLAGS.BREAK_BELOW] = bool.Parse(items[1]);
                                break;
                            case "is-chest": p.flags[(int)FFLAGS.IS_CHEST] = bool.Parse(items[1]);
                                break;
                            case "state_item_draw":
                                p.state_item_draw = int.Parse(items[1]);
                                break;
                            case "share_power":
                                p.share_power = Byte.Parse(items[1]);
                                break;
                            case "settimeout":
                                p.timeout = int.Parse(items[1]);
                                break;
                            case "complexity":
                                p.complexity = int.Parse(items[1]);
                                break;
                            case "connectiontype":
                                switch (items[1].ToLower())
                                {
                                    case "liquidpumpto": p.default_connection_make = ItemConnectionType.LiquidPumpTo; break;
                                    case "liquidpumpfrom": p.default_connection_make = ItemConnectionType.LiquidPumpFrom; break;
                                }
                                break;
                            case "facet":
                                string[] pi = items[1].Split(';');
                                for (int i = 0; i < pi.Length; i++)
                                {
                                    p.facets.Add(new FrameFacet(pi[i]));
                                }
                                break;
                            case "vehicle":
                                p.vehicle_properties += items[1];
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        p.furniture_id = (ushort)furniture.Count;
                        furniture.Add(p);
                        Exilania.text_stream.WriteLine("Furniture Item '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
            }
            else
            {
                Exilania.text_stream.Write("ERROR! No furniture.txt file.");
            }
        }
    }
}
