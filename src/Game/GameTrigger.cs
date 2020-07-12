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

using linerider.Utils;
using OpenTK;
using OpenTK.Platform;
using System;

namespace linerider.Game
{
    public class GameTrigger
    {
        public const int TriggerTypes = 4;
        public int Start;
        public int End;
        public TriggerType TriggerType;
        //Zoom
        public float ZoomTarget = 4;
        //BG
        public int backgroundRed;
        public int backgroundGreen;
        public int backgroundBlue;
        //Line Color
        public int lineRed;
        public int lineGreen;
        public int lineBlue;
        //Camera Offset
        public float XOffsetInPixels = 0;
        public float YOffsetInPixels = 0;

        public bool CompareTo(GameTrigger other)
        {
            if (other == null)
                return false;
            return TriggerType == other.TriggerType &&
            Start == other.Start &&
            End == other.End &&
            ZoomTarget == other.ZoomTarget;
        }
        public bool ActivateZoom(int hitdelta, ref float currentzoom)
        {
            bool handled = false;
            if (TriggerType == TriggerType.Zoom)
            {
                int zoomframes = End - Start;
                if (currentzoom != ZoomTarget)
                {
                    if (hitdelta >= 0 && hitdelta < zoomframes)
                    {
                        var diff = ZoomTarget - currentzoom;
                        currentzoom = currentzoom + (diff / (zoomframes - hitdelta));
                        handled = true;
                    }
                    else
                    {
                        currentzoom = ZoomTarget;
                    }
                }
            }
            return handled;
        }
        public bool ActivateBackgroundColor(int delta)
        {
            return false;
        }
        public bool ActivateLineColor(int delta)
        {
            return false;
        }
        public bool ActivateCameraOffset(int delta, ref Vector2d cameraoffset, Vector2d from)
        {
            bool handled = false;
            if (TriggerType == TriggerType.CameraOffset)
            {
                int frames = End - Start;
                float amt = ((float)delta / (float)frames);

                Vector2d to = new Vector2d(XOffsetInPixels - (float)from.X, YOffsetInPixels - (float)from.Y);

                if (!cameraoffset.Equals(to))
                {
                    if (delta >= 0 && delta < frames)
                    {
                        cameraoffset = ((to) * amt)+from;
                        handled = true;
                    }
                    else
                    {
                        cameraoffset = new Vector2d(XOffsetInPixels, YOffsetInPixels); 
                    }
                }
            }
            return handled;
        }
        public GameTrigger Clone()
        {
            return new GameTrigger()
            {
                Start = Start,
                End = End,
                TriggerType = TriggerType,
                ZoomTarget = ZoomTarget
            };
        }
    }
}   