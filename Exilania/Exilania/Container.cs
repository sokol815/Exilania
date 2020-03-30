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
    public class ContHelper
    {
        public static Rectangle border = new Rectangle(1603, 151, 2, 2);
        public static Rectangle[] colors = new Rectangle[] { new Rectangle(1607,153,3,16), new Rectangle(1610,153,3,16), new Rectangle(1613,153,3,16), new Rectangle(1616,153,3,16 )};

        public static void draw_rectangle_surround(SpriteBatch s, Rectangle placement, float percent, int col_choice)
        {
            if (col_choice >= colors.Length)
                col_choice = colors.Length - 1;
            s.Draw(Exilania.display.sprites, new Rectangle(placement.X, placement.Y, placement.Width, 2), border, Color.White); //top
            s.Draw(Exilania.display.sprites, new Rectangle(placement.X, placement.Bottom - 4, placement.Width, 2), border, Color.White); //bottom
            s.Draw(Exilania.display.sprites, new Rectangle(placement.X, placement.Y, 2, placement.Height - 4), border, Color.White); //left
            s.Draw(Exilania.display.sprites, new Rectangle(placement.Right - 2, placement.Y, 2, placement.Height - 4), border, Color.White); //right
            s.Draw(Exilania.display.sprites, new Rectangle(placement.X + 2, placement.Y + 2, (int)((float)(placement.Width - 4) * percent), placement.Height - 6), colors[col_choice], Color.White);
        }
    }

    public class Container
    {
        public int cur_val;
        public int max_val;
        public float percent;

        public Container()
        {

        }

        public Container(int cur, int max)
        {
            max_val = max;
            cur_val = cur;
            percent = (float)Math.Round((float)cur_val / (float)max_val, 3);
        }

        public Container(System.IO.BinaryReader r)
        {
            max_val = r.ReadInt32();
            cur_val = r.ReadInt32();
            percent = (float)Math.Round((float)cur_val / (float)max_val, 3);
        }

        public Container(Lidgren.Network.NetIncomingMessage r)
        {
            max_val = r.ReadInt32();
            cur_val = r.ReadInt32();
            percent = (float)Math.Round((float)cur_val / (float)max_val, 3);
        }

        public void write_container(System.IO.BinaryWriter w)
        {
            w.Write(max_val);
            w.Write(cur_val);
        }

        public void send_container(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(max_val);
            w.Write(cur_val);
        }

        public int change_val(int dval)
        {
            cur_val += dval;
            if (cur_val > max_val)
                cur_val = max_val;
            else if (cur_val < 0)
                cur_val = 0;
            percent = (float)Math.Round((float)cur_val / (float)max_val, 3);
            return cur_val;
        }

        

        public void draw_at(SpriteBatch s, Point screen_loc, int width, int height,int col_choice)
        {
            if (width < 24)
                width = 24;
            if (height < 8)
                height = 8;
            Rectangle d = new Rectangle(screen_loc.X, screen_loc.Y, width, height);
            ContHelper.draw_rectangle_surround(s, d, percent, col_choice);
        }

        public override string ToString()
        {
            return cur_val + "/" + max_val;
        }
    }
    public class FloatContainer
    {
        public float cur_val;
        public float max_val;
        public float percent;

        public FloatContainer()
        {

        }

        public FloatContainer(float cur, float max)
        {
            max_val = max;
            cur_val = cur;
            percent = (float)Math.Round(cur_val / max_val, 3);
        }

        public FloatContainer(System.IO.BinaryReader r)
        {
            max_val = r.ReadSingle();
            cur_val = r.ReadSingle();
            percent = (float)Math.Round(cur_val / max_val, 3);
        }

        public FloatContainer(Lidgren.Network.NetIncomingMessage r)
        {
            max_val = r.ReadSingle();
            cur_val = r.ReadSingle();
            percent = (float)Math.Round(cur_val / max_val, 3);
        }

        public void write_FloatContainer(System.IO.BinaryWriter w)
        {
            w.Write(max_val);
            w.Write(cur_val);
        }

        public void send_FloatContainer(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(max_val);
            w.Write(cur_val);
        }

        public float change_val(float dval)
        {
            cur_val += dval;
            if (cur_val > max_val)
                cur_val = max_val;
            else if (cur_val < 0)
                cur_val = 0;
            percent = (float)Math.Round(cur_val / max_val, 3);
            return cur_val;
        }

        public void draw_at(SpriteBatch s, Point screen_loc, int width, int height, int col_choice)
        {
            if (width < 24)
                width = 24;
            if (height < 8)
                height = 8;
            Rectangle d = new Rectangle(screen_loc.X, screen_loc.Y, width, height);
            ContHelper.draw_rectangle_surround(s, d,percent,col_choice);
        }

        public override string ToString()
        {
            return Math.Round(cur_val,1) + "/" + Math.Round(max_val,1);
        }
    }

}
