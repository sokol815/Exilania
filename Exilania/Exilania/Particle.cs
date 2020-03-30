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
    public class Particle
    {
        Vector2 world_loc;
        Vector2 inertia;
        int particle_id;
        float sec_run;
        bool enabled;
        bool gravity_affect;
        int bounces;
        public static float slowdown = 1f;
        float sec_delay;
        bool orient;
        float orient_angle;
        Vector2 center;
        bool pass_through;


        public Particle()
        {
            world_loc = new Vector2();
            inertia = new Vector2();
            particle_id = 0;
            sec_run = 0f;
            gravity_affect = true;
            enabled = false;
            bounces = -1;
            orient = false;
            sec_delay = 0f;
            orient_angle = 0;
            center = new Vector2(0, 0);
            pass_through = false;
        }

        public static void step_time(List<Particle> particles, float sec_fraction, World w)
        {
            for (int x = 0; x < particles.Count; x++)
            {
                if (particles[x].enabled)
                {
                    particles[x].sec_run -= sec_fraction;
                    if (particles[x].sec_run < 0 || (!particles[x].pass_through && particles[x].bounces == 0))
                        particles[x].enabled = false;
                    else
                    {
                        particles[x].inertia *= slowdown;
                        particles[x].world_loc += sec_fraction * particles[x].inertia;


                        if (!particles[x].pass_through && (particles[x].world_loc.Y / 24f) > -1 && (particles[x].world_loc.Y / 24f) < w.map.GetLength(1) &&
                         !w.map[(int)(w.wraparound_x((int)particles[x].world_loc.X / 24)), (int)(particles[x].world_loc.Y / 24f)].transparent)
                        {
                            if (particles[x].bounces > 0)
                                particles[x].bounces--;
                            if (particles[x].world_loc.X > w.map.GetLength(0) * 24)
                                particles[x].world_loc.X -= w.map.GetLength(0) * 24;
                            else if (particles[x].world_loc.X < 0)
                                particles[x].world_loc.X += w.map.GetLength(0) * 24;
                        }
                        else if (particles[x].pass_through)
                        {
                            if (particles[x].world_loc.Y < 0 || particles[x].world_loc.Y > w.map.GetLength(1) * 24)
                            {
                                particles[x].enabled = false;
                            }
                            else
                            {
                                if (particles[x].world_loc.X > w.map.GetLength(0) * 24)
                                    particles[x].world_loc.X -= w.map.GetLength(0) * 24;
                                else if (particles[x].world_loc.X < 0)
                                    particles[x].world_loc.X += w.map.GetLength(0) * 24;
                            }
                        }
                          
                          //          particles[x].dx *= -1.0;
                            //        particles[x].loc.X += 1;
                              //      particles[x].sub_loc.X -= 32;
                                //    particles[x].orient_angle = (float)Math.Atan2((float)particles[x].dy, (float)particles[x].dx);
                                  //  if (particles[x].bounces > 0) particles[x].bounces--;
                               
                        if (!particles[x].pass_through && particles[x].bounces == 0)
                        {
                            particles[x].enabled = false;
                        }
                    }
                }
            }
            particles.Sort((x, y) => y.enabled.CompareTo(x.enabled));
        }

        public static void initialize_projectile(World lev, Point start, Point end, Actor attacker)
        {


        }

        public static void initialize_particle(List<Particle> p, int particle_template, float duration, double angle, int start_x, int start_y, int pbounces, bool allow_pass, Display d)
        {
            //angle is passed in degrees, need to convert it to radions
            //angle = Math.PI * (angle / 180.0);
            p.Sort((x, y) => y.enabled.CompareTo(x.enabled));
            int use_id = -1;
            for (int x = 0; x < p.Count; x++)
            {
                if (p[x].enabled == false)
                {
                    use_id = x;
                    x = p.Count + 1;
                }
            }
            if (use_id == -1 && p.Count < 1500)
            {

                p.Add(new Particle());
                use_id = p.Count - 1;
            }
            if (use_id != -1)
            {
                p[use_id].enabled = true;
                p[use_id].particle_id = particle_template;
                p[use_id].sec_run = duration;
                p[use_id].world_loc.X = start_x;
                p[use_id].world_loc.Y = start_y;
                p[use_id].bounces = pbounces;
                p[use_id].orient_angle = (float)angle;
                p[use_id].orient = true;
                p[use_id].pass_through = allow_pass;
                p[use_id].inertia.X = (float)Math.Cos(angle) * Exilania.particle_manager.particles[particle_template].speed;
                p[use_id].inertia.Y = (float)Math.Sin(angle) * Exilania.particle_manager.particles[particle_template].speed;
                p[use_id].center = new Vector2(Exilania.particle_manager.particles[p[use_id].particle_id].image.Width / 2,
                    Exilania.particle_manager.particles[p[use_id].particle_id].image.Height / 2);
                p[use_id].orient_angle = (float)Math.Atan2(p[use_id].inertia.Y, p[use_id].inertia.X);
            }
        }

        public override string ToString()
        {
            return "E: " + enabled.ToString() + " S: " + sec_run.ToString() + " Loc: " + world_loc.ToString();
        }

        public static void draw_particles(List<Particle> p, Display d, SpriteBatch s, World l)
        {
            if (Exilania.gstate == 100)
                for (int x = 0; x < p.Count; x++)
                {
                    if (p[x].enabled && 
                        p[x].world_loc.X > l.top_left.X - 50 && p[x].world_loc.X < l.top_left.X + Exilania.screen_size.X + 50 &&
                        p[x].world_loc.Y > l.top_left.Y - 50 && p[x].world_loc.Y < l.top_left.Y + Exilania.screen_size.Y + 50 &&
                        (int)(p[x].world_loc.Y/24f) > -1 && (int)(p[x].world_loc.Y/24f) < l.map.GetLength(1) &&
                        l.map[(int)(l.wraparound_x((int)p[x].world_loc.X/24)), (int)(p[x].world_loc.Y/24f)].light_level.PackedValue > 0)
                    {
                        //conditions for drawing the sprites...
                                s.Draw(d.sprites,  new Rectangle((int)(p[x].world_loc.X - l.top_left.X),(int)(p[x].world_loc.Y - l.top_left.Y),
                                    Exilania.particle_manager.particles[p[x].particle_id].image.Width * 2, Exilania.particle_manager.particles[p[x].particle_id].image.Height * 2)
                               , Exilania.particle_manager.particles[p[x].particle_id].image, l.map[l.wraparound_x((int)p[x].world_loc.X/24),(int)p[x].world_loc.Y/24].light_level, p[x].orient_angle, p[x].center, SpriteEffects.None, 0);
                           
                    }
                }
        }

    }
}
