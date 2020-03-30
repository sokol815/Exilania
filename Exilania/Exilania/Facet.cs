using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    /// <summary>
    /// a facet is a piece of a furniture item that can rotate!
    /// </summary>
    public class Facet
    {
        public int furniture_template_id;
        public int facet_template_id;
        public Point world_furniture_loc;
        public double rotation;
        public FACETTRACKERS rotation_type;
        public Point offset_facet;
        public Point center_of_rotation;

        public Facet()
        {

        }

        public Facet(int furn_temp_id, Point world_furn_loc, int facet_temp_id, FrameFacet f)
        {
            rotation = 0;
            furniture_template_id = furn_temp_id;
            facet_template_id = facet_temp_id;
            world_furniture_loc = world_furn_loc;
            rotation_type = f.rotation_type;
            offset_facet = f.attach_point;
            center_of_rotation = f.center_of_rotation;
        }

        public void update_facet(World w, Furniture f)
        {
            switch (rotation_type)
            {
                case FACETTRACKERS.ENEMY:
                    break;
                case FACETTRACKERS.LEFTRIGHT:
                    break;
                case FACETTRACKERS.PROXIMITY:
                    break;
                case FACETTRACKERS.ROTATECWFAST:
                    rotation = (w.world_time % 1f) * Math.PI * 2;
                    break;
                case FACETTRACKERS.ROTATECWMEDIUM:
                    rotation = (w.world_time % 10f) / 10f * Math.PI * 2;
                    break;
                case FACETTRACKERS.ROTATECWSLOW:
                    rotation = (w.world_time % 30f) / 30f * Math.PI * 2;
                    break;
                case FACETTRACKERS.ROTATECCWSLOW:
                    rotation = (w.world_time % 1f) * Math.PI * -2;
                    break;
                case FACETTRACKERS.ROTATECCWMEDIUM:
                    rotation = (w.world_time % 10f) / 10f * Math.PI * -2;
                    break;
                case FACETTRACKERS.ROTATECCWFAST:
                    rotation = (w.world_time % 30f) / 30f * Math.PI * -2;
                    break;
                case FACETTRACKERS.ROTATEHOUR:
                    rotation = Math.PI * 2f * ((float)(w.cur_hour % 12) / 12f); 
                    break;
                case FACETTRACKERS.ROTATEMINUTE:
                    rotation = Math.PI * 2f * ((float)w.cur_minute / 60f);
                    break;
                case FACETTRACKERS.ROTATESPAWN:
                    break;
                case FACETTRACKERS.USERDIRECTIONS:
                    break;
                case FACETTRACKERS.USERMOUSE:
                    break;
            }
        }
        
        /// <summary>
        /// used to draw the furniture item in question in a non-normal orientation... uses the center of the image as the origin of rotation
        /// </summary>
        /// <param name="s"></param>
        /// <param name="top_left"></param>
        public void draw_rotated_facet(SpriteBatch s, World w)
        {
            Point size_drawing = new Point(Exilania.furniture_manager.furniture[furniture_template_id].facets[facet_template_id].width,
                Exilania.furniture_manager.furniture[furniture_template_id].facets[facet_template_id].height);
            int[] imgs = Exilania.furniture_manager.furniture[furniture_template_id].facets[facet_template_id].images;
            Rectangle t = w.furniture[w.map[world_furniture_loc.X,world_furniture_loc.Y].furniture_index].get_rect();
            t.X -= (int)w.top_left.X;
            t.Y -= (int)w.top_left.Y;


            //the line between the center of the parent and the center of the block depends upon base distance and upon current rotation:
            Vector2 cur_loc_delta = new Vector2();
            double dist;
            double this_angle = 0;
            int x_offset = 12;
            int y_offset = 12;
            Point color_use = new Point();
            for (int x = 0; x < size_drawing.X; x++)
            {
                for (int y = 0; y < size_drawing.Y; y++)
                {
                    x_offset = ((t.Width / 24) - size_drawing.X) * 12 + 12 + center_of_rotation.X;
                    y_offset = ((t.Height / 24) - size_drawing.Y) * 12 + 12 + center_of_rotation.Y;
                    dist = Acc.get_distance(new Point(t.Width / 2, t.Height / 2), new Point(x * 24 + x_offset, y * 24 + y_offset));
                    this_angle = Math.Atan2((y * 24) + y_offset - (t.Height / 2), (x * 24) + x_offset - (t.Width / 2)) + rotation;
                    cur_loc_delta.X = (float)(Math.Cos(this_angle) * dist);
                    cur_loc_delta.Y = (float)(Math.Sin(this_angle) * dist);
                    color_use = new Point(w.wraparound_x(((int)w.top_left.X + t.X + (int)cur_loc_delta.X + 12) / 24),
                        ((int)w.top_left.Y + t.Y + (int)cur_loc_delta.Y + 12) / 24);
                    Exilania.display.draw_rotated_image(s, t, new Vector2(offset_facet.X, offset_facet.Y), cur_loc_delta, rotation,
                        w.map[color_use.X,color_use.Y].light_level, imgs[y * size_drawing.X + x], false);
                }
            }
        }
    }
}
