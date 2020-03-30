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
    public class PointLight
    {
        /// <summary>
        /// the center about which to orient the images.
        /// </summary>
        public Vector2 location;
        public int r_intensity;
        public int g_intensity;
        public int b_intensity;
        public bool is_char_point;
        public bool is_other_light;
        public int max_intensity;
        public Color use_col;

        public PointLight()
        {

        }

        public PointLight(Vector2 loc, byte r, byte g, byte b, bool pischarpoint, bool pisotherlight)
        {
            r_intensity = r;
            g_intensity = g;
            b_intensity = b;
            max_intensity = World.get_max(new byte[] { r, g, b });
            max_intensity /= 10;
            location = loc;
            is_char_point = pischarpoint;
            is_other_light = pisotherlight;
            use_col = Color.FromNonPremultiplied((byte)((float)r_intensity / (float)max_intensity * 255), (byte)((float)g_intensity / (float)max_intensity * 255), (byte)((float)b_intensity / (float)max_intensity * 255), 255);

                use_col.A = (byte)(r_intensity);
        }

        public void draw(SpriteBatch s, Lighting t, int num)
        {
            int smudge = 0;// (int)(DateTime.Now.Ticks / 10000 % 1500);
            //smudge -= 750;
            //smudge /= 20;
            //smudge = Math.Abs(smudge);
            if (!is_char_point && !is_other_light)
            {
                s.Draw(t.pointlights, new Rectangle((int)location.X - (max_intensity * 21 / Exilania.light_buffer_scaling_factor), 
                    (int)location.Y - (max_intensity * 9 / Exilania.light_buffer_scaling_factor),
                    max_intensity * 42 / Exilania.light_buffer_scaling_factor, max_intensity * 18 / Exilania.light_buffer_scaling_factor), Lighting.whitepoint, use_col);
                return;
            }
            else
            {
                s.Draw(t.pointlights, new Rectangle((int)location.X - (24 * max_intensity / Exilania.light_buffer_scaling_factor) + smudge, (int)location.Y - (24 * max_intensity / Exilania.light_buffer_scaling_factor) + smudge,
                    48 * max_intensity / Exilania.light_buffer_scaling_factor - (smudge * 2), 48 * max_intensity / Exilania.light_buffer_scaling_factor - (smudge * 2)), Lighting.charpoint, use_col);
                return;
            }
        }
    }

    public class BeamLight : PointLight
    {
        public float angle;

        public BeamLight()
        {

        }

        public BeamLight(Vector2 loc, byte r, byte g, byte b, float pangle)
        {
            r_intensity = r;
            g_intensity = g;
            b_intensity = b;
            location = loc;
            angle = pangle;
            max_intensity = World.get_max(new byte[] { r, g, b });
            use_col = Color.FromNonPremultiplied((byte)((float)r_intensity / (float)max_intensity * 255), (byte)((float)g_intensity / (float)max_intensity * 255), (byte)((float)b_intensity / (float)max_intensity * 255), 255);

            use_col.A = (byte)(r_intensity * 25 + 5);
        }

        public void draw_beam(SpriteBatch s, Lighting t)
        {
            s.Draw(t.pointlights, new Rectangle((int)location.X, (int)location.Y, Lighting.flashpoint.Width, max_intensity * 12),
                Lighting.flashpoint, use_col, angle, new Vector2(Lighting.flashpoint.Width / 2, Lighting.flashpoint.Height), SpriteEffects.None, 0f);
        }
    }

    public class StaticLight : PointLight
    {
        Rectangle location_rect;
        Color calculated;
        public StaticLight(Point block_loc, int r, int g, int b)
        {
            location_rect = new Rectangle(block_loc.X, block_loc.Y, 6, 6);
            r_intensity = r;
            g_intensity = g;
            b_intensity = b;
            calculated = Color.FromNonPremultiplied(r * 25 + 5, g * 25 + 5, b * 25 + 5, 255);
        }


        public void draw_static(SpriteBatch s, Lighting t, Display d)
        {
            //767, 243 is a white spot.
            s.Draw(d.sprites,location_rect, new Rectangle(767, 243, 1, 1),calculated );
        }
    }

    public class Lighting
    {
        public Texture2D pointlights;
        //public static int light_size = 512;
        //public static Rectangle redpoint = new Rectangle(0, 0, light_size, light_size);
        public static Rectangle flashpoint = new Rectangle(190, 0, 64,64);
        public static Rectangle charpoint = new Rectangle(126, 0, 64, 64);
        public static Rectangle whitepoint = new Rectangle(0, 0, 126, 54);


        public RenderTarget2D lightsTarget;
        RenderTarget2D mainTarget;
        Effect lightshader;

        public List<PointLight> point_lights;
        public List<BeamLight> beam_lights;
        public List<StaticLight> static_lights;
        
        public Lighting()
        {
         
        }

        public Lighting(ContentManager content, GraphicsDevice g)
        {
            pointlights = content.Load<Texture2D>("pointlights");
            lightshader = content.Load<Effect>("LightShader");
            point_lights = new List<PointLight>();
            beam_lights = new List<BeamLight>();
            static_lights = new List<StaticLight>();
            var pp = g.PresentationParameters;
            lightsTarget = new RenderTarget2D(g, pp.BackBufferWidth / Exilania.light_buffer_scaling_factor, pp.BackBufferHeight / Exilania.light_buffer_scaling_factor);
            mainTarget = new RenderTarget2D(g, pp.BackBufferWidth, pp.BackBufferHeight);
            //lightsTarget = new RenderTarget2D(g, 2048, 2048);
            //mainTarget = new RenderTarget2D(g, 2048, 2048);
            
        }

        public void resize_graphicsdevice(GraphicsDevice g)
        {
            var pp = g.PresentationParameters;
            lightsTarget = new RenderTarget2D(g, pp.BackBufferWidth / Exilania.light_buffer_scaling_factor, pp.BackBufferHeight / Exilania.light_buffer_scaling_factor);
            mainTarget = new RenderTarget2D(g, pp.BackBufferWidth, pp.BackBufferHeight);
            g.SamplerStates[1] = Exilania._clampTextureAddressMode;
        }

        public void update_lighting(World w)
        {
            
            point_lights.Clear();
            beam_lights.Clear();
            static_lights.Clear();
            Vector2 center = new Vector2(w.players[Exilania.game_my_user_id].avatar.world_loc.X, w.players[Exilania.game_my_user_id].avatar.world_loc.Y);

            Vector2 tl_use = new Vector2(center.X - (Exilania.screen_size.X / 2), center.Y - (Exilania.screen_size.Y / 2));
            if (tl_use.Y < 0)
                tl_use.Y = 0;
            else if (tl_use.Y > w.map.GetLength(1) * 24 - (Exilania.screen_size.Y))
            {
                tl_use.Y = w.map.GetLength(1) * 24 - Exilania.screen_size.Y;
            }
            Point offset = new Point((int)tl_use.X / 24, (int)tl_use.Y / 24);
            int x_offset = w.get_leftover_pixels((int)tl_use.X) / 4;
            int y_offset = w.get_leftover_pixels((int)tl_use.Y) / 4;
            float day_only_portion = w.world_time % w.day_length / w.day_length;
            byte[] night_light = new byte[] { 4, 4, 4 };
            w.calc_sky_color = World.day_sky;
            if (1f - day_only_portion > w.night_percent_length)
            {
                night_light = new byte[] { 10, 10, 10 };
            }
            else
            {
                float light_intensity = 1f - day_only_portion; //get elapsed night portion

                light_intensity = w.night_percent_length - light_intensity; //get remaining night portion
                light_intensity /= w.night_percent_length; //get percentage night elapsed.

                if (light_intensity <= .10f)
                {
                    //decreasing light dusk
                    light_intensity = .10f - light_intensity;
                    light_intensity /= .10f;
                }
                else if (light_intensity >= .90f)
                {
                    //increasing light dawn!
                    light_intensity -= .90f;
                    light_intensity /= .10f;
                }
                else
                { //middle of night... low light.
                    light_intensity = 0f;
                }
                w.calc_sky_color.R = (byte)(light_intensity * (float)World.day_sky.R);
                w.calc_sky_color.G = (byte)(light_intensity * (float)World.day_sky.G);
                w.calc_sky_color.B = (byte)(light_intensity * (float)World.day_sky.B);


                light_intensity *= 7f;
                night_light = new byte[] { (byte)(3 + Math.Round(light_intensity, 0)), (byte)(3 + Math.Round(light_intensity, 0)), (byte)(3 + Math.Round(light_intensity, 0)) };
            }
            PointLight temp = new PointLight();
            StaticLight temp2;
            for (int x = -10; x < Exilania.screen_size.X / 24 + 20; x++)
            {
                for (int y = -10; y < Exilania.screen_size.Y / 24 + 20; y++)
                {
                    if (w.map[w.wraparound_x(x + offset.X), offset.Y + y].light_source != null)
                    {
                        //luminesce just like you would the ground, but according to this cell's properties.
                        temp = new PointLight(new Vector2(x * 6 + 3 - x_offset, y * 6 + 3 - y_offset),
                            w.map[w.wraparound_x(x + offset.X), offset.Y + y].light_source[0],
                            w.map[w.wraparound_x(x + offset.X), offset.Y + y].light_source[1],
                            w.map[w.wraparound_x(x + offset.X), offset.Y + y].light_source[2], false, true);
                        point_lights.Add(temp);

                    }
                    if (w.map[w.wraparound_x(x + offset.X), offset.Y + y].transparent && w.map[w.wraparound_x(x + offset.X), offset.Y + y].bkd_transparent)
                    {
                        //give it night luminescence (single, all by itself)
                        temp2 = new StaticLight(new Point(x * 6 - x_offset, y * 6 - y_offset), night_light[0], night_light[1], night_light[2]);
                        static_lights.Add(temp2);
                    }
                    if (w.map[w.wraparound_x(x + offset.X), offset.Y + y].transparent && w.map[w.wraparound_x(x + offset.X), offset.Y + y].bkd_transparent &&
                        w.has_non_transparent_neighbors(new Point(x + offset.X, offset.Y + y)))
                    {
                        if (night_light[0] > 4)
                        {
                            //luminesce the ground because it is by it!
                            temp = new PointLight(new Vector2(x * 6 + 3 - x_offset, y * 6 + 3 - y_offset), night_light[0], night_light[1], night_light[2], false,false);
                            point_lights.Add(temp);
                        }
                    }
                }
            }
            beam_lights.Add(new BeamLight(w.players[Exilania.game_my_user_id].avatar.screen_loc/ Exilania.light_buffer_scaling_factor, (byte)10, (byte)10, (byte)10, (float)w.players[Exilania.game_my_user_id].avatar.mouse_angle + (float)Math.PI/2f));
            for (int x = 0; x < w.players.Count; x++)
            {
                if (w.players[x].avatar.screen_loc.X > -100 && w.players[x].avatar.screen_loc.X < Exilania.screen_size.X + 100 &&
                w.players[x].avatar.screen_loc.Y > -100 && w.players[x].avatar.screen_loc.Y < Exilania.screen_size.Y + 100)
                {
                    temp = new PointLight(new Vector2(w.players[x].avatar.screen_loc.X / Exilania.light_buffer_scaling_factor, w.players[x].avatar.screen_loc.Y / Exilania.light_buffer_scaling_factor),
                        w.players[x].avatar.light_source[0], w.players[x].avatar.light_source[1], w.players[x].avatar.light_source[2], false,true);
                    point_lights.Add(temp);
                }
            }
            for (int x = 0; x < w.npcs.Count; x++)
            {
                if (w.npcs[x].screen_loc.X > -100 && w.npcs[x].screen_loc.X < Exilania.screen_size.X + 100 &&
                w.npcs[x].screen_loc.Y > -100 && w.npcs[x].screen_loc.Y < Exilania.screen_size.Y + 100)
                {
                    temp = new PointLight(new Vector2(w.npcs[x].screen_loc.X / Exilania.light_buffer_scaling_factor, w.npcs[x].screen_loc.Y / Exilania.light_buffer_scaling_factor),
                        w.npcs[x].light_source[0], w.npcs[x].light_source[1], w.npcs[x].light_source[2], false,true);
                    point_lights.Add(temp);
                }
            }
        }

        public void draw_scene(SpriteBatch s, Display d, World w, Input i, GraphicsDevice g)
        {
            //render lights
            s.End();
            g.SamplerStates[1] = Exilania._clampTextureAddressMode;
            //if(lightsTarget == null)
              //  resize_graphicsdevice(g);
            g.SetRenderTarget(lightsTarget);
            g.Clear(Color.FromNonPremultiplied(16,16,16,255));
            s.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            for (int x = 0; x < point_lights.Count; x++)
            {
                if(!point_lights[x].is_char_point && !point_lights[x].is_other_light)
                    point_lights[x].draw(s, this, x);
            }
            for (int x = 0; x < static_lights.Count; x++)
            {
                static_lights[x].draw_static(s, this, d);
            }
            for (int x = 0; x < point_lights.Count; x++)
            {
                if (point_lights[x].is_char_point)
                    point_lights[x].draw(s, this, x);
            }
            for (int x = 0; x < beam_lights.Count; x++)
            {
                    beam_lights[x].draw_beam(s, this);
            }

            for (int x = 0; x < point_lights.Count; x++)
            {
                if (point_lights[x].is_other_light)
                    point_lights[x].draw(s, this, x);
            }
            
            s.End();
            
            //render scene
            g.SetRenderTarget(mainTarget);
            g.Clear(Color.Black);
            s.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            w.draw_world(Exilania.game_my_user_id, s, d, i);
            s.End();

            // render scene using lights, resulting in hidden stuff.
            if (Exilania.take_screen_shot)
            {
                Exilania.renderTarget = new RenderTarget2D(g, Exilania.screen_size.X, Exilania.screen_size.Y);
                g.SetRenderTarget(Exilania.renderTarget);
            }
            else
                g.SetRenderTarget(null);
            g.Clear(Color.CornflowerBlue);
            s.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            lightshader.Parameters["lightMask"].SetValue(lightsTarget);
            lightshader.CurrentTechnique.Passes[0].Apply();
            s.Draw(mainTarget, Vector2.Zero, Color.White);
            s.End();
            s.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
        }

    }
}
