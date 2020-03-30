using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public enum TravelType
    {
        Straight,
        Boomerang,
        Bouncy,
        Piercing,
        Graceful,
        Haphazard
    }
    public class ParticleTemplate
    {
        public string name;
        public bool gravity_affected;
        public Rectangle image;
        public TravelType travel_path;
        public float speed;

        public ParticleTemplate()
        {
            name = "";
            gravity_affected = false;
            image = new Rectangle(0, 0, 6, 6);
            travel_path = TravelType.Straight;
            speed = 300f;
        }
    }
    public class ParticleManager
    {
        public List<ParticleTemplate> particles;

        public ParticleManager()
        {
            particles = new List<ParticleTemplate>();
            if (System.IO.File.Exists(@"particles.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"particles.txt");
                string line = "";
                ParticleTemplate p = new ParticleTemplate();
                bool cont = true;
                while (cont)
                {
                    line = r.ReadLine();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        //skip this line
                    }
                    else
                    {
                        string[] items = line.Split(':');
                        switch (items[0].ToLower())
                        {
                            case "name":
                                if (p.name == "")
                                {
                                    p.name = items[1].Trim();
                                }
                                else
                                {
                                    particles.Add(p);
                                    Exilania.text_stream.WriteLine("Particle Template '" + p.name + "' Loaded.");
                                    p = new ParticleTemplate();
                                    p.name = items[1].Trim();
                                }
                                break;
                            case "pic": 
                                string[] vals = Acc.script_remove_outer_parentheses(items[1]).Split(',');
                                p.image = new Rectangle(int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]), int.Parse(vals[3]));
                                break;
                            case "travel":
                                switch (items[1].ToLower())
                                {
                                    case "straight":
                                        p.travel_path = TravelType.Straight;
                                        break;
                                    case "boomerang":
                                        p.travel_path = TravelType.Boomerang;
                                        break;
                                    case "piercing":
                                        p.travel_path = TravelType.Piercing;
                                        break;
                                    case "bouncing":
                                        p.travel_path = TravelType.Bouncy;
                                        break;
                                    case "graceful":
                                        p.travel_path = TravelType.Graceful;
                                        break;
                                    case "haphazard":
                                        p.travel_path = TravelType.Haphazard;
                                        break;
                                }
                                break;
                            case "gravity":
                                p.gravity_affected = bool.Parse(items[1]);
                                break;
                            case "speed":
                                p.speed = float.Parse(items[1]);
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        particles.Add(p);
                        Exilania.text_stream.WriteLine("Particle Template '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
            }
            else
            {
                Exilania.text_stream.Write("ERROR! No furniture.txt file.");
            }
        }

        public void initialize_n_particles_in_rectangle(Rectangle area, int num_particles, string particle_group, World w)
        {
            int[] part_use = get_particles_containing_name(particle_group);
            if (part_use.Length == 0)
                return;

            int cur_choice = -1;
            for (int i = 0; i < num_particles; i++)
            {
                cur_choice = part_use[Exilania.rand.Next(0, part_use.Length)];
                Particle.initialize_particle(w.particles, cur_choice, 2f,
                    (float)Exilania.rand.Next(0, 10000) / 10000f * (float)Math.PI * 2f, 
                    Exilania.rand.Next(area.Left, area.Right), 
                    Exilania.rand.Next(area.Top, area.Bottom), 0, true, Exilania.display);
            }
        }

        public int[] get_particles_containing_name(string name)
        {
            List<int> ret_particles = new List<int>();
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].name.ToLower().Contains(name.ToLower()))
                {
                    ret_particles.Add(i);
                }
            }
            int[] t = new int[ret_particles.Count];
            ret_particles.CopyTo(t);
            return t;
        }

        public int get_particle_id_by_name(string name)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
