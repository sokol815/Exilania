using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;

namespace Exilania
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Exilania : Microsoft.Xna.Framework.Game
    {
        public static Point windowed_size = new Point(1200, 800);
        public static bool allow_hi_def = false;
        public static int light_buffer_scaling_factor = 4;
        public static long time;
        public static System.IO.StreamWriter text_stream;
        public static SavedPlayers saved_players;
        public static int game_my_user_id = 0;
        public static int cur_using_local_id = 0;
        public static string home = "127.0.0.1";
        public static string my_ip_address = "";
        public static string network_ip_address = "127.0.0.1";
        public static NetPeerConfiguration udp_server_config;
        public static NetPeerConfiguration udp_client_config;
        public static string network_status = "EMPTY";
        public static string play_type = "Single Player";
        public static bool multiplayer_connected = false;
        public static bool start_full_screen;
        public static int socket = 6453;
        public static int server_socket = 6454;
        public static Server network_server;
        public static bool game_server = false;
        public static Client network_client;
        public static bool game_client = false;
        public static bool pause_game = false;
        public static bool game_accept_outside = false;
        public static bool disable_chat = false;
        public static long game_code = DateTime.Now.Ticks;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Display display = new Display();
        public static Settings settings;
        MainMenu menu;
        public static RenderTarget2D renderTarget;
        public static Point screen_size;
        public static bool draw_debug = false;
        public static bool take_screen_shot = false;
        public static string debug = Display.color_code + "06hello";
        public static int seed_id = 0;
        public static int gstate = 10;
        public static int previous_rocker_state = 0;
        public static long time_elapsed = 0;
        public static int people_connected = 0;
        public static List<BodyTemplate> body_templates;
        public static Input input = new Input();
        public static Lidgren.Network.NetRandom rand;
        //public static Random rand;
        bool server_running = false;
        public World world;
        public static BlockManager block_types;
        public static ItemManager item_manager;
        public static FurnitureManager furniture_manager;
        public static PlantManager plant_manager;
        public static MaterialManager material_manager;
        public static CraftManager crafting_manager;
        public static WorldManager world_manager;
        public static WorldCreator world_definition_manager;
        public static ParticleManager particle_manager;
        public static Lighting lighting;
        public static bool changing_graphics_state = false;
        public static Sounds sounds;
        public static float long_skip = 0f;
        public static float cur_leftover_time = 0f;
        public static float time_wedge = 1f / 60f;
        public static string cur_open_world = "";
        public static float cur_showing = 1f;

        public Effect none;

        public static SamplerState _clampTextureAddressMode=new SamplerState  
                                             {  
                                                AddressU = TextureAddressMode.Clamp,  
                                                AddressV = TextureAddressMode.Clamp  
                                             }; 

        System.IO.StreamReader r;

        public Exilania()
        {

            if (GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef))
            {
                windowed_size = new Point(1200, 800);
                allow_hi_def = true;
            }
            //this.IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            if(allow_hi_def)
                graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            r = new System.IO.StreamReader("exilania.ini");
            settings = new Settings(r);
            r.Close();
            // TODO: Add your initialization logic here
            if (!settings.use_custom_dimensions)
            {
                graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                screen_size = new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            }
            else
            {
                try
                {
                    graphics.PreferredBackBufferHeight = settings.custom_height;
                    graphics.PreferredBackBufferWidth = settings.custom_width;
                    screen_size = new Point(settings.custom_width, settings.custom_height);
                }
                catch
                {
                    graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                    graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                    screen_size = new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
                }
            }
            graphics.ToggleFullScreen();
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            graphics.ApplyChanges();
            screen_size = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            try
            {
                System.IO.Directory.CreateDirectory(@"logs");
            }
            catch
            {

            }
            text_stream = new System.IO.StreamWriter(@"logs/debug" + DateTime.Now.Ticks.ToString() + ".txt");
            text_stream.AutoFlush = true;
            text_stream.WriteLine("Open the document");

            block_types = new BlockManager();
            BlockData.enum_blocks();
            World.liquid_blocks = new int[] { 0,(int)block_types.get_block_by_name("Water") };
            //BlockData.export_block_data(block_types.blocks);
            item_manager = new ItemManager();
            furniture_manager = new FurnitureManager();
            plant_manager = new PlantManager();
            material_manager = new MaterialManager();
            crafting_manager = new CraftManager();
            world_manager = new WorldManager();
            world_definition_manager = new WorldCreator();
            particle_manager = new ParticleManager();
            
            if (settings.use_seed)
            {
                rand = new Lidgren.Network.NetRandom(settings.seed_id);
            }
            else
                rand = new Lidgren.Network.NetRandom((int)System.DateTime.Now.Ticks);
            menu = new MainMenu();
            body_templates = new List<BodyTemplate>();
            r = new System.IO.StreamReader("bodies.txt");
            while (!r.EndOfStream)
            {
                body_templates.Add(new BodyTemplate(r,body_templates.Count));
            }
            r.Close();
            saved_players = new SavedPlayers();
            draw_debug = settings.debugging;
            seed_id = (int)DateTime.Now.Ticks;
            if (settings.use_seed)
            {
                seed_id = settings.seed_id;
            }
            display.add_message(Display.color_code + "00Welcome to Exilania. Seed ID: " + seed_id.ToString());
            rand = new Lidgren.Network.NetRandom(seed_id);
            sounds = new Sounds();
            
            //CubicSpline cs = new CubicSpline(new double[]{1,10,30,60,100},new double[]{1,2,5,9,13});
            CubicSpline cs = new CubicSpline(new double[] { 0, 4, 10, 30, 100 }, new double[] { .05, .4, .7, 1, 1 });
            int total_cost = 0;
            for (float i = 0; i < 100.1f; i+=.5f)
            {
                total_cost += (int)cs.get_val_at(i);
                //Exilania.display.add_message("@05Cubic Spline[" + (i).ToString().PadLeft(3,'0') + "]:" + cs.get_val_at(i));
                //text_stream.WriteLine(Acc.sanitize_text_color(display.messages[display.messages.Count - 1]));
            }
            //Exilania.display.add_message("@06To get to level 100, you would need " + total_cost + " points.");

            base.Initialize();
        }

        /// <summary>
        /// initializes networking, will also cut all connections (currently not nice exit)
        /// </summary>
        /// <param name="p_game_server"></param>
        /// <param name="p_game_client"></param>
        public void initialize_networking()
        {
            if (game_client)
            {
                if (network_client == null)
                    network_client = new Client();
                if (game_my_user_id > -1 && !game_server)
                {
                    network_client.quit((byte)game_my_user_id);
                    game_my_user_id = -1;
                    people_connected = 0;
                }
                network_client.udp_client.Shutdown("Program Restarting");
            }
            if (game_server)
            {
                if (network_server == null)
                    network_server = new Server();
                if (world != null)
                {
                    for (int x = 0; x < world.players.Count; x++)
                    {
                        if (x != game_my_user_id)
                            network_client.quit((byte)x);
                    }
                }
                network_server.udp_server.Shutdown("Program Restarting");
                game_my_user_id = -1;
                people_connected = 0;
            }

            switch (play_type)
            {
                case "Single Player":
                    game_server = true;
                    game_client = true;
                    game_accept_outside = false;
                    game_my_user_id = -1;
                    people_connected = 0;
                    break;

                case "MultiPlayer Host":
                    game_server = true;
                    game_client = true;
                    game_accept_outside = true;
                    game_my_user_id = -1;
                    people_connected = 0;
                    break;

                case "MultiPlayer Join":
                    game_server = false;
                    game_client = true;
                    game_accept_outside = true;
                    game_my_user_id = -1;
                    people_connected = 0;
                    break;
            }

            udp_client_config = new NetPeerConfiguration("ExilaniaNetworking-0.01");
            //udp_client_config.SimulatedMinimumLatency = .1f;
            udp_client_config.Port = socket;
            udp_server_config = new NetPeerConfiguration("ExilaniaNetworking-0.01");
            //udp_server_config.SimulatedMinimumLatency = .1f;
            udp_server_config.Port = server_socket;

            if (game_server)
            {
                if (network_server == null)
                    network_server = new Server();
                else
                    network_server.udp_server.Shutdown("Releasing the socket");
                network_server.connect(this);
            }
            //else if(network_server!=null && network_server.tcp_server != null)
            //network_server.tcp_server.Shutdown("Releasing the socket");

            if (game_client)
            {
                if (network_client == null)
                    network_client = new Client();
                else if (network_client.udp_client.Connections.Count > 0)
                    network_client.udp_client.Shutdown("Releasing the socket");
                switch (play_type)
                {
                    case "Single Player":
                        network_client.connect(home, server_socket, this);
                        break;

                    case "MultiPlayer Host":
                        network_client.connect(home, server_socket, this);
                        break;

                    case "MultiPlayer Join":
                        network_client.connect(network_ip_address, server_socket, this);
                        break;
                }
            }
            //else
            //    network_client.tcp_client.Shutdown("Releasing the socket");
        }

        /// <summary>
        /// initialize the game, should also initialize networking! this is getting exciting!
        /// </summary>
        /// <param name="game_setting">0 = singleplayer; 1 = multiplyer host; 2 = multiplayer join</param>
        public void initialize_game(int game_setting)
        {
            socket = settings.client_port;
            server_socket = settings.server_port;
            network_ip_address = settings.server_ip;
            switch (game_setting)
            {
                case 0: //singleplayer
                    network_ip_address = "127.0.0.1";
                    play_type = "Single Player";
                    break;
                case 1: //multiplayer host
                    network_ip_address = "127.0.0.1";
                    play_type = "MultiPlayer Host";
                    break;
                case 2: //multiplayer join
                    network_ip_address = settings.server_ip;
                    play_type = "MultiPlayer Join";
                    break;
            }
            initialize_networking();
            gstate = 60; //state 60 is choosing player/creating a new player
        }

        /// <summary>
        /// this function physically initializes the world.
        /// </summary>
        public void start_world(string world_name, GraphicsDevice g)
        {
            input.ctrl_state = 0;
            input.saved_delta = new Vector2();
            input.last_ctrl_press = 0;
            if (settings.use_seed)
            {
                rand = new Lidgren.Network.NetRandom(settings.seed_id);
            }
            else
                rand = new Lidgren.Network.NetRandom((int)System.DateTime.Now.Ticks);
            if (cur_open_world != world_name)
                world = new World(world_name,g);
            else
            {
                world.world_ending = false;
            }
            cur_open_world = world_name;
            network_client.send_user_info(saved_players.players[cur_using_local_id]);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            display.font = Content.Load<SpriteFont>("Corbel_Big");
            display.small_font = Content.Load<SpriteFont>("Lucida_Tiny");
            display.middle_font = Content.Load<SpriteFont>("Constantia_twenty");
            display.sprites = Content.Load<Texture2D>("Exilania");
            display.planet_bkd = Content.Load<Texture2D>("planet8152012");
            display.Exilania_title = Content.Load<Texture2D>("Exilaniatitle");
            display.backgrounds = Content.Load<Texture2D>("background");
            lighting = new Lighting(Content,GraphicsDevice);
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            sounds.load_content(Content);
            
           
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void do_networking()
        {
            if (game_client)
            {
                network_client.client_messages(this, display, ref world);
            }
            if (game_server)
            {
                network_server.server_messages(this, display, world);
            }
        }

        public void exit_game()
        {
            
            if (network_client != null && network_client.udp_client.ConnectionsCount > 0 && game_my_user_id > -1)
            {
                network_client.quit((byte)game_my_user_id);
                saved_players.players[cur_using_local_id] = world.players[game_my_user_id];
            }
            saved_players.save_players();
            this.Exit();
        }

        public void state_progression()
        {
            switch (gstate)
            {
                case 70: //progress from character creation to character selection
                    if (!Exilania.settings.force_new_character)
                    {
                        gstate = 60;
                    }
                    else
                    {
                        gstate = 10;
                        network_client.quit((byte)game_my_user_id);
                    }
                    disable_chat = false;
                    break;
                case 90: // choosing a character to use
                    gstate = 60;
                    break;
                case 91: // choosing a new world to create
                    gstate = 90;
                    disable_chat = false;
                    break;
                case 92: // can't go back... must finish creating the world.
                    display.add_message("@08Please wait for the World Generator to finish.");
                    break;
                case 100: //in a game, quitting!
                    saved_players.players[cur_using_local_id] = world.players[game_my_user_id];
                    saved_players.save_players();
                    network_client.quit((byte)game_my_user_id);
                    world.write_world();
                    gstate = 10;
                    break;
                default:
                    disable_chat = false;
                    gstate = 10;
                    break;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            time = DateTime.Now.Ticks / 10000;
            
            do_networking();
            
            switch (input.process_input(this,gameTime.ElapsedGameTime.Milliseconds))
            {
                case -1:
                    if (Exilania.input.current_input_type!= InputConsume.Normal)
                    {
                        Exilania.input.current_input_type = InputConsume.Normal;
                        display.show_messages = false;
                    }
                    else
                    {
                        if (gstate != 10)
                            state_progression();
                        else
                            exit_game();
                    }
                    break;
                case -2: //F12 causes this to occur
                    take_screen_shot = true;
                    break;
                case 1: //F11 causes this to occur.
                    
                    break;
                case 2: //F2
                    if (game_server)
                    {
                        world.world_time -= world.world_time % world.day_length;
                        network_server.send_world_time(world.world_time);
                    }
                    break;
                case 4:// f3
                    if (world != null )
                    {
                        int tot = 0;
                        for (int i = 0; i < world.liquid_simulator.liquid_cells.Count; i++)
                        {
                            if (!world.liquid_simulator.liquid_cells[i].empty)
                                tot += world.liquid_simulator.liquid_cells[i].tot_liquid_level;
                        }
                        Exilania.display.add_message("@10Total Liquid: " + tot);
                        world.npcs.Add(new Actor(body_templates[Exilania.rand.Next(1,body_templates.Count)], false));
                        world.npcs[world.npcs.Count - 1].world_loc = world.players[game_my_user_id].avatar.world_loc;
                    }
                    break;
                case 5:// pressed F10, save a picture of the world!
                    if (world != null)
                    {
                        world.minimap.save_to_file(world);
                    }
                    break;
                case 20:
                    /*if (world != null && game_my_user_id > -1)
                    {
                        display.fading_text.Add(new FadeText("@00+1000 Water", 620,
                            (int)world.players[game_my_user_id].avatar.world_loc.X, (int)world.players[game_my_user_id].avatar.world_loc.Y, true, true));
                        world.players[game_my_user_id].avatar.items.pickup_block(block_types.get_block_by_name("Water"), 1000);
                        world.players[game_my_user_id].avatar.stats.get_experience_crafting(world.players[game_my_user_id].avatar.stats.level + 1, world.players[game_my_user_id].avatar);
                        for (int x = 0; x < world.map.GetLength(0); x++)
                        {
                            for (int y = 0; y < world.map.GetLength(1); y++)
                            {
                                if (world.map[x, y].bkd_block != -1)
                                    world.remove_block(new Point(x, y), true);
                            }
                        }
                    }
                    for (int x = 0; x < world.map.GetLength(0); x++)
                    {
                        for (int y = 0; y < world.map.GetLength(1); y++)
                        {
                            world.map[x, y].liquid_cell_id = -1;
                            world.map[x, y].liquid_id = 0;
                            world.map[x, y].liquid_level = 0;
                        }
                    }
                    world.liquid_simulator.liquid_cells.Clear();*/
                    break;
                default:
                    break;
            }

            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11) && !input.keys_previous.Contains(Microsoft.Xna.Framework.Input.Keys.F11))
            {
                changing_graphics_state = true;
                ToggleDisplay();
            }

            switch (gstate)
            {
                //main menu
                case 10:
                    menu.update_menu(input, this);
                    break;
                case 60: //character selection menu
                    saved_players.update(input, this);
                    break;
                case 70:
                    saved_players.update_create_character(input, this);
                    break;
                case 80:
                    //initializing game with chosen character
                    if (game_server)
                    {
                        gstate = 90;
                    }
                    else
                    {
                        network_client.send_user_info(saved_players.players[cur_using_local_id]);
                        gstate = 95;
                    }
                    break;
                case 90: //choosing world to use (if host)
                    if (game_server)
                    {
                        world_manager.update(input, this);
                    }
                    else
                    {
                        gstate = 95;
                    }
                    break;
                case 91: //naming a new world and choosing world type to be created.
                    world_definition_manager.update_choose_name(input, this);
                    break;
                case 92: //generating a new world
                    world_definition_manager.creating_world_update();
                    break;
                case 99: //server initializing game
                    if (game_my_user_id != -1)
                    {
                        gstate = 100;
                    }
                    break;
                //in game
                case 100:
                    //the code below makes people flash!
                  /*  float transfer_speed = 3f;
                    long p2 = System.DateTime.Now.Ticks / 10000;
                    float pos = (float)(p2 % (int)(transfer_speed * 1000)); //milliseconds passed total
                    pos /= 1000f * transfer_speed;

                    if (pos > .5f)
                    {
                        pos = 1f - pos;
                    }
                    pos *= 2f;
                    cur_showing = pos;*/
                   
                    cur_leftover_time += (gameTime.ElapsedGameTime.Milliseconds / 1000f);
                    while (cur_leftover_time >= time_wedge)
                    {
                        cur_leftover_time -= time_wedge;
                        world.world_update_logic(time_wedge, input, display, this);
                    }
                    if (settings.use_hardware_lighting)
                    {
                        world.light_time.start();
                        lighting.update_lighting(world);
                        world.light_time.stop();
                    }

                    break;
            }

            
            if (world == null || game_my_user_id == -1 || world.players.Count <= game_my_user_id)
                display.input_message_display(input, settings, null,null);
            else
                display.input_message_display(input, settings, world.players[game_my_user_id].avatar,world);


            
            // TODO: Add your update logic here
            display.update(gameTime.ElapsedGameTime.Milliseconds, world);

            input.finish_input();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (take_screen_shot)
            {
                renderTarget = new RenderTarget2D(GraphicsDevice, screen_size.X, screen_size.Y);
                GraphicsDevice.SetRenderTarget(renderTarget);
                
            }
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            Vector2 size = new Vector2();
            switch (gstate)
            {
                case 10: //title screen
                    menu.draw_menu(spriteBatch, display);
                    break;
                case 60:
                    saved_players.draw_list(spriteBatch, display);
                    break;
                case 70:
                    saved_players.draw_create_chracter(spriteBatch, display, input);
                    break;
                case 90://server choosing world to use or to create a new world.
                    world_manager.draw_choosing_menu(spriteBatch, display);
                    break;
                case 91://choosing the world to create
                    world_definition_manager.draw_choosing_menu(spriteBatch, display);
                    break;
                case 92://creating a new world... show status text.
                    world_definition_manager.creating_draw(spriteBatch, display);
                    break;
                case 95://client waiting to connect
                    size = display.font.MeasureString("Waiting for Initialization");
                    display.draw_text(spriteBatch, display.font, "@11Waiting for Initialization", (int)((screen_size.X / 2) - (size.X / 2)), (int)screen_size.Y / 2, screen_size.X);
                    break;
                case 100: //in-game
                    Vector2 mouse_world_pos = new Vector2(world.top_left.X + input.mouse_now.X, world.top_left.Y + input.mouse_now.Y);
                    if (game_my_user_id != -1)
                    {
                        world.total_fps.stop();
                        world.total_fps.start();
                        world.frame_render_time.start();
                        if (settings.use_hardware_lighting)
                            lighting.draw_scene(spriteBatch, display, world, input, GraphicsDevice);
                        else
                            world.draw_world(game_my_user_id, spriteBatch, display, input);
                        display.draw_fade_text(spriteBatch,world.top_left);
                        world.draw_hud(game_my_user_id, spriteBatch, display, input);
                        world.frame_render_time.stop();

                    }
                    else
                    {
                        size = display.font.MeasureString("Waiting for Initialization");
                        display.draw_text(spriteBatch, display.font, "@11Waiting for Initialization", (int)((screen_size.X / 2) - (size.X / 2)), (int)screen_size.Y / 2, screen_size.X);
                    }
                    if (draw_debug && world.players.Count > 0)
                    {
                        //display.draw_text(spriteBatch, "@00" + network_server.udp_server.Statistics.ToString(), 4, 155, screen_size.X, false);
                        display.draw_text(spriteBatch, "@00" + world.ToString(), 4, 155, screen_size.X, false);
                        string stats = world.frame_render_time.ToString();
                        Vector2 size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y, screen_size.X);

                        stats = world.water_update_time.ToString();
                        size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y - 30, screen_size.X);

                        stats = world.light_time.ToString();
                        size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y - 60, screen_size.X);

                        stats = world.updating.ToString();
                        size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y - 90, screen_size.X);

                        stats = world.total_fps.ToString();
                        size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y - 120, screen_size.X);

                        stats = world.world_generation.ToString();
                        size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y - 150, screen_size.X);

                        stats = world.flowing_blocks.ToString();
                        size_str = display.middle_font.MeasureString(stats);
                        display.draw_text(spriteBatch, display.middle_font, "@07" + stats, screen_size.X - (int)size_str.X, screen_size.Y - (int)size_str.Y - 180, screen_size.X);

                    }
                    if (world.players.Count > 0 && settings.show_ping && play_type != "Single Player")
                    {
                        string stats = "";
                        Vector2 size_str = new Vector2();

                        if (game_server)
                        {
                            stats = "Hosting";
                            size_str = display.small_font.MeasureString(stats);
                            display.draw_text(spriteBatch, display.small_font, Display.color_code + "00Hosting", screen_size.X - (int)size_str.X, 0, 220);
                        }
                        else if (network_client.udp_client.Connections.Count > 0)
                        {
                            stats = "ping: " + Math.Round(network_client.udp_client.Connections[0].AverageRoundtripTime * 1000f, 1).ToString() + "ms";
                            size_str = display.small_font.MeasureString(stats);
                            display.draw_text(spriteBatch, display.small_font, Display.color_code + "00" + stats, screen_size.X - (int)size_str.X, 0, 120);
                        }
                        else
                        {
                            stats = "ping: DISCONNECTED";
                            size_str = display.small_font.MeasureString(stats);
                            display.draw_text(spriteBatch, display.small_font, Display.color_code + "00ping: @07DISCONNECTED", screen_size.X - (int)size_str.X, 0, 220);
                        }
                    }
                    break;
            }
            display.draw_messages(spriteBatch, input);
            if (draw_debug)
            {
                display.draw_text_with_outline(spriteBatch, display.small_font, Display.color_code + "00" + debug, 4, 204, screen_size.X,AccColors.Adamantium);
            }
            spriteBatch.Draw(display.sprites, new Rectangle(input.mouse_now.X, input.mouse_now.Y, (int)(16.0 * 2), (int)(16.0 * 2)), display.mouse, Color.White);
            spriteBatch.End();
           
            if (take_screen_shot)
            {
                GraphicsDevice.SetRenderTarget(null);
                try
                {
                    System.IO.Directory.CreateDirectory(@"screenshots");
                }
                catch
                {

                }
                System.IO.StreamWriter w = new System.IO.StreamWriter(@"screenshots/screenshot_" + DateTime.Now.Ticks.ToString() + ".png");
                renderTarget.SaveAsPng(w.BaseStream, screen_size.X, screen_size.Y);
                w.Close();
                Exilania.display.add_message("@05Screenshot Saved.");
                if (settings.use_hardware_lighting)
                {
                    /*w = new System.IO.StreamWriter(@"screenshots/screenshot_" + DateTime.Now.Ticks.ToString() + "lights.png");
                    lighting.lightsTarget.SaveAsPng(w.BaseStream, lighting.lightsTarget.Width, lighting.lightsTarget.Height);
                    w.Close();*/
                }
                take_screen_shot = false;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                spriteBatch.Draw(renderTarget, new Vector2(0, 0), Color.White);
                spriteBatch.End();

            }
            base.Draw(gameTime);
        }

        public void ToggleDisplay()
        {
            if (graphics.IsFullScreen)
            {
                if (allow_hi_def)
                {
                    graphics.PreferredBackBufferHeight = settings.highdef_window_size.Y;
                    graphics.PreferredBackBufferWidth = settings.highdef_window_size.X;
                    screen_size.X = settings.highdef_window_size.X;
                    screen_size.Y = settings.highdef_window_size.Y;
                    display.use_small_font = true;
                }
                else
                {
                    graphics.PreferredBackBufferHeight = windowed_size.Y;
                    graphics.PreferredBackBufferWidth = windowed_size.X;
                    screen_size.X = windowed_size.X;
                    screen_size.Y = windowed_size.Y;
                    display.use_small_font = true;
                }
            }
            else
            {
                if (!settings.use_custom_dimensions)
                {
                    graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                    graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                    screen_size = new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
                }
                else
                {
                    try
                    {
                        graphics.PreferredBackBufferHeight = settings.custom_height;
                        graphics.PreferredBackBufferWidth = settings.custom_width;
                        screen_size = new Point(settings.custom_width, settings.custom_height);
                    }
                    catch
                    {
                        graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                        graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                        screen_size = new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
                    }
                }
            }
            graphics.ToggleFullScreen();
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            graphics.ApplyChanges();
            screen_size = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            lighting.resize_graphicsdevice(GraphicsDevice);
            changing_graphics_state = false;
        }
    }
}
