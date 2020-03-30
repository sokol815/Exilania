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
    class MainMenu
    {
        string[] options;
        int cur_option;

        public MainMenu()
        {
            options = new string[6];
            options[0] = "SinglePlayer";
            options[1] = "MultiPlayer - Host";
            options[2] = "MultiPlayer - Join";
            options[3] = "Configuration";
            options[4] = "About";
            options[5] = "Quit";
            cur_option = 0;
        }


        public void update_menu(Input input, Exilania e)
        {
            int prev_option = cur_option;
            if (input.mouse_cur_spot.X < Exilania.screen_size.X / 2 - 200 || input.mouse_cur_spot.X > Exilania.screen_size.X / 2 + 100)
            {
                cur_option = -1;
                return;
            }
            if (input.mouse_now.Y < Exilania.screen_size.Y * (.30f))
            {
                cur_option = -1;
            }
            else if (input.mouse_now.Y > Exilania.screen_size.Y * (.30f + (options.Length * .05)))
            {
                cur_option = -1;
            }
            else
            {
                float loc = (float)input.mouse_now.Y / (float)Exilania.screen_size.Y;
                loc -= .285f;
                cur_option = (int)(loc * 100f) / 5;
            }
            if (cur_option != prev_option)
            {
                Exilania.sounds.hit_block.Play(Exilania.sounds.master_volume, 0f, 0f);
            }
            if (input.left_clicked && input.mouse_now.X > -1 && input.mouse_now.Y > -1 && 
                input.mouse_now.X <= Exilania.screen_size.X && input.mouse_now.Y <= Exilania.screen_size.Y && e.IsActive)
            {
                Exilania.sounds.finish_hit_block.Play(Exilania.sounds.master_volume,0f,0f);
                input.left_clicked = false;
                switch (cur_option)
                {
                    case 0: //singleplayer
                        e.initialize_game(0);
                        break;
                    case 1: //multiplayer host
                        e.initialize_game(1);
                        break;
                    case 2: //multiplayer join
                        e.initialize_game(2);
                        break;
                    case 5: //quit game
                        e.Exit();
                        break;
                }
            }
        }

        public void draw_menu(SpriteBatch s, Display d)
        {
            string add_beginning = "@00";
            s.Draw(d.planet_bkd, new Rectangle((Exilania.screen_size.X / 2) - (1920 / 2), (Exilania.screen_size.Y / 2) - (1080 / 2), 1920, 1080),new Rectangle(0,0,1920,1080), Color.White);
            Vector2 size_text = d.font.MeasureString("Main Menu");
            d.draw_text(s, d.font, "@33Main Menu", Exilania.screen_size.X / 2 - (int)size_text.X / 2, 10, Exilania.screen_size.X);
            s.Draw(d.Exilania_title, new Rectangle(Exilania.screen_size.X - 400, (int)((float)Exilania.screen_size.Y - 100), 392, 87), new Rectangle(0, 0, 392, 87), Color.White);
            for (int x = 0; x < options.Length; x++)
            {
                if (x == cur_option)
                    add_beginning = "@08";
                else
                    add_beginning = "@00";
                    d.draw_text(s, d.middle_font, add_beginning + options[x], Exilania.screen_size.X / 2 - 166, (int)((float)Exilania.screen_size.Y * (.30f + ((float)x * .05f))), Exilania.screen_size.X / 2);
            }
        }
    }
}
