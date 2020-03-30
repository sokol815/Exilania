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
    public class Chunk
    {
        public Rectangle area;
        /// <summary>
        /// this timestamp is in milliseconds.
        /// </summary>
        public long timestamp;

        public Chunk()
        {

        }

        public Chunk(int x, int y, int width, int height)
        {
            area = new Rectangle(x, y, width, height);
            timestamp = System.DateTime.Now.Ticks / 10000;
            
        }

        public Chunk(Lidgren.Network.NetIncomingMessage i, World w)
        {
            area = new Rectangle(i.ReadUInt16(), i.ReadUInt16(), i.ReadUInt16(), i.ReadUInt16());
            timestamp = System.DateTime.Now.Ticks / 10000;
            for (int x = area.X; x < area.Right; x++)
            {
                for (int y = area.Y; y < area.Bottom; y++)
                {
                    w.map[x, y] = new Voxel(i);
                }
            }
        }

        public void send_chunk(Lidgren.Network.NetOutgoingMessage o, World w)
        {
            o.Write((UInt16)area.X);
            o.Write((UInt16)area.Y);
            o.Write((UInt16)area.Width);
            o.Write((UInt16)area.Height);
            for (int x = area.X; x < area.Right; x++)
            {
                for (int y = area.Y; y < area.Bottom; y++)
                {
                    w.map[x, y].send_voxel(o);
                }
            }
        }
    }
}
