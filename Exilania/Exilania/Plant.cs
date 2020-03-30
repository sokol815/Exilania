using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public class DisplayGive
    {
        public int count;
        public ItemType type;
        public int id;

        public DisplayGive()
        {

        }

        public DisplayGive(int pcount, ItemType t, int pid)
        {
            count = pcount;
            type = t;
            id = pid;
        }

        public void add_n(int number)
        {
            count += number;
        }
    }
    public class PlantUnit
    {
        public Point top_left;
        public ushort[] images;
        public byte width;
        public byte height;
        public string piece_type;

        public PlantUnit()
        {
        }

        public PlantUnit(PlantUnit p)
        {
            top_left = p.top_left;
            images = new ushort[p.images.Length];
            p.images.CopyTo(images, 0);
            width = p.width;
            height = p.height;
            piece_type = p.piece_type;
        }

        public PlantUnit(int pheight, PlantData p, int piece_num, Point ptop_left)
        {
            top_left = ptop_left;
            height = (byte)pheight;
            width = (byte)p.pieces[piece_num].width_min;
            piece_type = p.pieces[piece_num].name;
            images = new ushort[width * height];
            for (int x = 0; x < images.Length; x++)
            {
                if (p.pieces[piece_num].random_images)
                {
                    images[x] = (ushort)p.pieces[piece_num].images[Exilania.rand.Next(0, p.pieces[piece_num].images.Length)];
                }
                else
                {
                    images[x] = (ushort)p.pieces[piece_num].images[x];
                }
            }
        }

        public PlantUnit(System.IO.BinaryReader r)
        {
            top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            width = r.ReadByte();
            height = r.ReadByte();
            piece_type = r.ReadString();
            images = new ushort[r.ReadByte()];
            for (int x = 0; x < images.Length; x++)
            {
                images[x] = r.ReadUInt16();
            }
        }

        public PlantUnit(Lidgren.Network.NetIncomingMessage r)
        {
            top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            width = r.ReadByte();
            height = r.ReadByte();
            piece_type = r.ReadString();
            images = new ushort[r.ReadByte()];
            for (int x = 0; x < images.Length; x++)
            {
                images[x] = r.ReadUInt16();
            }
        }

        public void write_unit(System.IO.BinaryWriter w)
        {
            w.Write((ushort)top_left.X);
            w.Write((ushort)top_left.Y);
            w.Write((byte)width);
            w.Write((byte)height);
            w.Write(piece_type);
            w.Write((byte)images.Length);
            for (int x = 0; x < images.Length; x++)
            {
                w.Write((ushort)images[x]);
            }
        }

        public void send_unit(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write((ushort)top_left.X);
            w.Write((ushort)top_left.Y);
            w.Write((byte)width);
            w.Write((byte)height);
            w.Write(piece_type);
            w.Write((byte)images.Length);
            for (int x = 0; x < images.Length; x++)
            {
                w.Write((ushort)images[x]);
            }
        }

        public void update_plant_indeces(World w, int world_id)
        {
            for (int x = top_left.X; x < top_left.X + width; x++)
            {
                for (int y = top_left.Y; y < top_left.Y + height; y++)
                {
                    w.map[w.wraparound_x(x), y].plant_index = world_id;
                }
            }
        }

        public int destroy_part(int new_height, World w, int world_plant_id, PlantData p)
        {
            update_plant_indeces(w, -1);
            //add instead of subtract because Y increases as it moves south. e.g. 24 is further down on the screen compared to 23. Origin is top left corner of screen (world map).
            top_left.Y += (height - new_height);
            int old_height = height;
            height = (byte)new_height;
            ushort[] new_images = new ushort[height * width];
            for (int x = 0; x < width * height; x++)
            {
                new_images[x] = images[((old_height - new_height) * width) + x];
            }
            images = new ushort[new_images.Length];
            for (int x = 0; x < images.Length; x++)
            {
                images[x] = new_images[x];
            }
            for (int x = 0; x < p.pieces.Count; x++)
            {
                if (p.pieces[x].name.ToLower() == "cut" + piece_type.ToLower() || (piece_type.ToLower().Contains("cut") && p.pieces[x].name.ToLower() == piece_type.ToLower()))
                {
                    if (p.pieces[x].random_images)
                    {
                        for (int i = 0; i < width; i++)
                        {
                            images[i] = (ushort)p.pieces[x].images[Exilania.rand.Next(0, p.pieces[x].images.Length)];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < width; i++)
                        {
                            images[i] = (ushort)p.pieces[x].images[i];
                        }
                    }
                    piece_type = p.pieces[x].name;
                    x = p.pieces.Count;
                }
            }
            update_plant_indeces(w, world_plant_id);
            //return the amount of blocks removed from this item.
            return old_height * width - (new_height * width);
        }
    }
    public class Plant
    {
        public bool is_empty;
        public byte plant_index;
        public string name;
        public Point top_left;
        public int width;
        public int height;
        public List<PlantUnit> pieces;
        public bool passable;
        /// <summary>
        /// used only to give items to the player... not for anything else!
        /// </summary>
        public static List<DisplayGive> gives;

        public Plant()
        {
            is_empty = true;
        }

        public Plant(Plant p)
        {
            is_empty = p.is_empty;
            plant_index = p.plant_index;
            name = p.name;
            top_left = p.top_left;
            width = p.width;
            height = p.height;
            pieces = new List<PlantUnit>();
            for (int i = 0; i < p.pieces.Count; i++)
            {
                pieces.Add(new PlantUnit(p.pieces[i]));
            }
            passable = p.passable;
        }

        public Plant(PlantData p, World w, Point top_left_loc, int pheight, int f_num)
        {
            plant_index = (byte)p.plant_id;
            name = p.name;
            top_left = top_left_loc;
            is_empty = false;
            height = pheight;
            width = p.get_width();
            passable = p.passable;
            pieces = new List<PlantUnit>();
            int[] portions = new int[p.anatomy.Length];
            int sum = 0;
            int use_id = 0;
            while (sum != pheight)
            {
                sum = 0;
                for (int x = 0; x < portions.Length; x++)
                {
                    use_id = p.get_part_id_by_name(p.anatomy[x]);
                    if (p.pieces[use_id].height_max != p.pieces[use_id].height_min)
                    {
                        portions[x] = Exilania.rand.Next(p.pieces[use_id].height_min, p.pieces[use_id].height_max + 1);
                        sum += portions[x];
                    }
                    else
                    {
                        sum += p.pieces[use_id].height_min;
                        portions[x] = p.pieces[use_id].height_min;
                    }
                }
            }
            Point pass = new Point(top_left_loc.X, top_left_loc.Y);
            int cur_top = 0;
            for (int x = 0; x < portions.Length; x++)
            {
                use_id = p.get_part_id_by_name(p.anatomy[x]);
                pass.X = ((width - p.pieces[use_id].width_max) / 2) + top_left_loc.X;
                pass.Y = cur_top + top_left_loc.Y;
                pieces.Add(new PlantUnit(portions[x], p, use_id, pass));
                cur_top += portions[x];
                pieces[x].update_plant_indeces(w, f_num);
            }
        }

       
        public Plant(System.IO.BinaryReader r)
        {
            is_empty = false;
            plant_index = r.ReadByte();
            name = r.ReadString();
            top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            height = r.ReadByte();
            int num_pieces = r.ReadByte();
            pieces = new List<PlantUnit>();
            for (int x = 0; x < num_pieces; x++)
            {
                pieces.Add(new PlantUnit(r));
            }
            passable = Exilania.plant_manager.plants[plant_index].passable;
        }

        public Plant(Lidgren.Network.NetIncomingMessage r)
        {
            is_empty = false;
            plant_index = r.ReadByte();
            name = r.ReadString();
            top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            height = r.ReadByte();
            int num_pieces = r.ReadByte();
            pieces = new List<PlantUnit>();
            for (int x = 0; x < num_pieces; x++)
            {
                pieces.Add(new PlantUnit(r));
            }
            passable = Exilania.plant_manager.plants[plant_index].passable;
        }

        public void write_plant(System.IO.BinaryWriter w)
        {
            w.Write(plant_index);
            w.Write(name);
            w.Write((ushort)top_left.X);
            w.Write((ushort)top_left.Y);
            w.Write((byte)height);
            w.Write((byte)pieces.Count);
            for (int x = 0; x < pieces.Count; x++)
            {
                pieces[x].write_unit(w);
            }
        }

        public void send_plant(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(plant_index);
            w.Write(name);
            w.Write((ushort)top_left.X);
            w.Write((ushort)top_left.Y);
            w.Write((byte)height);
            w.Write((byte)pieces.Count);
            for (int x = 0; x < pieces.Count; x++)
            {
                pieces[x].send_unit(w);
            }
        }

        public int get_frame(Point loc, World w)
        {
            for (int x = 0; x < pieces.Count; x++)
            {
                if (pieces[x].top_left.Y + pieces[x].height - 1 >= loc.Y)
                {
                    if (loc.X < pieces[x].top_left.X)
                        loc.X += w.map.GetLength(0);
                    return pieces[x].images[(loc.Y - pieces[x].top_left.Y) * pieces[x].width + (loc.X - pieces[x].top_left.X)]; 
                }
            }
            return 0;
        }

        /// <summary>
        /// this is used for full or partial destruction of a plant... will return true if the plant needs to be removed from World w.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="w"></param>
        /// <param name="a"></param>
        public bool destroy_give_items(Point loc, World w, Actor a, int world_plant_id)
        {
            gives = new List<DisplayGive>();
            bool destroy_plant = false;
            if (Exilania.plant_manager.plants[plant_index].behavior_cut == "Destroy")
            {
                give_items(a, 1);
                destroy_plant = true;
            }
            else
            {
                int remove_to = loc.Y;
                int num_add = 0;
                for (int x = 0; x < pieces.Count; x++)
                {
                    if (pieces[x].top_left.Y + pieces[x].height - 1 <= remove_to)
                    {
                        num_add += pieces[x].width * pieces[x].height;
                        pieces[x].update_plant_indeces(w, -1);
                        pieces.RemoveAt(x);
                        x--;
                    }
                    else
                    {
                        int height_remove = loc.Y - pieces[x].top_left.Y + 1;
                        num_add += pieces[x].destroy_part(pieces[x].height - height_remove, w, world_plant_id, Exilania.plant_manager.plants[plant_index]);
                        x = pieces.Count;
                    }
                }
                give_items(a, num_add);
                if (pieces.Count == 0)
                    destroy_plant = true;
            }

            //DESTROY PLANT STUFF HERE
            for (int i = 0; i < gives.Count; i++)
            {
                switch (gives[i].type)
                {
                    case ItemType.Block:
                        Exilania.display.fading_text.Add(new FadeText("@00+" + gives[i].count + " " + Exilania.block_types.blocks[gives[i].id].name,
                                                        Display.default_msec_show_fade_text, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40 - (i * 20), true, true));
                        break;
                    case ItemType.Furniture:
                        Exilania.display.fading_text.Add(new FadeText("@00+" + gives[i].count + " " + Exilania.furniture_manager.furniture[gives[i].id].name,
                                                        Display.default_msec_show_fade_text, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40 - (i * 20), true, true));
                        break;
                    case ItemType.ItemPiece:
                        Exilania.display.fading_text.Add(new FadeText("@00+" + gives[i].count + " " + Exilania.item_manager.item_pieces[gives[i].id].name,
                                                        Display.default_msec_show_fade_text, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40 - (i * 20), true, true));
                        break;
                    case ItemType.Material:
                        Exilania.display.fading_text.Add(new FadeText("@00+" + gives[i].count + " " + Exilania.material_manager.materials[gives[i].id].name,
                                                       Display.default_msec_show_fade_text, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40 - (i * 20), true, true));
                        break;
                }
            }
            return destroy_plant;
        }

        public void give_items(Actor a, int replicate)
        {
            bool match_found = false;
            for (int x = 0; x < replicate; x++)
            {
                foreach (var pair in Exilania.plant_manager.plants[plant_index].drops)
                {
                    if (Exilania.rand.Next(0, 101) <= pair.Value)
                    {
                        item_descriptor t = Acc.get_item_by_name(pair.Key);
                        switch (t.item_type)
                        {
                            case ItemType.Block:
                                a.items.pickup_block((sbyte)t.item_id);
                                match_found = false;
                                for (int i = 0; i < gives.Count; i++)
                                {
                                    if (gives[i].type == ItemType.Block && gives[i].id == t.item_id)
                                    {
                                        match_found = true;
                                        gives[i].count++;
                                        i = gives.Count;
                                    }
                                }
                                if (!match_found)
                                {
                                    gives.Add(new DisplayGive(1, ItemType.Block, t.item_id));
                                }
                                break;
                            case ItemType.Empty:
                                break;
                            case ItemType.Furniture:
                                a.items.pickup_furniture(t.item_id,1);
                                match_found = false;
                                for (int i = 0; i < gives.Count; i++)
                                {
                                    if (gives[i].type == ItemType.Furniture && gives[i].id == t.item_id)
                                    {
                                        match_found = true;
                                        gives[i].count++;
                                        i = gives.Count;
                                    }
                                }
                                if (!match_found)
                                {
                                    gives.Add(new DisplayGive(1, ItemType.Furniture, t.item_id));
                                }
                                break;
                            case ItemType.ItemPiece:
                                Item temp = new Item();
                                temp.add_piece(Exilania.item_manager.item_pieces[t.item_id], t.item_id, -1, 0, 0, 0);
                                temp.construct_item(Exilania.item_manager.item_pieces[t.item_id].name);
                                a.items.pickup_item(temp);
                                match_found = false;
                                for (int i = 0; i < gives.Count; i++)
                                {
                                    if (gives[i].type == ItemType.ItemPiece && gives[i].id == t.item_id)
                                    {
                                        match_found = true;
                                        gives[i].count++;
                                        i = gives.Count;
                                    }
                                }
                                if (!match_found)
                                {
                                    gives.Add(new DisplayGive(1, ItemType.ItemPiece, t.item_id));
                                }
                                break;
                            case ItemType.Material:
                                a.items.pickup_material(t.item_id, 1);
                                match_found = false;
                                for (int i = 0; i < gives.Count; i++)
                                {
                                    if (gives[i].type == ItemType.Material && gives[i].id == t.item_id)
                                    {
                                        match_found = true;
                                        gives[i].count++;
                                        i = gives.Count;
                                    }
                                }
                                if (!match_found)
                                {
                                    gives.Add(new DisplayGive(1, ItemType.Material, t.item_id));
                                }
                                break;
                        }
                        //give item to player!
                    }
                }
            }
        }

        public void unset_map(World w)
        {
            for (int x = 0; x < pieces.Count; x++)
            {
                pieces[x].update_plant_indeces(w, -1);
            }
        }

        public void reset_map(World w,int plant_world_id)
        {
            for (int x = 0; x < pieces.Count; x++)
            {
                pieces[x].update_plant_indeces(w, plant_world_id);
            }
        }
    }
}
