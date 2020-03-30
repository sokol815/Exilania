using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    /// <summary>
    /// used to craft basic things like blocks, furniture, material, item pieces from other types of such items.... does not have anything to do with ccrafting vehicles or creating items.
    /// </summary>
    public class CraftPieceInterface
    {
        public List<CraftRecipe> cur_available_crafts;
        public int first_show;
        public long last_time_updated;
        public int cur_click = -1;
        public string cur_click_name = "";
        public long update_interval = 1000;
        public long last_time_bought;
        public long buy_delay = 200;
        public long last_scroll_frame;
        public long scroll_frame_delay = 100;
        public static int num_show = 6;
        public int hover_over = -1;
        public int buys_in_a_row = 0;
        public List<int> id_nearby_furniture;
        public Microsoft.Xna.Framework.Input.MouseState last_mouse_state;
        public Microsoft.Xna.Framework.Input.MouseState cur_mouse_state;
        Rectangle crafting_area;
        item_descriptor infos_item_type;
        List<KeyValuePair<string, Vector2>> infos;
        Point draw_size;

        public CraftPieceInterface()
        {
            cur_available_crafts = new List<CraftRecipe>();
            first_show = 0;
            last_time_updated = System.DateTime.Now.Ticks / 10000;
            last_time_bought = last_time_updated = last_scroll_frame;
            id_nearby_furniture = new List<int>();
            infos = new List<KeyValuePair<string, Vector2>>();
            draw_size = new Point();
            infos_item_type = new item_descriptor();
            infos_item_type.item_type = ItemType.Empty;
        }

        public void update(Actor a, World w)
        {
            long ticks_now = System.DateTime.Now.Ticks / 10000;
            if (ticks_now > last_time_updated + update_interval)
            {
                last_time_updated = ticks_now;
                cur_available_crafts = new List<CraftRecipe>();
                id_nearby_furniture = w.collision_table.get_furniture_in_range(new Point((int)a.world_loc.X,(int)a.world_loc.Y), 120, w);
                for (int x = 0; x < Exilania.crafting_manager.recipes.Count; x++)
                {
                    if (check_is_recipe_valid(Exilania.crafting_manager.recipes[x], a, w))
                        cur_available_crafts.Add(Exilania.crafting_manager.recipes[x]);
                }
            }
            if (a.items.show_backpack)
            {
                crafting_area = new Rectangle(Exilania.screen_size.X - 140, 60, 140, 10 + (50 * num_show));
                cur_mouse_state = Microsoft.Xna.Framework.Input.Mouse.GetState();
                Point mouse_loc = new Point(cur_mouse_state.X, cur_mouse_state.Y);

                if (crafting_area.Contains(mouse_loc))
                {
                    if (cur_mouse_state.ScrollWheelValue != last_mouse_state.ScrollWheelValue && ticks_now > last_scroll_frame + scroll_frame_delay)
                    {
                        last_scroll_frame = ticks_now;
                        if (cur_mouse_state.ScrollWheelValue > last_mouse_state.ScrollWheelValue)
                        {
                            first_show--;
                        }
                        else
                        {
                            first_show++;
                        }
                       
                    }
                    if (first_show < 0)
                        first_show = 0;
                    if (first_show + num_show > cur_available_crafts.Count)
                    {
                        if (cur_available_crafts.Count > num_show)
                            first_show = cur_available_crafts.Count - num_show;
                        else
                            first_show = 0;
                    }
                    if (cur_click == -1)
                    {
                        hover_over = -1;
                        //mouse is hovering in the crafting area.
                        for (int x = first_show; x < num_show + first_show; x++)
                        {
                            Rectangle temp = new Rectangle(Exilania.screen_size.X - 60, 70 + (x - first_show) * 50, 46, 46);
                            if (temp.Contains(mouse_loc) && x < cur_available_crafts.Count)
                            {
                                hover_over = x;
                            }
                        }
                    }
                    else
                        hover_over = cur_click;
                    if (cur_mouse_state.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && last_time_bought + buy_delay <= ticks_now
                        && cur_available_crafts.Count > 0 &&
                        ((cur_click > -1 && cur_available_crafts.Count > cur_click && cur_click_name == cur_available_crafts[cur_click].name)
                        || (cur_click_name == "" && hover_over > -1 && cur_available_crafts.Count > hover_over)))
                    {
                        buys_in_a_row++;
                        if (buys_in_a_row == 1)
                        {
                            cur_click = hover_over;
                            cur_click_name = cur_available_crafts[cur_click].name;
                        }
                        buy_delay -= 10;
                        if (buy_delay < 20)
                            buy_delay = 20;
                        if (check_is_recipe_valid(cur_available_crafts[hover_over], a, w))
                        {
                            a.items.click_consumed = true;
                            take_item_requirements(cur_available_crafts[hover_over], a, w);
                            a.stats.get_experience_crafting(cur_available_crafts[hover_over].complexity,a);
                            for (int i = 0; i < cur_available_crafts[hover_over].output.Count;i++ )
                            {
                                switch (cur_available_crafts[hover_over].output[i].type)
                                {
                                    case ItemType.Block:
                                        a.items.pickup_block((sbyte)cur_available_crafts[hover_over].output[i].item_id, (ushort)cur_available_crafts[hover_over].output[i].item_quantity);
                                        Exilania.display.fading_text.Add(new FadeText("@00+" + cur_available_crafts[hover_over].output[i].item_quantity + " " +
                                            Exilania.block_types.blocks[cur_available_crafts[hover_over].output[i].item_id].name,
                                                Display.default_msec_show_fade_text / 2, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40, true, true));
                                        break;
                                    case ItemType.Furniture:
                                        a.items.pickup_furniture(cur_available_crafts[hover_over].output[i].item_id, (ushort)cur_available_crafts[hover_over].output[i].item_quantity);
                                        Exilania.display.fading_text.Add(new FadeText("@00+" + cur_available_crafts[hover_over].output[i].item_quantity + " " +
                                            Exilania.furniture_manager.furniture[cur_available_crafts[hover_over].output[i].item_id].name,
                                                Display.default_msec_show_fade_text / 2, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40, true, true));
                                        break;
                                    case ItemType.ItemPiece:
                                        Item temp = new Item();
                                        temp.add_piece(Exilania.item_manager.item_pieces[cur_available_crafts[hover_over].output[i].item_id], cur_available_crafts[hover_over].output[i].item_id, -1, 0, 0, 0);
                                        temp.construct_item(Exilania.item_manager.item_pieces[cur_available_crafts[hover_over].output[i].item_id].name);
                                        a.items.pickup_item(temp);
                                        Exilania.display.fading_text.Add(new FadeText("@00Received " +
                                            temp.item_name,
                                                Display.default_msec_show_fade_text / 2, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40, true, true));
                                        break;
                                    case ItemType.Material:
                                        a.items.pickup_material(cur_available_crafts[hover_over].output[i].item_id, (ushort)cur_available_crafts[hover_over].output[i].item_quantity);
                                        Exilania.display.fading_text.Add(new FadeText("@00+" + cur_available_crafts[hover_over].output[i].item_quantity + " " +
                                            Exilania.material_manager.materials[cur_available_crafts[hover_over].output[i].item_id].name,
                                                Display.default_msec_show_fade_text / 2, (int)a.world_loc.X + 18, (int)a.world_loc.Y - 40, true, true));
                                        break;
                                }
                            }
                            last_time_bought = ticks_now;
                        }
                        a.items.click_consumed = true;
                        a.items.last_left_state = true;
                    }
                    else if (last_time_bought + buy_delay <= ticks_now && cur_mouse_state.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                    {
                        cur_click = -1;
                        cur_click_name = "";
                        buys_in_a_row = 0;
                        buy_delay = 200;
                    }

                }
                if (first_show < 0)
                    first_show = 0;
                if (first_show + num_show > cur_available_crafts.Count)
                {
                    if (cur_available_crafts.Count > num_show)
                        first_show = cur_available_crafts.Count - num_show;
                    else
                        first_show = 0;
                }
                last_mouse_state = cur_mouse_state;
            }
        }

        public void write_info_about_item(SpriteBatch s, CraftRecipe r, Point loc, Display d)
        {
            if (r.output[0].item_id != infos_item_type.item_id || r.output[0].type != infos_item_type.item_type)
            {
                infos_item_type.item_id = r.output[0].item_id;
                infos_item_type.item_type = r.output[0].type;
                infos = new List<KeyValuePair<string, Vector2>>();
                string writing = "";
                switch (cur_available_crafts[hover_over].output[0].type)
                {
                    case ItemType.Block:
                        writing = r.output[0].item_quantity + " " + Exilania.block_types.blocks[cur_available_crafts[hover_over].output[0].item_id].name;
                        infos.Add(new KeyValuePair<string,Vector2>(writing,d.small_font.MeasureString(writing)));
                        break;
                    case ItemType.Furniture:
                        writing = r.output[0].item_quantity + " " + Exilania.furniture_manager.furniture[r.output[0].item_id].name;
                        infos.Add(new KeyValuePair<string,Vector2>(writing,d.small_font.MeasureString(writing)));
                        break;
                    case ItemType.ItemPiece:
                        writing = Exilania.item_manager.item_pieces[r.output[0].item_id].name;
                        infos.Add(new KeyValuePair<string,Vector2>(writing,d.small_font.MeasureString(writing)));
                        break;
                    case ItemType.Material:
                        writing = Exilania.material_manager.materials[r.output[0].item_id].name;
                        infos.Add(new KeyValuePair<string,Vector2>(writing,d.small_font.MeasureString(writing)));
                        break;
                }
                if (r.furniture_require.Length > 0)
                {
                    writing = "";
                    for (int x = 0; x < r.furniture_require.Length; x++)
                    {
                        if (x == 0)
                            writing = "Equipment Required: " + r.furniture_require[x];
                        else
                            writing += ", " + r.furniture_require[x];
                    }
                    infos.Add(new KeyValuePair<string, Vector2>(writing, d.small_font.MeasureString(writing)));
                }
                writing = "Requires: " + r.ToString();
                draw_size = new Point(0, infos.Count * 20 + 40);
                infos.Add(new KeyValuePair<string, Vector2>(writing, d.small_font.MeasureString(writing)));
                writing = "Complexity: " + r.complexity;
                infos.Add(new KeyValuePair<string, Vector2>(writing, d.small_font.MeasureString(writing)));
                for (int x = 0; x < infos.Count; x++)
                {
                    if (infos[x].Value.X > draw_size.X)
                        draw_size.X = (int)infos[x].Value.X;
                }
                draw_size.X += 20;
                draw_size.Y += 20;
                
            }
            d.draw_bounding_box(s, new Rectangle((int)loc.X - draw_size.X, (int)loc.Y, draw_size.X, draw_size.Y));
            d.draw_bounding_box(s, new Rectangle((int)loc.X - draw_size.X, (int)loc.Y, draw_size.X, draw_size.Y));
            for (int x = 0; x < infos.Count; x++)
            {
                d.draw_text(s, d.small_font, "@00" + infos[x].Key, (int)loc.X + 10 - draw_size.X, (int)loc.Y + 10 + (x * 20), 600);
            }
        }

        public void take_item_requirements(CraftRecipe r, Actor a, World w)
        {
            for (int x = 0; x < r.input.Count; x++)
            {
                item_descriptor t = Acc.get_item_by_name(r.input[x].Key);
                if (t.item_type != ItemType.Empty && t.item_id != -1)
                {
                    a.items.use_item(a,t.item_type, t.item_id, r.input[x].Value);
                }
            }
        }

        public bool check_is_recipe_valid(CraftRecipe r, Actor a, World w)
        {
            for (int x = 0; x < r.input.Count; x++)
            {
                item_descriptor t = Acc.get_item_by_name(r.input[x].Key);
                if (t.item_type == ItemType.Empty || t.item_id == -1 || !a.items.has_item(t.item_type,t.item_id,r.input[x].Value))
                    return false;
            }
            for (int x = 0; x < r.furniture_require.Length; x++)
            {
                bool contained = false;
                for (int i = 0; i < id_nearby_furniture.Count; i++)
                {
                    if (Exilania.furniture_manager.furniture[w.furniture[id_nearby_furniture[i]].furniture_id].name.ToLower() == r.furniture_require[x].ToLower())
                        contained = true;
                }
                if (!contained)
                    return false;
            }
            return true;
        }

        public void draw_crafts(SpriteBatch s, Display d, Player p, World w)
        {
            if (p.avatar.items.show_backpack)
            {
                d.draw_text(s, d.small_font, "@00Basic Crafting", Exilania.screen_size.X - 140, 25, 160);
                d.draw_text(s, d.small_font, "@00" + cur_available_crafts.Count + " useable", Exilania.screen_size.X - 140, 45, 160);

                Point on_screen_loc = new Point(Microsoft.Xna.Framework.Input.Mouse.GetState().X, Microsoft.Xna.Framework.Input.Mouse.GetState().Y);
                if (first_show != 0)
                {
                    d.draw_text(s, d.small_font, "@00" + first_show, Exilania.screen_size.X - 40, 55, 160);
                }
                for (int x = first_show; x < first_show + num_show; x++)
                {
                    if (x < cur_available_crafts.Count && x > -1)
                    {
                        s.Draw(d.sprites, new Vector2(Exilania.screen_size.X - 60, 70 + (x-first_show) * 50), new Rectangle(Cubby.loc_first.X + ((7 % 4) * 46), Cubby.loc_first.Y + ((7 / 4) * 46), 46, 46), Color.White);
                        switch (cur_available_crafts[x].output[0].type)
                        {
                            case ItemType.Block:
                                s.Draw(d.sprites, new Rectangle(Exilania.screen_size.X - 55, 75 + (x-first_show) * 50, 36, 36),
                                    d.frames[Exilania.block_types.blocks[cur_available_crafts[x].output[0].item_id].image_pointers[Exilania.block_types.blocks[cur_available_crafts[x].output[0].item_id].block_image_use]],
                                    Color.White);
                                break;
                            case ItemType.Furniture:
                                Exilania.furniture_manager.furniture[cur_available_crafts[x].output[0].item_id].draw_item_furniture(s, d, new Point(Exilania.screen_size.X - 60, 70 + (x - first_show) * 50));
                                break;
                            case ItemType.ItemPiece:
                                Rectangle draw_at = new Rectangle();
                                float width = Exilania.item_manager.item_pieces[cur_available_crafts[x].output[0].item_id].image.Width;
                                float height = Exilania.item_manager.item_pieces[cur_available_crafts[x].output[0].item_id].image.Height;
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
                                    Exilania.screen_size.X - 60 + 1 + (int)((44 - width) / 2),
                                    70 + (x - first_show) * 50 + 1 + (int)((44 - height) / 2),
                                    (int)width, (int)height);
                                s.Draw(d.sprites, draw_at,
                                    Exilania.item_manager.item_pieces[cur_available_crafts[x].output[0].item_id].image, Color.White);
                                break;
                            case ItemType.Material:
                                s.Draw(d.sprites, new Rectangle(Exilania.screen_size.X - 55, 75 + (x - first_show) * 50, 36, 36),
                                    d.frames[Exilania.material_manager.materials[cur_available_crafts[x].output[0].item_id].image],
                                    Color.White);
                                break;
                        }
                        if (cur_available_crafts[x].complexity > p.avatar.stats.complexity)
                        {
                            d.draw_text(s, d.small_font, "@00TC: " + cur_available_crafts[x].complexity, Exilania.screen_size.X - 80, 70 + (x - first_show) * 50, 160);
                        }
                        //draw info of hovered over item if there is one.
                        
                    }
                }
                if (hover_over > -1 && cur_available_crafts.Count > hover_over)
                {
                        write_info_about_item(s, cur_available_crafts[hover_over], new Point(Exilania.screen_size.X - 60, 70 + (hover_over - first_show) * 50), d);
                }
                if (first_show  + num_show < cur_available_crafts.Count)
                {
                    d.draw_text(s, d.small_font, "@00" + (cur_available_crafts.Count - first_show - num_show), Exilania.screen_size.X - 40, 60 + (50 * num_show), 160);
                }
            }
        }

    }
}
