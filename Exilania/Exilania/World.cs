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
using System.IO.Compression;
using System.Collections;

namespace Exilania
{
    /// <summary>
    /// represents 1 box in the world. this is 8 bytes in size... not recommended to put more than 150 boxes in a single packet.
    /// </summary>
    public class Voxel
    {
        public static Color bkd_mult = Color.FromNonPremultiplied(192, 192, 192, 255);
        /// <summary>
        /// this denotes the type of block:
        /// -1 = none;
        /// 0 = bricks;
        /// 1 = dirt;
        /// 2 = stone/ore;
        /// 3 = wood;
        /// 4 = Misc/Furniture
        /// 5 = Liquids
        /// </summary>
        public sbyte fgd_block_type;
        /// <summary>
        /// corrsponds to index number of block_types
        /// </summary>
        public sbyte fgd_block; //pass over network
        public int furniture_index;
        public int plant_index;
        /// <summary>
        /// the determined connectivity model for this block... which walls are the same..
        /// </summary>
        public byte fgd_ver;
        public sbyte bkd_block; //pass over network
        public byte bkd_ver;
        /// <summary>
        /// byte array... number of squares lit from this block in r, g, b.
        /// </summary>
        public byte[] light_source;
        public Color light_level = Color.Black;

        public bool passable;
        public bool transparent;
        public bool bkd_transparent;
        public bool djikstra_node;

        public byte liquid_level;
        public byte liquid_id;
        public int liquid_cell_id;

        public Voxel()
        {
            init_voxel();
        }

        public Voxel(Voxel t)
        {
            init_voxel();
            bkd_block = t.bkd_block;
            fgd_block = t.fgd_block;
            liquid_level = t.liquid_level;
            liquid_id = t.liquid_id;
            set_voxel();
        }

        public Voxel(Lidgren.Network.NetIncomingMessage i)
        {
            init_voxel();

            bkd_block = i.ReadSByte();
            fgd_block = i.ReadSByte();
            liquid_level = i.ReadByte();
            liquid_id = i.ReadByte();
            //need to call set_voxel() after the data has been sanitized.
            
        }

        public Voxel(System.IO.BinaryReader i)
        {
            init_voxel();

            bkd_block = i.ReadSByte();
            fgd_block = i.ReadSByte();
            liquid_level = i.ReadByte();
            liquid_id = i.ReadByte();
            //need to call set_voxel() after the data has been sanitized.
        }

        public void set_voxel()
        {
            light_level = Color.Black;
            if (fgd_block != -1)
            {
                light_source = Exilania.block_types.blocks[fgd_block].light_source;
                passable = Exilania.block_types.blocks[fgd_block].passable;
                transparent = Exilania.block_types.blocks[fgd_block].transparent;
                fgd_block_type = Exilania.block_types.blocks[fgd_block].block_group;
            }
            else
            {
                passable = true;
                transparent = true;
            }
            if (bkd_block != -1)
                bkd_transparent = Exilania.block_types.blocks[bkd_block].bkd_transparent;
            else
                bkd_transparent = true;
        }

        public void send_voxel(Lidgren.Network.NetOutgoingMessage o)
        {
            o.Write(bkd_block);
            o.Write(fgd_block);
            o.Write(liquid_level);
            o.Write(liquid_id);
        }

        public void write_voxel(System.IO.BinaryWriter w)
        {
            w.Write(bkd_block);
            w.Write(fgd_block);
            w.Write(liquid_level);
            w.Write(liquid_id);
        }


        public void init_voxel()
        {
            fgd_ver = 0;
            bkd_ver = 16;
            bkd_block = -1;
            light_source = null;
            light_level = Color.Black;
            passable = true;
            transparent = true;
            bkd_transparent = true;
            djikstra_node = false;
            fgd_block = -1;
            fgd_block_type = -1;
            furniture_index = -1;
            plant_index = -1;
            liquid_cell_id = -1;
            liquid_id = 0;
            liquid_level = 0;
        }

        public void draw_bkd(SpriteBatch s, Display d, int x_offset, int y_offset, Point screen_loc)
        {
            if (Exilania.settings.use_hardware_lighting)
                s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), 
                    d.frames[Exilania.block_types.blocks[bkd_block].image_pointers[bkd_ver]], Color.White);
            else
            {
                if (!Exilania.block_types.blocks[bkd_block].bkd_transparent)
                    if(!Exilania.block_types.blocks[bkd_block].lighter_background)
                        bkd_mult = Color.FromNonPremultiplied((int)(.3f * light_level.R), (int)(.3f * light_level.G), (int)(.3f * light_level.B), 255);
                    else
                        bkd_mult = Color.FromNonPremultiplied((int)(.7f * light_level.R), (int)(.7f * light_level.G), (int)(.7f * light_level.B), 255);
                else
                {
                    bkd_mult = light_level;
                    bkd_mult.A = 178;
                }
                s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), 
                    d.frames[Exilania.block_types.blocks[bkd_block].image_pointers[bkd_ver]], bkd_mult);
            }
        }

        public void draw_fgd(SpriteBatch s, Display d, int x_offset, int y_offset, Point screen_loc)
        {
            if (Exilania.settings.use_hardware_lighting)
            {
                s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), d.frames[Exilania.block_types.blocks[fgd_block].image_pointers[fgd_ver]], Color.White);
            }
            else
            {
                if (liquid_id != 0 && liquid_level > 2)
                {
                    s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), 
                        d.frames[Exilania.block_types.blocks[World.liquid_blocks[liquid_id]].image_pointers[Math.Min(liquid_level / 9,11)]], light_level);
                }
                if(fgd_block_type != 5 && fgd_block_type != -1)
                    s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), d.frames[Exilania.block_types.blocks[fgd_block].image_pointers[fgd_ver]], light_level);
            }
        }

        public void draw_furniture(SpriteBatch s, Display d, int x_offset, int y_offset, Point screen_loc, List<Furniture> f, Point loc, World w)
        {
            if (furniture_index != -1)
            {
                //need to figure out what image to draw.  take index, refernece back to furniture list, get the frame back from the furniture piece itself by pasing it the x,y coordinate of this frame
                s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), d.frames[f[furniture_index].get_frame(loc,w)], light_level);
            }
        }

        public void draw_plant(SpriteBatch s, Display d, int x_offset, int y_offset, Point screen_loc, List<Plant> p, Point loc, World w)
        {
            if (plant_index != -1)
            {
                s.Draw(d.sprites, new Rectangle(screen_loc.X * 24 - x_offset, screen_loc.Y * 24 - y_offset, 24, 24), d.frames[p[plant_index].get_frame(loc,w)], light_level);
            }
        }

        public override string ToString()
        {
            return light_level.ToString();
            //return "FGD_TYPE = " + fgd_block_type + "; FGD_BLOCK = " + fgd_block + "; BKD = " + bkd_block + " Lighting: " + light_level.ToString();
            //return "Liquid ID: " + liquid_id + " AMT: " + liquid_level + " cell id: " + liquid_cell_id;
        }

        public string to_file_string()
        {
            return "{" + bkd_block + "," + fgd_block + "," + liquid_level + "," + liquid_id + "}";
        }
    }
    public enum HeightVals
    {
        SPACE = 0,
        SEALEVEL = 1,
        UNDERGROUNDONE = 2,
        UNDERGROUNDTWO = 3,
        CORE = 4
    }
    public class World
    {
        public static int num_bytes_written = 0;
        public int[] world_height_points = new int[5];
        public static int[] liquid_blocks = new int[] { 0,0,0};
        public sbyte window_block_id = Exilania.block_types.get_block_by_name("Glass");
        public sbyte grass_block_id = Exilania.block_types.get_block_by_name("Grass");
        public sbyte sand_block_id = Exilania.block_types.get_block_by_name("Sand");
        public sbyte water_block_id = Exilania.block_types.get_block_by_name("Water");
        public string name = "Default World 1";
        public int world_number;
        public int seed_number;
        public int unique_time_id;
        public static int num_light_rays = 60;
        public static float ray_inc = (float)Math.PI * 2 / num_light_rays;
        public static Color day_sky = Color.FromNonPremultiplied(101, 198, 232, 255);
        public Color calc_sky_color = day_sky;
        public Voxel[,] map;
        public Chunk[,] world_chunker;
        public List<Furniture> furniture;
        public List<Plant> plants;
        public List<Player> players;
        public List<Actor> npcs;
        public List<ItemChest> chests;
        public Background background = new Background();
        //this includes all days on the planet... always progressing, never reversing!
        public float world_time = 0;
        public float day_only_portion = 0f;
        public float day_length = 20f;
        public float night_percent_length = .40f;
        public int atmosphere_start = 100;
        //how much the dropping pull can be increased per second (in pixels)
        //12 pixels = 1 foot (1 pixel = 1 inch)
        public float gravity_constant = 1200f;
        public float world_gravity = 1f;
        public float fall_speed = 1.5f;
        public Vector2 top_left = new Vector2();
        public List<Particle> bullets;
        public List<Particle> particles;
        public List<Point> grass;
        public List<Point> areas_checked;
        public HashSet<Point> flowing_sand;
        public HashSet<Point> flowing_overflow;
        public MiniMap minimap;
        public NightSky night;
        /// <summary>
        /// this is turned to true when generating the world to avoid countless connectivity calls... false when in the actual game.
        /// </summary>
        public bool disable_connectivity = false;

        public static int[] xes = new int[] { -1, 0, 1, 0 };
        public static int[] yes = new int[] { 0, -1, 0, 1 };
        public static int[] xfallorder = new int[] { 0, -1, 1, 2, -2 };
        public static int[][] xfallchecks = new int[][] {
                                                new int[]{},
                                                new int[]{-1},
                                                new int[]{1},
                                                new int[]{2,1},
                                                new int[]{-2,-1}
                                                        };

        public static int[] lightxes = new int[] { -1, 0, 1, 0, -1, 1, -1, 1 };
        public static int[] lightyes = new int[] { 0, -1, 0, 1, -1, 1, 1, -1 };
        public static byte[] value = new byte[] { 1, 2, 4, 8 };

        public int starti, startj, di, dj, endi, endj;
        public int skip_frames = 0;
        
        public float tiles_spread_elapsed_time = 0;
        public int tiles_spread_frame = 0;
        public static float tiles_spread_sec_between_frame = 20f;
        public bool is_initialized = false;
        public float light_intensity = 0f;
        public Point world_spawn_loc;
        public Rectangle spawn_protect;
        public float elapsed_furniture_update_time = 0;
        public int elapsed_light_update_time = 0;
        public bool world_ending = false;
        public float cur_hour = 0;
        public float cur_minute = 0;

        public CraftPieceInterface piece_crafter = new CraftPieceInterface();
        public DjikstraLight djikstra = new DjikstraLight();
        public CollisionHashTable collision_table;
        public LiquidSimulator liquid_simulator;
        public Timing light_time = new Timing("Lighting");
        public Timing updating = new Timing("Updating");
        public Timing frame_render_time = new Timing("Frame Render Speed");
        public Timing world_generation = new Timing("WG");
        public Timing water_update_time = new Timing("Liquids");
        public Timing total_fps = new Timing("SPF");
        public Timing flowing_blocks = new Timing("Sand Falling");

        public bool is_daytime = true;

        public World()
        {
            Exilania.rand = new Lidgren.Network.NetRandom(Exilania.seed_id);
            seed_number = Exilania.seed_id;
            unique_time_id = (int)System.DateTime.Now.Ticks;
            world_number = WorldManager.next_world_id++;
            Exilania.display.add_message("@03New world is using world number " + world_number);
            world_generation.start();
            bullets = new List<Particle>();
            particles = new List<Particle>();
            furniture = new List<Furniture>();
            plants = new List<Plant>();
            players = new List<Player>();
            npcs = new List<Actor>();
            grass = new List<Point>();
            areas_checked = new List<Point>();
            flowing_sand = new HashSet<Point>();
            flowing_overflow = new HashSet<Point>();
            chests = new List<ItemChest>();
            minimap = new MiniMap();
            night = new NightSky();
            disable_connectivity = true;
            map = new Voxel[2000, 1000]; //try 10000,2500
            collision_table = new CollisionHashTable(this);
            init_world_chunker();
            int[] terrain = generate_terrain(map.GetLength(1) / 4, map.GetLength(0), map.GetLength(1));

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = new Voxel();
                }
            }

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (y > map.GetLength(1)*1/4)
                    {
                        place_block(new Point(x, y), 9, false,true);
                        place_block(new Point(x, y), 9, true, true);
                    }
                }
            }
            for (int x = 0; x < map.GetLength(0) * map.GetLength(1) / 1000; x++)
            {
                switch (x % 3)
                {
                    case 0:
                        random_terrain(Exilania.block_types.get_block_by_name("Stone"), Exilania.rand.Next(60, 140), Exilania.rand.Next(0, map.GetLength(1)/2));
                        break;
                    case 1:
                        random_terrain(Exilania.block_types.get_block_by_name("Mettite"), Exilania.rand.Next(10, 20), Exilania.rand.Next(0, map.GetLength(1)/2));
                        break;
                    case 2:
                        random_terrain(Exilania.block_types.get_block_by_name("Hematite"), Exilania.rand.Next(10, 20), Exilania.rand.Next(0, map.GetLength(1) / 2));
                        break;
                }
                if (x % 10 == 0)
                    random_terrain(-1, 1400, Exilania.rand.Next(0, map.GetLength(1)/2));
            }
            for (int x = 0; x < 1; x++)
            {
                smooth_terrain();
            }

            final_touchup();
            int num_lakes = map.GetLength(0) * map.GetLength(1) / 99106;
            Exilania.plant_manager.spawn_plants_in_world(this);
            liquid_simulator = new LiquidSimulator();
            disable_connectivity = false;
            determine_connectivity();
            is_initialized = true;
            world_spawn_loc = spawn_actor(new Rectangle(-18, -36, 36, 72));
            spawn_protect = new Rectangle(world_spawn_loc.X - 96, world_spawn_loc.Y - 96, 192, 192);
            collision_table.add_all_furniture_to_table(furniture);
            world_generation.stop();
        }

        public World(string pname, int pseed_id, int punique_time)
        {
            world_generation.start();
            world_number = WorldManager.next_world_id++;
            name = pname;
            seed_number = pseed_id;
            unique_time_id = punique_time;
            players = new List<Player>();
            npcs = new List<Actor>();
            bullets = new List<Particle>();
            particles = new List<Particle>();
            furniture = new List<Furniture>();
            plants = new List<Plant>();
            grass = new List<Point>();
            chests = new List<ItemChest>();
            flowing_overflow = new HashSet<Point>();
            flowing_sand = new HashSet<Point>();
            areas_checked = new List<Point>();
            disable_connectivity = true;
        }

        public void finalize_world_creation()
        {
            Exilania.plant_manager.spawn_plants_in_world(this);
            disable_connectivity = false;
            //determine_connectivity();
            is_initialized = true;
            //collision_table = new CollisionHashTable(this);
            world_spawn_loc = spawn_actor(new Rectangle(-18, -36, 36, 72));
            spawn_protect = new Rectangle(world_spawn_loc.X - 96, world_spawn_loc.Y - 96, 192, 192);
            //collision_table.add_all_furniture_to_table(furniture);
            world_generation.stop();
            validate_spawn_point(new Rectangle(-18, -36, 36, 72));
            write_world();
        }

        public World(string filename, GraphicsDevice g)
        {
            world_generation.start();
            System.IO.StreamReader fs;
            System.IO.BinaryReader r;
            //try
            //{
            fs = new System.IO.StreamReader(@"worlds/" + filename + ".wld");
            r = new System.IO.BinaryReader(fs.BaseStream);

            players = new List<Player>();
            npcs = new List<Actor>();
            bullets = new List<Particle>();
            particles = new List<Particle>();
            furniture = new List<Furniture>();
            plants = new List<Plant>();
            grass = new List<Point>();
            areas_checked = new List<Point>();
            flowing_sand = new HashSet<Point>();
            flowing_overflow = new HashSet<Point>();
            night = new NightSky();
            chests = new List<ItemChest>();
            disable_connectivity = true;
            world_number = r.ReadInt32();
            unique_time_id = r.ReadInt32();
            seed_number = r.ReadInt32();
            name = r.ReadString();
            
            //name = filename;
            map = new Voxel[r.ReadInt32(), r.ReadInt32()];
            for (int i = 0; i < 5; i++)
            {
                world_height_points[i] = r.ReadInt32();
            }
            Voxel last_voxel = new Voxel();
            int count_identical = -1;
            int temp_hold = 0;
            int line_read = 0;
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (count_identical <= 0)
                    {
                        count_identical = 0;
                        last_voxel = new Voxel(r);
                        line_read++;
                        if (last_voxel.liquid_level >= 128)
                        {
                            last_voxel.liquid_level -= 128;
                            if (last_voxel.liquid_id >= 127)
                            {
                                last_voxel.liquid_id -= 128;
                                temp_hold = r.ReadByte() * 32;
                                count_identical += temp_hold;
                                temp_hold = 0;
                            }
                            if (last_voxel.liquid_id > 3)
                            {
                                temp_hold = last_voxel.liquid_id >> 2;
                                last_voxel.liquid_id -= (byte)(temp_hold * 4);
                                count_identical += temp_hold;
                                temp_hold = 0;
                            }
                        }
                        if (count_identical == 0)
                            count_identical++;
                    }
                    //Exilania.text_stream.WriteLine("{" + x + "," + y + "} " + last_voxel.ToString() + " " + count_identical); 
                    map[x, y] = new Voxel(last_voxel);
                    if (count_identical > 0)
                        count_identical--;

                }
            }
            int num_furniture = r.ReadInt32();
            for (int x = 0; x < num_furniture; x++)
            {
                furniture.Add(new Furniture(r));
            }
            init_loaded_furniture();
            int num_chests = r.ReadInt32();
            for (int i = 0; i < num_chests; i++)
            {
                chests.Add(new ItemChest(r));
            }
            int num_plants = r.ReadInt32();
            for (int x = 0; x < num_plants; x++)
            {
                plants.Add(new Plant(r));
            }
            init_loaded_plants();
            atmosphere_start = r.ReadInt32();
            Exilania.text_stream.WriteLine("Loading World: Read a total of " + line_read + " lines from file.");
            world_spawn_loc = new Point(r.ReadInt32(), r.ReadInt32());
            spawn_protect = new Rectangle(world_spawn_loc.X - 96, world_spawn_loc.Y - 96, 192, 192);
            world_time = r.ReadSingle();
            tiles_spread_frame = (int)world_time / (int)tiles_spread_sec_between_frame;
            day_length = r.ReadSingle();
            //day_length = 20;
            night_percent_length = r.ReadSingle();
            world_gravity = r.ReadSingle();
            Exilania.text_stream.WriteLine("Finished reading from the file. " + r.BaseStream.Position + " Bytes?");
            r.Close();
            liquid_simulator = new LiquidSimulator();
            liquid_simulator.init_liquid_cells(this);
            disable_connectivity = false;
            determine_connectivity();
            is_initialized = true;
            
            /*}
            catch
            {
                Exilania.text_stream.WriteLine("Error encountered... end of stream?");
                return;
            }*/
            collision_table = new CollisionHashTable(this);
            collision_table.add_all_furniture_to_table(furniture);
            minimap = new MiniMap(this, g);
            world_generation.stop();
        }

        public void write_world()
        {
            //finish up any running things... like sand falling
            while (flowing_sand.Count > 0)
            {
                world_ending = true;
                freeflowing_blocks();
            }
            
            world_generation.start();
            try
            {
                System.IO.Directory.CreateDirectory(@"worlds");
            }
            catch
            {

            }
            System.IO.StreamWriter fs = new System.IO.StreamWriter(@"worlds/world" + world_number + ".wld");
            System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs.BaseStream);
            w.Write(world_number);
            w.Write(unique_time_id);
            w.Write(seed_number);
            w.Write(name);
           
            w.Write(map.GetLength(0));
            w.Write(map.GetLength(1));
            for (int i = 0; i < 5; i++)
            {
                w.Write(world_height_points[i]);
            }
            Voxel last_voxel = new Voxel();
            last_voxel.bkd_block = -126;
            int count_identical = 0;
            byte write_byte = 0;
            int write_line = 0;
            world_generation = new Timing("Write Time");
            world_generation.start();
            Exilania.text_stream.WriteLine("WRITING world to File.");
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    if (map[x, y].liquid_level > 100)
                        map[x, y].liquid_level = 100;
                    if (map[x, y].bkd_block == last_voxel.bkd_block && map[x, y].fgd_block == last_voxel.fgd_block
                        && map[x, y].liquid_id == last_voxel.liquid_id && map[x, y].liquid_level == last_voxel.liquid_level 
                        && count_identical <= 8190) //normally 8190
                    { //we have a repeater block on our hands!
                        if (count_identical == 0)
                            count_identical = 2;
                        else
                            count_identical++;
                    }
                    else
                    { //not a repeater, see how many repeats we had.
                        if (count_identical > 0)
                        { //we had a repeater... write it out into the block you are going to send in.
                           // Exilania.text_stream.Write("Entry had " + count_identical + " blocks in a row. ");
                            last_voxel.liquid_level += 128; //indicates we need to check for repeats
                            if (count_identical < 32) //normally 32
                            { //fit into the current block
                                last_voxel.liquid_id += (byte)(count_identical * 4); //the liquid id has 5 bits set aside for holding repeats (up to 31)
                            }
                            else
                            { //does not fit in the block... tack on an extra byte.
                                write_byte = (byte)(count_identical / 32); //normally 32 and byte; the write_byte contains a number that is represents how many groups of 32 blocks are identical
                                count_identical = count_identical % 32; //normally 32; this is the remainder
                                last_voxel.liquid_id += (byte)(count_identical * 4); //this will put the remainder in to bits 2-6 in liquid id
                                last_voxel.liquid_id += 128; //high bit for additional byte being written.; high bit denotes additional byte being written.
                            }
                            //Exilania.text_stream.WriteLine(last_voxel.to_file_string());
                        }
                        count_identical = 0;
                        //ok write out to file, reset file.
                        if (last_voxel.bkd_block != -126)
                        {
                            last_voxel.write_voxel(w);
                            write_line++;
                        }
                        if(write_byte > 0)
                        {
                            w.Write((byte)write_byte); //normally byte
                            write_byte = 0;
                        }
                        last_voxel = new Voxel(map[x, y]);
                    }
                }
            }
            //final writes for the whole thing:
            if (count_identical > 0)
            { //we had a repeater... write it out into the block you are going to send in.
                last_voxel.liquid_level += 128;
                if (count_identical < 32)
                { //fit into the current block
                    last_voxel.liquid_id += (byte)(count_identical * 4);
                }
                else
                { //does not fit in the block... tack on an extra byte.
                    write_byte = (byte)(count_identical / 32);
                    count_identical = count_identical % 32;
                    last_voxel.liquid_id += (byte)(count_identical * 4);
                    last_voxel.liquid_id += 128; //high bit for additional byte being written.
                }
            }
            world_generation.stop();
            Exilania.text_stream.WriteLine("World " + world_generation.ToString());
            count_identical = 0;
            //ok write out to file, reset file.
            if (last_voxel.bkd_block != -126)
            {
                last_voxel.write_voxel(w);
                write_line++;
            }
            if (write_byte > 0)
            {
                w.Write((byte)write_byte);
                //Exilania.text_stream.WriteLine("Additional Byte: " + write_byte);
                write_byte = 0;
            }
            int num_write_out = 0;
            for (int x = 0; x < furniture.Count; x++)
            {
                if (!furniture[x].flags[(int)FFLAGS.EMPTY])
                    num_write_out++;
                else
                {
                    for (int i = 0; i < furniture.Count; i++)
                    {
                        for (int j = 0; j < furniture[i].connections.Count; j++)
                        {
                            if (furniture[i].connections[j].target_type == TargetType.Furniture)
                            {
                                if (furniture[furniture[i].connections[j].target_id].flags[(int)FFLAGS.EMPTY])
                                {
                                    furniture[i].connections.RemoveAt(j);
                                    j--;
                                }
                                else if(furniture[i].connections[j].target_id > num_write_out)
                                furniture[i].connections[j].target_id--;
                            }
                        }
                    }
                }
            }
            for (int x = 0; x < furniture.Count; x++)
            {
                if (furniture[x].flags[(int)FFLAGS.EMPTY])
                {
                    for (int i = 0; i < chests.Count; i++)
                    {
                        if (chests[i].furniture_id == x)
                        { //this chest needs to be deleted, so does the furniture... update the furniture_ids for all the chests afterwards(all decrease by 1)
                            for (int j = i; j < chests.Count; j++)
                            {
                                chests[j].furniture_id--;
                            }
                            chests.RemoveAt(i);
                        }
                    }
                    furniture.RemoveAt(x);
                    x--;
                }
            }
            w.Write(num_write_out);
            for (int x = 0; x < furniture.Count; x++)
            {
                if (!furniture[x].flags[(int)FFLAGS.EMPTY])
                    furniture[x].write_furniture(w);
            }
            w.Write(chests.Count);
            for (int i = 0; i < chests.Count; i++)
            {
                chests[i].write_chest(w);
            }
            
            //plant writing out
            for (int x = 0; x < plants.Count; x++)
            {
                if (plants[x].is_empty)
                {
                    plants.RemoveAt(x);
                    x--;
                }
            }
            w.Write(plants.Count);
            for (int x = 0; x < plants.Count; x++)
            {
                if (!plants[x].is_empty)
                    plants[x].write_plant(w);
            }
            w.Write(atmosphere_start);
            w.Write(world_spawn_loc.X);
            w.Write(world_spawn_loc.Y);
            //end final writes.
            w.Write(world_time);
            w.Write(day_length);
            w.Write(night_percent_length);
            w.Write(world_gravity);
            //this second one is so that it won't run out of things to read.. not sure exactly what is going on there... padding?
            w.Write(world_gravity);
            Exilania.text_stream.WriteLine("Finished writing to the file. " + w.BaseStream.Position.ToString() + " Bytes?");
           
            w.Close();
            //fs.Dispose();
            world_generation.stop();
        }

        public void init_loaded_furniture()
        {
            Point size = new Point();
            for (int i = 0; i < furniture.Count; i++)
            {
                size = new Point(Exilania.furniture_manager.furniture[furniture[i].furniture_id].image_frames[furniture[i].state].width,
                    Exilania.furniture_manager.furniture[furniture[i].furniture_id].image_frames[furniture[i].state].height);
                for (int x = furniture[i].top_left.X; x < furniture[i].top_left.X + size.X; x++)
                {
                    for (int y = furniture[i].top_left.Y; y < furniture[i].top_left.Y + size.Y; y++)
                    {
                        map[x, y].furniture_index = i;
                    }
                }
            }
        }

        public void init_loaded_plants()
        {
            for (int i = 0; i < plants.Count; i++)
            {
                for (int j = 0; j < plants[i].pieces.Count; j++)
                {
                    for (int x = plants[i].pieces[j].top_left.X; x < plants[i].pieces[j].width + plants[i].pieces[j].top_left.X; x++)
                    {
                        for (int y = plants[i].pieces[j].top_left.Y; y < plants[i].pieces[j].height + plants[i].pieces[j].top_left.Y; y++)
                        {
                            map[wraparound_x(x), y].plant_index = i;
                        }
                    }
                }
            }
        }

        public void init_world_chunker()
        {
            world_chunker = new Chunk[(int)Math.Ceiling((float)map.GetLength(0) / 100f), (int)Math.Ceiling((float)map.GetLength(1) / 100f)];
            for (int x = 0; x < world_chunker.GetLength(0); x++)
            {
                for (int y = 0; y < world_chunker.GetLength(1); y++)
                {
                    world_chunker[x, y] = new Chunk(x * 100, y * 100, Math.Min(100, map.GetLength(0) - ((x + 1) * 100)), Math.Min(100, map.GetLength(1) - ((y + 1) * 100)));
                }
            }
        }

        public int[] generate_terrain(int sea_level, int map_width, int map_height)
        {
            int[] terrain = new int[map_width];
            int last_was = sea_level;
            for (int x = 0; x < terrain.GetLength(0); x++)
            {
                if (Exilania.rand.Next(0, 20) == 0)
                    last_was += Exilania.rand.Next(0, 21) - 10;
                else
                    last_was += Exilania.rand.Next(0, 5) - 2;
                
                if (last_was < 50)
                    last_was = sea_level;
                if (last_was > map_height - 100)
                    last_was = sea_level;
                terrain[x] = last_was;
            }
            return terrain;
        }
       
        public void random_terrain(int block_type, int size_field, int depth)
        {
            Point start = new Point(Exilania.rand.Next(0, map.GetLength(0)), depth);
            while (size_field > 0)
            {
                size_field--;
                if (start.Y > -1 && start.Y < map.GetLength(1))
                {
                    if (map[wraparound_x(start.X), start.Y].fgd_block_type != -1)
                    {
                        if (block_type == -1)
                        {
                            remove_block(start, false);
                        }
                        else
                            place_block(start, (sbyte)block_type, false, true);
                    }
                }
                if (Exilania.rand.Next(0, 2) == 0)
                {
                    start.X += (Exilania.rand.Next(0, 2) * 2) - 1;
                }
                else
                    start.Y += (Exilania.rand.Next(0, 2) * 2) - 1;
            }
        }

        public void final_touchup()
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                bool done_top = false;
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (!done_top)
                    {
                        remove_block(new Point(x, y), true);
                    }
                    if (!done_top && !map[x, y].passable)
                    {
                        done_top = true;
                        remove_block(new Point(x, y), true);
                        place_block(new Point(x, y), 12, false, true);
                    }
                    else if (done_top)
                    {
                        place_block(new Point(x, y), 12, true, true);

                    }
                }
            }
        }

        public void smooth_terrain()
        {
            bool is_first = false;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                is_first = true;
                for (int y = 0; y < map.GetLength(1)-1; y++)
                {
                    if (number_of_walls_next_to(x, y) <= 3)
                    {
                        if (!map[x, y].passable)
                        {
                            remove_block(new Point(x, y), false);
                            if(is_first)
                            remove_block(new Point(x, y), true);
                            is_first = false;
                        }
                    }
                    else if (!map[x, y].passable)
                    {
                        is_first = false;
                    }
                }
            }

        }

        public bool has_empty_block_foreground_neighbors(Point use)
        {
            Point temp = new Point();
            for (int i = 0; i < xes.Length; i++)
            {
                temp.X = wraparound_x(use.X + xes[i]);
                temp.Y = use.Y + yes[i];
                if (temp.Y < map.GetLength(1) && map[temp.X, temp.Y].fgd_block == -1)
                    return true;
            }
            return false;
        }
        public int has_liquid_neighbor(Point use)
        {
            Point temp = new Point();
            int num = 0;
            for (int i = 0; i < xes.Length; i++)
            {
                temp.X = wraparound_x(use.X + xes[i]);
                temp.Y = use.Y + yes[i];
                if (map[temp.X, temp.Y].liquid_id != 0 && map[temp.X, temp.Y].liquid_level >= 50)
                    num++;
            }
            return num;
        }
        
        public void spread_block(sbyte spreading_block, sbyte spread_to_block, bool killed_by_liquid)
        {
            if (spreading_block == grass_block_id)
            {
                Point use = new Point();
                Point[] temp = new Point[grass.Count];
                grass.CopyTo(temp);
                for (int i = 0; i < temp.Length; i++)
                {
                    if (!killed_by_liquid || has_liquid_neighbor(temp[i]) == 0)
                    {
                        for (int j = 0; j < lightxes.Length; j++)
                        {
                            use.X = wraparound_x(temp[i].X + lightxes[j]);
                            use.Y = temp[i].Y + lightyes[j];
                            if (use.Y > -1 && use.Y < map.GetLength(1) && map[use.X, use.Y].fgd_block == spread_to_block &&
                                has_empty_block_foreground_neighbors(use))
                            {
                                grass.Add(use);
                                place_block(use, spreading_block, false, true);
                            }
                        }
                        if (Exilania.game_server && Exilania.rand.Next(0, 20) == 0 && is_daytime)
                        {
                            Exilania.plant_manager.spawn_plant_in_world(this,temp[i],"random");
                        }
                    }
                    else
                    {
                        place_block(temp[i], spread_to_block, false, true);
                    }
                }
            }
            else
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    for (int y = 0; y < map.GetLength(1) - 1; y++)
                    {
                        if (map[x, y].fgd_block == spread_to_block &&
                            num_block_type_next_to(spreading_block, x, y) > 0 &&
                            has_transparent_neighbors(new Point(x, y)))
                        {
                            place_block(new Point(x, y), spreading_block, false, true);
                        }
                    }
                }
            }
        }

        public int num_block_type_next_to(sbyte block_id, int locx, int locy)
        {
            int num_type = 0;
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0)
                    {

                    }
                    else if (locy + y >= 0 && locy + y < map.GetLength(1) && map[wraparound_x(locx + x), locy + y].fgd_block == block_id)
                    {
                        num_type++;
                    }
                }
            }

            return num_type;
        }

        /// <summary>
        /// this will basically use ray casting and 3 part light sources to create lights. (R, G, B) they will end up being nice circles of light.
        /// </summary>
        public void do_composition_lighting()
        {
            Vector2 center = new Vector2(players[Exilania.game_my_user_id].avatar.world_loc.X, players[Exilania.game_my_user_id].avatar.world_loc.Y);
            Vector2 tl_use = new Vector2(center.X - (Exilania.screen_size.X / 2), center.Y - (Exilania.screen_size.Y / 2));
            if (tl_use.Y < 0)
                tl_use.Y = 0;
            else if (tl_use.Y > map.GetLength(1) * 24 - (Exilania.screen_size.Y))
            {
                tl_use.Y = map.GetLength(1) * 24 - Exilania.screen_size.Y;
            }
            if (Exilania.input.ctrl_state == 1)
            {
                center -= Exilania.input.saved_delta;
                tl_use = new Vector2(center.X - (Exilania.screen_size.X / 2), center.Y - (Exilania.screen_size.Y / 2));
            }

            Point offset = new Point((int)tl_use.X / 24, (int)tl_use.Y / 24);
            day_only_portion = world_time % day_length / day_length;
            byte[] night_light = new byte[]{10,10,10};
            calc_sky_color = day_sky;
            
            if (1f - day_only_portion > night_percent_length)
            { //daytime!
                is_daytime = true;
                night_light = new byte[] { 100, 100, 100 };
                light_intensity = 100f;
            }
            else
            {
                is_daytime = false;
                light_intensity = 1f - day_only_portion; //get elapsed night portion
                
                light_intensity = night_percent_length - light_intensity; //get remaining night portion
                light_intensity /= night_percent_length; //get percentage night elapsed.

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
                calc_sky_color.R = (byte)(light_intensity * (float)day_sky.R);
                calc_sky_color.G = (byte)(light_intensity * (float)day_sky.G);
                calc_sky_color.B = (byte)(light_intensity * (float)day_sky.B);


                light_intensity *= 70f;
                night_light = new byte[] { (byte)(30 + Math.Round(light_intensity, 0)), (byte)(30 + Math.Round(light_intensity, 0)), (byte)(30 + Math.Round(light_intensity, 0)) };
            }
            day_only_portion = day_only_portion / (1f - night_percent_length);
            djikstra.run_djikstra(this, night_light, new Rectangle((int)offset.X - 10, (int)offset.Y -10, 20 + Exilania.screen_size.X / 24, 20 + Exilania.screen_size.Y / 24));
        }

        public bool has_non_transparent_neighbors(Point loc)
        {
            for (int i = 0; i < xes.Length; i++)
            {
                if (loc.Y + yes[i] > -1 && loc.Y + yes[i] < map.GetLength(1) &&
                    (
                       !map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].transparent
                    || !map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].bkd_transparent
                    || (map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].liquid_id != 0)
                    || map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].bkd_block == window_block_id
                    || map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].furniture_index != -1
                    || map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].plant_index != -1))
                    return true;
            }
            return false;
        }

        public bool has_transparent_neighbors(Point loc)
        {
            for (int i = 0; i < xes.Length; i++)
            {
                if (loc.Y + yes[i] > -1 && loc.Y + yes[i] < map.GetLength(1) &&
                    (
                       map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].transparent
                    && map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].bkd_transparent
                    && (map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].plant_index == -1 || 
                    Exilania.plant_manager.plants[plants[map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].plant_index].plant_index].passable)
                    && (map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].furniture_index == -1 ||
                        (map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].furniture_index != -1 &&
                        furniture[map[wraparound_x(loc.X + xes[i]), loc.Y + yes[i]].furniture_index].flags[(int)FFLAGS.TRANSPARENT]))))
                    return true;
            }
            return false;
        }

        public static int get_max(byte[] a)
        {
            int cur_max = 0;
            for (int x = 0; x < a.Length; x++)
            {
                if (a[x] > cur_max)
                    cur_max = a[x];
            }
            return cur_max;
        }

        public int number_of_walls_next_to(int locx, int locy)
        {
            int num_walls = 0;
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0)
                    {

                    }
                    else if (locy +y >= 0 && locy + y < map.GetLength(1) && !map[wraparound_x(locx + x),locy + y].passable)
                    {
                        num_walls++;
                    }
                }
            }
            return num_walls;
        }

        public sbyte get_block_at(Point loc, bool bkd)
        {
            if (bkd)
                return map[loc.X, loc.Y].bkd_block;
            else
            {
                if (map[loc.X, loc.Y].plant_index != -1)
                {
                    return (sbyte)BlockData.block_enum[Exilania.plant_manager.plants[plants[map[loc.X, loc.Y].plant_index].plant_index].material];
                }
                return map[loc.X, loc.Y].fgd_block;
            }
        }

        public sbyte get_block_group_at(Point loc, bool bkd)
        {
            if (bkd)
            {
                if (map[loc.X, loc.Y].bkd_block < 0)
                    return -1;
                return Exilania.block_types.blocks[map[loc.X, loc.Y].bkd_block].block_group;
            }
            else
            {
                if (map[loc.X, loc.Y].fgd_block < 0)
                    return -1;
                return Exilania.block_types.blocks[map[loc.X, loc.Y].fgd_block].block_group;
            }
        }
    
        /// <summary>
        /// this will switch out a block in the world...
        /// </summary>
        /// <param name="xloc"></param>
        /// <param name="yloc"></param>
        /// <param name="block_replace">the id of the block_type to put into the map</param>
        /// <param name="bkd">true means switching background</param>
        /// <returns>the id of the block that was there before (or -1 if nothing was there)</returns>
        public sbyte remove_block(Point loc, bool bkd)
        {
            int use_x = wraparound_x(loc.X);
            sbyte ret = -1;
            sbyte btype = -1;
            if (!bkd)
            {
                
                if (map[use_x, loc.Y].fgd_block == grass_block_id)
                {
                    grass.Remove(new Point(use_x, loc.Y));
                    map[use_x, loc.Y].fgd_block = Exilania.block_types.get_block_by_name("Dirt");
                    return -2;
                }
                btype = map[use_x, loc.Y].fgd_block_type;
                ret = map[use_x, loc.Y].fgd_block;
                if (map[use_x, loc.Y].fgd_block_type == -1)
                    ret = -1;
                if (map[use_x,loc.Y].fgd_block_type!=-1)
                {
                    map[use_x, loc.Y].fgd_block = -1;
                    map[use_x, loc.Y].fgd_block_type = -1;
                    if (!disable_connectivity && !world_ending)
                    {
                        liquid_simulator.add_liquid_of_interest(new Point(use_x, loc.Y), 0);
                        minimap.update_loc(loc.X, loc.Y, -1);
                    }
                    //map[use_x, loc.Y].liquid_id = 0;
                    //map[use_x, loc.Y].liquid_level = 0;
                    map[use_x, loc.Y].passable = true;
                    map[use_x, loc.Y].transparent = true;
                    map[use_x, loc.Y].light_source = null;
                    determine_connectivity(new Rectangle(use_x - 1, loc.Y - 1, 3, 3),btype);
                }
                if (!disable_connectivity && !world_ending)
                {
                    for (int i = 0; i < lightxes.Length; i++)
                    {
                        liquid_simulator.add_liquid_of_interest(new Point(wraparound_x(use_x + lightxes[i]), loc.Y + lightyes[i]), 0);
                    }
                    if (map[use_x, loc.Y].liquid_cell_id != -1 && liquid_simulator.liquid_cells[map[use_x,loc.Y].liquid_cell_id].can_transfer == false)
                    {
                        liquid_simulator.liquid_cells[map[use_x, loc.Y].liquid_cell_id].can_transfer = true;
                    }
                }
                if (!disable_connectivity && loc.Y > 0)
                {
                    for (int i = -2; i < 3; i++)
                    {
                        Point t = new Point(wraparound_x(use_x + i), loc.Y - 1);
                        if(map[wraparound_x(use_x + i),loc.Y -1].fgd_block == sand_block_id)
                            add_freeflowing(t);
                    }
                    if (map[wraparound_x(use_x - 1), loc.Y].fgd_block == sand_block_id)
                        add_freeflowing(new Point(wraparound_x(use_x - 1), loc.Y));
                    if (map[wraparound_x(use_x + 1), loc.Y].fgd_block == sand_block_id)
                        add_freeflowing(new Point(wraparound_x(use_x + 1), loc.Y));
                    //check if you need to do sand falling.
                }
                return ret;
            }
            else //replacing the background block
            {
                ret = map[use_x, loc.Y].bkd_block;
                if (ret != -1 && is_free_block(use_x,loc.Y))
                {
                    map[use_x, loc.Y].bkd_block = -1;
                    map[use_x, loc.Y].bkd_transparent = true;
                }
                if(!disable_connectivity && ret!=-1)
                    determine_bkd_connectivity(new Rectangle(use_x - 1, loc.Y - 1, 3, 3), ret);
                return ret;
            }
        }

        public void remove_line_of_blocks(Vector2 origin, float angle, float distance, Actor remover, bool bkd)
        {
            Vector2 cur_loc = origin;
            Point last_block_loc = new Point((int)origin.X/24,(int)origin.Y/24);
            Point cur_block_loc = new Point(0,0);
            Vector2 delta = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Dictionary<sbyte, short> blocks_rmvd = new Dictionary<sbyte, short>();
            while (Acc.get_distance(origin, new Point((int)cur_loc.X, (int)cur_loc.Y)) < distance)
            {
                cur_loc += delta * 5;
                cur_block_loc = new Point((int)cur_loc.X / 24, (int)cur_loc.Y / 24);
                if ((cur_block_loc.X != last_block_loc.X || cur_block_loc.Y != last_block_loc.Y) && cur_block_loc.Y > -1 && cur_block_loc.Y < map.GetLength(1))
                {
                    cur_block_loc.X = wraparound_x(cur_block_loc.X);
                    last_block_loc = cur_block_loc;
                    if (can_break(cur_block_loc) || bkd)
                    {
                        sbyte temp = remove_block(cur_block_loc, bkd);
                        if (temp != -1)
                        {
                            Exilania.network_client.send_remove_block(cur_block_loc, bkd, this);
                            if (blocks_rmvd.ContainsKey(temp))
                            {
                                blocks_rmvd[temp]++;
                            }
                            else
                            {
                                blocks_rmvd.Add(temp, 1);
                            }
                        }
                    }
                    else
                    {
                        foreach (var ped in blocks_rmvd)
                        {
                            short temp = ped.Value;
                            while (temp > 0)
                            {
                                remover.items.pickup_block(ped.Key);
                                temp--;
                            }
                        }
                        return;
                    }
                }
            }
            foreach (var ped in blocks_rmvd)
            {
                short temp = ped.Value;
                while (temp > 0)
                {
                    remover.items.pickup_block(ped.Key);
                    temp--;
                }
            }
        }

        public void add_freeflowing(Point loc)
        {
           /* if (flowing_sand.Count < 1000)
            {
                if(!flowing_sand.ContainsKey(loc))
                    flowing_sand.Add(loc,2);
            }
            else
                if(!flowing_overflow.ContainsKey(loc) && !flowing_sand.ContainsKey(loc))
                    flowing_overflow.Add(loc,2);
            */
            if (flowing_sand.Count < 1000)
            {
                if (!flowing_overflow.Contains(loc) && !flowing_sand.Contains(loc))
                    flowing_sand.Add(loc);
            }
            else
                if (!flowing_overflow.Contains(loc) && !flowing_sand.Contains(loc))
                    flowing_overflow.Add(loc);
            
        }

        public void copy_freeflowing_temp()
        {
            Point tmp = new Point();
            Point[] t;
            if (flowing_overflow.Count + flowing_sand.Count > 1000)
            {
                t = new Point[Math.Min(flowing_overflow.Count, 1000 - flowing_sand.Count)];
                flowing_overflow.CopyTo(t, 0, 1000 - flowing_sand.Count);
            }
            else
            {
                t = new Point[flowing_overflow.Count];
                flowing_overflow.CopyTo(t);
            }
            //flowing_overflow.Keys.CopyTo(t, 0);
            int t_iterator = 0;
            while (flowing_sand.Count < 1000 && t_iterator < t.Length)
            {
                tmp = t[t_iterator];
                if (!flowing_sand.Contains(tmp))
                    flowing_sand.Add(tmp);
                else
                    Exilania.display.add_message("@00DUPLICATE " + tmp.ToString());
                flowing_overflow.Remove(tmp);
                t_iterator++;
            }
        }

        public void freeflowing_blocks()
        {
            Exilania.debug = "@04sand: " + flowing_sand.Count + " overflow-sand: " + flowing_overflow.Count;
            copy_freeflowing_temp();
            Point[] temp = new Point[flowing_sand.Count];
            flowing_sand.CopyTo(temp,0);
            flowing_sand.Clear();
            for (int x = 0; x < temp.Length; x++)
            {
                if (map[temp[x].X, temp[x].Y].fgd_block == sand_block_id && temp[x].Y < map.GetLength(1) - 1)
                {
                    //only sand can flow...
                    for (int i = 0; i < xfallorder.Length; i++)
                    {
                        bool can_flow = true;
                        for (int j = 0; j < xfallchecks[i].Length; j++)
                        {
                            if (map[wraparound_x(temp[x].X + xfallchecks[i][j]), temp[x].Y].fgd_block != -1)
                                can_flow = false;
                        }
                        if (can_flow && map[wraparound_x(temp[x].X + xfallorder[i]), temp[x].Y + 1].fgd_block == -1)
                        {
                            Point add = temp[x];
                            add.X = wraparound_x(add.X + xfallorder[i]);
                            add.Y += 1;
                            if (add.X == temp[x].X)
                            { //pulling from above... find the highest sand in the stack and grab it!
                                while (temp[x].Y > 0 && map[temp[x].X, temp[x].Y - 1].fgd_block == sand_block_id && (world_ending || add.Y - temp[x].Y < 20))
                                {
                                    temp[x].Y--;
                                }
                            }
                            remove_block(temp[x], false);
                            if (temp[x].Y != add.Y)
                            {
                                for (int k = -2; k < 3; k++)
                                {
                                    Point t = new Point(wraparound_x(add.X + k), add.Y - 1);
                                    if (map[wraparound_x(add.X + k), add.Y - 1].fgd_block == sand_block_id)
                                        add_freeflowing(t);
                                }
                                if (map[wraparound_x(add.X - 1), add.Y].fgd_block == sand_block_id)
                                    add_freeflowing(new Point(wraparound_x(add.X - 1), add.Y));
                                if (map[wraparound_x(add.X + 1), add.Y].fgd_block == sand_block_id)
                                    add_freeflowing(new Point(wraparound_x(add.X + 1), add.Y));
                            }
                            place_block(add, sand_block_id, false, true);
                            break;
                        }
                    }
                }
            }
        }

        public bool is_free_block(int locx, int locy)
        {
            if (disable_connectivity)
                return true;

            if (locy == 0)
                return true;
            if ((Exilania.block_types.blocks[map[locx, locy].bkd_block].block_group == 1 || Exilania.block_types.blocks[map[locx, locy].bkd_block].block_group == 2))
            {
                for (int i = 0; i < xes.Length; i++)
                {
                    if (locy + yes[i] < map.GetLength(1) && (map[wraparound_x(locx + xes[i]), locy + yes[i]].bkd_block == -1 ||(
                        Exilania.block_types.blocks[map[wraparound_x(locx + xes[i]), locy + yes[i]].bkd_block].block_group != 1 &&
                        Exilania.block_types.blocks[map[wraparound_x(locx + xes[i]), locy + yes[i]].bkd_block].block_group != 2)))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// use this to place any type of block in the world (or modify the world in any block tile way)
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="block_replace"></param>
        /// <param name="bkd"></param>
        /// <returns>-1 if the block was placed, otherwise the block is sent back.</returns>
        public sbyte place_block(Point loc, sbyte block_replace, bool bkd, bool regardless)
        {
            int use_x = wraparound_x(loc.X);
            sbyte ret = 0;
            if (regardless || neighbor_occupied(new Point(loc.X, loc.Y)) || (Exilania.block_types.blocks[block_replace].block_group == 5))
            {
                if (!bkd)
                {
                   
                    if (map[use_x, loc.Y].fgd_block_type == -1 || map[use_x,loc.Y].fgd_block_type == 5 || regardless)
                    {
                        if (map[use_x, loc.Y].fgd_block == grass_block_id)
                            grass.Remove(new Point(use_x, loc.Y));
                        if (Exilania.block_types.blocks[block_replace].liquid_id == 0)
                        {
                            ret = map[use_x, loc.Y].fgd_block;
                            map[use_x, loc.Y].fgd_block = block_replace;
                            map[use_x, loc.Y].fgd_block_type = Exilania.block_types.blocks[block_replace].block_group;
                            map[use_x, loc.Y].passable = Exilania.block_types.blocks[block_replace].passable;
                            map[use_x, loc.Y].transparent = Exilania.block_types.blocks[block_replace].transparent;
                            map[use_x, loc.Y].light_source = Exilania.block_types.blocks[block_replace].light_source;
                        }
                        else
                        {
                            ret = -1;
                            map[use_x, loc.Y].liquid_id = Exilania.block_types.blocks[block_replace].liquid_id;
                                map[use_x, loc.Y].liquid_level = (byte)(disable_connectivity?100:100);// (byte)Exilania.rand.Next(75, 101);
                        }
                        if (!disable_connectivity && !world_ending)
                        {
                            minimap.update_loc(loc.X, loc.Y, block_replace);
                            for (int i = 0; i < lightxes.Length; i++)
                            {
                                liquid_simulator.add_liquid_of_interest(new Point(wraparound_x(use_x + lightxes[i]), loc.Y + lightyes[i]), 0);
                            }
                            if (map[use_x, loc.Y].liquid_level > 0 && Exilania.block_types.blocks[block_replace].name != "Sand")
                            {
                                liquid_simulator.add_to_before_cell_update(new Point(use_x,loc.Y));
                            }
                        }
                        //else
                        //    map[use_x, loc.Y].liquid_level = 0;
                        determine_connectivity(new Rectangle(use_x - 1, loc.Y - 1, 3, 3), Exilania.block_types.blocks[block_replace].block_group);
                        if (!disable_connectivity && block_replace == sand_block_id)
                        {
                            add_freeflowing(new Point(use_x, loc.Y));
                        }
                        return ret;
                    }
                    else
                        return block_replace;
                }
                else if (block_replace != -1 && (Exilania.block_types.blocks[block_replace].place_wall || regardless))//replacing the background block
                {
                    if (map[use_x, loc.Y].bkd_block == -1 || regardless)
                    {
                        ret = map[use_x, loc.Y].bkd_block;
                        map[use_x, loc.Y].bkd_block = block_replace;
                        map[use_x, loc.Y].bkd_transparent = Exilania.block_types.blocks[block_replace].bkd_transparent;
                        if (!disable_connectivity)
                            determine_bkd_connectivity(new Rectangle(use_x - 1, loc.Y - 1, 3, 3), block_replace);
                        return ret;
                    }
                    else
                    {
                        return block_replace;
                    }
                }
            }
            return block_replace;
        }

        public int convert_length_to_indeces(int pixel_length)
        {
            return pixel_length / 24;
        }

        public int get_leftover_pixels(int pixel_length)
        {
            return pixel_length % 24;
        }

        public void validate_spawn_point(Rectangle size)
        {
            Point loc_spawn = new Point(world_spawn_loc.X/24,world_spawn_loc.Y/24);
            int height = (size.Height / 24) + 1;
            int width = (size.Width / 24) + 1;
            bool taken = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[loc_spawn.X + x, loc_spawn.Y + y].fgd_block_type != -1 || map[loc_spawn.X + x, loc_spawn.Y + y].liquid_id > 0)
                        taken = true;
                }
            }
            if (!taken && (map[loc_spawn.X, loc_spawn.Y + height].fgd_block_type == -1 || map[loc_spawn.X, loc_spawn.Y + height].liquid_id > 0 || !map[loc_spawn.X, loc_spawn.Y].bkd_transparent ||
                map[loc_spawn.X, loc_spawn.Y + height].fgd_block_type == -1 || map[loc_spawn.X, loc_spawn.Y].liquid_id > 0))
            {
                taken = true;
            }
            if (taken)
            {
                int[] valid_locs = new int[map.GetLength(0)];
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    int y = 0;
                    while (map[x, y].bkd_block == -1 && map[x, y].fgd_block_type == -1 && map[x, y].liquid_id == 0 && y < map.GetLength(1))
                    {
                        y++;
                    }
                    if (map[x, y].liquid_id != 0)
                        y = map.GetLength(1) - 1;
                    valid_locs[x] = y;
                }
                int spot = Exilania.rand.Next(0, valid_locs.GetLength(0));
                int count = 0;
                while (valid_locs[spot] >= map.GetLength(1) - 50 && count < map.GetLength(0)*3)
                {
                    spot = Exilania.rand.Next(0, valid_locs.GetLength(0));
                    count++;
                }
                loc_spawn.X = spot;
                loc_spawn.Y = valid_locs[spot]-3;
            }
            world_spawn_loc = new Point(loc_spawn.X * 24 + (int)(size.Width / 2), loc_spawn.Y * 24 + (int)(size.Height / 2));
        }

        public Point spawn_actor(Rectangle size)
        {
            Point loc_spawn = new Point();
            int height = (size.Height / 24) + 1;
            int width = (size.Width / 24) + 1;
            bool taken = true;
            int count_do = 10000;
            while (taken && count_do-- > 0)
            {
                taken = false;
                loc_spawn.X = Exilania.rand.Next(0, map.GetLength(0) - width);
                loc_spawn.Y = Exilania.rand.Next(0, map.GetLength(1) - height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (map[loc_spawn.X + x, loc_spawn.Y + y].fgd_block_type != -1 || map[loc_spawn.X, loc_spawn.Y + height].liquid_level > 49)
                            taken = true;
                    }
                }
                if (!taken && (map[loc_spawn.X, loc_spawn.Y].fgd_block_type != -1 || !map[loc_spawn.X, loc_spawn.Y].bkd_transparent || 
                    map[loc_spawn.X, loc_spawn.Y + height].fgd_block_type == -1 || map[loc_spawn.X, loc_spawn.Y + height].liquid_level > 49))
                {
                    taken = true;
                }

            }
            Point return_spot = new Point(loc_spawn.X * 24 + (int)(size.Width / 2), loc_spawn.Y * 24 + (int)(size.Height / 2));
            return return_spot;
        }

        public void update_world(float sec_fraction, Display d, Input i)
        {
            float world_start_time = world_time;
            world_time += sec_fraction;
            Particle.step_time(bullets, sec_fraction, this);
            Particle.step_time(particles, sec_fraction, this);
            piece_crafter.update(players[Exilania.game_my_user_id].avatar, this);
            
            players[Exilania.game_my_user_id].avatar.update_body(sec_fraction, world_start_time, d, i, this, true, true, Exilania.game_my_user_id);
            background.update(top_left, this);
            night.update_night((int)(world_time / day_length),world_time,night_percent_length,day_length);
            for (int x = 0; x < players.Count; x++)
            {
                if (x == Exilania.game_my_user_id)
                {
                    //players[x].items.input(
                }
                else if (players[x] != null && !players[x].is_player_empty)
                {
                    players[x].avatar.items.input(players[x].avatar, d, top_left);
                    players[x].avatar.update_body(sec_fraction, world_start_time, d, i, this, false, true, x);
                }
            }
            for (int x = 0; x < npcs.Count; x++)
            {
                if(!npcs[x].empty)
                    npcs[x].update_body(sec_fraction,world_start_time, d, i, this, false,false,x);
            }
            bool entered = false;
            while (elapsed_furniture_update_time > .050f)
            {
                entered = true;
                Furniture.dtime += 50;
                elapsed_furniture_update_time -= .050f;
                for (int x = 0; x < furniture.Count; x++)
                {
                    if (!furniture[x].flags[(int)FFLAGS.EMPTY])
                        furniture[x].update_furniture(this);
                }
            }
            if(!entered)
            {
                for(int x = 0; x < furniture.Count;x++)
                {
                    furniture[x].update_facets(this);
                }
            }
            
        }

        public int wraparound_x(int xloc)
        {
            if (xloc < 0)
                return xloc + map.GetLength(0);
            else if(xloc >= map.GetLength(0))
                return xloc - map.GetLength(0);
            return xloc;
        }

        public byte get_image_version(int locx, int locy, int match_block_number)
        {
            if (Exilania.block_types.blocks[map[wraparound_x(locx), locy].fgd_block].random_tiles)
            {
                return (byte)Exilania.rand.Next(0, Exilania.block_types.blocks[map[wraparound_x(locx), locy].fgd_block].image_pointers.Length - 1);
            }
            byte cur_score = 0;
            for (int i = 0; i < 4; i++)
            {
                int dx = xes[i];
                int dy = yes[i];
                if (locy + dy > -1 && locy + dy < map.GetLength(1))
                {
                    if (map[wraparound_x(locx), locy].fgd_block_type != map[wraparound_x(locx + dx), locy + dy].fgd_block_type)
                    {
                        cur_score += value[i];
                    }
                }
            }
            return cur_score;
        }

        public byte get_bkd_image_version(int locx, int locy, int match_block_number)
        {
            if (Exilania.block_types.blocks[map[wraparound_x(locx), locy].bkd_block].random_tiles)
            {
                return (byte)Exilania.rand.Next(0, Exilania.block_types.blocks[map[wraparound_x(locx), locy].bkd_block].image_pointers.Length - 1);
            }
            byte cur_score = 15;
            for (int i = 0; i < 4; i++)
            {
                int dx = xes[i];
                int dy = yes[i];
                if (locy + dy > -1 && locy + dy < map.GetLength(1))
                {
                    if (map[wraparound_x(locx), locy].bkd_block != -1 && map[wraparound_x(locx + dx), locy + dy].bkd_block != -1 
                        && Exilania.block_types.blocks[map[wraparound_x(locx), locy].bkd_block].block_group == 
                        Exilania.block_types.blocks[map[wraparound_x(locx + dx), locy + dy].bkd_block].block_group)
                    {
                        cur_score -= value[i];
                    }
                }
            }
            return cur_score;
        }

        public byte get_platform_version(int locx, int locy, int match_block_number)
        {
            int[] x_use = new int[] { -1, 1, };
            int[] y_use = new int[]  { 0, 0, };
            bool[] values = new bool[] { false, false };
            int cur_score = 0;
            for (int i = 0; i < 2; i++)
            {
                if (map[wraparound_x(locx + x_use[i]), locy + y_use[i]].fgd_block_type != -1)
                {
                    values[i] = true;
                    cur_score += (int)Math.Pow(2, i);
                }
            }

            if (cur_score == 0)
            {
                return 3;
            }
            else if (cur_score == 1)
            {
                return 2;
            }
            else if (cur_score == 2)
            {
                return 1;
            }
            else if (cur_score == 3)
            {
                return 0;
            }

            return 0;
        }

        public void determine_connectivity()
        {
            grass = new List<Point>();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y].fgd_block_type != -1)
                    {
                        if (map[x,y].fgd_block == grass_block_id)
                        {
                            grass.Add(new Point(x, y));
                        }
                        if (map[x, y].fgd_block_type == 5)
                        {
                            if (map[x, y].liquid_level > 116)
                                map[x, y].fgd_ver = 12;
                            else
                                map[x, y].fgd_ver = (byte)(map[x, y].liquid_level / 9);
                        }
                        else if (map[x, y].fgd_block_type == 4)
                        {
                            map[x, y].fgd_ver = (byte)0;
                        }
                        else if (map[x, y].fgd_block_type == 3)
                        {
                            map[x, y].fgd_ver = (byte)get_platform_version(x, y, map[x, y].fgd_block);
                        }
                        else
                            map[x, y].fgd_ver = (byte)get_image_version(x, y, map[x, y].fgd_block);
                    }
                    else
                        map[x, y].fgd_ver = 0;
                    if (map[x, y].bkd_block > -1 && Exilania.block_types.blocks[map[x, y].bkd_block].block_group != -1)
                    {
                        map[x, y].bkd_ver = (byte)get_bkd_image_version(x, y, map[x, y].bkd_ver);
                    }
                    else
                        map[x, y].bkd_ver = 0;
                }
            }
        }

        public void determine_connectivity(Rectangle area, sbyte type_replaced)
        {
            if (!disable_connectivity)
            {
                int use_x = 0;
                for (int x = area.X; x < area.Width + area.X; x++)
                {
                    for (int y = area.Y; y < area.Height + area.Y; y++)
                    {
                        use_x = wraparound_x(x);
                        if (y > -1 && y < map.GetLength(1))
                        {
                            if (map[use_x, y].fgd_block_type == -1)
                            {
                                map[use_x, y].fgd_ver = 0;
                            }
                            else if (map[use_x, y].fgd_block_type == 5)
                            {
                                if (map[use_x, y].liquid_level > 116)
                                    map[use_x, y].fgd_ver = 12;
                                else
                                    map[use_x, y].fgd_ver = (byte)(map[use_x, y].liquid_level / 9);
                            }
                            else if (map[use_x, y].fgd_block_type == 4)
                            {
                                map[use_x, y].fgd_ver = (byte)0;
                            }
                            else if (map[use_x, y].fgd_block_type == 3)
                            {
                                map[use_x, y].fgd_ver = (byte)get_platform_version(x, y, map[use_x, y].fgd_block);
                            }
                            else if(type_replaced == map[use_x,y].fgd_block_type)
                                map[use_x, y].fgd_ver = (byte)get_image_version(x, y, map[use_x, y].fgd_block);
                            
                        }
                    }
                }
            }
        }
        public void determine_bkd_connectivity(Rectangle area, sbyte type_replaced)
        {
            if (!disable_connectivity)
            {
                int use_x = 0;
                for (int x = area.X; x < area.Width + area.X; x++)
                {
                    for (int y = area.Y; y < area.Height + area.Y; y++)
                    {
                        use_x = wraparound_x(x);
                        if (y > -1 && y < map.GetLength(1))
                        {
                            //set background block type to use
                            if (map[use_x, y].bkd_block == -1 || type_replaced == -1 || Exilania.block_types.blocks[map[use_x, y].bkd_block].block_group == -1)
                            {
                                 map[use_x, y].bkd_ver = 0;
                            }
                            else if (Exilania.block_types.blocks[map[use_x, y].bkd_block].block_group == Exilania.block_types.blocks[type_replaced].block_group)
                            {
                                map[use_x, y].bkd_ver = (byte)get_bkd_image_version(use_x, y, map[use_x, y].bkd_block);
                            }
                        }
                            
                    }
                }
            }
        }

        public bool neighbor_occupied(Point loc_placing)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0)
                    {

                    }
                    else
                    {
                        if (y + loc_placing.Y < map.GetLength(1) && y + loc_placing.Y >= 0 && (
                            (map[wraparound_x(loc_placing.X + x), loc_placing.Y + y].bkd_block != -1) || 
                            (map[wraparound_x(loc_placing.X + x), loc_placing.Y + y].fgd_block_type != -1 &&
                            map[wraparound_x(loc_placing.X + x), loc_placing.Y + y].fgd_block_type != 5 )))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool can_break(Point loc)
        {
            if (map[loc.X, loc.Y].furniture_index != -1)
            {
                return false;
            }
            if (map[loc.X, loc.Y].plant_index != -1)
            {
                return false;
                
            }
            if (loc.Y > 0 && map[loc.X, loc.Y - 1].furniture_index != -1)
            {
                int furn_id = furniture[map[loc.X, loc.Y - 1].furniture_index].furniture_id;
                return Exilania.furniture_manager.furniture[furn_id].flags[(int)FFLAGS.BREAK_BELOW];
            }
            if(loc.Y > 0 && map[loc.X,loc.Y-1].plant_index != -1)
            {
                int plant_id = plants[map[loc.X, loc.Y-1].plant_index].plant_index;
                return Exilania.plant_manager.plants[plant_id].break_below;
            }
            return true;

        }

        public void draw_world(int focus_player, SpriteBatch s, Display d, Input i)
        { //blocks are 24 px wide/tall...
            Vector2 center = new Vector2(players[Exilania.game_my_user_id].avatar.world_loc.X, players[Exilania.game_my_user_id].avatar.world_loc.Y);
            center -= i.saved_delta;
            top_left = new Vector2(center.X - (Exilania.screen_size.X / 2), center.Y - (Exilania.screen_size.Y / 2));
            if (top_left.Y < 0)
                top_left.Y = 0;
            else if (top_left.Y > map.GetLength(1) * 24 - (Exilania.screen_size.Y ))
            {
                top_left.Y = map.GetLength(1) * 24 - Exilania.screen_size.Y;
            }
            int x_offset = get_leftover_pixels((int)top_left.X);
            int y_offset = get_leftover_pixels((int)top_left.Y);
            Point offset = new Point((int)top_left.X / 24, (int)top_left.Y / 24);
            float day_only_portion = world_time % day_length / day_length;
            float daylight_only = world_time % day_length / (day_length - (day_length * night_percent_length));

            Rectangle temp = new Rectangle(2028, 968, 20, 1080);

            s.Draw(d.backgrounds, new Rectangle(0, 0, Exilania.screen_size.X, Exilania.screen_size.Y), temp, calc_sky_color);


            s.Draw(d.sprites,
                new Rectangle((int)(Exilania.screen_size.X * daylight_only) - 64, 
                    get_sun_height((double)daylight_only),
                    (int)(16.0 * 8), (int)(16.0 * 8)), d.sun, Color.White);

            if (1f - day_only_portion < night_percent_length)
            {
                night.draw_night(s, d);
            }

            background.draw_background(s);

            Point screen_loc = new Point();
            Point cur_loc = new Point();
            List<int> on_screen_furn = new List<int>();
            //background block drawing
            for (int x = -1; x < Exilania.screen_size.X / 24 + 2; x++)
            {
                for (int y = -1; y < Exilania.screen_size.Y / 24 + 2; y++)
                {
                    cur_loc.X = -1;
                    cur_loc.Y = -1;
                    screen_loc.X = x;
                    screen_loc.Y = y;
                    //draw background blocks
                    if (x + offset.X < 0 && y + offset.Y > -1 && y + offset.Y < map.GetLength(1))
                    {
                        cur_loc.X = x + offset.X + map.GetLength(0);
                        cur_loc.Y = y + offset.Y;
                        if (map[x + offset.X + map.GetLength(0), y + offset.Y].bkd_block != -1)
                            map[cur_loc.X, cur_loc.Y].draw_bkd(s, d, x_offset, y_offset, screen_loc);
                    }
                    else if (x + offset.X > -1 && y + offset.Y > -1 && x + offset.X < map.GetLength(0) && y + offset.Y < map.GetLength(1))
                    {
                        cur_loc.X = x + offset.X;
                        cur_loc.Y = y + offset.Y;
                        if (map[x + offset.X, y + offset.Y].bkd_block != -1)
                            map[cur_loc.X, cur_loc.Y].draw_bkd(s, d, x_offset, y_offset, screen_loc);
                    }
                    else if (x + offset.X >= map.GetLength(0) && y + offset.Y > -1 && y + offset.Y < map.GetLength(1))
                    {
                        cur_loc.X = x + offset.X - map.GetLength(0);
                        cur_loc.Y = y + offset.Y;
                        if (map[x + offset.X - map.GetLength(0), y + offset.Y].bkd_block != -1)
                            map[cur_loc.X, cur_loc.Y].draw_bkd(s, d, x_offset, y_offset, screen_loc);
                    }
                    if (cur_loc.X != -1 && cur_loc.Y != -1)
                    {
                        if (map[cur_loc.X, cur_loc.Y].furniture_index != -1 && !on_screen_furn.Contains(map[cur_loc.X, cur_loc.Y].furniture_index))
                            on_screen_furn.Add(map[cur_loc.X, cur_loc.Y].furniture_index);
                        //map[cur_loc.X, cur_loc.Y].draw_furniture(s, d, x_offset, y_offset, screen_loc, furniture, cur_loc);
                        //map[cur_loc.X, cur_loc.Y].draw_plant(s, d, x_offset, y_offset, screen_loc, plants, cur_loc, this);
                    }

                }
            }
            //furniture and plant drawing
            for (int x = -1; x < Exilania.screen_size.X / 24 + 2; x++)
            {
                for (int y = -1; y < Exilania.screen_size.Y / 24 + 2; y++)
                {
                    cur_loc.X = -1;
                    cur_loc.Y = -1;
                    screen_loc.X = x;
                    screen_loc.Y = y;
                    //draw furniture blocks
                    if (x + offset.X < 0 && y + offset.Y > -1 && y + offset.Y < map.GetLength(1))
                    {
                        cur_loc.X = x + offset.X + map.GetLength(0);
                        cur_loc.Y = y + offset.Y;
                    }
                    else if (x + offset.X > -1 && y + offset.Y > -1 && x + offset.X < map.GetLength(0) && y + offset.Y < map.GetLength(1))
                    {
                        cur_loc.X = x + offset.X;
                        cur_loc.Y = y + offset.Y;
                    }
                    else if (x + offset.X >= map.GetLength(0) && y + offset.Y > -1 && y + offset.Y < map.GetLength(1))
                    {
                        cur_loc.X = x + offset.X - map.GetLength(0);
                        cur_loc.Y = y + offset.Y;
                    }
                    if (cur_loc.X != -1 && cur_loc.Y != -1)
                    {
                        map[cur_loc.X, cur_loc.Y].draw_furniture(s, d, x_offset, y_offset, screen_loc, furniture, cur_loc,this);
                        map[cur_loc.X, cur_loc.Y].draw_plant(s, d, x_offset, y_offset, screen_loc, plants, cur_loc, this);
                    }
                }
            }
            
            //draw enemies!
            for (int x = 0; x < npcs.Count; x++)
            {
                if (!npcs[x].empty)
                    npcs[x].draw_actor(s, 1f, d);
            }
            //draw the other players... just not the current player
            for (int x = 0; x < players.Count; x++)
            {
                if (x != Exilania.game_my_user_id && players[x] != null && !players[x].is_player_empty)
                    players[x].draw_player(s, d,this);
            }
            //draw the player
            if (Exilania.game_my_user_id != -1)
                players[Exilania.game_my_user_id].draw_player(s, d, this);
            for (int n = 0; n < on_screen_furn.Count; n++)
            {
                if (furniture[on_screen_furn[n]].facets.Count > 0)
                {
                    foreach (var t in furniture[on_screen_furn[n]].facets)
                    {
                        t.draw_rotated_facet(s, this);
                    }
                }
            }
            //draw foreground blocks
            for (int x = -1; x < Exilania.screen_size.X / 24 + 2; x++)
            {
                for (int y = -1; y < Exilania.screen_size.Y / 24 + 2; y++)
                {
                    screen_loc.X = x;
                    screen_loc.Y = y;
                    //draw foreground blocks
                    if (x + offset.X < 0 && y + offset.Y > -1 && y + offset.Y < map.GetLength(1) && 
                        (map[x + offset.X + map.GetLength(0), y + offset.Y].fgd_block_type != -1 || map[x + offset.X + map.GetLength(0), y + offset.Y].liquid_level > 0))
                    {
                        cur_loc.X = x + offset.X + map.GetLength(0);
                        cur_loc.Y = y + offset.Y;
                        map[cur_loc.X,cur_loc.Y].draw_fgd(s, d, x_offset, y_offset, screen_loc);
                    }
                    else if (x + offset.X > -1 && y + offset.Y > -1 && x + offset.X < map.GetLength(0) && y + offset.Y < map.GetLength(1) && 
                        (map[x + offset.X, y + offset.Y].fgd_block_type != -1 || map[x + offset.X, y + offset.Y].liquid_level > 0))
                    {
                        cur_loc.X = x + offset.X;
                        cur_loc.Y = y + offset.Y;
                        map[cur_loc.X, cur_loc.Y].draw_fgd(s, d, x_offset, y_offset, screen_loc);
                    }
                    else if (x + offset.X >= map.GetLength(0) && y + offset.Y > -1 && y + offset.Y < map.GetLength(1) && 
                        (map[x + offset.X - map.GetLength(0), y + offset.Y].fgd_block_type != -1 || map[x + offset.X - map.GetLength(0), y + offset.Y].liquid_level > 0))
                    {
                        cur_loc.X = x + offset.X - map.GetLength(0);
                        cur_loc.Y = y + offset.Y;
                        map[cur_loc.X, cur_loc.Y].draw_fgd(s, d, x_offset, y_offset, screen_loc);
                    }
                }
            }
            //draw life info above the npcs
            for (int x = 0; x < npcs.Count; x++)
            {
                if(!npcs[x].empty && (npcs[x].light_level.R+npcs[x].light_level.B+npcs[x].light_level.G) > 0)
                    npcs[x].draw_hud(s, d, false, true);
            }
            //draw life info above all the players (Except the current player, they already know how much life they have)
            for (int x = 0; x < players.Count; x++)
            {
                if (x != Exilania.game_my_user_id && players[x] != null && !players[x].is_player_empty &&
                    (players[x].avatar.light_level.R + players[x].avatar.light_level.B + players[x].avatar.light_level.G) > 0)
                    players[x].avatar.draw_hud(s, d, true, true);
            }
            //if (Exilania.game_my_user_id != -1)
            //    players[Exilania.game_my_user_id].avatar.draw_hud(s, d, true, true);
            Particle.draw_particles(bullets, d, s, this);
            Particle.draw_particles(particles, d, s, this);
            if(Exilania.settings.liquid_debugging)
                liquid_simulator.draw_liquid_cells(s,d,this,x_offset,y_offset,top_left);
            piece_crafter.draw_crafts(s, d, players[Exilania.game_my_user_id], this);
            
            //s.DrawString(d.small_font, "Rotated String", new Vector2(100, 100), Color.White, (float)(Math.PI / 2), new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
            //draw_mark_lines(CollisionHashTable.world_size_cell, s, d);
            //draw_debug_checks(s, d, x_offset, y_offset);
        }

        public void draw_debug_checks(SpriteBatch s, Display d, int x_offset, int y_offset)
        {
            /*Point draw_at = new Point();
            for (int i = 0; i < players[0].avatar.body.Count; i++)
            {
                if (players[0].avatar.body[i].name == "Left Arm")
                {
                    draw_at = new Point((int)players[0].avatar.body[i].grid_location.X, (int)players[0].avatar.body[i].grid_location.Y);
                }
            }

            s.Draw(d.sprites, new Rectangle(draw_at.X - 24, draw_at.Y - 48, 48, 96), new Rectangle(1118, 284, 48, 48), Color.White);
            */
            Rectangle on_screen = new Rectangle(0,0, Exilania.screen_size.X, Exilania.screen_size.Y);
            foreach (var p in areas_checked)
            {
                Rectangle temp = new Rectangle(p.X * 24 - (int)top_left.X, (p.Y * 24) - (int)top_left.Y, 24, 24);
                if (on_screen.Intersects(temp))
                {
                    s.Draw(d.sprites, temp, d.frames[1021], Color.White);
                }
            }
            if (((world_time/day_length)%1f) < .01f)
            {
                areas_checked.Clear();
            }
        }
            

        public void draw_mark_lines(int spacing, SpriteBatch s, Display d)
        {
            Point top_left_mark_line = new Point(0, 0);
            Rectangle source = new Rectangle(1143,71,1,1);
            int first_mult = (int)top_left.X / spacing;
            top_left_mark_line.X = first_mult;
            while (first_mult * spacing < top_left.X + Exilania.screen_size.X)
            {
                s.Draw(d.sprites, new Rectangle(first_mult++ * spacing - (int)top_left.X, 0, 1, Exilania.screen_size.Y), source, Color.White);
            }
            first_mult = (int)top_left.Y / spacing;
            top_left_mark_line.Y = first_mult;
            while (first_mult * spacing < top_left.Y + Exilania.screen_size.Y)
            {
                s.Draw(d.sprites, new Rectangle(0, first_mult++ * spacing - (int)top_left.Y, Exilania.screen_size.X, 1), source, Color.White);
            }
            int use_x = 0;
            int use_y = 0;
            for (int x = (top_left_mark_line.X-1) * spacing - (int)top_left.X; x < Exilania.screen_size.X; x += spacing)
            {
                for (int y = top_left_mark_line.Y * spacing - (int)top_left.Y; y < Exilania.screen_size.Y; y += spacing)
                {
                    use_x = ((x + (int)top_left.X) / spacing);
                    if (use_x < 0)
                        use_x += (map.GetLength(0) * 24) / spacing;
                    else if (use_x >= map.GetLength(0) * 24 / spacing)
                    {
                        use_x -= (map.GetLength(0) * 24) / spacing;
                    }
                    use_y = ((y + (int)top_left.Y) / spacing);
                    d.draw_text(s, d.small_font, "@07{" + use_x + "," + ((y + (int)top_left.Y) / spacing) + "}" + collision_table.cells[use_x,use_y].ToString(), x, y, spacing);
                }
            }
        }

        public void draw_hud(int focus_player, SpriteBatch s, Display d, Input i)
        {
            if (players[Exilania.game_my_user_id].avatar.is_furn_clicking &&
                map[players[Exilania.game_my_user_id].avatar.furn_start_click.X, players[Exilania.game_my_user_id].avatar.furn_start_click.Y].furniture_index != -1)
            {
                Point from = furniture[map[players[Exilania.game_my_user_id].avatar.furn_start_click.X, 
                    players[Exilania.game_my_user_id].avatar.furn_start_click.Y].furniture_index].get_rect().Center;
                Point to = players[Exilania.game_my_user_id].avatar.input.mouse_loc;
                Point furn_end_click = new Point(wraparound_x(players[Exilania.game_my_user_id].avatar.input.mouse_loc.X / 24), players[Exilania.game_my_user_id].avatar.input.mouse_loc.Y / 24);
                if (map[furn_end_click.X, furn_end_click.Y].furniture_index != -1)
                {
                    to = furniture[map[furn_end_click.X, furn_end_click.Y].furniture_index].get_rect().Center;
                }
                //int true_x_dist = Acc.get_signed_min_horizontal_distance(from.X, to.X, map.GetLength(0) * 24);
                float angle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
                //float angle = (float)Math.Atan2(to.Y - from.Y, true_x_dist);
                //double length = Acc.get_distance(0, from.Y, true_x_dist, to.Y);
                double length = Acc.get_distance(from, to);
                if (length > CollisionHashTable.world_size_cell)
                    length = CollisionHashTable.world_size_cell;
                s.Draw(d.sprites, new Rectangle(from.X - (int)top_left.X, from.Y - (int)top_left.Y, (int)length, 2), new Rectangle(443, 552, 3, 1), Color.White, angle, new Vector2(0, 1), SpriteEffects.None, 0);
                d.draw_text_with_outline(s, d.small_font, "@00From: " + 
                    players[Exilania.game_my_user_id].avatar.furn_start_click.ToString() + ", To: " + furn_end_click.ToString(), 5,1060,1920,AccColors.Black); 
            }
            if (i.keys_now.Contains(Keys.Q) && i.current_input_type == InputConsume.Normal)
            {
                List<int> furn_vis = collision_table.get_furniture_in_range(new Point((int)top_left.X + Exilania.screen_size.X/2,(int)top_left.Y + Exilania.screen_size.Y/2),Exilania.screen_size.X,this);
                Point use = new Point();
                int single_furn = map[wraparound_x(players[Exilania.game_my_user_id].avatar.input.mouse_loc.X / 24), players[Exilania.game_my_user_id].avatar.input.mouse_loc.Y / 24].furniture_index;
                for (int x = 0; x < furn_vis.Count; x++)
                {
                    if ((map[furniture[furn_vis[x]].top_left.X, furniture[furn_vis[x]].top_left.Y].light_level.R > 0
                        || map[furniture[furn_vis[x]].top_left.X, furniture[furn_vis[x]].top_left.Y].light_level.G > 0
                        || map[furniture[furn_vis[x]].top_left.X, furniture[furn_vis[x]].top_left.Y].light_level.B > 0))
                    {
                        if ((single_furn == furn_vis[x] || single_furn == -1))
                        {
                            use = new Point(furniture[furn_vis[x]].get_rect().Center.X - (int)top_left.X, furniture[furn_vis[x]].get_rect().Center.Y - (int)top_left.Y);
                            furniture[furn_vis[x]].hover_over(use, s, d, this, (single_furn == furn_vis[x] || single_furn == -1));
                        }
                    }
                }
                int mouse_x = (int)players[Exilania.game_my_user_id].avatar.input.mouse_loc.X;
                if (mouse_x < 0)
                {
                    mouse_x /= 24;
                    mouse_x--;
                }
                else
                    mouse_x /= 24;
                Point mouse_world_loc = new Point(wraparound_x(mouse_x), players[focus_player].avatar.input.mouse_loc.Y / 24);
                if ((map[mouse_world_loc.X, mouse_world_loc.Y].light_level.R > 0
                    || map[mouse_world_loc.X, mouse_world_loc.Y].light_level.G > 0 || map[mouse_world_loc.X, mouse_world_loc.Y].light_level.B > 0))
                {
                    if (map[mouse_world_loc.X, mouse_world_loc.Y].fgd_block != -1)
                    {
                        Exilania.block_types.blocks[map[mouse_world_loc.X, mouse_world_loc.Y].fgd_block].hover_over(i.mouse_cur_spot, s, d, this);
                    }
                    else if (map[mouse_world_loc.X, mouse_world_loc.Y].plant_index != -1)
                    {
                        Exilania.plant_manager.plants[plants[map[mouse_world_loc.X, mouse_world_loc.Y].plant_index].plant_index].hover_over(i.mouse_cur_spot, s, d, this);
                    }
                }
            }
            if (Exilania.input.ctrl_state == 1)
            {
                Vector2 dzise = d.small_font.MeasureString(Acc.sanitize_text_color("@00Double Tap Ctrl to return to normal view"));
                d.draw_text_with_outline(s, d.small_font, "@08Double Tap Ctrl to return to normal view", Exilania.screen_size.X / 2 - (int)(dzise.X / 2), Exilania.screen_size.Y - (int)dzise.Y,1000,AccColors.Black);
            }
            if (players[Exilania.game_my_user_id].avatar.break_block_swing > 0)
            {
                Vector2 size = d.small_font.MeasureString((players[Exilania.game_my_user_id].avatar.break_block_swings_needed - players[Exilania.game_my_user_id].avatar.break_block_swing).ToString());
                d.draw_text(s, d.small_font, "@00" + (players[Exilania.game_my_user_id].avatar.break_block_swings_needed - players[Exilania.game_my_user_id].avatar.break_block_swing), 
                    players[Exilania.game_my_user_id].avatar.last_break_block_loc.X * 24 + 12 - (int)size.X/2 - (int)top_left.X, players[Exilania.game_my_user_id].avatar.last_break_block_loc.Y * 24 - (int)top_left.Y, 50);
            }
            s.Draw(d.sprites, new Rectangle(550, 10, 100, 60), new Rectangle(1502, 92, 100, 60), Color.White);
            players[Exilania.game_my_user_id].avatar.items.draw(s, d);
            players[Exilania.game_my_user_id].avatar.stats.draw_stats(s);
            if (players[Exilania.game_my_user_id].avatar.active_chest_id != -1)
            {
                chests[players[Exilania.game_my_user_id].avatar.active_chest_id].draw_inventory(s, d, players[focus_player].avatar);
            }
            //d.draw_text(s, d.small_font,"@01Power: " + players[focus_player].avatar.power.ToString(), 100, 100, 500);
            if (!players[focus_player].avatar.items.temporary.is_empty)
                players[focus_player].avatar.items.temporary.draw_cubby(s, d, true);
            players[focus_player].avatar.items.hover_inventory(this, players[focus_player].avatar, s, d);

            if (i.alt && (
                (players[focus_player].avatar.items.cur_left_hand != -1 && players[focus_player].avatar.items.hotbar_items[players[focus_player].avatar.items.cur_left_hand].is_block)
                || (players[focus_player].avatar.items.cur_right_hand != -1 && players[focus_player].avatar.items.hotbar_items[players[focus_player].avatar.items.cur_right_hand].is_block)
                ))
            {
                Rectangle t = new Rectangle(players[focus_player].avatar.input.mouse_loc.X,players[focus_player].avatar.input.mouse_loc.Y,48,48);
                t.X -= t.X % 24 + (int)top_left.X;
                if (players[focus_player].avatar.world_loc.X < map.GetLength(0) * 12 && 
                    wraparound_x((int)(players[Exilania.game_my_user_id].avatar.input.mouse_loc.X / 24)) > map.GetLength(0)/2)
                {
                    t.X -= 24;
                }
                t.Y -= t.Y % 24 + (int)top_left.Y;
                s.Draw(d.sprites, t, d.frames[1021], Color.White);
            }
            //drawing clock
            s.Draw(d.sprites, new Rectangle(207, 13, 80, 40), new Rectangle(1370,93,80,40), Color.White);
            cur_hour = (((((world_time + (day_length * .25f)) % day_length) / day_length) * 24f));
            cur_minute = ((((world_time + (day_length * .25f)) % day_length) / day_length * 1440) % 60);
            string hour = ((int)cur_hour).ToString().PadLeft(2,'0');
            Vector2 measure_s = Exilania.display.small_font.MeasureString(hour);
            string minutes = ((int)cur_minute).ToString().PadLeft(2,'0');
            Exilania.display.draw_text(s, Exilania.display.small_font, "@04" + hour , 232 - (int)(measure_s.X/2) , 21, 30);
            measure_s = Exilania.display.small_font.MeasureString(minutes);
            Exilania.display.draw_text(s, Exilania.display.small_font, "@04" + minutes, 264 - (int)(measure_s.X/2), 21, 30);
            if (((int)world_time) % 2 == 0)
            {
                s.Draw(d.sprites, new Rectangle(245, 27, 4, 11), new Rectangle(1446, 134, 4, 11), Color.White);
            }
            if (!minimap.full_map_activated)
            {
                minimap.draw_view(s, new Rectangle(5, 150, 200, 200),
                    new Rectangle((int)(top_left.X / 24) + (Exilania.screen_size.X / 48) - 100, (int)(top_left.Y / 24) + (Exilania.screen_size.Y / 48) - 100, 200, 200),
                    //new Rectangle(1,500,300,300),
                    this, Color.FromNonPremultiplied(255, 255, 255, 195));
            }
            else
            {
                Point view_port_size = new Point(Exilania.screen_size.X - 10, Exilania.screen_size.Y - 200);
                minimap.draw_view(s, new Rectangle(5, 150, Exilania.screen_size.X - 5, Exilania.screen_size.Y - 200),
                    new Rectangle((int)(top_left.X / 24) + (Exilania.screen_size.X / 48) - (view_port_size.X / 2), 
                        (int)(top_left.Y / 24) + (Exilania.screen_size.Y / 48) - (view_port_size.Y/2), view_port_size.X,view_port_size.Y),
                        this, Color.FromNonPremultiplied(255, 255, 255, 195));
            }
        }
        
        public int get_sun_height(double percent_of_day)
        {
            percent_of_day -= .5;
            //y=-x^2+.85
            percent_of_day *= (percent_of_day);
            percent_of_day *= 3;
            return 100 + (int)(percent_of_day * Exilania.screen_size.Y);
        }

        /// <summary>
        /// this is the function called by the main game update class... this function will then update all actors and the world in which it is running.
        /// after it is called, it will call update_world.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="input"></param>
        /// <param name="display"></param>
        public void world_update_logic(float elapsed_time, Input input, Display display, Exilania e)
        {
            updating.start();
            if (Exilania.game_my_user_id != -1)
                players[Exilania.game_my_user_id].get_input(elapsed_time,input, display, this, e);
            for (int x = 0; x < npcs.Count; x++)
            {
                if(!npcs[x].empty)
                    npcs[x].input.update_input_ai(display, this, npcs[x]);
            }
            if (Exilania.pause_game)
            {
                update_world(0, display, input);
            }
            else
            {
                update_world(elapsed_time, display, input);
            }
            elapsed_furniture_update_time += elapsed_time;
            skip_frames++;
            if (skip_frames % 3 > 0)
            {
                elapsed_light_update_time += 17;
            }
            else
            {
                elapsed_light_update_time += 16;
            }
            if (elapsed_light_update_time >= 150 && !Exilania.settings.use_hardware_lighting)
            {
                elapsed_light_update_time -= 150;
                light_time.start();
                do_composition_lighting();
                light_time.stop();
            }
            flowing_blocks.start();
            freeflowing_blocks();
            flowing_blocks.stop();
            if (world_time >= ((tiles_spread_frame + 1) * tiles_spread_sec_between_frame))
            {
                tiles_spread_frame++;
                if (is_daytime)
                {
                    spread_block(Exilania.block_types.get_block_by_name("Grass"), Exilania.block_types.get_block_by_name("Dirt"), true);
                    if (Exilania.game_server)
                        Exilania.plant_manager.update_growing_plants(this);
                }
            }
            
            updating.stop();
            water_update_time.start();
            liquid_simulator.update(this, elapsed_time,false);
            water_update_time.stop();
            minimap.update_markers(this);
            //Exilania.debug = players[Exilania.game_my_user_id].avatar.ToString();
        }

        public override string ToString()
        {
            int mouse_x = (int)players[Exilania.game_my_user_id].avatar.input.mouse_loc.X;
            if(mouse_x < 0)
            {
                mouse_x /= 24;
                mouse_x--;
            }
            else
                mouse_x /= 24;
            Point mouse_loc = new Point(wraparound_x(mouse_x),
                (int)(players[Exilania.game_my_user_id].avatar.input.mouse_loc.Y / 24));
            if (mouse_loc.Y > -1 && mouse_loc.Y < map.GetLength(1))
                return "Mouse Loc: " + mouse_loc.ToString() + " " //+ map[wraparound_x(mouse_loc.X),mouse_loc.Y].ToString() 
                    + " " + (map[wraparound_x(mouse_loc.X),mouse_loc.Y].liquid_cell_id>
                    -1 ? liquid_simulator.liquid_cells[map[wraparound_x(mouse_loc.X), mouse_loc.Y].liquid_cell_id].ToString() : "No Liquid") + " " +
                    (int)(players[Exilania.game_my_user_id].avatar.input.mouse_loc.X);
            //return players[Exilania.game_my_user_id].avatar.input.mouse_loc.ToString() + " total water: " + total_world_water;
            return players[Exilania.game_my_user_id].avatar.ToString();
            //return night.screen_loc.ToString();
        }
    }
}
