//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using OpenTK;
using linerider.Utils;
using linerider.Rendering;
using System.Drawing;
using System.Collections.Generic;

namespace linerider
{
    public static class Models
    {
        public static int SledTexture;
        public static int BrokenSledTexture;
        public static int BodyTexture;
        public static int BodyDeadTexture;
        public static int ArmTexture;
        public static int LegTexture;

        public static int ArglinBodyTexture;
        public static int ArglinBodyDeadTexture;

        public static int BasicBodyTexture;
        public static int BasicBodyDeadTexture;

        public static readonly DoubleRect SledRect = new DoubleRect(-0.6875, -2.3125, 17.9195, 8.95975);
        public static readonly DoubleRect BrokenSledRect = new DoubleRect(-0.3645, -2.3125, 17.477, 8.7385);
        public static readonly DoubleRect BodyRect = new DoubleRect(0.026, -3.145, 13.944, 6.972);
        public static readonly DoubleRect ArmRect = new DoubleRect(-0.657, -1.2305, 7.82, 3.91);
        public static readonly DoubleRect LegRect = new DoubleRect(-0.6535, -2.013, 8.02, 4.01);

        public static readonly FloatRect BodyUV = new FloatRect(0, 0, 1, 1);
        public static readonly FloatRect DeadBodyUV = new FloatRect(0, 0, 1, 1);

        public static readonly FloatRect SledUV = new FloatRect(0, 0, 1, 1);
        public static readonly FloatRect BrokenSledUV = new FloatRect(0, 0, 1, 1);

        public static readonly FloatRect ArmUV = new FloatRect(0, 0, 1, 1);
        public static readonly FloatRect LegUV = new FloatRect(0, 0, 1, 1);

        public static void LoadModels()
        {
            BodyTexture = StaticRenderer.LoadTexture(GameResources.body_img);
            BodyDeadTexture = StaticRenderer.LoadTexture(GameResources.bodydead_img);
            
            SledTexture = StaticRenderer.LoadTexture(GameResources.sled_img);
            BrokenSledTexture = StaticRenderer.LoadTexture(GameResources.brokensled_img);

            ArmTexture = StaticRenderer.LoadTexture(GameResources.arm_img);
            LegTexture = StaticRenderer.LoadTexture(GameResources.leg_img);

            ArglinBodyTexture = StaticRenderer.LoadTexture(GameResources.arglinbody_img);
            ArglinBodyDeadTexture = StaticRenderer.LoadTexture(GameResources.arglinbodydead_img);

            BasicBodyTexture = StaticRenderer.LoadTexture(GameResources.basicbody_img);
            BasicBodyDeadTexture = StaticRenderer.LoadTexture(GameResources.basicbodydead_img);
        }
        public static void LoadModels(string folder)
        {
            if (folder.Equals("*default*")) { LoadModels(); return; }

            string riderLocation = Program.UserDirectory + "\\Riders\\" + folder + "\\";

            BodyTexture = StaticRenderer.LoadTexture(new Bitmap(riderLocation+"body.png"));
            BodyDeadTexture = StaticRenderer.LoadTexture(new Bitmap(riderLocation + "bodydead.png"));

            SledTexture = StaticRenderer.LoadTexture(new Bitmap(riderLocation + "sled.png"));
            BrokenSledTexture = StaticRenderer.LoadTexture(new Bitmap(riderLocation + "brokensled.png"));

            ArmTexture = StaticRenderer.LoadTexture(new Bitmap(riderLocation + "arm.png"));
            LegTexture = StaticRenderer.LoadTexture(new Bitmap(riderLocation + "leg.png"));
        }
    }
}