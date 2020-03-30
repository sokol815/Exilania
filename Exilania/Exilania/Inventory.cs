using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace Exilania
{
    public enum ItemType
    {
        Block = 0,
        ItemPiece = 1,
        Furniture = 2,
        Material = 3,
        //Item = 4,
        Empty = -1
    }

    /// <summary>
    /// this class has it's stream writing/reading done.
    /// </summary>
    public class Cubby
    {
        public static Point loc_first = new Point(1345, 0);
        public static int size = 46;
        public bool is_empty;
        public bool is_block;
        public bool is_furniture;
        public bool is_material;
        public int material_id;
        public int furniture_id;
        public bool draw_background;
        public ushort quantity;
        public sbyte block_id;
        public Item phys_item;
        public byte color_code;
        public Vector2 draw_loc;
        public string title;
        public string tool_tip;
        public string info_text;

        public Cubby()
        {
            is_empty = true;
            is_block = false;
            is_material = false;
            material_id = -1;
            is_furniture = false;
            furniture_id = -1;
            draw_background = true;
            quantity = 0;
            block_id = 0;
            color_code = 0;
            draw_loc = new Vector2(0, 0);
            title = "";
            tool_tip = "";
            info_text = "";

        }

        /// <summary>
        /// switches everything on items except the draw locations and background state...
        /// </summary>
        /// <param name="c"></param>
        public Cubby(Cubby c, Vector2 pdraw_loc, bool pdraw_background, string pinfo_text)
        {
            is_empty = c.is_empty;
            is_block = c.is_block;
            is_material = c.is_material;
            material_id = c.material_id;
            is_furniture = c.is_furniture;
            furniture_id = c.furniture_id;
            quantity = c.quantity;
            block_id = c.block_id;
            phys_item = c.phys_item;
            draw_loc = pdraw_loc;
            color_code = c.color_code;
            draw_background = pdraw_background;
            title = c.title;
            tool_tip = c.tool_tip;
            info_text = pinfo_text;
        }

        /// <summary>
        /// does a 1 to 1 copy of another cubby
        /// </summary>
        /// <param name="c"></param>
        public Cubby(Cubby c)
        {
            is_empty = c.is_empty;
            is_block = c.is_block;
            is_furniture = c.is_furniture;
            furniture_id = c.furniture_id;
            is_material = c.is_material;
            material_id = c.material_id;
            draw_background = c.draw_background;
            quantity = c.quantity;
            block_id = c.block_id;
            phys_item = c.phys_item;
            draw_loc = c.draw_loc;
            color_code = c.color_code;
            title = c.title;
            tool_tip = c.tool_tip;
            info_text = c.info_text;
        }

        
        public Cubby(sbyte pblock_id, ushort pnum_blocks)
        {
            is_empty = false;
            is_block = true;
            is_furniture = false;
            furniture_id = -1;
            material_id = -1;
            is_material = false;
            draw_background = true;
            quantity = pnum_blocks;
            block_id = pblock_id;
            phys_item = null;
            color_code = 0;
            draw_loc = new Vector2(0, 0);
            title = Exilania.block_types.blocks[pblock_id].name;
            tool_tip = "";
            info_text = "";
        }

        public Cubby(System.IO.BinaryReader r)
        {
            is_empty = r.ReadBoolean();
            is_block = r.ReadBoolean();
            is_material = r.ReadBoolean();
            material_id = r.ReadInt32();
            is_furniture = r.ReadBoolean();
            furniture_id = r.ReadInt32();
            draw_background = r.ReadBoolean();
            quantity = r.ReadUInt16();
            block_id = r.ReadSByte();
            if (r.ReadBoolean())
            {
                phys_item = new Item(r);
            }
            draw_loc = new Vector2((float)r.ReadDouble(), (float)r.ReadDouble());
            color_code = r.ReadByte();
            title = r.ReadString();
            tool_tip = r.ReadString();
            info_text = r.ReadString();
        }

        public Cubby(NetIncomingMessage r)
        {
            is_empty = r.ReadBoolean();
            is_block = r.ReadBoolean();
            is_material = r.ReadBoolean();
            material_id = r.ReadInt32();
            is_furniture = r.ReadBoolean();
            furniture_id = r.ReadInt32();
            draw_background = r.ReadBoolean();
            quantity = r.ReadUInt16();
            block_id = r.ReadSByte();
            if (r.ReadBoolean())
            {
                phys_item = new Item(r);
            }
            draw_loc = new Vector2((float)r.ReadDouble(), (float)r.ReadDouble());
            color_code = r.ReadByte();
            title = r.ReadString();
            tool_tip = r.ReadString();
            info_text = r.ReadString();
        }

        public void write_cubby(System.IO.BinaryWriter w)
        {
            w.Write(is_empty);
            w.Write(is_block);
            w.Write(is_material);
            w.Write(material_id);
            w.Write(is_furniture);
            w.Write(furniture_id);
            w.Write(draw_background);
            w.Write(quantity);
            w.Write(block_id);
            w.Write(phys_item != null);
            if (phys_item != null)
            {
                phys_item.write_item(w);
            }
            w.Write((double)draw_loc.X);
            w.Write((double)draw_loc.Y);
            w.Write(color_code);
            w.Write(title);
            w.Write(tool_tip);
            w.Write(info_text);
        }

        public void send_cubby(NetOutgoingMessage w)
        {
            w.Write(is_empty);
            w.Write(is_block);
            w.Write(is_material);
            w.Write(material_id);
            w.Write(is_furniture);
            w.Write(furniture_id);
            w.Write(draw_background);
            w.Write(quantity);
            w.Write(block_id);
            w.Write(phys_item != null);
            if (phys_item != null)
            {
                phys_item.send_item(w);
            }
            w.Write((double)draw_loc.X);
            w.Write((double)draw_loc.Y);
            w.Write(color_code);
            w.Write(title);
            w.Write(tool_tip);
            w.Write(info_text);
        }

        public void set_cubby_item(Item p)
        {
            is_empty = false;
            is_block = false;
            is_furniture = false;
            is_material = false;
            phys_item = p;
            title = p.item_name;
            color_code = 0;
        }

        public void set_cubby_furniture(FurnitureData f, int quant)
        {
            is_empty = false;
            is_block = false;
            is_material = false;
            phys_item = null;
            is_furniture = true;
            quantity = (ushort)quant;
            color_code = 0;
            title = f.name;
            furniture_id = f.furniture_id;
        }

        public void set_cubby_material(MaterialData m, ushort quant)
        {
            is_empty = false;
            is_block = false;
            is_furniture = false;
            phys_item = null;
            is_material = true;
            quantity = quant;
            color_code = 0;
            title = m.name;
            tool_tip = m.description;
            material_id = m.material_id;

        }

        public void reset_cubby(sbyte pblock_id, ushort pnum_blocks)
        {
            is_empty = false;
            is_block = true;
            is_material = false;
            is_furniture = false;
            quantity = pnum_blocks;
            block_id = pblock_id;
            phys_item = null;
            color_code = 0;
            title = Exilania.block_types.blocks[pblock_id].name;
        }

        /// <summary>
        /// pass this the block type (not the group id... that is self-calculated)
        /// </summary>
        /// <param name="block_id"></param>
        /// <returns></returns>
        public bool is_block_breaker(sbyte block_id, bool pis_furniture)
        {
            if (is_empty || is_block || is_material || phys_item == null)
            {
                return false;
            }
            if(pis_furniture)
                return phys_item.break_blocks.ContainsKey(4);
            sbyte get_group = -1;
            if(block_id > -1)
                get_group = Exilania.block_types.blocks[block_id].block_group;
            return phys_item.break_blocks.ContainsKey(get_group);
        }

        public int get_swings_needed_to_break(sbyte block_id, bool is_group)
        {
            if (is_group)
            {
                if (phys_item.break_blocks.ContainsKey(block_id))
                {
                    return phys_item.break_blocks[block_id];
                }
            }
            else
            {
                sbyte get_group = -1;
                if (block_id > -1)
                    get_group = Exilania.block_types.blocks[block_id].block_group;
                if (phys_item.break_blocks.ContainsKey(get_group))
                    return phys_item.break_blocks[get_group];
            }
            return 1000000;
        }

      

        public void use_block()
        {
            if (is_block && !is_empty)
            {
                quantity--;
                if (quantity == 0)
                {
                    is_empty = true;
                    is_block = false;
                }
            }
        }

        public void add_block()
        {
            if (is_block && !is_empty)
            {
                quantity++;
                if (quantity == 0)
                {
                    quantity = ushort.MaxValue;
                }
            }
        }

        public void draw_cubby(SpriteBatch s, Display d, bool draw_text)
        {
            int width = 0;
            if (is_empty)
            {
                if (draw_background)
                {
                    s.Draw(d.sprites, draw_loc, new Rectangle(Cubby.loc_first.X + ((color_code % 4) * size), Cubby.loc_first.Y + ((color_code / 4) * size), size, size), Color.White);
                }
                if (info_text != "")
                {
                    width = (int)d.small_font.MeasureString(info_text.ToString()).X;
                    d.draw_text(s, d.small_font, "@08" + info_text, (int)draw_loc.X + 23 - (width / 2), (int)draw_loc.Y - 15, 200);
                }
            }
            else
            {
                if (draw_background)
                {
                    s.Draw(d.sprites, draw_loc, new Rectangle(Cubby.loc_first.X + ((color_code % 4) * size), Cubby.loc_first.Y + ((color_code / 4) * size), size, size), Color.White);
                }
                
                if (is_block)
                {
                    s.Draw(d.sprites, new Rectangle((int)draw_loc.X + 11, (int)draw_loc.Y + 11, 24, 24), d.frames[Exilania.block_types.blocks[block_id].image_pointers[Exilania.block_types.blocks[block_id].block_image_use]], Color.White);
                    string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                    string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                    width = (int)d.small_font.MeasureString(write_quant).X;
                    d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_loc.X + 23 - (width / 2), (int)draw_loc.Y + 26, 200);
                }
                else if(phys_item!=null)
                {
                    phys_item.draw_item(s, d, new Point((int)draw_loc.X,(int)draw_loc.Y));
                }
                else if (is_furniture)
                {
                    Exilania.furniture_manager.furniture[furniture_id].draw_item_furniture(s, d, new Point((int)draw_loc.X, (int)draw_loc.Y));
                    string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                    string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                    width = (int)d.small_font.MeasureString(write_quant).X;
                    d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_loc.X + 23 - (width / 2), (int)draw_loc.Y + 26, 200);
                }
                else if (is_material)
                {
                    s.Draw(d.sprites, new Rectangle((int)draw_loc.X + 11, (int)draw_loc.Y + 11, 24, 24), d.frames[Exilania.material_manager.materials[material_id].image], Color.White);
                    string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                    string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                    width = (int)d.small_font.MeasureString(write_quant).X;
                    d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_loc.X + 23 - (width / 2), (int)draw_loc.Y + 26, 200);
                }
                if (info_text != "")
                {
                    width = (int)d.small_font.MeasureString(info_text.ToString()).X;
                    d.draw_text(s, d.small_font, "@08" + info_text, (int)draw_loc.X + 23 - (width / 2), (int)draw_loc.Y - 15, 200);
                }
                if (draw_text)
                {

                }
            }
        }

        public void draw_at_left(SpriteBatch s, Display d, bool draw_text)
        {
            Vector2 draw_left = new Vector2(550, 10);
            //draw block
            int width = 0;
            if (is_block)
            {
                s.Draw(d.sprites, new Rectangle((int)draw_left.X + 11, (int)draw_left.Y + 11, 24, 24), d.frames[Exilania.block_types.blocks[block_id].image_pointers[Exilania.block_types.blocks[block_id].block_image_use]], Color.White);
                //measure text
                string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                width = (int)d.small_font.MeasureString(write_quant).X;
                d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_left.X + 28 - (width / 2), (int)draw_left.Y + 20, 200);
            }
            else if (phys_item!= null)
            {
                phys_item.draw_item(s, d, new Point((int)draw_left.X, (int)draw_left.Y));
            }
            else if(is_furniture)
            {
                Exilania.furniture_manager.furniture[furniture_id].draw_item_furniture(s, d, new Point((int)draw_left.X, (int)draw_left.Y));
                string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                width = (int)d.small_font.MeasureString(write_quant).X;
                d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_left.X + 23 - (width / 2), (int)draw_left.Y + 26, 200);
            }
            if (is_material)
            {
                s.Draw(d.sprites, new Rectangle((int)draw_left.X + 11, (int)draw_left.Y + 11, 24, 24), d.frames[Exilania.material_manager.materials[material_id].image], Color.White);
                //measure text
                string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                width = (int)d.small_font.MeasureString(write_quant).X;
                d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_left.X + 28 - (width / 2), (int)draw_left.Y + 20, 200);
            }
            if (info_text != "")
            { //draw info_text
                width = (int)d.small_font.MeasureString(info_text.ToString()).X;
                d.draw_text(s, d.small_font, "@05" + info_text, (int)draw_left.X + 28 - (width / 2), (int)draw_left.Y - 15, 200);
            }
            if (draw_text)
            {

            }
        }

        public void draw_at_right(SpriteBatch s, Display d, bool draw_text)
        {
            Vector2 draw_left = new Vector2(600, 10);
            //draw block
            int width = 0;
            if (is_block)
            {
                s.Draw(d.sprites, new Rectangle((int)draw_left.X + 11, (int)draw_left.Y + 11, 24, 24), d.frames[Exilania.block_types.blocks[block_id].image_pointers[Exilania.block_types.blocks[block_id].block_image_use]], Color.White);
                //measure text
                string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                width = (int)d.small_font.MeasureString(write_quant).X;
                //draw quantity_text
                d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_left.X + 28 - (width / 2), (int)draw_left.Y + 20, 200);
            }
            else if(phys_item!=null)
            {
                phys_item.draw_item(s, d, new Point((int)draw_left.X, (int)draw_left.Y));
            }
            else if(is_furniture)
            {
                Exilania.furniture_manager.furniture[furniture_id].draw_item_furniture(s, d, new Point((int)draw_left.X, (int)draw_left.Y));
                string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                width = (int)d.small_font.MeasureString(write_quant).X;
                d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_left.X + 23 - (width / 2), (int)draw_left.Y + 26, 200);
            }
            else if (is_material)
            {
                s.Draw(d.sprites, new Rectangle((int)draw_left.X + 11, (int)draw_left.Y + 11, 24, 24), d.frames[Exilania.material_manager.materials[material_id].image], Color.White);
                //measure text
                string write_quant = (quantity >= 1000 ? (quantity / 1000) + "K" : quantity.ToString());
                string rich_write = (quantity >= 1000 ? (quantity / 1000) + "@10K" : quantity.ToString());
                width = (int)d.small_font.MeasureString(write_quant).X;
                //draw quantity_text
                d.draw_text(s, d.small_font, "@00" + rich_write, (int)draw_left.X + 28 - (width / 2), (int)draw_left.Y + 20, 200);
            }
            if (info_text != "")
            { //draw info_text
                width = (int)d.small_font.MeasureString(info_text.ToString()).X;
                d.draw_text(s, d.small_font, "@05" + info_text, (int)draw_left.X + 28 - (width / 2), (int)draw_left.Y - 15, 200);
            }
            if (draw_text)
            {

            }
        }

        public void draw_info(Point loc, SpriteBatch s, Display d, Actor a)
        {
            if (phys_item != null)
            {
                phys_item.draw_info(new Vector2(draw_loc.X + 70, draw_loc.Y + 15), s, d, a);
            }
            else
            {
                Vector2 size_title = d.small_font.MeasureString(title);
                Vector2 size_quantity = d.small_font.MeasureString("Quantity: " + quantity);
                Vector2 size_tooltip = d.small_font.MeasureString(tool_tip);
                Vector2 worth_tip;
                Vector2 total_size = new Vector2();
                total_size.X = Math.Max(size_title.X, size_quantity.X);
                total_size.X = Math.Max(total_size.X, size_tooltip.X);
                total_size.Y = 60;
                if (is_material || is_furniture)
                {
                    if (is_furniture)
                    {
                        worth_tip = d.small_font.MeasureString("Value: " + (Exilania.furniture_manager.furniture[furniture_id].worth * quantity) + "cr.");
                        total_size.X = Math.Max(total_size.X, worth_tip.X);
                    }
                    else
                    {
                        worth_tip = d.small_font.MeasureString("Value: " + (Exilania.material_manager.materials[material_id].worth * quantity) + "cr.");
                        total_size.X = Math.Max(total_size.X, worth_tip.X);
                    }
                    total_size.Y += 20;
                }
                total_size.X += 20;
                
                if (size_tooltip.X > 0)
                    total_size.Y += 20;

                d.draw_bounding_box(s, new Rectangle((int)draw_loc.X + 70, (int)draw_loc.Y + 15, (int)total_size.X, (int)total_size.Y));
                d.draw_bounding_box(s, new Rectangle((int)draw_loc.X + 70, (int)draw_loc.Y + 15, (int)total_size.X, (int)total_size.Y));
                d.draw_text(s, d.small_font, "@00" + title, (int)draw_loc.X + 80, (int)draw_loc.Y + 20, 500);
                d.draw_text(s, d.small_font, "@00Quantity: " + quantity, (int)draw_loc.X + 80, (int)draw_loc.Y + 40, 500);
                d.draw_text(s, d.small_font, "@00" + tool_tip, (int)draw_loc.X + 80, (int)draw_loc.Y + 60, 1000);
                if (is_furniture || is_material)
                {
                    if (is_furniture)
                    {
                        d.draw_text(s, d.small_font, "@00" + "Value: " + (Exilania.furniture_manager.furniture[furniture_id].worth * quantity) + "cr.", (int)draw_loc.X + 80, (int)draw_loc.Y + 60, 1000);
                    }
                    else
                    {
                        d.draw_text(s, d.small_font, "@00" + "Value: " + (Exilania.material_manager.materials[material_id].worth * quantity) + "cr.", (int)draw_loc.X + 80, (int)draw_loc.Y + 80, 1000);
                    }
                }
            }
        }

        public override string ToString()
        {
            return (is_block ? "block" : "") + (is_furniture ? "furniture" : "") + (is_empty ? "empty" : "") + (is_material ? "material" : "");
        }
    }

    /// <summary>
    /// this class has it's stream writing/reading done.
    /// </summary>
    public class Inventory
    {
        public static Rectangle toolbar_area = new Rectangle(300, 0, 598, 46);
        public static Rectangle left_hand_area = new Rectangle(550, 10, 49, 60);
        public static Rectangle right_hand_area = new Rectangle(599, 10, 49, 60);
        public static Rectangle backpack_area = new Rectangle(351, 75, 496, 246);
        public static Rectangle closed_area = new Rectangle(300, 10, 598, 60);
        public static Rectangle opened_area = new Rectangle(300, 10, 598, 311);
        public static Rectangle trash_area = new Rectangle(300, 325, 46, 46);
        /// <summary>
        /// tells other classes how to interact with this class... only if it is a player will it be used as a full inventory, otherwise it will become a whole lot less.
        /// </summary>
        public bool is_player;
        public static char[] char_equivs = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')' };
        /// <summary>
        /// these are the 10 items in the top that are visible... they correspond to 1 through 0.
        /// </summary>
        public List<Cubby> hotbar_items;
        public Cubby trash;
        /// <summary>
        /// item pointer in the left hand; holding an item in temporary will override this.
        /// </summary>
        public int cur_left_hand;
        /// <summary>
        /// item pointer in the right hand; holding an item in temporary will override this.
        /// </summary>
        public int cur_right_hand;
        /// <summary>
        /// these are all the items carried on the character... most likely something like 50 cubby holds in rows of 10.
        /// </summary>
        public List<Cubby> backpack_items;
        public bool show_backpack = false;
        /// <summary>
        /// this is the item that the mouse is currently holding... if the mouse isn't holding anything, then it is your currently selected item.
        /// </summary>
        public Cubby temporary;
        public bool last_left_state = false;
        /// <summary>
        /// this gets turned to true if the check_mouse function in Inventory.cs consumes the click.
        /// </summary>
        public bool click_consumed = false;
        public int body_item_id = -1;


        public Inventory()
        {
           
        }

        public Inventory(bool pis_player)
        {
            is_player = pis_player;
            if (is_player)
            {
                trash = new Cubby();
                trash.draw_loc = new Vector2(300, 275);
                trash.info_text = "Trash";
                trash.color_code = 1;
                hotbar_items = new List<Cubby>();
                for (int x = 0; x < 10; x++)
                {
                    hotbar_items.Add(new Cubby());
                    hotbar_items[x].draw_loc.X = 300 + (x * 50);
                    if (x > 4)
                        hotbar_items[x].draw_loc.X += 102;
                    hotbar_items[x].draw_loc.Y = 10;
                    hotbar_items[x].info_text = ((x + 1) % 10).ToString();
                    hotbar_items[x].color_code = 1;
                    if (x == 3)
                        hotbar_items[x].is_empty = true;
                }
                cur_left_hand = -1;
                cur_right_hand = -1;
                backpack_items = new List<Cubby>();
                for (int x = 0; x < 50; x++)
                {
                    backpack_items.Add(new Cubby());
                    backpack_items[x].draw_loc = new Vector2(351 + ((x % 10) * 50), 75 + (x / 10 * 50));
                }
                temporary = new Cubby();
            }
            else
            {

            }
        }

        public Inventory(System.IO.BinaryReader r, Actor a, Display d)
        {
            is_player = r.ReadBoolean();
            if (is_player)
            {
                trash = new Cubby(r);
                hotbar_items = new List<Cubby>();
                int num_items = r.ReadUInt16();
                for (int x = 0; x < num_items; x++)
                {
                    hotbar_items.Add(new Cubby(r));
                }
                backpack_items = new List<Cubby>();
                num_items = r.ReadUInt16();
                for (int x = 0; x < num_items; x++)
                {
                    backpack_items.Add(new Cubby(r));
                }
                temporary = new Cubby(r);
                cur_left_hand = r.ReadSByte();
                cur_right_hand = r.ReadSByte();
                equip_item(a, d, true, true);
            }
        }

        public Inventory(NetIncomingMessage r, Actor a, Display d)
        {
            is_player = r.ReadBoolean();
            if (is_player)
            {
                trash = new Cubby(r);
                hotbar_items = new List<Cubby>();
                int num_items = r.ReadUInt16();
                for (int x = 0; x < num_items; x++)
                {
                    hotbar_items.Add(new Cubby(r));
                }
                backpack_items = new List<Cubby>();
                num_items = r.ReadUInt16();
                for (int x = 0; x < num_items; x++)
                {
                    backpack_items.Add(new Cubby(r));
                }
                temporary = new Cubby(r);
                cur_left_hand = r.ReadSByte();
                cur_right_hand = r.ReadSByte();
                equip_item(a, d, true, true);
            }
        }

        public void write_inventory(System.IO.BinaryWriter w)
        {
            w.Write(is_player);
            if (is_player)
            {
                trash.write_cubby(w);
                w.Write((ushort)hotbar_items.Count);
                for (int x = 0; x < hotbar_items.Count; x++)
                {
                    hotbar_items[x].write_cubby(w);
                }
                w.Write((ushort)backpack_items.Count);
                for (int x = 0; x < backpack_items.Count; x++)
                {
                    backpack_items[x].write_cubby(w);
                }
                temporary.write_cubby(w);
                w.Write((sbyte)cur_left_hand);
                w.Write((sbyte)cur_right_hand);
            }
        }

        public void send_inventory(NetOutgoingMessage w)
        {
            w.Write(is_player);
            if (is_player)
            {
                trash.send_cubby(w);
                w.Write((ushort)hotbar_items.Count);
                for (int x = 0; x < hotbar_items.Count; x++)
                {
                    hotbar_items[x].send_cubby(w);
                }
                w.Write((ushort)backpack_items.Count);
                for (int x = 0; x < backpack_items.Count; x++)
                {
                    backpack_items[x].send_cubby(w);
                }
                temporary.send_cubby(w);
                w.Write((sbyte)cur_left_hand);
                w.Write((sbyte)cur_right_hand);
            }
        }

        public bool is_block_breaker(bool check_left, sbyte block_type, bool furniture)
        {
            if (check_left)
            {
                if (!temporary.is_empty)
                {
                    if (temporary.is_block_breaker(block_type, furniture))
                        return true;
                }
                if (cur_left_hand == -1)
                    return false;
                return hotbar_items[cur_left_hand].is_block_breaker(block_type, furniture);
            }
            else
            {
                if (cur_right_hand == -1)
                    return false;
                return hotbar_items[cur_right_hand].is_block_breaker(block_type, furniture);
            }
        }

        public bool can_swing_item(bool check_left)
        {
            if (check_left)
            {
                if (!temporary.is_empty)
                {
                    if (temporary.phys_item != null && temporary.phys_item.can_swing)
                        return true;
                }
                if (cur_left_hand == -1)
                    return false;
                if (hotbar_items[cur_left_hand].phys_item != null && hotbar_items[cur_left_hand].phys_item.can_swing)
                    return true;
                else
                    return false;
            }
            else
            {
                if (cur_right_hand == -1)
                    return false;
                if (hotbar_items[cur_right_hand].phys_item != null && hotbar_items[cur_right_hand].phys_item.can_swing)
                    return true;
                else
                    return false;
            }
        }

        public int cubby_type(bool check_left,bool empty_block)
        {
            if (check_left)
            {
                if (!temporary.is_empty)
                {
                    if (temporary.is_empty) 
                        return -1;
                    if (temporary.is_block)
                        return 0;
                    if (temporary.phys_item != null && empty_block)
                        return 1;
                    if (temporary.is_furniture)
                        return 2;
                    if (temporary.is_material)
                        return 3;
                }
                if (cur_left_hand == -1 || hotbar_items[cur_left_hand].is_empty)
                    return -1;
                else if (hotbar_items[cur_left_hand].is_block)
                    return 0;
                else if (hotbar_items[cur_left_hand].phys_item != null)
                    return 1;
                else if (hotbar_items[cur_left_hand].is_furniture)
                    return 2;
                else if (hotbar_items[cur_left_hand].is_material)
                    return 3;

            }
            else
            {
                if (cur_right_hand == -1 || hotbar_items[cur_right_hand].is_empty)
                    return -1;
                else if (hotbar_items[cur_right_hand].is_block)
                    return 0;
                else if (hotbar_items[cur_right_hand].phys_item != null)
                    return 1;
                else if (hotbar_items[cur_right_hand].is_furniture)
                    return 2;
                else if (hotbar_items[cur_right_hand].is_material)
                    return 3;
            }
            return -1;
        }

        public int strikes_to_break(bool check_left, sbyte block_type, bool furniture, bool plant)
        {
            if (check_left)
            {
                if (temporary.phys_item != null)
                {
                    if (furniture && temporary.get_swings_needed_to_break(4,plant) > -1)
                        return temporary.get_swings_needed_to_break(4, plant);
                    if (temporary.get_swings_needed_to_break(block_type, plant) > -1)
                        return temporary.get_swings_needed_to_break(block_type, plant);
                }
                if (cur_left_hand == -1)
                    return -1;
                if (furniture && hotbar_items[cur_left_hand].get_swings_needed_to_break(4, plant) > -1)
                    return hotbar_items[cur_left_hand].get_swings_needed_to_break(4, plant);
                return hotbar_items[cur_left_hand].get_swings_needed_to_break(block_type, plant);
            }
            else
            {
                if (cur_right_hand == -1)
                    return -1;
                if (furniture && hotbar_items[cur_right_hand].get_swings_needed_to_break(4, plant) > -1)
                    return hotbar_items[cur_right_hand].get_swings_needed_to_break(4, plant);
                return hotbar_items[cur_right_hand].get_swings_needed_to_break(block_type, plant);
            }
        }

        /// <summary>
        /// this covers picking up an actual item.
        /// </summary>
        /// <param name="item"></param>
        public void pickup_item(Item item)
        {
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (hotbar_items[x].is_empty)
                {
                    hotbar_items[x].set_cubby_item(item);
                    return;
                }
            }

            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (backpack_items[x].is_empty)
                {
                    backpack_items[x].set_cubby_item(item);
                    return;
                }

            }
        }

        public sbyte pickup_block(sbyte block_id)
        {
            if (block_id < 0)
                return -1;

            if (!temporary.is_empty && temporary.is_block && temporary.block_id == block_id && temporary.quantity < ushort.MaxValue)
            {
                temporary.quantity++;
                return -1;
            }
            int empty_hotbar = -1;
            int empty_backpack = -1;
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (!hotbar_items[x].is_empty && hotbar_items[x].is_block && hotbar_items[x].block_id == block_id)
                {
                    hotbar_items[x].add_block();
                    return -1;
                }
                else if (hotbar_items[x].is_empty && empty_hotbar == -1)
                {
                    empty_hotbar = x;
                }

            }
            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (!backpack_items[x].is_empty && backpack_items[x].is_block && backpack_items[x].block_id == block_id)
                {
                    backpack_items[x].add_block();
                    return -1;
                }
                else if (backpack_items[x].is_empty && empty_backpack == -1)
                {
                    empty_backpack = x;
                }
            }
            if (empty_hotbar > -1)
            {
                hotbar_items[empty_hotbar].reset_cubby(block_id, 1);
                return -1;
            }
            else if (empty_backpack > -1)
            {
                backpack_items[empty_backpack].reset_cubby(block_id, 1);
                return -1;
            }
            return block_id;
        }

        public void pickup_furniture(Point loc, World w, int player_id)
        {
            int id_pickup = w.furniture[w.map[loc.X, loc.Y].furniture_index].furniture_id;
            int cur_power = w.furniture[w.map[loc.X, loc.Y].furniture_index].power_storage;
            Exilania.furniture_manager.furniture[id_pickup].remove_furniture(w, loc);
            if (cur_power >= Exilania.furniture_manager.furniture[id_pickup].power_storage)
            {
               if (temporary.is_furniture && temporary.furniture_id == id_pickup)
                {
                    if (temporary.quantity < ushort.MaxValue)
                    {
                        temporary.quantity++;
                        return;
                    }
                }
                int possible_hotbar = -1;
                for (int x = 0; x < hotbar_items.Count; x++)
                {
                    if (hotbar_items[x].is_furniture && hotbar_items[x].furniture_id == id_pickup)
                    {
                        if (hotbar_items[x].quantity < ushort.MaxValue)
                        {
                            hotbar_items[x].quantity++;
                            return;
                        }
                    }
                    else if (hotbar_items[x].is_empty && possible_hotbar == -1)
                        possible_hotbar = x;
                }
                int possible_backpack = -1;
                for (int x = 0; x < backpack_items.Count; x++)
                {
                    if (backpack_items[x].is_furniture && backpack_items[x].furniture_id == id_pickup)
                    {
                        if (backpack_items[x].quantity < ushort.MaxValue)
                        {
                            backpack_items[x].quantity++;
                            return;
                        }
                    }
                    else if (backpack_items[x].is_empty && possible_backpack == -1)
                        possible_backpack = x;
                }
                if (possible_hotbar > -1)
                {
                    hotbar_items[possible_hotbar].set_cubby_furniture(Exilania.furniture_manager.furniture[id_pickup], 1);
                    return;
                }
                else if (possible_backpack > -1)
                {
                    backpack_items[possible_backpack].set_cubby_furniture(Exilania.furniture_manager.furniture[id_pickup], 1);
                    return;
                }
            }
            else
            {
                float num = (float)Exilania.furniture_manager.furniture[id_pickup].worth * .7f + (.3f * ((float)cur_power / (float)Exilania.furniture_manager.furniture[id_pickup].power_storage));
                Exilania.display.add_message("@08There was not enough power left in the device to salvage it. Scrapped for @00 " + Math.Round(num, 0) + " @08Credits.");
                w.players[player_id].credits += (int)Math.Round(num, 0);
            }
        }

        public void pickup_furniture(int id_pickup, ushort num)
        {
            if (temporary.is_furniture && temporary.furniture_id == id_pickup)
            {
                if (temporary.quantity + num <= ushort.MaxValue)
                {
                    temporary.quantity += num;
                    return;
                }
                else
                {
                    num -= (ushort)(ushort.MaxValue - temporary.quantity);
                    temporary.quantity = ushort.MaxValue;
                }
            }
            int possible_hotbar = -1;
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (hotbar_items[x].is_furniture && hotbar_items[x].furniture_id == id_pickup)
                {
                    if (hotbar_items[x].quantity + num <= ushort.MaxValue)
                    {
                        hotbar_items[x].quantity+= num;
                        return;
                    }
                    else
                    {
                        num -= (ushort)(ushort.MaxValue - hotbar_items[x].quantity);
                        hotbar_items[x].quantity = ushort.MaxValue;
                    }
                }
                else if (hotbar_items[x].is_empty && possible_hotbar == -1)
                    possible_hotbar = x;
            }
            int possible_backpack = -1;
            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (backpack_items[x].is_furniture && backpack_items[x].furniture_id == id_pickup)
                {
                    if (backpack_items[x].quantity + num <= ushort.MaxValue)
                    {
                        backpack_items[x].quantity+=num;
                        return;
                    }
                    else
                    {
                        num -= (ushort)(ushort.MaxValue - backpack_items[x].quantity);
                        backpack_items[x].quantity = ushort.MaxValue;
                    }
                }
                else if (backpack_items[x].is_empty && possible_backpack == -1)
                    possible_backpack = x;
            }
            if (possible_hotbar > -1)
            {
                hotbar_items[possible_hotbar].set_cubby_furniture(Exilania.furniture_manager.furniture[id_pickup], num);
                return;
            }
            else if (possible_backpack > -1)
            {
                backpack_items[possible_backpack].set_cubby_furniture(Exilania.furniture_manager.furniture[id_pickup], num);
                return;
            }

        }

        public int pickup_material(int material_id, ushort amt)
        {
            int open_hotbar_spot = -1;
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (hotbar_items[x].is_material && hotbar_items[x].material_id == material_id && hotbar_items[x].quantity + amt < ushort.MaxValue)
                {
                    hotbar_items[x].quantity += amt;
                    return -1;
                }
                else if (hotbar_items[x].is_empty && open_hotbar_spot == -1)
                {
                    open_hotbar_spot = x;
                }
            }
            int open_backpack_spot = -1;
            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (backpack_items[x].is_material && backpack_items[x].material_id == material_id && backpack_items[x].quantity + amt < ushort.MaxValue)
                {
                    backpack_items[x].quantity += amt;
                    return -1;
                }
                else if (backpack_items[x].is_empty && open_backpack_spot == -1)
                {
                    open_backpack_spot = x;
                }
            }
            if (open_hotbar_spot != -1)
            {
                hotbar_items[open_hotbar_spot].set_cubby_material(Exilania.material_manager.materials[material_id], amt);
                return -1;
            }
            if (open_backpack_spot != -1)
            {
                backpack_items[open_backpack_spot].set_cubby_material(Exilania.material_manager.materials[material_id], amt);
                return -1;
            }
            return material_id;
        }

        public sbyte pickup_block(sbyte block_id, ushort amt)
        {
            if (!temporary.is_empty && temporary.is_block && temporary.block_id == block_id && temporary.quantity < ushort.MaxValue)
            {
                if (temporary.quantity + amt > ushort.MaxValue)
                    temporary.quantity = ushort.MaxValue;
                else
                    temporary.quantity += amt;
                return -1;
            }
            int empty_hotbar = -1;
            int empty_backpack = -1;
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (!hotbar_items[x].is_empty && hotbar_items[x].is_block && hotbar_items[x].block_id == block_id)
                {
                    if (hotbar_items[x].quantity + amt <= ushort.MaxValue)
                    {
                        hotbar_items[x].quantity += amt;
                    }
                    else
                    {
                        amt -= (ushort)(ushort.MaxValue - hotbar_items[x].quantity);
                        hotbar_items[x].quantity = ushort.MaxValue;
                    }
                    return -1;
                }
                else if (hotbar_items[x].is_empty && empty_hotbar == -1)
                {
                    empty_hotbar = x;
                }

            }

            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (!backpack_items[x].is_empty && backpack_items[x].is_block && backpack_items[x].block_id == block_id)
                {
                    if (backpack_items[x].quantity + amt <= ushort.MaxValue)
                    {
                        backpack_items[x].quantity += amt;
                    }
                    else
                    {
                        amt -= (ushort)(ushort.MaxValue - backpack_items[x].quantity);
                        backpack_items[x].quantity = ushort.MaxValue;
                    }
                    return -1;
                }
                else if (backpack_items[x].is_empty && empty_backpack == -1)
                {
                    empty_backpack = x;
                }
            }
            if (empty_hotbar != -1)
            {
                hotbar_items[empty_hotbar].reset_cubby(block_id, amt);
                return -1;
            }
            if (empty_backpack != -1)
            {
                backpack_items[empty_backpack].reset_cubby(block_id, amt);
                return -1;
            }
            return block_id;
        }

        public void switch_items(bool is_on_hotbar, int slot_id, Actor a, Display d)
        {
            Cubby holder = new Cubby(temporary);
            if (is_on_hotbar)
            {
               
                if (hotbar_items[slot_id].is_block == temporary.is_block && (hotbar_items[slot_id].is_block && hotbar_items[slot_id].block_id == temporary.block_id))
                {
                    if (hotbar_items[slot_id].quantity + temporary.quantity <= ushort.MaxValue)
                    {
                        hotbar_items[slot_id].quantity += temporary.quantity;
                        temporary.is_empty = true;
                        temporary.is_block = false;
                    }
                    else
                    {
                        temporary.quantity = (ushort)(ushort.MaxValue - hotbar_items[slot_id].quantity);
                        hotbar_items[slot_id].quantity = ushort.MaxValue;
                    }
                }
                else if (hotbar_items[slot_id].is_furniture == temporary.is_furniture && (hotbar_items[slot_id].is_furniture && hotbar_items[slot_id].furniture_id == temporary.furniture_id))
                {
                    if (hotbar_items[slot_id].quantity + temporary.quantity <= ushort.MaxValue)
                    {
                        hotbar_items[slot_id].quantity += temporary.quantity;
                        temporary.is_empty = true;
                        temporary.is_furniture = false;
                    }
                    else
                    {
                        temporary.quantity = (ushort)(ushort.MaxValue - hotbar_items[slot_id].quantity);
                        hotbar_items[slot_id].quantity = ushort.MaxValue;
                    }
                }
                else if (hotbar_items[slot_id].is_material == temporary.is_material && (hotbar_items[slot_id].is_material && hotbar_items[slot_id].material_id == temporary.material_id))
                {
                    if (hotbar_items[slot_id].quantity + temporary.quantity <= ushort.MaxValue)
                    {
                        hotbar_items[slot_id].quantity += temporary.quantity;
                        temporary.is_empty = true;
                        temporary.is_furniture = false;
                    }
                    else
                    {
                        temporary.quantity = (ushort)(ushort.MaxValue - hotbar_items[slot_id].quantity);
                        hotbar_items[slot_id].quantity = ushort.MaxValue;
                    }
                }
                else
                {
                    temporary = new Cubby(hotbar_items[slot_id], temporary.draw_loc, false, temporary.info_text);
                    hotbar_items[slot_id] = new Cubby(holder, hotbar_items[slot_id].draw_loc, true, hotbar_items[slot_id].info_text);
                }
                if (cur_left_hand == slot_id)
                { //update the left hand...
                    Exilania.network_client.send_changed_item_in_hands(hotbar_items[slot_id], true);
                    equip_item(a, d, true, false);
                }
                else if (cur_right_hand == slot_id)
                { //do the same for the right hand...
                    Exilania.network_client.send_changed_item_in_hands(hotbar_items[slot_id], false);
                    equip_item(a, d, false, true);
                }
            }
            else
            { //it is in the backpack
                if (slot_id == -1)
                { //trash location
                    if (temporary.is_empty)
                    {
                        if (!trash.is_empty)
                        {
                            temporary = new Cubby(trash, temporary.draw_loc, false, temporary.info_text);
                            trash.is_empty = true;
                            trash.is_block = false;
                            trash.is_furniture = false;
                            trash.is_material = false;
                            trash.phys_item = null;
                        }
                    }
                    else
                    {
                        trash = new Cubby(temporary, trash.draw_loc, true, trash.info_text);
                        temporary.is_empty = true;
                        temporary.is_block = false;
                        temporary.is_furniture = false;
                        temporary.is_material = false;
                        temporary.phys_item = null;
                    }
                }
                else if (backpack_items[slot_id].is_block == temporary.is_block && (backpack_items[slot_id].is_block && backpack_items[slot_id].block_id == temporary.block_id))
                {
                    if (backpack_items[slot_id].quantity + temporary.quantity <= ushort.MaxValue)
                    {
                        backpack_items[slot_id].quantity += temporary.quantity;
                        temporary.is_empty = true;
                        temporary.is_block = false;
                    }
                    else
                    {
                        temporary.quantity = (ushort)(ushort.MaxValue - backpack_items[slot_id].quantity);
                        backpack_items[slot_id].quantity = ushort.MaxValue;
                    }
                }
                else if (backpack_items[slot_id].is_furniture == temporary.is_furniture && (backpack_items[slot_id].is_furniture && backpack_items[slot_id].furniture_id == temporary.furniture_id))
                {
                    if (backpack_items[slot_id].quantity + temporary.quantity <= ushort.MaxValue)
                    {
                        backpack_items[slot_id].quantity += temporary.quantity;
                        temporary.is_empty = true;
                        temporary.is_furniture = false;
                    }
                    else
                    {
                        temporary.quantity = (ushort)(ushort.MaxValue - backpack_items[slot_id].quantity);
                        backpack_items[slot_id].quantity = ushort.MaxValue;
                    }
                }
                else if (backpack_items[slot_id].is_material == temporary.is_material && (backpack_items[slot_id].is_material && backpack_items[slot_id].material_id == temporary.material_id))
                {
                    if (backpack_items[slot_id].quantity + temporary.quantity <= ushort.MaxValue)
                    {
                        backpack_items[slot_id].quantity += temporary.quantity;
                        temporary.is_empty = true;
                        temporary.is_material = false;
                    }
                    else
                    {
                        temporary.quantity = (ushort)(ushort.MaxValue - backpack_items[slot_id].quantity);
                        backpack_items[slot_id].quantity = ushort.MaxValue;
                    }
                }
                else
                {
                    temporary = new Cubby(backpack_items[slot_id], temporary.draw_loc, false, temporary.info_text);
                    backpack_items[slot_id] = new Cubby(holder, backpack_items[slot_id].draw_loc, true, backpack_items[slot_id].info_text);
                }
            }
        }

        public void check_mouse(Actor a, Display d, Vector2 top_left_world)
        {
            Vector2 mouse_screen_loc = new Vector2(a.input.mouse_loc.X - top_left_world.X,a.input.mouse_loc.Y - top_left_world.Y);
            temporary.draw_loc.X = mouse_screen_loc.X + 33f;
            temporary.draw_loc.Y = mouse_screen_loc.Y - 10f;
            if (a.input.hold_left_mouse_click && !last_left_state)
            {
                if ((show_backpack && Inventory.opened_area.Contains((int)mouse_screen_loc.X, (int)mouse_screen_loc.Y)) 
                    || (show_backpack && Inventory.trash_area.Contains((int)mouse_screen_loc.X, (int)mouse_screen_loc.Y)) 
                    || (!show_backpack && Inventory.closed_area.Contains((int)mouse_screen_loc.X, (int)mouse_screen_loc.Y)))
                {
                    last_left_state = true;
                    int hotbar_clicked = -1;
                    int backpack_clicked = -1;
                    for (int x = 0; x < hotbar_items.Count; x++)
                    {
                        if (mouse_screen_loc.X >= hotbar_items[x].draw_loc.X && mouse_screen_loc.X <= hotbar_items[x].draw_loc.X + Cubby.size &&
                                mouse_screen_loc.Y >= hotbar_items[x].draw_loc.Y && mouse_screen_loc.Y <= hotbar_items[x].draw_loc.Y + Cubby.size)
                        {
                            hotbar_clicked = x;
                            x = hotbar_items.Count;
                        }
                    }
                    if (hotbar_clicked != -1)
                    { //you clicked the hotbar
                        switch_items(true, hotbar_clicked, a, d);
                        click_consumed = true;
                        return;
                    }
                    else if (show_backpack)
                    {
                        for (int x = 0; x < backpack_items.Count; x++)
                        {
                            if (mouse_screen_loc.X >= backpack_items[x].draw_loc.X && mouse_screen_loc.X <= backpack_items[x].draw_loc.X + Cubby.size &&
                                mouse_screen_loc.Y >= backpack_items[x].draw_loc.Y && mouse_screen_loc.Y <= backpack_items[x].draw_loc.Y + Cubby.size)
                            {
                                backpack_clicked = x;
                                x = backpack_items.Count;

                            }
                        }
                    }
                    if (backpack_clicked > -1)
                    {
                        switch_items(false, backpack_clicked, a, d);
                        click_consumed = true;
                        return;
                    }
                    else if (mouse_screen_loc.X >= trash.draw_loc.X && mouse_screen_loc.X <= trash.draw_loc.X + Cubby.size &&
                                mouse_screen_loc.Y >= trash.draw_loc.Y && mouse_screen_loc.Y <= trash.draw_loc.Y + Cubby.size)
                    {
                        switch_items(false, -1, a, d);
                        click_consumed = true;
                        return;
                    }
                    click_consumed = false;
                }
                else
                {
                    click_consumed = false;
                    last_left_state = a.input.hold_left_mouse_click;
                }
            }
            else
            {
                last_left_state = a.input.hold_left_mouse_click;
            }
        }

        public void hover_inventory(World w, Actor a, SpriteBatch s, Display d)
        {

            Point on_screen_loc = new Point(Microsoft.Xna.Framework.Input.Mouse.GetState().X, Microsoft.Xna.Framework.Input.Mouse.GetState().Y);
            if ((show_backpack && Inventory.opened_area.Contains(on_screen_loc.X, on_screen_loc.Y))
                || (show_backpack && Inventory.trash_area.Contains(on_screen_loc.X, on_screen_loc.Y)) 
                || (!show_backpack && Inventory.closed_area.Contains(on_screen_loc.X, on_screen_loc.Y)))
            {
                //hovering over inventory somewhere... write out text if that location is not blank.
                int hotbar_hover = -1;
                int backpack_hover = -1;
                for (int x = 0; x < hotbar_items.Count; x++)
                {
                    if (on_screen_loc.X >= hotbar_items[x].draw_loc.X && on_screen_loc.X <= hotbar_items[x].draw_loc.X + Cubby.size &&
                            on_screen_loc.Y >= hotbar_items[x].draw_loc.Y && on_screen_loc.Y <= hotbar_items[x].draw_loc.Y + Cubby.size)
                    {
                        hotbar_hover = x;
                        x = hotbar_items.Count;

                    }
                }
                if (hotbar_hover != -1)
                { //you hovered over the hotbar
                    if (!hotbar_items[hotbar_hover].is_empty)
                    {
                        hotbar_items[hotbar_hover].draw_info(on_screen_loc, s,d,a);
                    }
                    return;
                }
                else if (show_backpack)
                {
                    for (int x = 0; x < backpack_items.Count; x++)
                    {
                        if (on_screen_loc.X >= backpack_items[x].draw_loc.X && on_screen_loc.X <= backpack_items[x].draw_loc.X + Cubby.size &&
                            on_screen_loc.Y >= backpack_items[x].draw_loc.Y && on_screen_loc.Y <= backpack_items[x].draw_loc.Y + Cubby.size)
                        {
                            backpack_hover = x;
                            x = backpack_items.Count;

                        }
                    }
                }
                if (backpack_hover > -1)
                {
                    if (!backpack_items[backpack_hover].is_empty)
                    {
                        backpack_items[backpack_hover].draw_info(on_screen_loc, s, d, a);
                    }
                    return;
                }
                else
                {
                    if (!trash.is_empty && on_screen_loc.X >= trash.draw_loc.X && on_screen_loc.X <= trash.draw_loc.X + Cubby.size &&
                            on_screen_loc.Y >= trash.draw_loc.Y && on_screen_loc.Y <= trash.draw_loc.Y + Cubby.size)
                    {
                        trash.draw_info(on_screen_loc, s, d, a);
                    }
                    return;
                }
            }
        }

        public void equip_item(Actor a, Display d, bool left_hand, bool right_hand)
        {
            if (left_hand && cur_left_hand > -1)
            {
                for (int x = 0; x < a.body.Count; x++)
                {
                    if (a.body[x].name == "Item Left")
                    {
                        if (hotbar_items[cur_left_hand].is_block)
                        {
                            hotbar_items[cur_left_hand].color_code = 6;
                            a.body[x].picture = d.frames[Exilania.block_types.blocks[hotbar_items[cur_left_hand].block_id].image_pointers[Exilania.block_types.blocks[hotbar_items[cur_left_hand].block_id].block_image_use]];
                            a.body[x].my_attachment_point = new Vector2(0, 6);
                            a.body[x].center = new Vector2(11, 11);
                            a.body[x].magnification = 1f;
                        }
                        else if(hotbar_items[cur_left_hand].phys_item != null)
                        {
                            if (hotbar_items[cur_left_hand].phys_item.image_hash == 0)
                            {
                                hotbar_items[cur_left_hand].color_code = 6;
                                a.body[x].picture = Exilania.item_manager.item_pieces[hotbar_items[cur_left_hand].phys_item.pieces[0].itempiece_id].image;
                                a.body[x].my_attachment_point = new Vector2(hotbar_items[cur_left_hand].phys_item.attachment_point.X - 3, hotbar_items[cur_left_hand].phys_item.attachment_point.Y);
                                a.body[x].center = new Vector2((a.body[x].picture.Width / 2) - (a.body[x].picture.Width % 2 == 0 ? 1 : 0), (a.body[x].picture.Height / 2) - (a.body[x].picture.Height % 2 == 0 ? 1 : 0));
                            }
                        }
                        else if (hotbar_items[cur_left_hand].is_furniture)
                        {
                            hotbar_items[cur_left_hand].color_code = 6;
                            a.body[x].picture = Exilania.furniture_manager.furniture[hotbar_items[cur_left_hand].furniture_id].get_source_rect(d);
                            a.body[x].my_attachment_point = new Vector2(0, a.body[x].picture.Height / 2);
                            a.body[x].center = new Vector2(a.body[x].picture.Center.X - a.body[x].picture.Left, a.body[x].picture.Center.Y - a.body[x].picture.Top);
                        }
                        else if (hotbar_items[cur_left_hand].is_material)
                        {
                            hotbar_items[cur_left_hand].color_code = 6;
                            a.body[x].picture = d.frames[Exilania.material_manager.materials[hotbar_items[cur_left_hand].material_id].image];
                            a.body[x].my_attachment_point = new Vector2(0, a.body[x].picture.Height / 2);
                            a.body[x].center = new Vector2(a.body[x].picture.Center.X - a.body[x].picture.Left, a.body[x].picture.Center.Y - a.body[x].picture.Top);
                        }
                        else if (hotbar_items[cur_left_hand].is_empty)
                        {
                            hotbar_items[cur_left_hand].color_code = 0;
                            cur_left_hand = -1;
                            a.body[x].magnification = 1f;
                            a.body[x].picture = new Rectangle(0, 0, 1, 1);
                            a.body[x].my_attachment_point = new Vector2(0, 0);
                            a.body[x].center = new Vector2(0, 0);
                        }
                        x = a.body.Count;
                    }
                }
            }
            if (right_hand && cur_right_hand > -1)
            {
                for (int x = 0; x < a.body.Count; x++)
                {
                    if (a.body[x].name == "Item Right")
                    {
                        if (hotbar_items[cur_right_hand].is_block)
                        {
                            hotbar_items[cur_right_hand].color_code = 7;
                            a.body[x].picture = d.frames[Exilania.block_types.blocks[hotbar_items[cur_right_hand].block_id].image_pointers[Exilania.block_types.blocks[hotbar_items[cur_right_hand].block_id].block_image_use]];
                            a.body[x].my_attachment_point = new Vector2(0, 6);
                            a.body[x].center = new Vector2(11, 11);
                        }
                        else if (hotbar_items[cur_right_hand].phys_item != null)
                        {
                            if (hotbar_items[cur_right_hand].phys_item.image_hash == 0)
                            {
                                hotbar_items[cur_right_hand].color_code = 7;
                                a.body[x].picture = Exilania.item_manager.item_pieces[hotbar_items[cur_right_hand].phys_item.pieces[0].itempiece_id].image;
                                a.body[x].my_attachment_point = new Vector2(hotbar_items[cur_right_hand].phys_item.attachment_point.X - 3, hotbar_items[cur_right_hand].phys_item.attachment_point.Y);
                                a.body[x].center = new Vector2((a.body[x].picture.Width / 2) - (a.body[x].picture.Width % 2 == 0 ? 1 : 0), (a.body[x].picture.Height / 2) - (a.body[x].picture.Height % 2 == 0 ? 1 : 0));
                            }
                        }
                        else if (hotbar_items[cur_right_hand].is_furniture)
                        {
                            hotbar_items[cur_right_hand].color_code = 7;
                            a.body[x].picture = Exilania.furniture_manager.furniture[hotbar_items[cur_right_hand].furniture_id].get_source_rect(d);
                            a.body[x].my_attachment_point = new Vector2(0, a.body[x].picture.Height / 2);
                            a.body[x].center = new Vector2(a.body[x].picture.Center.X - a.body[x].picture.Left, a.body[x].picture.Center.Y - a.body[x].picture.Top);
                        }
                        else if (hotbar_items[cur_right_hand].is_material)
                        {
                            hotbar_items[cur_right_hand].color_code = 6;
                            a.body[x].picture = d.frames[Exilania.material_manager.materials[hotbar_items[cur_right_hand].material_id].image];
                            a.body[x].my_attachment_point = new Vector2(0, a.body[x].picture.Height / 2);
                            a.body[x].center = new Vector2(a.body[x].picture.Center.X - a.body[x].picture.Left, a.body[x].picture.Center.Y - a.body[x].picture.Top);
                        }
                        else if (hotbar_items[cur_right_hand].is_empty)
                        {
                            hotbar_items[cur_right_hand].color_code = 0;
                            cur_right_hand = -1;
                            a.body[x].picture = new Rectangle(0, 0, 1, 1);
                            a.body[x].my_attachment_point = new Vector2(0, 0);
                            a.body[x].center = new Vector2(0, 0);
                        }
                        x = a.body.Count;
                    }
                }
            }
            set_actor_self_light(a);
        }

        public void set_actor_self_light(Actor a)
        {
            a.light_source = new byte[] { 0, 0, 0 };
            if (cur_left_hand != -1)
            {
                if (hotbar_items[cur_left_hand].is_block && Exilania.block_types.blocks[hotbar_items[cur_left_hand].block_id].light_source != null)
                {
                    for (int i = 0; i < 3; i++)
                        a.light_source[i] = Math.Max(Exilania.block_types.blocks[hotbar_items[cur_left_hand].block_id].light_source[i], a.light_source[i]);
                }
                else if (hotbar_items[cur_left_hand].phys_item != null && hotbar_items[cur_left_hand].phys_item.light != null)
                {
                    for (int i = 0; i < 3; i++)
                        a.light_source[i] = Math.Max(hotbar_items[cur_left_hand].phys_item.light[i], a.light_source[i]);
                }
            }
            if (cur_right_hand != -1)
            {
                if (hotbar_items[cur_right_hand].is_block && Exilania.block_types.blocks[hotbar_items[cur_right_hand].block_id].light_source != null)
                {
                    for (int i = 0; i < 3; i++)
                        a.light_source[i] = Math.Max(Exilania.block_types.blocks[hotbar_items[cur_right_hand].block_id].light_source[i], a.light_source[i]);
                }
                else if (hotbar_items[cur_right_hand].phys_item != null && hotbar_items[cur_right_hand].phys_item.light != null)
                {
                    for (int i = 0; i < 3; i++)
                        a.light_source[i] = Math.Max(hotbar_items[cur_right_hand].phys_item.light[i], a.light_source[i]);
                }
            }
        }

        public void try_place_furniture(bool use_left, Point world_loc, World w, Actor a, Display d)
        {
            if (use_left && !click_consumed)
            {
                if (temporary.is_furniture)
                {
                    if (Exilania.furniture_manager.furniture[temporary.furniture_id].try_place(a, world_loc, w,
                        new Rectangle((int)a.world_loc.X + a.bounding_box.X, (int)a.world_loc.Y + a.bounding_box.Y, a.bounding_box.Width, a.bounding_box.Height)))
                    {
                        temporary.quantity--;
                        if (temporary.quantity == 0)
                        {
                            temporary.is_furniture = false;
                            temporary.is_empty = true;
                        }
                        a.input.start_left_click = (System.DateTime.Now.Ticks / 10000) - 400;
                        Exilania.network_client.send_place_furniture(world_loc, temporary.furniture_id);
                        return;
                    }
                }
                if (cur_left_hand != -1 && hotbar_items[cur_left_hand].is_furniture)
                {
                    if (Exilania.furniture_manager.furniture[hotbar_items[cur_left_hand].furniture_id].try_place(a, world_loc, w,
                        new Rectangle((int)a.world_loc.X + a.bounding_box.X, (int)a.world_loc.Y + a.bounding_box.Y, a.bounding_box.Width, a.bounding_box.Height)))
                    {
                        hotbar_items[cur_left_hand].quantity--;
                        Exilania.network_client.send_place_furniture(world_loc, hotbar_items[cur_left_hand].furniture_id);
                        if (hotbar_items[cur_left_hand].quantity == 0)
                        {
                            hotbar_items[cur_left_hand].is_furniture = false;
                            hotbar_items[cur_left_hand].is_empty = true;
                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[cur_left_hand], true);
                            equip_item(a, d, true, false);

                        }
                        a.input.start_left_click = (System.DateTime.Now.Ticks / 10000) - 400;
                        return;
                    }
                }
            }
            else if (!use_left)
            {
                if (cur_right_hand != -1 && hotbar_items[cur_right_hand].is_furniture)
                {
                    if (Exilania.furniture_manager.furniture[hotbar_items[cur_right_hand].furniture_id].try_place(a, world_loc, w,
                        new Rectangle((int)a.world_loc.X + a.bounding_box.X, (int)a.world_loc.Y + a.bounding_box.Y, a.bounding_box.Width, a.bounding_box.Height)))
                    {
                        hotbar_items[cur_right_hand].quantity--;
                        Exilania.network_client.send_place_furniture(world_loc, hotbar_items[cur_right_hand].furniture_id);
                        if (hotbar_items[cur_right_hand].quantity == 0)
                        {
                            hotbar_items[cur_right_hand].is_furniture = false;
                            hotbar_items[cur_right_hand].is_empty = true;
                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[cur_right_hand], false);
                            equip_item(a, d, false, true);
                        }
                        a.input.start_right_click = (System.DateTime.Now.Ticks / 10000) - 400;
                        return;
                    }
                }
            }
        }

        public void try_use_material(bool use_left, Point world_loc, World w, Actor a, Display d)
        {
            if (use_left && !click_consumed)
            {
                if (temporary.is_material)
                {
                    if (Exilania.material_manager.materials[temporary.material_id].use_action != "")
                    {
                        if (Exilania.material_manager.materials[temporary.material_id].apply_use_action(w, world_loc))
                        {
                            temporary.quantity--;
                            if (temporary.quantity <= 0)
                            {
                                temporary.is_empty = true;
                                temporary.is_material = false;
                                temporary.material_id = -1;
                            }
                        }
                        return;
                    }
                }
                if (cur_left_hand != -1 && hotbar_items[cur_left_hand].is_material)
                {
                    if (Exilania.material_manager.materials[hotbar_items[cur_left_hand].material_id].use_action != "")
                    {
                        if (Exilania.material_manager.materials[hotbar_items[cur_left_hand].material_id].apply_use_action(w, world_loc))
                        {
                            hotbar_items[cur_left_hand].quantity--;
                            if (hotbar_items[cur_left_hand].quantity <= 0)
                            {
                                hotbar_items[cur_left_hand].is_empty = true;
                                hotbar_items[cur_left_hand].material_id = -1;
                                hotbar_items[cur_left_hand].is_material = false;
                                Exilania.network_client.send_changed_item_in_hands(hotbar_items[cur_left_hand], true);
                                equip_item(a, d, true, false);
                            }
                        }
                    }
                }
                return;
            }
            else if (!use_left)
            {
                if (cur_right_hand != -1 && hotbar_items[cur_right_hand].is_material)
                {
                    if (Exilania.material_manager.materials[hotbar_items[cur_right_hand].material_id].use_action != "")
                    {
                        if (Exilania.material_manager.materials[hotbar_items[cur_right_hand].material_id].apply_use_action(w, world_loc))
                        {
                            hotbar_items[cur_right_hand].quantity--;
                            if (hotbar_items[cur_right_hand].quantity <= 0)
                            {
                                hotbar_items[cur_right_hand].is_empty = true;
                                hotbar_items[cur_right_hand].material_id = -1;
                                hotbar_items[cur_right_hand].is_material = false;
                                Exilania.network_client.send_changed_item_in_hands(hotbar_items[cur_right_hand], false);
                                equip_item(a, d, false, true);
                            }
                        }
                    }
                    return;
                }
            }
        }

        public bool has_item(ItemType t, int item_id, int quantity)
        {
            int tot_found = 0;
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (!hotbar_items[x].is_empty)
                {
                    switch (t)
                    {
                        case ItemType.Block:
                            if (hotbar_items[x].is_block && hotbar_items[x].block_id == item_id)
                                tot_found += hotbar_items[x].quantity;
                            break;
                        case ItemType.Furniture:
                            if (hotbar_items[x].is_furniture && hotbar_items[x].furniture_id == item_id)
                            {
                                tot_found += hotbar_items[x].quantity;
                            }
                            break;
                        case ItemType.ItemPiece:
                            if (hotbar_items[x].phys_item != null)
                            {
                                if (hotbar_items[x].phys_item.pieces.Count == 1 && hotbar_items[x].phys_item.pieces[0].itempiece_id == item_id)
                                    tot_found++;
                            }
                            break;
                        case ItemType.Material:
                            if (hotbar_items[x].is_material && hotbar_items[x].material_id == item_id)
                            {
                                tot_found += hotbar_items[x].quantity;
                            }
                            break;
                    }
                }
            }
            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (!backpack_items[x].is_empty)
                {
                    switch (t)
                    {
                        case ItemType.Block:
                            if (backpack_items[x].is_block && backpack_items[x].block_id == item_id)
                                tot_found += backpack_items[x].quantity;
                            break;
                        case ItemType.Furniture:
                            if (backpack_items[x].is_furniture && backpack_items[x].furniture_id == item_id)
                            {
                                tot_found += backpack_items[x].quantity;
                            }
                            break;
                        case ItemType.ItemPiece:
                            if (backpack_items[x].phys_item != null)
                            {
                                if (backpack_items[x].phys_item.pieces.Count == 1 && backpack_items[x].phys_item.pieces[0].itempiece_id == item_id)
                                    tot_found++;
                            }
                            break;
                        case ItemType.Material:
                            if (backpack_items[x].is_material && backpack_items[x].material_id == item_id)
                            {
                                tot_found += backpack_items[x].quantity;
                            }
                            break;
                    }
                }
            }
            return tot_found >= quantity;
        }

        public bool use_item(Actor a, ItemType t, int item_id, int quantity)
        {
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                if (!hotbar_items[x].is_empty)
                {
                    switch (t)
                    {
                        case ItemType.Block:
                            if (hotbar_items[x].is_block && hotbar_items[x].block_id == item_id)
                                if (hotbar_items[x].quantity >= quantity)
                                {
                                    hotbar_items[x].quantity -= (ushort)quantity;
                                    if (hotbar_items[x].quantity == 0)
                                    {
                                        hotbar_items[x].is_empty = true;
                                        hotbar_items[x].is_block = false;
                                        hotbar_items[x].block_id = -1;
                                        if (cur_left_hand == x)
                                        {
                                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                            equip_item(a, Exilania.display, true, false);
                                        }
                                        else if (cur_right_hand == x)
                                        {
                                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                            equip_item(a, Exilania.display, false, true);
                                        }
                                    }
                                    return true;
                                }
                                else
                                {
                                    quantity -= hotbar_items[x].quantity;
                                    hotbar_items[x].is_empty = true;
                                    hotbar_items[x].is_block = false;
                                    hotbar_items[x].block_id = -1;
                                    if (cur_left_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                        equip_item(a, Exilania.display, true, false);
                                    }
                                    else if (cur_right_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                        equip_item(a, Exilania.display, false, true);
                                    }
                                }
                            break;
                        case ItemType.Furniture:
                            if (hotbar_items[x].is_furniture && hotbar_items[x].furniture_id == item_id)
                            {
                                if (hotbar_items[x].quantity >= quantity)
                                {
                                    hotbar_items[x].quantity -= (ushort)quantity;
                                    if (hotbar_items[x].quantity == 0)
                                    {
                                        hotbar_items[x].is_empty = true;
                                        hotbar_items[x].is_furniture = false;
                                        hotbar_items[x].furniture_id = -1;
                                        if (cur_left_hand == x)
                                        {
                                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                            equip_item(a, Exilania.display, true, false);
                                        }
                                        else if (cur_right_hand == x)
                                        {
                                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                            equip_item(a, Exilania.display, false, true);
                                        }
                                    }
                                    return true;
                                }
                                else
                                {
                                    quantity -= hotbar_items[x].quantity;
                                    hotbar_items[x].is_empty = true;
                                    hotbar_items[x].is_furniture = false;
                                    hotbar_items[x].furniture_id = -1;
                                    if (cur_left_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                        equip_item(a, Exilania.display, true, false);
                                    }
                                    else if (cur_right_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                        equip_item(a, Exilania.display, false, true);
                                    }
                                }
                            }
                            break;
                        case ItemType.ItemPiece:
                            if (hotbar_items[x].phys_item != null)
                            {
                                if (hotbar_items[x].phys_item.pieces.Count == 1 && hotbar_items[x].phys_item.pieces[0].itempiece_id == item_id)
                                {
                                    hotbar_items[x].phys_item = null;
                                    hotbar_items[x].is_empty = true;
                                    if (cur_left_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                        equip_item(a, Exilania.display, true, false);
                                    }
                                    else if (cur_right_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                        equip_item(a, Exilania.display, false, true);
                                    }
                                    return true;
                                }
                            }
                            break;
                        case ItemType.Material:
                            if (hotbar_items[x].is_material && hotbar_items[x].material_id == item_id)
                            {
                                if (hotbar_items[x].quantity >= quantity)
                                {
                                    hotbar_items[x].quantity -= (ushort)quantity;
                                    if (hotbar_items[x].quantity == 0)
                                    {
                                        hotbar_items[x].is_empty = true;
                                        hotbar_items[x].is_material = false;
                                        hotbar_items[x].material_id = -1;
                                        if (cur_left_hand == x)
                                        {
                                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                            equip_item(a, Exilania.display, true, false);
                                        }
                                        else if (cur_right_hand == x)
                                        {
                                            Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                            equip_item(a, Exilania.display, false, true);
                                        }
                                    }
                                    return true;
                                }
                                else
                                {
                                    quantity -= hotbar_items[x].quantity;
                                    hotbar_items[x].is_empty = true;
                                    hotbar_items[x].is_material = false;
                                    hotbar_items[x].material_id = -1;
                                    if (cur_left_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], true);
                                        equip_item(a, Exilania.display, true, false);
                                    }
                                    else if (cur_right_hand == x)
                                    {
                                        Exilania.network_client.send_changed_item_in_hands(hotbar_items[x], false);
                                        equip_item(a, Exilania.display, false, true);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            for (int x = 0; x < backpack_items.Count; x++)
            {
                if (!backpack_items[x].is_empty)
                {
                    switch (t)
                    {
                        case ItemType.Block:
                            if (backpack_items[x].is_block && backpack_items[x].block_id == item_id)
                                if (backpack_items[x].quantity >= quantity)
                                {
                                    backpack_items[x].quantity -= (ushort)quantity;
                                    if (backpack_items[x].quantity == 0)
                                    {
                                        backpack_items[x].is_empty = true;
                                        backpack_items[x].is_block = false;
                                        backpack_items[x].block_id = -1;
                                    }
                                    return true;
                                }
                                else
                                {
                                    quantity -= backpack_items[x].quantity;
                                    backpack_items[x].is_empty = true;
                                    backpack_items[x].is_block = false;
                                    backpack_items[x].block_id = -1;
                                }
                            break;
                        case ItemType.Furniture:
                            if (backpack_items[x].is_furniture && backpack_items[x].furniture_id == item_id)
                            {
                                if (backpack_items[x].quantity >= quantity)
                                {
                                    backpack_items[x].quantity -= (ushort)quantity;
                                    if (backpack_items[x].quantity == 0)
                                    {
                                        backpack_items[x].is_empty = true;
                                        backpack_items[x].is_furniture = false;
                                        backpack_items[x].furniture_id = -1;
                                    }
                                    return true;
                                }
                                else
                                {
                                    quantity -= backpack_items[x].quantity;
                                    backpack_items[x].is_empty = true;
                                    backpack_items[x].is_furniture = false;
                                    backpack_items[x].furniture_id = -1;
                                }
                            }
                            break;
                        case ItemType.ItemPiece:
                            if (backpack_items[x].phys_item != null)
                            {
                                if (backpack_items[x].phys_item.pieces.Count == 1 && backpack_items[x].phys_item.pieces[0].itempiece_id == item_id)
                                {
                                    backpack_items[x].phys_item = null;
                                    backpack_items[x].is_empty = true;
                                    return true;
                                }
                            }
                            break;
                        case ItemType.Material:
                            if (backpack_items[x].is_material && backpack_items[x].material_id == item_id)
                            {
                                if (backpack_items[x].quantity >= quantity)
                                {
                                    backpack_items[x].quantity -= (ushort)quantity;
                                    if (backpack_items[x].quantity == 0)
                                    {
                                        backpack_items[x].is_empty = true;
                                        backpack_items[x].is_material = false;
                                        backpack_items[x].material_id = -1;
                                    }
                                    return true;
                                }
                                else
                                {
                                    quantity -= backpack_items[x].quantity;
                                    backpack_items[x].is_empty = true;
                                    backpack_items[x].is_material = false;
                                    backpack_items[x].material_id = -1;
                                }
                            }
                            break;
                    }
                }
            }
            return false;
        }

        public void input(Actor a, Display d, Vector2 top_left_world)
        {
            check_mouse(a, d, top_left_world);

            if (a.input.pressed_keys.Contains('I') && !a.input.old_pressed_keys.Contains('I'))
                show_backpack = !show_backpack;
            int pre_left = cur_left_hand;
            int pre_right = cur_right_hand;
            for (int x = 0; x < 10; x++)
            {
                if ((a.input.pressed_keys.Contains(x.ToString()[0]) && !a.input.old_pressed_keys.Contains(x.ToString()[0])) || (a.input.pressed_keys.Contains(char_equivs[x]) && !a.input.old_pressed_keys.Contains(char_equivs[x])))
                {
                    if (a.input.pressed_keys.Contains('`'))
                    {
                        if(cur_left_hand != x && !hotbar_items[x].is_empty)
                            cur_right_hand = x;
                    }
                    else
                    {

                        if (x == 0 && cur_right_hand != 9 && !hotbar_items[9].is_empty)
                            cur_left_hand = 9;
                        else if (x!= 0 && cur_right_hand != x - 1 && !hotbar_items[x-1].is_empty)
                            cur_left_hand = x-1;
                       
                    }
                }
            }

            if (pre_left != cur_left_hand)
            {
                if(pre_left!=-1)
                    hotbar_items[pre_left].color_code = 1;
                hotbar_items[cur_left_hand].color_code = 6;
                Exilania.network_client.send_changed_item_in_hands(hotbar_items[cur_left_hand], true);
                equip_item(a, d, true, false);
            }

            if (pre_right != cur_right_hand)
            {
                if(pre_right != -1)
                    hotbar_items[pre_right].color_code = 1;
                hotbar_items[cur_right_hand].color_code = 7;
                Exilania.network_client.send_changed_item_in_hands(hotbar_items[cur_right_hand], false);
                equip_item(a, d, false, true);
            }
        }

        public void draw(SpriteBatch s, Display d)
        {
            for (int x = 0; x < hotbar_items.Count; x++)
            {
                hotbar_items[x].draw_cubby(s, d, false);
            }
            if (cur_left_hand > -1)
            {
                hotbar_items[cur_left_hand].draw_at_left(s, d, true);
            }
            if (cur_right_hand > -1)
            {
                hotbar_items[cur_right_hand].draw_at_right(s, d, true);
            }
            if (show_backpack)
            {
                for (int x = 0; x < backpack_items.Count; x++)
                {
                    backpack_items[x].draw_cubby(s, d, false);
                }
                trash.draw_cubby(s, d, true);
            }
        }

        public short get_swinging_damage(bool use_left, Actor defender, Actor attacker)
        {
            if (use_left)
            {
                return (short)Math.Max(0,Acc.resolve_die_roll(hotbar_items[cur_left_hand].phys_item.melee_damage,0,0) - Acc.resolve_die_roll(defender.stats.armor,0,0));
            }
            else
            {
                return (short)Math.Max(0, Acc.resolve_die_roll(hotbar_items[cur_right_hand].phys_item.melee_damage, 0, 0) - Acc.resolve_die_roll(defender.stats.armor, 0, 0));
            }
        }
       
    }
}
