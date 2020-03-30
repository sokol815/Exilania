using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Exilania
{
    public enum MarkerType
    {
        PLAYER = 0,
        OTHER_PLAYER = 1,
        NPC = 2,
        FRIENDLY_TRANSPORTER = 3,
        CHEST = 4
    }
    public class MapMarker
    {
        public Point loc;
        public MarkerType type;
        public int type_id;

        public MapMarker()
        {

        }

        public MapMarker(Point p, MarkerType t, int id)
        {
            loc = p;
            type = t;
            type_id = id;
        }

        public void update(Point p)
        {
            loc = p;
        }

        public void draw_item(SpriteBatch s, Rectangle map_view, Rectangle screen_view)
        {
            //Exilania.display.add_message("@05Try Drawing: map:" + map_view.ToString() + " screen:" + screen_view.ToString() + " loc:" + loc.ToString());
            if (map_view.Contains(loc))
            {
                Point draw_at = new Point(screen_view.X,screen_view.Y);
                double percent_map = (double)(loc.X - map_view.X) / (double)map_view.Width;
                draw_at.X += (int)(percent_map * (double)screen_view.Width);
                percent_map = (double)(loc.Y - map_view.Y) / (double)map_view.Height;
                draw_at.Y += (int)(percent_map * (double)screen_view.Height);
                s.Draw(Exilania.display.sprites, new Rectangle(draw_at.X - 3, draw_at.Y - 3, 6, 6), MiniMap.item_types[(int)type], Color.White);
            }
        }
    }
   
    public class MiniMap
    {
        public static Rectangle[] item_types = { new Rectangle(1550, 53, 6, 6), new Rectangle(1557, 53, 6, 6), new Rectangle(1557, 60, 6, 6), 
                                                   new Rectangle(1550, 60, 6, 6), new Rectangle(1550, 67, 6, 6), new Rectangle(1557, 67, 6, 6), new Rectangle(1557, 74, 6, 6) };
        public Texture2D[,] map;
        public List<MapMarker> map_pieces;
        public bool full_map_activated = false;

        public MiniMap()
        {

        }

        public MiniMap(World w, GraphicsDevice g)
        {
            map_pieces = new List<MapMarker>();
            map = new Texture2D[w.map.GetLength(0) / 100, w.map.GetLength(1) / 100];
            int width = 100;
            int height = 100;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (x < map.GetLength(0) - 1)
                    {
                        width = 100;
                    }
                    else
                    {
                        if (w.map.GetLength(0) % 100 != 0)
                            width = w.map.GetLength(0) % 100;
                        else
                            width = 100;
                    }
                    if (y < map.GetLength(1) - 1)
                    {
                        height = 100;
                    }
                    else
                    {
                        if (w.map.GetLength(1) % 100 != 0)
                            height = w.map.GetLength(1) % 100;
                        else
                            height = 100;
                    }
                    map[x, y] = new Texture2D(g, width, height);
                    Color[] total_map = new Color[width * height];
                    sbyte temp_col = -1;
                    for (int ys = 0; ys < height; ys++)
                    {
                        for (int xs = 0; xs < width; xs++)
                        {
                            temp_col = w.map[(x * 100) + xs,(y*100) + ys].fgd_block;
                            if (temp_col < 0)
                            {
                                if (w.map[(x * 100) + xs, (y * 100) + ys].liquid_id > 0)
                                {
                                    total_map[(ys * width) + xs] = Exilania.block_types.blocks[World.liquid_blocks[w.map[(x * 100) + xs, (y * 100) + ys].liquid_id]].map_represent;
                                }
                                else
                                    total_map[(ys * width) + xs] = Color.FromNonPremultiplied(0, 0, 0, 127);
                            }
                            else
                                total_map[(ys * width) + xs] = Exilania.block_types.blocks[temp_col].map_represent;
                        }
                    }
                    map[x, y].SetData<Color>(total_map);
                }
            }
        }

        public void update_markers(World w)
        {
            if (Exilania.input.current_input_type == InputConsume.Normal && !Exilania.input.keys_previous.Contains(Microsoft.Xna.Framework.Input.Keys.T) && Exilania.input.keys_now.Contains(Microsoft.Xna.Framework.Input.Keys.T))
            {
                full_map_activated = !full_map_activated;
            }

            if (map_pieces.Count == 0)
            {
                map_pieces.Add(new MapMarker(new Point((int)w.players[Exilania.game_my_user_id].avatar.world_loc.X / 24,
                    (int)w.players[Exilania.game_my_user_id].avatar.world_loc.Y / 24), MarkerType.PLAYER, Exilania.game_my_user_id));
            }
            for (int i = 0; i < map_pieces.Count; i++)
            {
                switch (map_pieces[i].type)
                {
                    case MarkerType.PLAYER:
                        map_pieces[i].loc = new Point((int)w.players[Exilania.game_my_user_id].avatar.world_loc.X / 24, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.Y / 24);
                        break;
                    case MarkerType.OTHER_PLAYER:
                        map_pieces[i].loc = new Point((int)w.players[map_pieces[i].type_id].avatar.world_loc.X / 24, (int)w.players[map_pieces[i].type_id].avatar.world_loc.Y / 24);
                        break;
                    case MarkerType.NPC:
                        break;
                    case MarkerType.CHEST:
                    case MarkerType.FRIENDLY_TRANSPORTER:
                        if (w.furniture[map_pieces[i].type_id].flags[(int)FFLAGS.EMPTY])
                        {
                            //item has been deleted... get rid of marker.
                            map_pieces.RemoveAt(i);
                            i--;
                        }
                        else
                            map_pieces[i].loc = w.furniture[map_pieces[i].type_id].top_left;
                        break;
                }
            }
        }

        public void update_loc(int x, int y, sbyte fgd_block)
        {
            if (y < (map.GetLength(1) * 100) - map[0, map.GetLength(1) - 1].Height)
            {
                Color[] col = new Color[1];
                Rectangle r = new Rectangle(x % 100, y % 100, 1, 1);
                if (fgd_block < 0)
                    col[0] = Color.FromNonPremultiplied(0, 0, 0, 127);
                else
                    col[0] = Exilania.block_types.blocks[fgd_block].map_represent;
                map[x / 100, y / 100].SetData<Color>(0, r, col, 0, 1);
            }
        }

        public void save_to_file(World w)
        {
            try
            {
                Bitmap bmp = new Bitmap(w.map.GetLength(0), w.map.GetLength(1), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        Color[] cols = new Color[map[i, j].Width * map[i, j].Height];
                        map[i, j].GetData(cols);
                        for (int n = 0; n < cols.Length; n++)
                        {
                            bmp.SetPixel((i * 100) + (n % map[i, j].Width), (j * 100) + (n / map[i, j].Height), System.Drawing.Color.FromArgb(cols[n].R, cols[n].G, cols[n].B));
                        }
                    }
                }
                System.IO.StreamWriter b = new System.IO.StreamWriter(@"screenshots/WORLD_" + DateTime.Now.Ticks.ToString() + ".png");
                bmp.Save(b.BaseStream, System.Drawing.Imaging.ImageFormat.Png);
                b.Close();
                bmp = null;
                Exilania.display.add_message("@05World Image Saved!");
            }
            catch
            {
                Exilania.display.add_message("@05World is too large to save a screenshot...");
            }
        }

        public void draw_view(SpriteBatch s, Rectangle screen_area, Rectangle map_area, World w, Color draw)
        {
            if (map_area.Bottom > w.map.GetLength(1))
            {
                map_area.Y = w.map.GetLength(1) - map_area.Height;
            } 
            if (map_area.Y < 0)
                map_area.Y = 0;
            
            //most likely this little function will have to pull together multiple areas of the minimap to show just what you are trying to look at...
            //it should also be wrap-around compatible.
            if (map_area.X < 0 || map_area.Right > w.map.GetLength(0))
            { //wraparound.. do 2 calls!
                Rectangle area_1_draw = new Rectangle();
                Rectangle area_2_draw = new Rectangle();
                if (map_area.Right > w.map.GetLength(0))
                {
                    map_area.X -= w.map.GetLength(0);
                }
              
                area_1_draw = new Rectangle(w.map.GetLength(0) - Math.Abs(map_area.X), map_area.Y, Math.Abs(map_area.X), map_area.Height);
                area_2_draw = new Rectangle(0, map_area.Y, map_area.Width + map_area.X, map_area.Height);
                Rectangle temp1 = new Rectangle(screen_area.X, screen_area.Y, area_1_draw.Width * (screen_area.Width/map_area.Width), screen_area.Height);
                Rectangle temp2 = new Rectangle(screen_area.X + (area_1_draw.Width * (screen_area.Width / map_area.Width)),
                    screen_area.Y, area_2_draw.Width * (screen_area.Width / map_area.Width), screen_area.Height);

                //pass back in with appropriate values
                draw_view(s, temp1, area_1_draw, w, draw);
                draw_view(s, temp2, area_2_draw, w, draw);
            }
            else
            {
                //just draw using map_area! when we are here, we have a valid draw call!
                int coarse_x = map_area.X / 100;
                int coarse_y = map_area.Y / 100;
                int fine_x = 0;
                int end_fine_x = 0;
                int fine_y = 0;
                int end_fine_y = 0;
                double ratio_screen_to_map_x = (double)screen_area.Width / (double)map_area.Width;
                double ratio_screen_to_map_y = (double)screen_area.Height / (double)map_area.Height;
                for (int x = 0; x < (map_area.Width / 100) + 1; x++)
                {
                    coarse_x = (map_area.X / 100) + x;
                    fine_x = 0;
                    if((coarse_x * 100) + fine_x < map_area.Left)
                    {
                        fine_x = map_area.Left % 100;
                    }
                    end_fine_x = 100;
                    if ((coarse_x * 100) + end_fine_x > map_area.Right)
                        end_fine_x = map_area.Right % 100;
                    for (int y = 0; y < (map_area.Height / 100) + 1; y++)
                    {
                        coarse_y = (map_area.Y / 100) + y;
                        fine_y = 0;
                        if((coarse_y * 100) + fine_y < map_area.Top)
                        {
                            fine_y = map_area.Top % 100;
                        }
                        end_fine_y = 100;
                        if ((coarse_y * 100) + end_fine_y > map_area.Bottom)
                            end_fine_y = map_area.Bottom % 100;
                        Rectangle draw_from_pic = new Rectangle(fine_x, fine_y, end_fine_x - fine_x, end_fine_y - fine_y);
                        Rectangle draw_to_screen = new Rectangle(screen_area.X + (int)((double)(((coarse_x * 100) + fine_x) - map_area.X) * ratio_screen_to_map_x),
                            screen_area.Y + (int)((double)(((coarse_y * 100) + fine_y) - map_area.Y) * ratio_screen_to_map_y),
                            (int)((double)(end_fine_x - fine_x) * ratio_screen_to_map_x), (int)((double)(end_fine_y - fine_y) * ratio_screen_to_map_y));
                        s.Draw(map[coarse_x % map.GetLength(0), coarse_y % map.GetLength(1)], draw_to_screen, draw_from_pic, draw);
                    }
                }
                for (int i = 0; i < map_pieces.Count; i++)
                {
                    map_pieces[i].draw_item(s, map_area, screen_area);
                }
            }
        }
    }
}
