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
    public class ItemPieceEnumeration
    {
        public int itempiece_id;
        /// <summary>
        /// the spot on the grid to render this item.
        /// </summary>
        public Point loc;
        /// <summary>
        /// the number of 90 degree turns to make when drawing this itempiece
        /// </summary>
        public int rotation;
        public Dictionary<int,int> children;

        public ItemPieceEnumeration()
        {
            itempiece_id = 0;
            loc = new Point();
            rotation = 0;
            children = new Dictionary<int,int>();
        }

        public ItemPieceEnumeration(int piece_id, int locx, int locy, int protation)
        {
            itempiece_id = piece_id;
            loc = new Point(locx, locy);
            rotation = protation;
            children = new Dictionary<int,int>();
        }

        public ItemPieceEnumeration(System.IO.BinaryReader r)
        {
            itempiece_id = r.ReadInt32();
            loc = new Point((int)r.ReadInt16(), (int)r.ReadInt16());
            rotation = r.ReadByte();
            children = new Dictionary<int,int>();
            int num_child = r.ReadInt32();
            for (int x = 0; x < num_child; x++)
            {
                children[r.ReadInt32()] = r.ReadInt32();
            }
        }

        public ItemPieceEnumeration(Lidgren.Network.NetIncomingMessage r)
        {
            itempiece_id = r.ReadInt32();
            loc = new Point((int)r.ReadInt16(), (int)r.ReadInt16());
            rotation = r.ReadByte();
            children = new Dictionary<int,int>();
            int num_child = r.ReadInt32();
            for (int x = 0; x < num_child; x++)
            {
                children[r.ReadInt32()] = r.ReadInt32();
            }
        }

        public void write(System.IO.BinaryWriter w)
        {
            w.Write(itempiece_id);
            w.Write((short)loc.X);
            w.Write((short)loc.Y);
            w.Write((byte)rotation);
            w.Write(children.Count);
            foreach( var ped in children)
            {
                w.Write(ped.Key);
                w.Write(ped.Value);
            }
        }

        public void send_block_enum(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(itempiece_id);
            w.Write((short)loc.X);
            w.Write((short)loc.Y);
            w.Write((byte)rotation);
            w.Write(children.Count);
            foreach (var ped in children)
            {
                w.Write(ped.Key);
                w.Write(ped.Value);
            }
        }

    }
    public class Item
    {
        public string item_name;
        public List<ItemPieceEnumeration> pieces;
        public Dictionary<int, int> break_blocks;
        public Dictionary<string, int> materials;
        public int complexity;
        public int image_hash;
        public Point attachment_point;
        public Point light_attachment_point;
        public byte[] light;
        public bool is_flashlight;
        List<KeyValuePair<string, Vector2>> infos;
        Point draw_size;
        int worth;
        public float size_mag;

        public bool can_swing; //this is the click... unless false, then shoot is the click
        public string melee_damage;
        public int melee_range;
        public float swing_speed;
        public bool auto_swing;

        public bool can_shoot; //this is the shift-click unless not melee, then it is click
        public string ranged_damage;
        public Rectangle projectile_image;
        public float projectile_speed;
        public float fire_wait_time;
        public bool projectile_gravity_affect;
        public int projectile_energy_use;
        public int projectile_ammo_use;

        public Item()
        {
            init_empty();
        }

        public void init_empty()
        {
            item_name = "";
            pieces = new List<ItemPieceEnumeration>();
            break_blocks = new Dictionary<int, int>();
            materials = new Dictionary<string, int>();
            complexity = 0;
            image_hash = 0;
            attachment_point = new Point();
            can_swing = false;
            melee_damage = "0";
            melee_range = 48;
            swing_speed = .25f;
            auto_swing = false;
            can_shoot = false;
            ranged_damage = "0";
            projectile_image = new Rectangle();
            projectile_speed = 0;
            fire_wait_time = 0f;
            projectile_gravity_affect = false;
            worth = 0;
            projectile_ammo_use = 0;
            projectile_energy_use = 0;
        }

        public Item(System.IO.BinaryReader r)
        {
            init_empty();
            item_name = r.ReadString();
            int num_sub_pieces = r.ReadInt32();
            for (int x = 0; x < num_sub_pieces; x++)
            {
                pieces.Add(new ItemPieceEnumeration(r));
            }
            construct_item(item_name);
        }

        public Item(Lidgren.Network.NetIncomingMessage r)
        {
            init_empty();
            item_name = r.ReadString();
            int num_sub_pieces = r.ReadInt32();
            for (int x = 0; x < num_sub_pieces; x++)
            {
                pieces.Add(new ItemPieceEnumeration(r));
            }
            construct_item(item_name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p">item being added</param>
        /// <param name="ppiece_id">position in array of pieces (from the Exilania.ItemManager class)</param>
        /// <param name="parent_piece">id of parent to be added to</param>
        /// <param name="my_rotation">the number of clockwise 90 degree turns to do to attach it</param>
        /// <param name="my_attach">which point to attach to on the new piece</param>
        /// <param name="parent_attach">which point to attach to on the parent piece</param>
        public void add_piece(ItemPiece p,int ppiece_id, int parent_piece, int my_rotation, int my_attach, int parent_attach)
        {
            if (parent_piece == -1)
            {
                pieces.Add(new ItemPieceEnumeration(ppiece_id, 0, 0, my_rotation));
                for (int x = 0; x < p.item_attach_points.Count; x++)
                {
                    pieces[0].children[x] = -1;
                }
                return;
            }   
            //calculate the location of this item relative to the parent item, then get the center and that is the position of the item.
            Point parent_at = get_point_offset_position(parent_piece, parent_attach);
            pieces.Add(new ItemPieceEnumeration(ppiece_id, 0, 0, my_rotation));
            for (int x = 0; x < p.item_attach_points.Count; x++)
            {
                pieces[pieces.Count-1].children[x] = -1;
            }
            Point my_at = get_point_offset_position(pieces.Count - 1, my_attach);
            pieces[pieces.Count - 1].loc.X = pieces[parent_piece].loc.X + parent_at.X - my_at.X;
            pieces[pieces.Count - 1].loc.Y = pieces[parent_piece].loc.Y + parent_at.Y - my_at.Y;
        }

        /// <summary>
        /// this will transform the location of your point relative to the parents' origin. does not account for position.
        /// </summary>
        /// <param name="parent_piece">The index number of the building item piece in the item array</param>
        /// <param name="parent_attach">the attachment point on the parent to use.</param>
        /// <returns>returns the new local point for the rotated parent.</returns>
        public Point get_point_offset_position(int parent_piece, int parent_attach)
        {
            Point my_point = Exilania.item_manager.item_pieces[pieces[parent_piece].itempiece_id].item_attach_points[parent_attach];
            
            //offset for the center
            my_point.X -= Exilania.item_manager.item_pieces[pieces[parent_piece].itempiece_id].image.Width / 2;
            my_point.Y -= Exilania.item_manager.item_pieces[pieces[parent_piece].itempiece_id].image.Height / 2;

            if (pieces[parent_piece].rotation == 0)
            { //line is at the same point it was at before.
                //do nothing
            }
            else if (pieces[parent_piece].rotation == 1)
            { //line is now 90 degrees further clockwise; use -y,x
                my_point = new Point(my_point.Y * -1, my_point.X);
            }
            else if (pieces[parent_piece].rotation == 2)
            { //line is now 180 degrees further closkwise; use -x.-y
                my_point = new Point(my_point.X * -1, my_point.Y * -1);
            }
            else if (pieces[parent_piece].rotation == 3)
            { //line is now 270 degrees further clockwise; use y,-x
                my_point = new Point(my_point.Y, my_point.X * -1);
            }
            return my_point;
        }

        public void send_item(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(item_name);
            w.Write(pieces.Count);
            for (int x = 0; x < pieces.Count; x++)
            {
                pieces[x].send_block_enum(w);
            }
        }

        public void write_item(System.IO.BinaryWriter w)
        {
            w.Write(item_name);
            w.Write(pieces.Count);
            for (int x = 0; x < pieces.Count; x++)
            {
                pieces[x].write(w);
            }
        }

        public void construct_item(string name_item)
        {
            worth = 0;
            for (int x = 0; x < pieces.Count; x++)
            {
                worth += Exilania.item_manager.item_pieces[pieces[0].itempiece_id].worth;
            }
            item_name = name_item;
            if (pieces.Count > 1)
            {
                image_hash = this.ToString().GetHashCode();
            }
            else
            {
                item_name = Exilania.item_manager.item_pieces[pieces[0].itempiece_id].name;
                if (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].data.ContainsKey("SPEED"))
                    swing_speed += float.Parse(Exilania.item_manager.item_pieces[pieces[0].itempiece_id].data["SPEED"]) / 1000f;
                if (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].click_action.Contains("SWING"))
                    can_swing = true;
                if (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].click_action.Contains("FIRE"))
                    can_shoot = true;
                if (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].click_action == "")
                {
                    can_swing = true;
                }
                if (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].has_hand_attach_point)
                {
                    attachment_point = Exilania.item_manager.item_pieces[pieces[0].itempiece_id].hand_attach_point;
                }
                else
                {
                    attachment_point = new Point(
                        (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image.Width / 2) - (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image.Width % 2 == 0 ? 1 : 0),
                        (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image.Height / 2) - (Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image.Height % 2 == 0 ? 1 : 0));
                }
                image_hash = 0;
            }
            populate_break_blocks();
            populate_damage();
        }

        public void populate_damage()
        {
            if (!can_shoot)
            {
                ranged_damage = "0";
            }
            else
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (i == 0)
                    {
                        if (Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data.ContainsKey("DAMAGE"))
                            ranged_damage = Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data["DAMAGE"];
                        else
                            ranged_damage = "0";
                    }
                    else
                    {
                        if (Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data.ContainsKey("DAMAGE"))
                            ranged_damage += "+" + Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data["DAMAGE"];
                    }
                }
            }
            if (!can_swing)
            {
                melee_damage = "0";
            }
            else
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (i == 0)
                    {
                        if (Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data.ContainsKey("DAMAGE"))
                            melee_damage = Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data["DAMAGE"];
                        else
                            melee_damage = "0";
                    }
                    else
                    {
                        if (Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data.ContainsKey("DAMAGE"))
                            melee_damage += "+"+Exilania.item_manager.item_pieces[pieces[i].itempiece_id].data["DAMAGE"];
                    }
                }
            }
        }

        public void populate_break_blocks()
        {
            break_blocks = new Dictionary<int, int>();
            for (int i = 0; i < pieces.Count; i++)
            {
                foreach(KeyValuePair<int,int> pair in Exilania.item_manager.item_pieces[pieces[i].itempiece_id].break_block)
                {
                    if (break_blocks.ContainsKey(pair.Key))
                    {
                        break_blocks[pair.Key] = Math.Min(break_blocks[pair.Key], pair.Value);
                    }
                    else
                    {
                        break_blocks.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// this is used in the inventory only... physically drawing an item on a person is done by the body class itself.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="d"></param>
        /// <param name="location"></param>
        public void draw_item(SpriteBatch s, Display d, Point location)
        {
            if (pieces.Count == 1)
            { //this is just a single piece and can be drawn quite easily.

                Rectangle draw_at = new Rectangle();
                float width = Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image.Width;
                float height = Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image.Height;
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
                    location.X + 1 +  (int)((44 - width) / 2),
                    location.Y + 1 + (int)((44 - height) / 2),
                    (int)width, (int)height);
                s.Draw(d.sprites, draw_at, 
                    Exilania.item_manager.item_pieces[pieces[0].itempiece_id].image, Color.White);
            }
            else
            {
                s.Draw(Exilania.item_manager.custom_item_images[image_hash], new Rectangle(location.X,location.Y,Math.Min(44,Exilania.item_manager.custom_item_images[image_hash].Width),
                    Math.Min(44,Exilania.item_manager.custom_item_images[image_hash].Height))
                    , Exilania.item_manager.custom_item_images[image_hash].Bounds, Color.White);
            }
        }

        public string get_id(string pass, int parent_id)
        {
            if (parent_id == -1)
            {
                pass += ".-1";
                return pass;
            }
            if(pass!="")
                pass += "." + pieces[parent_id].itempiece_id.ToString();
            else
                pass = pieces[parent_id].itempiece_id.ToString();
            foreach (var ped in pieces[parent_id].children)
            {
                pass = get_id(pass, ped.Value);
            }
            return pass;
        }

        public void draw_info(Vector2 loc, SpriteBatch s, Display d, Actor a)
        {
            if (infos == null)
            {
                string meas = "";
                infos = new List<KeyValuePair<string, Vector2>>();
                infos.Add(new KeyValuePair<string, Vector2>(item_name, d.small_font.MeasureString(item_name)));
                if (can_swing)
                {
                    meas = "Swing Speed: " + (swing_speed * 1000) + "ms";
                    infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                    meas = "Damage: " + melee_damage;
                    infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                }
                if (can_shoot)
                {
                    meas = "Fire Speed: " + (fire_wait_time * 1000) + "ms";
                    infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                    meas = "Damage: " + ranged_damage;
                    infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                    if (projectile_energy_use != 0)
                    {
                        meas = "Energy/Shot: " + projectile_energy_use;
                        infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                    }
                    if (projectile_ammo_use != 0)
                    {
                        meas = "Ammo/Shot: " + projectile_ammo_use;
                        infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                    }
                }
                meas = "Value: " + worth + "cr.";
                infos.Add(new KeyValuePair<string, Vector2>(meas, d.small_font.MeasureString(meas)));
                draw_size = new Point(0,infos.Count * 20 + 20);
                for (int x = 0; x < infos.Count; x++)
                {
                    if (infos[x].Value.X > draw_size.X)
                    {
                        draw_size.X = (int)infos[x].Value.X;
                    }
                }
                draw_size.X += 20;
            }
            d.draw_bounding_box(s, new Rectangle((int)loc.X, (int)loc.Y, draw_size.X, draw_size.Y));
            d.draw_bounding_box(s, new Rectangle((int)loc.X, (int)loc.Y, draw_size.X, draw_size.Y));
            for (int x = 0; x < infos.Count; x++)
            {
                d.draw_text(s, d.small_font, "@00" + infos[x].Key, (int)loc.X + 10, (int)loc.Y + 10 + (x * 20), 500);
            }
        }

        public override string ToString()
        {
           return get_id("",0);
        }
    }
}
