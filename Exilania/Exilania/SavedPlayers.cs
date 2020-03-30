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
    /// <summary>
    /// manages the local players... writing to file, reading from file, creating new players.
    /// </summary>
    public class SavedPlayers
    {
        public string[] choice_defs = new string[] { "Skin Color","Pants Color","Primary Color","Secondary Color","Shoes Color","Hair Color"};
        public Color[] default_colors = new Color[] { Color.FromNonPremultiplied(255, 178, 127, 255), Color.Blue, Color.Gray, Color.Green, Color.Brown, Color.FromNonPremultiplied(127, 51, 0, 255) };
        public Color[] set_colors = new Color[] { Color.FromNonPremultiplied(255, 178, 127, 255), Color.Blue, Color.Gray, Color.Green, Color.Brown, Color.FromNonPremultiplied(127, 51, 0, 255) };
        public Color temp_col;
        public int last_active_player;
        public Actor char_disp;
        public List<Player> players;
        public int cur_choice;
        public int clicked_choice;

        /// <summary>
        /// this is only run if the system detects that there are saved player files.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="b"></param>
        public SavedPlayers()
        {
            if (System.IO.File.Exists(@"characters.dat"))
            {
                System.IO.StreamReader fs = new System.IO.StreamReader(@"characters.dat");
                System.IO.BinaryReader r = new System.IO.BinaryReader(fs.BaseStream);
                last_active_player = r.ReadInt32();
                int num_p = r.ReadInt32();
                players = new List<Player>();
                for (int x = 0; x < num_p; x++)
                {
                    players.Add(new Player(r));
                }
                r.Close();
            }
            else
            {
                last_active_player = 0;
                players = new List<Player>();
            }
            BodyTemplate b = Exilania.body_templates[0];
            char_disp = new Actor(b, false);
            char_disp.screen_loc = new Vector2(Exilania.screen_size.X / 5, Exilania.screen_size.Y / 2);
            char_disp.world_loc = new Vector2((int)char_disp.screen_loc.X,(int)char_disp.screen_loc.Y);
            char_disp.update_static_body(6f);
            temp_col = default_colors[0];
            clicked_choice = 0;
            apply_colors_to_character(char_disp);
        }

        public void switch_to_player(int id)
        {
            if (id < players.Count)
            {
                last_active_player = id;
            }
        }

        public void save_players()
        {
            System.IO.StreamWriter fs = new System.IO.StreamWriter(@"characters.dat");
            System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs.BaseStream);
            w.Write(last_active_player);
            w.Write(players.Count);
            for (int x = 0; x < players.Count; x++)
            {
                players[x].write_player(w);
            }
            w.Close();
        }

        public void apply_colors_to_character(Actor a)
        {
           for (int x = 0; x < char_disp.body.Count; x++)
            {
                if (char_disp.body[x].name == "Head" || char_disp.body[x].name == "Left Hand" || char_disp.body[x].name == "Right Hand")
                {
                    char_disp.body[x].draw_color = set_colors[0];
                }
                if (char_disp.body[x].name == "Left Leg" || char_disp.body[x].name == "Right Leg")
                {
                    char_disp.body[x].draw_color = set_colors[1];
                }
                if (char_disp.body[x].name == "Torso" || char_disp.body[x].name == "Left Arm" || char_disp.body[x].name == "Right Arm")
                {
                    char_disp.body[x].draw_color = set_colors[2];
                }
                if (char_disp.body[x].name == "Left Foot" || char_disp.body[x].name == "Right Foot")
                {
                    char_disp.body[x].draw_color = set_colors[4];
                }
                if (char_disp.body[x].name == "Helmet")
                {
                    char_disp.body[x].draw_color = set_colors[5];
                }
                if (char_disp.body[x].name == "Backpack")
                {
                    char_disp.body[x].draw_color = set_colors[3];
                }
            }
        }

        public void update(Input input, Exilania e)
        {
            int prev_choice = cur_choice;
            if (input.mouse_now.Y < 230)
            {
                cur_choice = -1;
            }
            else if (input.mouse_now.Y >= 230 + (players.Count * 30))
            {
                cur_choice = players.Count - 1;
            }
            else
            {
                int loc = input.mouse_now.Y;
                loc-=230;
                cur_choice = (int)(loc/30);
            }
            if (prev_choice != cur_choice)
            {
                Exilania.sounds.hit_block.Play(Exilania.sounds.master_volume, 0f, 0f);
            }
            if (input.left_clicked && input.mouse_now.X > -1 && input.mouse_now.Y > -1 &&
                input.mouse_now.X <= Exilania.screen_size.X && input.mouse_now.Y <= Exilania.screen_size.Y && e.IsActive)
            {
                Exilania.sounds.finish_hit_block.Play(Exilania.sounds.master_volume,0f,0f);
                input.left_clicked = false;
                switch (cur_choice)
                {
                    case -1: //create new character
                        Exilania.gstate = 70;
                        char_disp.name = "";
                        input.key_input = "";
                        Exilania.disable_chat = true;
                        break;
                    default:
                        Exilania.cur_using_local_id = cur_choice;
                        Exilania.gstate = 80;
                        break;
                }
            }
        }

        public void update_create_character(Input input, Exilania e)
        {
            char_disp.input.mouse_loc = new Point(input.mouse_now.X,input.mouse_now.Y);
            char_disp.update_static_body(6f);
            char_disp.light_actor(null);
            if (input.mouse_now.Y < 220)
            {
                cur_choice = 0;
            }
            else if (input.mouse_now.Y >= 220 + (choice_defs.Length * 30))
            {
                if (input.mouse_now.Y < 320 + (choice_defs.Length * 30))
                    cur_choice = choice_defs.Length - 1;
            }
            else
            {
                int loc = input.mouse_now.Y;
                loc -= 220;
                cur_choice = (int)(loc / 30);
            }
            if (input.key_input.Length > 25)
                input.key_input = input.key_input.Substring(0, 25);
            char_disp.name = input.key_input;
            if (input.mouse_now.LeftButton == ButtonState.Pressed && input.mouse_now.X > -1 && input.mouse_now.Y > -1 &&
                input.mouse_now.X <= Exilania.screen_size.X && input.mouse_now.Y <= Exilania.screen_size.Y && e.IsActive)
            {
                if (input.mouse_now.Y <= 220 + (choice_defs.Length * 30))
                {
                    clicked_choice = cur_choice;
                    temp_col = set_colors[cur_choice];
                }
                else if (input.mouse_now.Y > Exilania.screen_size.Y - 100 && char_disp.name.Length > 3)
                { //create the character.
                    char_disp.body.Sort((y, x) => y.original_order.CompareTo(x.original_order));
                    char_disp.reset_colors();
                    char_disp.body.Sort((x, y) => y.draw_order.CompareTo(x.draw_order));
                    players.Add(new Player(char_disp,1,100,0,12));
                    save_players();
                    Exilania.gstate = 60;
                    Exilania.disable_chat = false;
                }
                else
                {
                    //Exilania.screen_size.X / 2, 320 + (choice_defs.Length * 30)
                    bool color_changed = false;
                    if (input.mouse_now.X >= Exilania.screen_size.X / 2 && input.mouse_now.X < Exilania.screen_size.X / 2 + 15 &&
                        input.mouse_now.Y >= 325 + (choice_defs.Length * 30) && input.mouse_now.Y < 350 + (choice_defs.Length * 30))
                    {
                        if (temp_col.R > 0 && temp_col.R + temp_col.B + temp_col.G > 128)
                        {
                            color_changed = true;
                            temp_col.R--;
                        }
                    }
                    else if (input.mouse_now.X >= Exilania.screen_size.X / 2 + 15 && input.mouse_now.X < Exilania.screen_size.X / 2 + 35 &&
                        input.mouse_now.Y >= 325 + (choice_defs.Length * 30) && input.mouse_now.Y < 350 + (choice_defs.Length * 30))
                    {
                        if (temp_col.R < 255)
                        {
                            color_changed = true;
                            temp_col.R++;
                        }
                    }
                    if (input.mouse_now.X >= Exilania.screen_size.X / 2 && input.mouse_now.X < Exilania.screen_size.X / 2 + 15 &&
                       input.mouse_now.Y >= 355 + (choice_defs.Length * 30) && input.mouse_now.Y < 380 + (choice_defs.Length * 30))
                    {
                        if (temp_col.G > 0 && temp_col.R + temp_col.B + temp_col.G > 128)
                        {
                            color_changed = true;
                            temp_col.G--;
                        }
                    }
                    else if (input.mouse_now.X >= Exilania.screen_size.X / 2 + 15 && input.mouse_now.X < Exilania.screen_size.X / 2 + 35 &&
                        input.mouse_now.Y >= 355 + (choice_defs.Length * 30) && input.mouse_now.Y < 380 + (choice_defs.Length * 30))
                    {
                        if (temp_col.G < 255)
                        {
                            color_changed = true;
                            temp_col.G++;
                        }
                    }
                    if (input.mouse_now.X >= Exilania.screen_size.X / 2 && input.mouse_now.X < Exilania.screen_size.X / 2 + 15 &&
                       input.mouse_now.Y >= 385 + (choice_defs.Length * 30) && input.mouse_now.Y < 410 + (choice_defs.Length * 30))
                    {
                        if (temp_col.B > 0 && temp_col.R + temp_col.B + temp_col.G > 128)
                        {
                            color_changed = true;
                            temp_col.B--;
                        }
                    }
                    else if (input.mouse_now.X >= Exilania.screen_size.X / 2 + 15 && input.mouse_now.X < Exilania.screen_size.X / 2 + 35 &&
                        input.mouse_now.Y >= 385 + (choice_defs.Length * 30) && input.mouse_now.Y < 410 + (choice_defs.Length * 30))
                    {
                        if (temp_col.B < 255)
                        {
                            color_changed = true;
                            temp_col.B++;
                        }
                    }
                    if (color_changed && temp_col.R + temp_col.B + temp_col.G >= 128)
                    {
                        set_colors[clicked_choice] = temp_col;
                        apply_colors_to_character(char_disp);
                    }
                }
            }
        }

        public void draw_create_chracter(SpriteBatch s, Display d,  Input i)
        {
            s.Draw(d.planet_bkd, new Rectangle((Exilania.screen_size.X / 2) - (1920 / 2), (Exilania.screen_size.Y / 2) - (1080 / 2), 1920, 1080), new Rectangle(0, 0, 1920, 1080), Color.White);
            s.Draw(d.Exilania_title, new Rectangle(Exilania.screen_size.X - 400, (int)((float)Exilania.screen_size.Y - 100), 392, 87), new Rectangle(0, 0, 392, 87), Color.White);
            Vector2 size_text = d.font.MeasureString("New Character");
            d.draw_text(s, d.font, "@05New Character", Exilania.screen_size.X / 2 - (int)size_text.X / 2, 10, Exilania.screen_size.X);

            size_text = d.middle_font.MeasureString(char_disp.name);
            
            char_disp.draw_actor(s, 6f, d);
            int start_y = 200;
            for (int x = 0; x < choice_defs.Length; x++)
            {
                start_y = 220 + x * 30;
                if (x == clicked_choice)
                {
                    d.draw_text(s, d.middle_font, "@05" + choice_defs[x].ToString(), (int)(Exilania.screen_size.X / 2) - 400, start_y, Exilania.screen_size.X);
                }
                else
                    d.draw_text(s, d.middle_font, (cur_choice == x ? "@08" : "@00") + choice_defs[x].ToString(), (int)(Exilania.screen_size.X / 2) - 400, start_y, Exilania.screen_size.X);
            }
            d.draw_text(s, d.middle_font, "@05" + char_disp.name, Exilania.screen_size.X / 2 - 150, start_y + 30, Exilania.screen_size.X);
            d.draw_text(s, d.middle_font, "@05Character Name: ", Exilania.screen_size.X / 2 - 400, start_y + 30, Exilania.screen_size.X);
            d.draw_text(s, d.middle_font, "@05Changing " + choice_defs[clicked_choice], (int)Exilania.screen_size.X / 2 - 200, 290 + (choice_defs.Length * 30), 1000);
            s.DrawString(d.middle_font, "- + R: " + temp_col.R, new Vector2(Exilania.screen_size.X / 2, 320 + (choice_defs.Length * 30)), temp_col);
            s.DrawString(d.middle_font, "- + G: " + temp_col.G, new Vector2(Exilania.screen_size.X / 2, 350 + (choice_defs.Length * 30)), temp_col);
            s.DrawString(d.middle_font, "- + B: " + temp_col.B, new Vector2(Exilania.screen_size.X / 2, 380 + (choice_defs.Length * 30)), temp_col);
            bool c = (i.mouse_now.Y >= Exilania.screen_size.Y - 100 && i.key_input.Length > 3);
            if (!c && i.key_input.Length <= 3)
            {
                size_text = d.middle_font.MeasureString("Name must be at least 4 characters to save. (Just start typing)");
                d.draw_text(s, d.middle_font, "@05Name must be at least 4 characters to save. (Just start typing)", (int)Exilania.screen_size.X / 2 - (int)size_text.X / 2, Exilania.screen_size.Y - 130, 1000);
            }
            size_text = d.middle_font.MeasureString("[Create Character]");
            d.draw_text(s, d.middle_font, (c?"@08":"@00")+"[Create Character]", (int)Exilania.screen_size.X / 2 - (int)size_text.X/2, Exilania.screen_size.Y - 100, 1000);
            
        }

        public void draw_list(SpriteBatch s, Display d)
        {
            s.Draw(d.planet_bkd, new Rectangle((Exilania.screen_size.X / 2) - (1920 / 2), (Exilania.screen_size.Y / 2) - (1080 / 2), 1920, 1080), new Rectangle(0, 0, 1920, 1080), Color.White);
            s.Draw(d.Exilania_title, new Rectangle(Exilania.screen_size.X - 400, (int)((float)Exilania.screen_size.Y  - 100), 392, 87), new Rectangle(0, 0, 392, 87), Color.White);
            Vector2 size_text = d.font.MeasureString("Character Selection");
            d.draw_text(s, d.font, "@05Character Selection", Exilania.screen_size.X / 2 - (int)size_text.X / 2, 10, Exilania.screen_size.X);

            int start_y = 200;
            d.draw_text(s, d.middle_font, (cur_choice==-1?"@08":"@00")+"<<Create New Chracter>>", (int)(Exilania.screen_size.X/2) - 400, start_y, Exilania.screen_size.X);
            for (int x = 0; x < players.Count; x++)
            {
                start_y = 230 + x * 30;
                if(clicked_choice == x)
                    d.draw_text(s, d.middle_font, "@05" + players[x].ToString(), (int)(Exilania.screen_size.X / 2) - 400, start_y, Exilania.screen_size.X);
                else
                d.draw_text(s, d.middle_font, (cur_choice == x ? "@08" : "@00") + players[x].ToString(), (int)(Exilania.screen_size.X / 2) - 400, start_y, Exilania.screen_size.X);
            }

        }
    }
}
