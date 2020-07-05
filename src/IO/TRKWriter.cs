﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using linerider.Audio;
using linerider.Game;

namespace linerider.IO
{
    public static class TRKWriter
    {
        public static string SaveTrack(Track trk, string savename)
        {
            var dir = TrackIO.GetTrackDirectory(trk);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var filename = dir + savename + ".trk";
            using (var file = File.Create(filename))
            {
                var bw = new BinaryWriter(file);
                bw.Write(new byte[] { (byte)'T', (byte)'R', (byte)'K', 0xF2 });
                bw.Write((byte)1);
                string featurestring = "";
                var lines = trk.GetLines();
                var featurelist = TrackIO.GetTrackFeatures(trk);
                featurelist.TryGetValue(TrackFeatures.songinfo, out bool songinfo);
                featurelist.TryGetValue(TrackFeatures.redmultiplier, out bool redmultiplier);
                featurelist.TryGetValue(TrackFeatures.zerostart, out bool zerostart);
                featurelist.TryGetValue(TrackFeatures.scenerywidth, out bool scenerywidth);
                featurelist.TryGetValue(TrackFeatures.six_one, out bool six_one);
                featurelist.TryGetValue(TrackFeatures.ignorable_trigger, out bool ignorable_trigger);
                foreach (var feature in featurelist)
                {
                    if (feature.Value)
                    {
                        featurestring += feature.Key + ";";
                    }
                }
                WriteString(bw, featurestring);
                if (songinfo)
                {
                    // unfotrunately this relies on .net to save and parse in
                    // its own way, and we're kind of stuck with it instead of
                    // the right way to write strings
                    bw.Write(trk.Song.ToString());
                }
                bw.Write(trk.StartOffset.X);
                bw.Write(trk.StartOffset.Y);
                bw.Write(lines.Length);
                foreach (var line in lines)
                {
                    byte type = (byte)line.Type;
                    if (line is StandardLine l)
                    {
                        if (l.inv)
                            type |= 1 << 7;
                        var ext = (byte)l.Extension;
                        type |= (byte)((ext & 0x03) << 5); //bits: 2
                        bw.Write(type);
                        if (redmultiplier)
                        {
                            if (line is RedLine red)
                            {
                                bw.Write((byte)red.Multiplier);
                            }
                        }
                        if (ignorable_trigger)
                        {
                            if (l.Trigger != null)
                            {
                                if (l.Trigger.ZoomTrigger) // check other triggers here for at least one
                                {
                                    bw.Write(l.Trigger.ZoomTrigger);
                                    if (l.Trigger.ZoomTrigger)
                                    {
                                        bw.Write(l.Trigger.ZoomTarget);
                                        bw.Write((short)l.Trigger.ZoomFrames);
                                    }
                                }
                                else
                                {
                                    bw.Write(false);
                                }
                            }
                            else
                            {
                                bw.Write(false);//zoomtrigger=false
                            }
                        }
                        bw.Write(l.ID);
                        if (l.Extension != StandardLine.Ext.None)
                        {
                            // this was extension writing
                            // but we no longer support this.
                            bw.Write(-1);
                            bw.Write(-1);
                        }
                    }
                    else
                    {
                        bw.Write(type);
                        if (scenerywidth)
                        {
                            if (line is SceneryLine scenery)
                            {
                                byte b = (byte)(Math.Round(scenery.Width, 1) * 10);
                                bw.Write(b);
                            }
                        }
                    }

                    bw.Write(line.Position.X);
                    bw.Write(line.Position.Y);
                    bw.Write(line.Position2.X);
                    bw.Write(line.Position2.Y);
                }
                bw.Write(new byte[] { (byte)'M', (byte)'E', (byte)'T', (byte)'A' });
                List<string> metadata = new List<string>();
                metadata.Add(TrackMetadata.startzoom + "=" + trk.StartZoom.ToString(Program.Culture));
                StringBuilder triggerstring = new StringBuilder();
                for (int i = 0; i < trk.Triggers.Count; i++)
                {
                    GameTrigger t = trk.Triggers[i];
                    if (i != 0)
                        triggerstring.Append("&");

                    triggerstring.Append((int)TriggerType.Zoom);
                    triggerstring.Append(":");
                    if (t.TriggerType == TriggerType.Zoom)
                    {
                        triggerstring.Append(t.ZoomTarget.ToString(Program.Culture));
                        triggerstring.Append(":");
                    }
                    triggerstring.Append(t.Start.ToString(Program.Culture));
                    triggerstring.Append(":");
                    triggerstring.Append(t.End.ToString(Program.Culture));
                }
                if (trk.Triggers.Count > 0) //If here are not trigger don't add triggers entry
                {
                    metadata.Add(TrackMetadata.triggers + "=" + triggerstring.ToString());
                }
                bw.Write((short)metadata.Count);
                foreach (var str in metadata)
                {
                    WriteString(bw, str);
                }
            }
            return filename;
        }
        private static void WriteString(BinaryWriter bw, string str)
        {
            bw.Write((short)str.Length);
            bw.Write(Encoding.ASCII.GetBytes(str));
        }
    }
}
