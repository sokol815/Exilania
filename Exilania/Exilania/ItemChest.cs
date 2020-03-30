using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    public class ItemChest
    {
        public static bool allow_click = true;
        public static int max_range = 180;
        public List<Cubby> items;
        public bool destroyable;
        public string chest_name;
        public string security;
        public Point display_loc = new Point(351, 334);
        public static Rectangle bounding_box = new Rectangle(351, 324, 324, 274);
        public Point world_top_left;
        public int furniture_id;

        public ItemChest()
        {
        }

        public ItemChest(System.IO.BinaryReader r)
        {
            destroyable = r.ReadBoolean();
            chest_name = r.ReadString();
            security = r.ReadString();
            world_top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            furniture_id = r.ReadUInt16();
            items = new List<Cubby>();
            int num_items = r.ReadByte();
            for (int i = 0; i < num_items; i++)
            {
                items.Add(new Cubby(r));
            }
        }

        public ItemChest(Lidgren.Network.NetIncomingMessage r)
        {
            destroyable = r.ReadBoolean();
            chest_name = r.ReadString();
            security = r.ReadString();
            world_top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            furniture_id = r.ReadUInt16();
            items = new List<Cubby>();
            int num_items = r.ReadByte();
            for (int i = 0; i < num_items; i++)
            {
                items.Add(new Cubby(r));
            }
        }

        public ItemChest(int num_items, bool is_destroyable, string pchest_name, string psecurity, Point top_left, int pfurniture_id)
        {
            destroyable = is_destroyable;
            chest_name = pchest_name;
            security = psecurity;
            world_top_left = top_left;
            furniture_id = pfurniture_id;
            items = new List<Cubby>();
            for (int i = 0; i < num_items; i++)
            {
                items.Add(new Cubby());
                items[i].draw_loc = new Vector2(display_loc.X + ((i % (num_items / 5)) * 50), display_loc.Y + (i / (num_items / 5) * 50));
            }
        }

        public void send_chest(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(destroyable);
            w.Write(chest_name);
            w.Write(security);
            w.Write((ushort)world_top_left.X);
            w.Write((ushort)world_top_left.Y);
            w.Write((ushort)furniture_id);
            w.Write((byte)items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].send_cubby(w);
            }
        }

        public void write_chest(System.IO.BinaryWriter w)
        {
            w.Write(destroyable);
            w.Write(chest_name);
            w.Write(security);
            w.Write((ushort)world_top_left.X);
            w.Write((ushort)world_top_left.Y);
            w.Write((ushort)furniture_id);
            w.Write((byte)items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].write_cubby(w);
            }
        }

        public void do_input(World w, Actor a, Point mouse_loc)
        {
            if (allow_click)
            {
                allow_click = false;
                int clicked_id = -1;
                Rectangle temp = new Rectangle();
                for (int i = 0; i < items.Count; i++)
                {
                    temp = new Rectangle(display_loc.X + ((i % (25 / 5)) * 50), display_loc.Y + (i / (25 / 5) * 50), 50, 50);
                    if (temp.Contains(mouse_loc))
                    {
                        clicked_id = i;
                    }
                }
                if (clicked_id != -1)
                { //found where clicked... decide what to do.
                    bool changed = false;
                    Cubby holder = new Cubby(a.items.temporary);
                    //if (!a.items.temporary.is_empty && !items[clicked_id].is_empty)
                    //{
                        if (items[clicked_id].is_block == a.items.temporary.is_block && (items[clicked_id].is_block && items[clicked_id].block_id == a.items.temporary.block_id))
                        {
                            if (items[clicked_id].quantity + a.items.temporary.quantity <= ushort.MaxValue)
                            {
                                items[clicked_id].quantity += a.items.temporary.quantity;
                                a.items.temporary.is_empty = true;
                                a.items.temporary.is_block = false;
                                changed = true;
                            }
                            else
                            {
                                a.items.temporary.quantity = (ushort)(ushort.MaxValue - items[clicked_id].quantity);
                                items[clicked_id].quantity = ushort.MaxValue;
                                changed = true;
                            }
                        }
                        else if (items[clicked_id].is_furniture == a.items.temporary.is_furniture && (items[clicked_id].is_furniture && items[clicked_id].furniture_id == a.items.temporary.furniture_id))
                        {
                            if (items[clicked_id].quantity + a.items.temporary.quantity <= ushort.MaxValue)
                            {
                                items[clicked_id].quantity += a.items.temporary.quantity;
                                a.items.temporary.is_empty = true;
                                a.items.temporary.is_furniture = false;
                                changed = true;
                            }
                            else
                            {
                                a.items.temporary.quantity = (ushort)(ushort.MaxValue - items[clicked_id].quantity);
                                items[clicked_id].quantity = ushort.MaxValue;
                                changed = true;
                            }
                        }
                        else if (items[clicked_id].is_material == a.items.temporary.is_material && (items[clicked_id].is_material && items[clicked_id].material_id == a.items.temporary.material_id))
                        {
                            if (items[clicked_id].quantity + a.items.temporary.quantity <= ushort.MaxValue)
                            {
                                items[clicked_id].quantity += a.items.temporary.quantity;
                                a.items.temporary.is_empty = true;
                                a.items.temporary.is_furniture = false;
                                changed = true;
                            }
                            else
                            {
                                a.items.temporary.quantity = (ushort)(ushort.MaxValue - items[clicked_id].quantity);
                                items[clicked_id].quantity = ushort.MaxValue;
                                changed = true;
                            }
                        }
                        else
                        {
                            a.items.temporary = new Cubby(items[clicked_id], a.items.temporary.draw_loc, false, a.items.temporary.info_text);
                            items[clicked_id] = new Cubby(holder, items[clicked_id].draw_loc, true, items[clicked_id].info_text);
                            changed = true;
                        }
                    //}
                    if(changed)
                        Exilania.network_client.send_changed_chest(furniture_id, clicked_id, items[clicked_id]);
                }
            }               
        }

        public static bool is_empty(World w, int furn_id)
        {
            for (int i = 0; i < w.chests.Count; i++)
            {
                if (w.chests[i].furniture_id == furn_id)
                {
                    for (int j = 0; j < w.chests[i].items.Count; j++)
                    {
                        if (!w.chests[i].items[j].is_empty)
                            return false;
                    }
                    i = w.chests.Count;
                }
            }
            return true;
        }

        public void draw_inventory(SpriteBatch s, Display d, Actor a)
        {
            d.draw_metal_box(s, new Rectangle(display_loc.X - 10, display_loc.Y - 10, 324, 274));

            for (int i = 0; i < items.Count; i++)
            {
                items[i].draw_cubby(s, d, false);
            }
            Point on_screen_loc = new Point(Microsoft.Xna.Framework.Input.Mouse.GetState().X, Microsoft.Xna.Framework.Input.Mouse.GetState().Y);
            int hover_id = -1;
            Rectangle temp = new Rectangle();
            for (int i = 0; i < items.Count; i++)
            {
                temp = new Rectangle(display_loc.X + ((i % (25 / 5)) * 50), display_loc.Y + (i / (25 / 5) * 50), 50, 50);
                if (temp.Contains(on_screen_loc))
                {
                    hover_id = i;
                }
            }
            
             if (hover_id != -1 && !items[hover_id].is_empty)
            {
                items[hover_id].draw_info(on_screen_loc, s, d, a);
            }
        }
    }
}
