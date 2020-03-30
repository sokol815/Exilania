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
    public class NightSky
    {
        static Rectangle[] phase_images;
        public int cur_day_num;
        public Point screen_loc;
        public bool is_blood_moon;
        public float star_rotation;
        public Rectangle star_frame = new Rectangle(0, 992, 1024, 1024);
        float cur_day_percent = 0f;
        float percent_night = 0f;



        public NightSky()
        {
            phase_images = new Rectangle[14];
            for (int i = 0; i < 14; i++)
            {
                phase_images[i] = new Rectangle((13 - i) * 32,2016,32,32);
            }
            star_rotation = 0f;
        }

        public int get_moon_height(double percent_of_night)
        {
           /* if (percent_of_night < .5f)
            {
                percent_of_night *= .9f;
            }
            else
            {
                percent_of_night *= 1.1f;
            }*/
            percent_of_night -= .5;
            //y=-x^2+.85
            percent_of_night *= (percent_of_night);
            percent_of_night *= 3;
            return 100 + (int)(percent_of_night * Exilania.screen_size.Y);
        }

        public void update_night(int cur_day, float cur_time, float night_length, float day_length)
        {
            cur_day_percent = cur_time % day_length / day_length;
            //if (cur_day_percent > 1f - night_length) //nighttime is the last part of a single integer day... check to see if we are in that part.
           // {
                percent_night = (cur_day_percent - (1f - night_length)) / night_length;
                star_rotation = percent_night * (float)Math.PI;
                screen_loc = new Point((int)(percent_night * (double)Exilania.screen_size.X), get_moon_height(percent_night));
                cur_day_num = cur_day;
            //}
        }

        public void draw_night(SpriteBatch s, Display d)
        {
            int star_radius = (int)Acc.get_distance(0, 0, Exilania.screen_size.X / 2, Exilania.screen_size.Y) + 15;
            Color draw_stars = Color.White;
            if (percent_night < .1f)
            {
                draw_stars.A = (byte)(255f * 10f * percent_night);
                draw_stars.R = draw_stars.A;
                draw_stars.B = draw_stars.A;
                draw_stars.G = draw_stars.A;
            }
            else if (percent_night > .9f)
            {
                draw_stars.A = (byte)( 255f * 10f * (1f-percent_night));
                draw_stars.R = draw_stars.A;
                draw_stars.B = draw_stars.A;
                draw_stars.G = draw_stars.A;
            }
            s.Draw(d.backgrounds, new Rectangle(Exilania.screen_size.X / 2, Exilania.screen_size.Y, star_radius * 2, star_radius * 2), star_frame, draw_stars,
                (star_rotation * .25f) + (float)(Math.PI * 2f / 3f), new Vector2(star_frame.Width / 2, star_frame.Height / 2), SpriteEffects.None, 0);
            s.Draw(d.backgrounds, new Rectangle(Exilania.screen_size.X / 2, Exilania.screen_size.Y, star_radius * 2, star_radius * 2), star_frame, draw_stars,
                (star_rotation * .5f) + (float)(Math.PI * 4f / 3f), new Vector2(star_frame.Width / 2, star_frame.Height / 2), SpriteEffects.None, 0);
            s.Draw(d.backgrounds, new Rectangle(Exilania.screen_size.X / 2, Exilania.screen_size.Y, star_radius * 2, star_radius * 2), star_frame, draw_stars,
                star_rotation, new Vector2(star_frame.Width / 2, star_frame.Height / 2), SpriteEffects.None, 0);

            int mag = 4;
            s.Draw(d.backgrounds, new Rectangle(screen_loc.X - (16 * mag), screen_loc.Y - (16 * mag), mag * 32, mag * 32), phase_images[13], draw_stars);
            s.Draw(d.backgrounds, new Rectangle(screen_loc.X - (16 * mag), screen_loc.Y - (16 * mag), mag * 32, mag * 32), phase_images[cur_day_num % 14], Color.White);
        }
    }
    public class BiomeBackground
    {
        public string biome_name;
        public Rectangle image;
        public float x_offset;
        public float x_right;
        public float y_offset;
        public float magnification;
        public Color base_color;
        public Color draw_color;
        public float fade;

        public BiomeBackground(string name, Rectangle rect, float init_x, float init_y, float mag, Color pbase_color)
        {
            biome_name = name;
            image = rect;
            x_offset = init_x;
            y_offset = init_y;
            magnification = mag;
            base_color = pbase_color;
            draw_color = Color.White;
        }

        public void update_position(Vector2 left_pos,World w)
        {
            float light_intensity = w.light_intensity / 100f;
            if (light_intensity < 1)
                light_intensity *= 1f / .7f;
            if (light_intensity < .15f)
                light_intensity = .15f;
            draw_color.R = (byte)((float)base_color.R * light_intensity);
            draw_color.G = (byte)((float)base_color.G * light_intensity);
            draw_color.B = (byte)((float)base_color.B * light_intensity);
            draw_color.A = 255;
            x_offset = (((left_pos.X + Exilania.screen_size.X / 2) / (float)w.map.GetLength(0)) / 24f);
            //this sets it so the bottom of the image is right against the bottom of the screen.
            //our play room is then image.Height * magnification
            //at ground level, it should be 3/4 the way up
            //at backing change level it should be all the way up
            //at high levels, it shouold be 1/2 the way up. (ground_level/2)
            float top_spot = (w.top_left.Y)/24f;
            float percent_show = 0f;
            float per = 0f;
            if (top_spot < w.world_height_points[(int)HeightVals.SPACE])
            {
                percent_show = .5f;
                per = top_spot / (float)w.world_height_points[(int)HeightVals.SPACE];
                draw_color.A = (byte)(per * 255f);
                draw_color.R = Math.Min(draw_color.A, draw_color.R);
                draw_color.G = Math.Min(draw_color.A, draw_color.G);
                draw_color.B = Math.Min(draw_color.A, draw_color.B);
            }
            else if (top_spot >= w.world_height_points[(int)HeightVals.SPACE] && top_spot <= w.world_height_points[(int)HeightVals.SEALEVEL])
            { //between space and sealevel... mountains are 3/4 to 1/2 high
                percent_show = .50f + (.50f * ((float)top_spot - (float)w.world_height_points[(int)HeightVals.SPACE]) / 
                    ((float)w.world_height_points[(int)HeightVals.SEALEVEL] - (float)w.world_height_points[(int)HeightVals.SPACE]));

            }
            else if (top_spot <= w.world_height_points[(int)HeightVals.UNDERGROUNDONE])
            {
                percent_show = 1f;
                float fast = ((float)w.world_height_points[(int)HeightVals.UNDERGROUNDONE] - (float)w.world_height_points[(int)HeightVals.SEALEVEL]) * .25f;
                per = (((float)(top_spot - w.world_height_points[(int)HeightVals.SEALEVEL]) - fast ) / (fast));
                if (per >= -.01f)
                {
                    percent_show = 0f;
                    per = 1f;
                }
                draw_color.A = (byte)((1f - per) * 255f);
                draw_color.R = Math.Min(draw_color.A, draw_color.R);
                draw_color.G = Math.Min(draw_color.A, draw_color.G);
                draw_color.B = Math.Min(draw_color.A, draw_color.B);
            }
            y_offset = Exilania.screen_size.Y - (float)(image.Height * magnification * percent_show);
           
        }

        public void draw(SpriteBatch s)
        {
           

            Rectangle screen_loc = new Rectangle(0, (int)y_offset, Exilania.screen_size.X, (int)((float)image.Height * magnification));
            Rectangle source_loc = new Rectangle((int)image.X + (int)(x_offset * image.Width), image.Y, (int)((float)Exilania.screen_size.X/magnification), image.Height);
            if (source_loc.X + source_loc.Width > image.Right)
            {
                source_loc.Width = image.Right - (source_loc.X);
                screen_loc.Width = (int)((float)source_loc.Width * magnification);
                //also draw a second one
                Rectangle sec_screen = new Rectangle(screen_loc.Width, (int)y_offset, Exilania.screen_size.X - screen_loc.Width, (int)((float)image.Height * magnification));
                Rectangle sec_source = new Rectangle(0, image.Y, (int)((float)(Exilania.screen_size.X - screen_loc.Width) / magnification), image.Height);
                s.Draw(Exilania.display.backgrounds, sec_screen, sec_source, draw_color);
            }
            s.Draw(Exilania.display.backgrounds, screen_loc, source_loc, draw_color);
        }
    }
    public class Background
    {
        BiomeBackground forest = new BiomeBackground("Forest",new Rectangle(0,0,1920,300),.5f,.5f,3.5f, Color.FromNonPremultiplied(229,229,205,111));

        public Background()
        {

        }

        public void update(Vector2 world_pos, World w)
        {
            forest.update_position(world_pos, w);
        }

        public void draw_background(SpriteBatch s)
        {
            forest.draw(s);
        }
    }
}
