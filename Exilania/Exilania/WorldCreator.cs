using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Exilania
{
  
    public class WorldCreator
    {
        public List<DefinitionWorld> diff_worlds;
        World making;
        string cur_info_text;
        public static int max_time = 6; //in milliseconds
        public int world_template;
        public string chosen_world_name;
        public int cur_option;
        public int cur_hover;
        public int creation_state;
        public int creation_fine_control;

        public WorldCreator()
        {
            DefinitionWorld.FullWorldTypeReader(ref diff_worlds);
            cur_info_text = "";
            world_template = -1;
            chosen_world_name = "";
            cur_hover = -1;
            cur_option = -1;
            creation_state = -1;
            creation_fine_control = 0;
        }

        public void update_choose_name(Input input, Exilania e)
        {
            if (input.key_input.Length > 30)
                input.key_input = input.key_input.Substring(0, 30);
            chosen_world_name = input.key_input.Trim();
            if (input.mouse_cur_spot.X < Exilania.screen_size.X / 2 - 200 || input.mouse_cur_spot.X > Exilania.screen_size.X / 2 + 100)
            {
                cur_hover = -1;
                return;
            }
            if (input.mouse_now.Y < Exilania.screen_size.Y * (.30f))
            {
                cur_hover = -1;
            }
            else if (input.mouse_now.Y > Exilania.screen_size.Y - 100)
            {
                cur_hover = -2;
            }
            else
            {
                float loc = (float)input.mouse_now.Y / (float)Exilania.screen_size.Y;
                loc -= .285f;
                cur_hover = (int)(loc * 100f) / 5;
            }
            if (input.left_clicked && input.mouse_now.X > -1 && input.mouse_now.Y > -1 &&
                input.mouse_now.X <= Exilania.screen_size.X && input.mouse_now.Y <= Exilania.screen_size.Y && e.IsActive)
            {
                input.left_clicked = false;
                switch (cur_hover)
                {
                    case -2: //you have chosen to create the world... continue?
                        if (Acc.sanitize_text_color(input.key_input.Trim()).Length >= 5 && cur_option > -1)
                        {
                            chosen_world_name = input.key_input.Trim();
                            input.key_input = "";
                            world_template = cur_option;
                            making = new World(chosen_world_name, Exilania.seed_id, (int)(System.DateTime.Now.Ticks / 1000));
                            cur_info_text = "Initializing.";
                            Exilania.gstate = 92;
                            creation_state = -1;
                            creation_fine_control = 0;
                            Exilania.disable_chat = false;
                        }
                        break;
                    default: //learn more about a world type.
                        if (cur_hover > -1)
                        {
                            cur_option = cur_hover;
                        }
                        break;
                }
            }
        }

        public void creating_world_update()
        {
            switch (creation_state)
            {
                case -1:
                    cur_info_text = "Initializing World.";
                    creation_state++;
                    break;
                case 50:
                    making.finalize_world_creation();
                    making = null;
                    Exilania.world_manager = new WorldManager();
                    Exilania.display.add_message("@05World Creation Completed.");
                    Exilania.gstate = 90;
                    break;
                default:
                    
                    cur_info_text = diff_worlds[world_template].creating_world(ref making, ref creation_state, ref creation_fine_control);
                    break;
            }

        }

        public void creating_draw(SpriteBatch s, Display d)
        {
            s.Draw(d.planet_bkd, new Rectangle((Exilania.screen_size.X / 2) - (1920 / 2), (Exilania.screen_size.Y / 2) - (1080 / 2), 1920, 1080), new Rectangle(0, 0, 1920, 1080), Color.White);
            Vector2 size_text = d.font.MeasureString("World Creation Menu");
            d.draw_text(s, d.font, "@05World Creation Menu", Exilania.screen_size.X / 2 - (int)size_text.X / 2, 10, Exilania.screen_size.X);
            s.Draw(d.Exilania_title, new Rectangle(Exilania.screen_size.X - 400, (int)((float)Exilania.screen_size.Y - 100), 392, 87), new Rectangle(0, 0, 392, 87), Color.White);
            size_text = d.middle_font.MeasureString(cur_info_text);
            d.draw_text(s, d.middle_font, "@00" + cur_info_text, Exilania.screen_size.X / 2 - (int)(size_text.X/2), (int)((float)Exilania.screen_size.Y * (.50f)) - (int)(size_text.Y/2), Exilania.screen_size.X / 2);
        }

        public void draw_choosing_menu(SpriteBatch s, Display d)
        {
            string add_beginning = "@00";
            s.Draw(d.planet_bkd, new Rectangle((Exilania.screen_size.X / 2) - (1920 / 2), (Exilania.screen_size.Y / 2) - (1080 / 2), 1920, 1080), new Rectangle(0, 0, 1920, 1080), Color.White);
            Vector2 size_text = d.font.MeasureString("World Creation Menu");
            d.draw_text(s, d.font, "@05World Creation Menu", Exilania.screen_size.X / 2 - (int)size_text.X / 2, 10, Exilania.screen_size.X);
            s.Draw(d.Exilania_title, new Rectangle(Exilania.screen_size.X - 400, (int)((float)Exilania.screen_size.Y - 100), 392, 87), new Rectangle(0, 0, 392, 87), Color.White);

            for (int x = 0; x < diff_worlds.Count; x++)
            {
                if (x == cur_option)
                    add_beginning = "@05";
                else if (x == cur_hover)
                    add_beginning = "@08";
                else
                    add_beginning = "@00";
                d.draw_text(s, d.middle_font, add_beginning + diff_worlds[x].ToString(), Exilania.screen_size.X / 2 - 166, (int)((float)Exilania.screen_size.Y * (.30f + ((float)x * .05f))), Exilania.screen_size.X / 2);
            }

            string fix_text = "";
            if (Acc.sanitize_text_color(chosen_world_name).Length >= 5 && cur_option > -1)
            {
                add_beginning = "@05";
            }
            else
            {
                if(Acc.sanitize_text_color(chosen_world_name).Length < 5)
                fix_text += "Type a name for this world. It must have at least 5 characters.";
                if (cur_option < 0)
                    fix_text += " You must choose a template.";
                add_beginning = "@08";
            }
            if (fix_text != "")
            {
                Vector2 meas = d.middle_font.MeasureString(fix_text);
                d.draw_text(s, d.middle_font, "@00" + fix_text,
                    Exilania.screen_size.X / 2 - (int)(meas.X /2), (int)((float)Exilania.screen_size.Y - 150), Exilania.screen_size.X);
            }
            d.draw_text(s, d.middle_font, add_beginning + "[ Click to Generate World ]", Exilania.screen_size.X / 2 - 166, (int)((float)Exilania.screen_size.Y - 60), Exilania.screen_size.X / 2);
            d.draw_text(s, d.middle_font, "@00World Name: " + chosen_world_name, Exilania.screen_size.X / 2 - 166, (int)((float)Exilania.screen_size.Y - 200), Exilania.screen_size.X / 2);

        }
    }
}
