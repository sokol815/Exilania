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
    public class FadeText
    {
        public string text;
        public int time_left;
        //the center location of the text.
        public Point loc;
        public bool world_loc;
        public bool rise;
        public bool follow_owner;
        public TargetType owner_type;
        public int owner_id;
        public Point owner_offset;

        public FadeText()
        {

        }

        public FadeText(string p_text, int time_draw, int locx, int locy, bool pworld_local, bool prise)
        {
            text = p_text;
            time_left = time_draw;
            loc = new Point(locx, locy);
            world_loc = pworld_local;
            rise = prise;
            follow_owner = false;
            owner_id = -1;
            owner_type = TargetType.Empty;
        }

        public FadeText(string p_text, int time_draw, int locx, int locy, bool pworld_local, TargetType owner_t, int owner_id_num, World w)
        {
            text = p_text;
            time_left = time_draw;
            loc = new Point(locx, locy);
            world_loc = pworld_local;
            rise = false;
            follow_owner = true;
            owner_id = owner_id_num;
            owner_type = owner_t;
            owner_offset = new Point();
            switch (owner_type)
            {
                case TargetType.Player:
                    owner_offset.X = loc.X - (int)w.players[owner_id].avatar.world_loc.X;
                    owner_offset.Y = loc.Y - (int)w.players[owner_id].avatar.world_loc.Y;
                    break;
                case TargetType.NPC:
                    owner_offset.X = loc.X - (int)w.npcs[owner_id].world_loc.X;
                    owner_offset.Y = loc.Y - (int)w.npcs[owner_id].world_loc.Y;
                    break;

            }
        }

        public void update_text(World w)
        {
            if (follow_owner)
            {
                switch (owner_type)
                {
                    case TargetType.Player:
                        if(owner_id < w.players.Count && !w.players[owner_id].is_player_empty)
                        {
                            loc.X = (int)w.players[owner_id].avatar.world_loc.X + owner_offset.X;
                            loc.Y = (int)w.players[owner_id].avatar.world_loc.Y + owner_offset.Y;
                        }
                        break;
                    case TargetType.NPC:
                        if (owner_id < w.npcs.Count && !w.npcs[owner_id].empty)
                        {
                            loc.X = (int)w.npcs[owner_id].world_loc.X + owner_offset.X;
                            loc.Y = (int)w.npcs[owner_id].world_loc.Y + owner_offset.Y;
                        }
                        break;
                }
            }
        }


    }

    public class Display
    {
        public static string color_code = "@";
        public int cur_tileset_use = 0;
        int cur_floor = 10;
        public int start_show_messages = 1;
        public Rectangle[] frames;
        
        public Rectangle visible;
        public SpriteFont font;
        public SpriteFont small_font;
        public SpriteFont middle_font;
        public bool use_small_font;
        public bool show_messages;
        public Texture2D sprites;
        public Texture2D planet_bkd;
        public Texture2D Exilania_title;
        public Texture2D backgrounds;
        public Point char_size = new Point(11,22);
        public List<string> messages;
        public string[] stat_display = { Display.color_code + "00CONTENT", Display.color_code + "01CONTENT", Display.color_code + "03CONTENT", Display.color_code + "05CONTENT" };
        public List<FadeText> fading_text;
        public Point cur_Center;
        public Point level_size;
        public double magnification;
        public Rectangle sun = new Rectangle(1548,0,32,32);
        public Rectangle mouse = new Rectangle(1580, 0, 14, 16);
        public static float show_time_cur = 0f;
        public static float show_received_chat_time = 7f;
        public bool temp_show_chat = false;
        public int drawing_messages = -1;
        public static int default_msec_show_fade_text = 1000;

        public Display()
        {
            show_messages = false;
            magnification = 1;

            frames = new Rectangle[2720];
            for (int x = 0; x < frames.Length; x++)
            {
                frames[x] = new Rectangle((x % 32) * 24, (x / 32) * 24, 24, 24);
            }
            messages = new List<string>();
            fading_text = new List<FadeText>();
        }

        public void add_message(string message)
        {
            int temp = 0;
            if (!int.TryParse(message.Substring(1, 2), out temp))
            {
                message = "@00" + message;
            }
            
            messages.Add(message);
            start_show_messages = messages.Count - 1;
            if (start_show_messages < messages.Count)
            {
                start_show_messages = messages.Count - 1;
            }
            if (!show_messages)
            {
                temp_show_chat = true;
                show_time_cur = 0;
            }
            /*fading_text.Add(new FadeText(Acc.sanitize_color_from_string(message), 10000, Game1.screen_size.X / 2, 80 + (fading_text.Count * 32), Color.Yellow));
            if (fading_text.Count > 3)
            {
                for (int x = 0; x < fading_text.Count - 3; x++)
                    fading_text[x].time_left = -256;
            }*/
        }

        public void update_top_stat(int id, string stat_string)
        {
            if (id > -1 && id < stat_display.Length)
            {
                stat_display[id] = stat_string;
            }
        }

        public void update(int msec_passed, World w)
        {
            float sec_passed = ((float)msec_passed) / 1000f;
            show_time_cur += sec_passed;
            if (temp_show_chat && show_time_cur > show_received_chat_time)
            {
                temp_show_chat = false;
                show_time_cur = 0;
            }
            for (int i = 0; i < fading_text.Count; i++)
            {
                fading_text[i].update_text(w);
                fading_text[i].time_left -= msec_passed;
                if (fading_text[i].rise)
                    fading_text[i].loc.Y -= 2;
            }
        }

        /// <summary>
        /// this basically fits the messages into the area provided....
        /// </summary>
        /// <param name="input"></param>
        public void change_show_messages(Input input)
        {
            if (input.dx_dy.X == 0 && input.dx_dy.Y != 0)
            { //up or down arrow
                start_show_messages -= input.dx_dy.Y;
            }
            else if (input.dx_dy.X == 1 && input.dx_dy.Y != 0)
            { //pushed pgup or pgdown
                start_show_messages -= input.dx_dy.Y * (((Exilania.screen_size.Y - (char_size.Y + 6)) / (char_size.Y + 3)) - 2);
            }
            else if (input.dx_dy.X == -1 && input.dx_dy.Y != 0)
            { //pushed pgup or pgdown
                start_show_messages = input.dx_dy.Y * messages.Count * -1;
            }
            if (start_show_messages >= messages.Count)
            {
                start_show_messages = messages.Count - 1;
            }
            else if (start_show_messages < Math.Min(messages.Count, ((Exilania.screen_size.Y - (char_size.Y + 6)) / (char_size.Y + 3)) - 2))
            {
                start_show_messages = Math.Min(messages.Count - 1, ((Exilania.screen_size.Y - (char_size.Y + 6)) / (char_size.Y + 3)) - 2);
            }
        }

        /// <summary>
        /// this will take input like (color_code)01HELLO! and use it to write out colored text. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="text_to_draw"></param>
        public void draw_text(SpriteBatch s, string text_to_draw, int x_start, int y_start, int max_width, bool use_small_font)
        {
            if (!text_to_draw.Contains(Display.color_code[0]) || text_to_draw[0] != Display.color_code[0])
            {
                text_to_draw = Display.color_code+"01" + text_to_draw;
            }

            string[] texts = text_to_draw.Split(Display.color_code[0]);
            int cur_width = 0;
            int cur_measure = 0;
            int col = 0;
            for (int x = 0; x < texts.GetLength(0); x++)
            {
                if (texts[x].Length > 2 && int.TryParse(texts[x].Substring(0, 2), out col))
                {
                    if (use_small_font)
                    {
                        cur_measure = (int)small_font.MeasureString(texts[x].Substring(2)).X;
                    }
                    else
                        cur_measure = (int)middle_font.MeasureString(texts[x].Substring(2)).X;
                    if (cur_width + cur_measure < max_width)
                    {
                        if (use_small_font)
                            s.DrawString(small_font, texts[x].Substring(2), new Vector2(x_start + cur_width, y_start), Acc.int_to_col(col));
                        else
                            s.DrawString(middle_font, texts[x].Substring(2), new Vector2(x_start + cur_width, y_start), Acc.int_to_col(col));
                        cur_width += cur_measure;
                    }
                    else if (cur_width < max_width)
                    {
                        int over = cur_width + cur_measure - max_width;
                        over /= 12;
                        //texts[x] = texts[x].Substring(0, texts[x].Length - (3 + over)) + "...";
                        if (use_small_font)
                        {
                            s.DrawString(small_font, texts[x].Substring(2), new Vector2(x_start + cur_width, y_start), Acc.int_to_col(col));
                        }
                        else
                            s.DrawString(middle_font, texts[x].Substring(2), new Vector2(x_start + cur_width, y_start), Acc.int_to_col(col));
                        cur_width += cur_measure;
                    }
                }
            }
        }

        public void draw_text_with_outline(SpriteBatch s, SpriteFont sf, string text, int x_start, int y_start, int max_width, AccColors c_use)
        {
            string sanitized = "@" + ((int)c_use).ToString().PadLeft(2, '0') + Acc.sanitize_text_color(text);
            for (int i = 0; i < World.xes.Length; i++)
            {
                draw_text(s, small_font, sanitized, x_start + (World.xes[i] * 2), y_start + (World.yes[i] * 2), max_width);
            }
            draw_text(s, small_font, text, x_start,y_start,max_width);
        }

        /// <summary>
        /// this will take input like (color_code)01HELLO! and use it to write out colored text. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="text_to_draw"></param>
        public void draw_text(SpriteBatch s, SpriteFont sf, string text_to_draw, int x_start, int y_start, int max_width)
        {
            string[] texts = text_to_draw.Split(Display.color_code[0]);
            int cur_width = 0;
            int cur_measure = 0;
            int col = 0;
            string combined = "";
            for (int x = 0; x < texts.Length; x++)
            {
                combined = "";
                if (texts[x].Length >= 2)
                {
                    if ((byte)texts[x][0] >= 48 && (byte)texts[x][0] <= 57 && (byte)texts[x][1] >= 48 && (byte)texts[x][1] <= 57)
                    { //we have a color code... remove it
                        col = int.Parse(texts[x].Substring(0, 2));
                        texts[x] = texts[x].Substring(2);
                        combined = texts[x];
                    }
                    else
                    { //not a color code, display the @ symbol
                        combined = "@" + texts[x];
                    }
                }
                else if (x != 0)
                    combined = "@" + texts[x];
                if (combined != "")
                {
                    cur_measure = (int)sf.MeasureString(combined).X;
                    if (cur_width + cur_measure <= max_width)
                    {
                        s.DrawString(sf, combined, new Vector2(x_start + cur_width, y_start), Acc.int_to_col(col));
                        cur_width += cur_measure;
                    }
                    else if (cur_width < max_width) //adding the string is just too much!
                    {
                        int over = cur_width + cur_measure - max_width;
                        string final = combined;
                        int min_try = 0;
                        int max_try = combined.Length;
                        while (min_try + 1 < max_try)
                        {
                            final = combined.Substring(0, (min_try + max_try) / 2);
                            cur_measure = (int)sf.MeasureString(final).X;
                            over = cur_width + cur_measure - max_width;
                            if (over > 0)
                            {
                                max_try = final.Length;
                            }
                            else
                            {
                                min_try = final.Length;
                            }
                        }
                        s.DrawString(sf, final, new Vector2(x_start + cur_width, y_start), Acc.int_to_col(col));
                        if (drawing_messages > -1 && combined.Length > final.Length)
                        {
                            string precombined = combined.Substring(final.Length - 1);
                            combined = "@" + col.ToString().PadLeft(2, '0') + (combined.Substring(final.Length - 1).Trim());
                            while (x < texts.Length - 1)
                            {
                                x++;
                                combined += "@" + texts[x];
                                precombined += "@" + texts[x];
                            }
                            messages[messages.Count - 1] = messages[messages.Count - 1].Replace(precombined, "");
                            messages.Add(combined);
                            start_show_messages = messages.Count - 1;
                        }
                        return;
                    }
                    else
                        return;
                }
            }
        }

          /// <summary>
        /// this will take input like (color_code)01HELLO! and use it to write out colored text. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="text_to_draw"></param>
        public void draw_text(SpriteBatch s, SpriteFont sf, string text_to_draw, int x_start, int y_start, int max_width, byte opacity)
        {
            
            string[] texts = text_to_draw.Split(Display.color_code[0]);
            int cur_width = 0;
            int cur_measure = 0;
            Color col = Color.White;
            col.A = opacity;
            int cur_col = 0;
            string combined = "";
            for (int x = 0; x < texts.Length; x++)
            {
                combined = "";
                if (texts[x].Length >= 2)
                {
                    if ((byte)texts[x][0] >= 48 && (byte)texts[x][0] <= 57 && (byte)texts[x][1] >= 48 && (byte)texts[x][1] <= 57)
                    { //we have a color code... remove it
                        cur_col = int.Parse(texts[x].Substring(0, 2));
                        col = Acc.int_to_col(int.Parse(texts[x].Substring(0, 2)));
                        col.A = opacity;
                        texts[x] = texts[x].Substring(2);
                        combined = texts[x];
                    }
                    else
                    { //not a color code, display the @ symbol
                        combined = "@" + texts[x];
                    }
                }
                else if (x != 0)
                    combined = "@" + texts[x];
                if (combined != "")
                {
                    cur_measure = (int)sf.MeasureString(combined).X;
                    if (cur_width + cur_measure <= max_width)
                    {
                        s.DrawString(sf, combined, new Vector2(x_start + cur_width, y_start),Color.FromNonPremultiplied(col.R,col.G,col.B,opacity));
                        cur_width += cur_measure;
                    }
                    else if (cur_width < max_width) //adding the string is just too much!
                    {
                        int over = cur_width + cur_measure - max_width;
                        string final = combined;
                        int min_try = 0;
                        int max_try = combined.Length;
                        while (min_try + 1 < max_try)
                        {
                            final = combined.Substring(0, (min_try + max_try) / 2);
                            cur_measure = (int)sf.MeasureString(final).X;
                            over = cur_width + cur_measure - max_width;
                            if (over > 0)
                            {
                                max_try = final.Length;
                            }
                            else
                            {
                                min_try = final.Length;
                            }
                        }
                        s.DrawString(sf, final, new Vector2(x_start + cur_width, y_start), Color.FromNonPremultiplied(col.R, col.G, col.B, opacity));
                        if (drawing_messages > -1 && combined.Length > final.Length)
                        {
                            string precombined = combined.Substring(final.Length-1);
                            combined = "@" + cur_col.ToString().PadLeft(2,'0') + (combined.Substring(final.Length-1).Trim());
                            while (x < texts.Length - 1)
                            {
                                x++;
                                combined += "@" + texts[x];
                                precombined += "@" + texts[x];
                            }
                            messages.Insert(drawing_messages,combined);
                            messages[drawing_messages + 1] = messages[drawing_messages + 1].Replace(precombined, "");
                            start_show_messages = messages.Count - 1;
                        }
                        return;
                    }
                    else
                        return;
                }
            }
        }

        public void draw_stats(SpriteBatch s)
        {
            for (int x = 0; x < stat_display.Length; x++)
            {
                draw_text(s, stat_display[x], 10, 2 + (x * 24), Exilania.screen_size.X - 110, use_small_font);
            }
        }

        public void draw_bounding_box(SpriteBatch s, Rectangle outer_sides)
        {
            //corners
            s.Draw(sprites, new Rectangle(outer_sides.X, outer_sides.Y, 6, 6), new Rectangle(1529, 0, 6, 6), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + outer_sides.Width - 6, outer_sides.Y, 6, 6), new Rectangle(1541, 0, 6, 6), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + outer_sides.Width - 6, outer_sides.Y + outer_sides.Height - 6, 6, 6), new Rectangle(1541, 12, 6, 6), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X, outer_sides.Y + outer_sides.Height - 6, 6, 6), new Rectangle(1529, 12, 6, 6), Color.White);
            //sides
            s.Draw(sprites, new Rectangle(outer_sides.X + 6, outer_sides.Y, outer_sides.Width - 12, 6), new Rectangle(1535, 0, 6, 6), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + 6, outer_sides.Y + outer_sides.Height - 6, outer_sides.Width - 12, 6), new Rectangle(1535, 0, 6, 6), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X, outer_sides.Y + 6, 6, outer_sides.Height - 12), new Rectangle(1529, 6, 6, 6), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + outer_sides.Width - 6, outer_sides.Y + 6, 6, outer_sides.Height - 12), new Rectangle(1541, 6, 6, 6), Color.White);
            //middle
            s.Draw(sprites, new Rectangle(outer_sides.X + 6, outer_sides.Y + 6, outer_sides.Width - 12, outer_sides.Height - 12), new Rectangle(1535, 6, 6, 6), Color.White);

        }

        public void draw_metal_box(SpriteBatch s, Rectangle outer_sides)
        {
            //corners
            s.Draw(sprites, new Rectangle(outer_sides.X, outer_sides.Y, 13, 13), new Rectangle(1529, 44, 13, 13), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + outer_sides.Width - 13, outer_sides.Y, 13, 13), new Rectangle(1564, 44, 13, 13), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + outer_sides.Width - 13, outer_sides.Y + outer_sides.Height - 13, 13, 13), new Rectangle(1564, 79, 13, 13), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X, outer_sides.Y + outer_sides.Height - 13, 13, 13), new Rectangle(1529, 79, 13, 13), Color.White);
            //sides
            s.Draw(sprites, new Rectangle(outer_sides.X + 13, outer_sides.Y, outer_sides.Width - 26, 8), new Rectangle(1541, 44, 24, 8), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + 13, outer_sides.Y + outer_sides.Height - 8, outer_sides.Width - 26, 8), new Rectangle(1541, 84, 24, 8), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X, outer_sides.Y + 13, 8, outer_sides.Height - 26), new Rectangle(1529, 56, 8, 24), Color.White);
            s.Draw(sprites, new Rectangle(outer_sides.X + outer_sides.Width - 8, outer_sides.Y + 13, 8, outer_sides.Height - 26), new Rectangle(1569,56,8,24), Color.White);
            //middle
            s.Draw(sprites, new Rectangle(outer_sides.X + 8, outer_sides.Y + 8, outer_sides.Width - 16, outer_sides.Height - 16), new Rectangle(1549,74,7,7), Color.White);

        }

        public void draw_messages(SpriteBatch s,Input i)
        {
            if (show_messages || temp_show_chat)
            {
                int height_font = 18;
                int starty = Exilania.screen_size.Y - 458;
                int num_draw = 0;
                int last_message = start_show_messages + 1;
               
                num_draw = Math.Min(messages.Count, ((Exilania.screen_size.Y - starty - 20) / (height_font)) - 3);
                if(Exilania.settings.show_chat_background)
                    draw_bounding_box(s, new Rectangle(20, Exilania.screen_size.Y - 480, 800, 420));
                if (show_messages)
                    draw_text(s, small_font, Display.color_code + "08(M) to hide messages." + (Exilania.input.current_input_type == InputConsume.Message ? "                                        @05 ESC to cancel sending a message." : ""), //(x + 1 == num_draw && show_messages ? Display.color_code + "08(M) to hide messages." : "")
                            30, Exilania.screen_size.Y - 477, 800);
                else
                    draw_text(s, small_font, Display.color_code + "05This message will hide in " + 
                        (Math.Round((show_received_chat_time - show_time_cur), 0)) + " Second" + (Math.Round((show_received_chat_time - show_time_cur), 0) == 1 ? "." : "s."), 
                        30, Exilania.screen_size.Y - 477, 800);
                for (int x = 0; x < num_draw; x++)
                {
                    drawing_messages = last_message - 1 - x;
                    if (last_message - 1 - x > -1 && last_message - 1 - x < messages.Count)
                    {
                        draw_text(s, small_font, (Display.color_code + "00" + (last_message -  num_draw + x)),
                            30, starty + (x * (height_font)), 780);
                        draw_text(s, small_font, messages[last_message - num_draw + x], //(x + 1 == num_draw && show_messages ? Display.color_code + "08(M) to hide messages." : "")
                            110, starty + (x * (height_font)), 690);
                    }
                }
                drawing_messages = -1;
            }
            if (Exilania.input.current_input_type == InputConsume.Message)
            {
                if(Exilania.settings.show_chat_background)
                    draw_bounding_box(s, new Rectangle(20, Exilania.screen_size.Y - 50, Exilania.screen_size.X - 40, 40));
                draw_text(s, small_font, (Display.color_code + "00>>>" + i.key_input + (DateTime.Now.Millisecond <500?"|":"" ) ), 30, Exilania.screen_size.Y - 40, Exilania.screen_size.X - 70);
            }
        }

        
        public void draw_fade_text(SpriteBatch s,Vector2 top_left)
        {
            Vector2 size_string;
            int left, top;
            byte use;
            for (int x = 0; x < fading_text.Count; x++)
            {
                size_string = small_font.MeasureString(Acc.sanitize_text_color(fading_text[x].text));
                if (size_string.X >= Exilania.screen_size.X || size_string.Y >= Exilania.screen_size.Y)
                {
                    fading_text.RemoveAt(x);
                    x--;
                }
                else
                {
                    if (fading_text[x].world_loc)
                    {
                        left = fading_text[x].loc.X - (int)(size_string.X / 2) - (int)top_left.X;
                        top = fading_text[x].loc.Y - (int)(size_string.Y / 2) - (int)top_left.Y;
                    }
                    else
                    {
                        left = fading_text[x].loc.X - (int)(size_string.X / 2);
                        top = fading_text[x].loc.Y - (int)(size_string.Y / 2);
                    }
                    if (fading_text[x].time_left < 0 && fading_text[x].time_left > -255)
                    {
                        use = (byte)((255 + fading_text[x].time_left) / 2);
                        //use.A = Convert.ToByte(255 + fading_text[x].time_left);
                    }
                    else
                        use = 255;
                    if (fading_text[x].time_left > -256)
                    {
                        string sanitized = "@31" + Acc.sanitize_text_color(fading_text[x].text);
                        for (int i = 0; i < World.xes.Length; i++)
                        {
                            draw_text(s, small_font, sanitized, left + (World.xes[i] * 2), top + (World.yes[i] * 2), (int)size_string.X, use);
                        }
                        draw_text(s, small_font, fading_text[x].text, left, top, (int)size_string.X, use);
                    }
                    if (fading_text[x].time_left <= -255)
                    {
                        fading_text.RemoveAt(x);
                        x--;
                    }
                }
            }
        }

        /// <summary>
        /// this is used to draw an image rotated... will be used extensively to draw ships when they are tilted.
        /// </summary>
        /// <param name="s">spritebatch draw</param>
        /// <param name="rotation">rotation of the item in question</param>
        /// <param name="item_center">the center of rotation for the image</param>
        /// <param name="image_offset">X and Y distances from center of this square to center of item</param>
        /// <param name="image_id">image number to draw</param>
        /// <param name="col_use">color to use to draw image</param>
        /// <param name="x_mirror">should it be flipped?</param>
        public void draw_rotated_baseimage(SpriteBatch s, double rotation, Vector2 item_center, Vector2 image_offset, int image_id, Color col_use, bool x_mirror)
        {
            if (x_mirror)
            {
                image_offset.X *= -1;
                rotation = (Math.PI * 2) - rotation;
            }
            s.Draw(sprites, item_center, frames[image_id], col_use, (float)rotation, new Vector2(image_offset.X,image_offset.Y), 
                1f, (x_mirror ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }


        public void draw_rotated_image(SpriteBatch s, Rectangle parent_loc, Vector2 parent_center_offset, Vector2 my_center_on_parent, Double rotation, Color col_use, int image_id, bool x_mirror)
        {
            s.Draw(sprites, new Vector2(parent_loc.Center.X + my_center_on_parent.X + parent_center_offset.X, parent_loc.Center.Y + my_center_on_parent.Y + parent_center_offset.Y),
                frames[image_id], col_use, (float)rotation, new Vector2(12, 12),1f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// controls the viewing of the messaging system... always visible, always able to be turned off (unless enter has been pressed and message typing is commenced)
        /// </summary>
        /// <param name="input"></param>
        public void input_message_display(Input input, Settings settings, Actor me, World w)
        {
            if (Exilania.input.current_input_type != InputConsume.Message && !Exilania.disable_chat)
            {
                if (input.enter)
                {
                    input.enter = false;
                    show_messages = true;
                    input.key_input = "";
                    Exilania.input.current_input_type = InputConsume.Message;
                }
                if (input.key_input.Contains('M') || input.key_input.Contains('m'))
                {
                    input.key_input = input.key_input.Replace("M", "");
                    input.key_input = input.key_input.Replace("m", "");

                    show_messages = !show_messages;
                }
                if (input.key_input.Contains('-'))
                {
                    input.key_input = input.key_input.Replace("-", "");
                    start_show_messages--;
                    if (start_show_messages < Math.Min(20, messages.Count - 1))
                    {
                        start_show_messages = Math.Min(20, messages.Count - 1);
                    }
                }
                if (input.key_input.Contains('+'))
                {
                    input.key_input = input.key_input.Replace("+", "");
                    start_show_messages++;
                    if (start_show_messages > messages.Count - 1)
                        start_show_messages = messages.Count - 1;
                }
                if (input.keys_now.Contains(Keys.PageUp) && !input.keys_previous.Contains(Keys.PageUp))
                {
                    start_show_messages -= 20;
                    if (start_show_messages < Math.Min(20, messages.Count - 1))
                    {
                        start_show_messages = Math.Min(20, messages.Count - 1);
                    }
                }
                if (input.keys_now.Contains(Keys.PageDown) && !input.keys_previous.Contains(Keys.PageDown))
                {
                    start_show_messages += 20;
                    if (start_show_messages > messages.Count - 1)
                        start_show_messages = messages.Count - 1;
                }
                if (input.keys_now.Contains(Keys.Home) && !input.keys_previous.Contains(Keys.Home))
                {
                    start_show_messages = Math.Min(20, messages.Count - 1);
                }
                if (input.keys_now.Contains(Keys.End) && !input.keys_previous.Contains(Keys.End))
                {
                    start_show_messages = messages.Count - 1;
                }
            }
            else if(!Exilania.disable_chat)
            {
                if (input.enter)
                {
                    input.enter = false;
                    show_messages = true;
                    input.key_input = input.key_input.Trim();
                    if (input.key_input != "")
                    {
                        if (input.key_input[0] == '/')
                        {
                            add_message(settings.modify_settings(input.key_input.Substring(1),w));
                        }
                        else
                        {
                            if (Exilania.game_client)
                            {
                                Exilania.network_client.send_chat(Exilania.game_my_user_id, input.key_input);
                            }
                            if (input.key_input[0] == '!' && me!=null)
                            {
                                for (int i = 0; i < input.key_input.Length; i++)
                                {
                                    if (input.key_input[i] == '!')
                                    {
                                        input.key_input = input.key_input.Substring(0, i) + input.key_input.Substring(i + 1);
                                        break;
                                    }
                                }
                                fading_text.Add(new FadeText("@00"+input.key_input, 3000, (int)me.world_loc.X, (int)me.world_loc.Y - 60, true,TargetType.Player,Exilania.game_my_user_id,w));
                            }
                            else
                                add_message("@02You@00: " + input.key_input);
                            Exilania.text_stream.WriteLine(Acc.sanitize_text_color("@02You@00: " + input.key_input));
                        }
                    }
                    input.key_input = "";
                    Exilania.input.current_input_type = InputConsume.Normal;
                }
            }
        }
    }
}
