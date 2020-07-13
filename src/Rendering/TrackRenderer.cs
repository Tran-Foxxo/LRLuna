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
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using linerider.Utils;
using linerider.Drawing;
using linerider.Game;

namespace linerider.Rendering
{
    internal class TrackRenderer : GameService
    {
        private enum LineActionType
        {
            Add,
            Change,
            Remove
        }
        public bool RequiresUpdate = true;

        private LineDecorator _bluedecorator;
        private LineDecorator _acceldecorator;
        private LineRenderer _physvbo;
        private LineRenderer _accelvbo;
        private LineRenderer _sceneryvbo;
        /// <summary>
        /// A dictionary of [line id] -> [index of first vertex]
        /// </summary>
        private Dictionary<int, int> _physlines;

        /// <summary>
        /// A dictionary of [line id] -> [index of first vertex]
        /// </summary>
        /// /// <remarks>
        /// Seperate so different colors can be used
        /// </remarks>
        private Dictionary<int, int> _accellines;

        /// <summary>
        /// A dictionary of [line id] -> [index of first IBO index]
        /// </summary>
        /// <remarks>
        /// We have a seperate scenery vbo for rendering it under the sim one.
        /// </remarks>
        private Dictionary<int, int> _scenerylines;

        /// <summary>
        /// We use an action queue system instead of instantly adding to the renderer
        /// for the sake of multithreading safety
        /// </summary>
        private Queue<Tuple<LineActionType, GameLine>> _lineactions;
        private ResourceSync _sync;
        const int linesize = 6;
        public TrackRenderer()
        {
            _sync = new ResourceSync();
            _lineactions = new Queue<Tuple<LineActionType, GameLine>>();
            _physlines = new Dictionary<int, int>();
            _accellines = new Dictionary<int, int>();
            _scenerylines = new Dictionary<int, int>();
            _bluedecorator = new LineDecorator();
            _acceldecorator = new LineDecorator();
            _physvbo = new LineRenderer(Shaders.LineShader);
            _physvbo.OverrideColor = Constants.DefaultLineColor; 
            _accelvbo = new LineRenderer(Shaders.LineShader);
            _accelvbo.OverrideColor = Constants.DefaultLineColor;

            _sceneryvbo = new LineRenderer(Shaders.LineShader);
            _sceneryvbo.OverrideColor = Color.Black;
        }
        public void Render(DrawOptions options)
        {
            using (new GLEnableCap(EnableCap.Texture2D))
            {
                UpdateBuffers();
                GL.BlendFunc(
                    BlendingFactorSrc.SrcAlpha,
                    BlendingFactorDest.OneMinusSrcAlpha);
                GameDrawingMatrix.Enter();
                _physvbo.Scale = options.Zoom;
                _accelvbo.Scale = options.Zoom;
                _physvbo.KnobState = options.KnobState;
                _accelvbo.KnobState = options.KnobState;

                _sceneryvbo.Scale = options.Zoom;
                //green lines dont get lifelock
                if (options.KnobState != KnobState.Hidden)
                {
                    _sceneryvbo.KnobState = KnobState.Shown;
                    _accelvbo.KnobState = KnobState.Shown;
                }
                else
                {
                    _sceneryvbo.KnobState = KnobState.Hidden;
                    _accelvbo.KnobState = KnobState.Hidden;
                }

                if (options.ColoredMainLine)
                {
                    _sceneryvbo.OverrideColor = Constants.ColorSceneryLine;
                    _physvbo.OverrideColor = Constants.ColorDefaultLine;
                    _accelvbo.OverrideColor = Constants.ColorAccelerationLine;
                }
                else if (options.NightMode)
                {
                    _sceneryvbo.OverrideColor = Constants.DefaultNightLineColor;
                    _physvbo.OverrideColor = Constants.DefaultNightLineColor;
                    _accelvbo.OverrideColor = Constants.DefaultNightLineColor;
                }
                else
                {
                    _sceneryvbo.OverrideColor = Constants.DefaultLineColor;
                    _physvbo.OverrideColor = Constants.DefaultLineColor;
                    _accelvbo.OverrideColor = Constants.DefaultLineColor;
                }
                if (options.LineColors)
                {
                    _sceneryvbo.OverrideColor = (Settings.SceneryColorChange ? Constants.SceneryLineColored : Constants.SceneryLineColor);
                    //_physvbo.OverrideColor = (Settings.NormalColorChange ? Constants.BlueLineColored : Constants.BlueLineColor);
                    //_accelvbo.OverrideColor = (Settings.AccelerationColorChange ? Constants.RedLineColored : Constants.RedLineColor);
                    _sceneryvbo.OverridePriority = 1;
                    _physvbo.OverridePriority = 1;
                    _accelvbo.OverridePriority = 1;
                }
                else
                {
                    _sceneryvbo.OverridePriority = 255;//force override
                    _physvbo.OverridePriority = 255;
                    _accelvbo.OverridePriority = 255;
                }
                

                

                _physvbo.Overlay = options.Overlay;
                _accelvbo.Overlay = options.Overlay;
                _sceneryvbo.Overlay = options.Overlay;
                
                _sceneryvbo.Draw();
                
                _bluedecorator.DrawUnder(options);
                _physvbo.Draw();
                
                _acceldecorator.DrawUnder(options); 
                _accelvbo.Draw();
                
                GameDrawingMatrix.Exit();
            }
        }
        /// <summary>
        /// Clears the renderer and initializes it with new lines.
        /// </summary>
        public void InitializeTrack(Track track)
        {
            AutoArray<GameLine> scenery = new AutoArray<GameLine>(track.SceneryLines);
            scenery.UnsafeSetCount(track.SceneryLines);
            AutoArray<GameLine> phys = new AutoArray<GameLine>(track.BlueLines);
            AutoArray<GameLine> accel = new AutoArray<GameLine>(track.RedLines);
            var sorted = track.GetSortedLines();
            using (_sync.AcquireWrite())
            {
                _lineactions.Clear();
                _physvbo.Clear();
                _sceneryvbo.Clear();
                _accelvbo.Clear();
                _bluedecorator.Clear();
                _acceldecorator.Clear();
                _physlines.Clear();
                _scenerylines.Clear();
                _accellines.Clear();
                RequiresUpdate = true;

                int scenerycount = 0;
                for (int i = 0; i < sorted.Length; i++)
                {
                    var line = sorted[i];

                    switch (line.Type)
                    {
                        case LineType.Scenery:
                            scenery.unsafe_array[scenery.Count - (1 + scenerycount++)] = line;
                            break;
                        case LineType.Blue:
                            phys.Add(line);
                            break;
                        case LineType.Red:
                            accel.Add(line);
                            break;
                    }
                }

                Debug.Assert(scenerycount == scenery.Count,
                    "Predicted scenery count was not accurate");
                if (scenery.Count != 0)
                {
                    LineVertex[] sceneryverts = new LineVertex[scenery.Count * linesize];
                    System.Threading.Tasks.Parallel.For(0, scenery.Count, (index) =>
                    {
                        int counter = 0;
                        var verts = (GenerateLine(scenery[index], false));
                        foreach (var vert in verts)
                        {
                            sceneryverts[index * linesize + counter++] = vert;
                        }
                    });
                    _scenerylines = _sceneryvbo.AddLines(
                        scenery,
                        sceneryverts);
                }
                if (phys.Count != 0)
                {
                    LineVertex[] physverts = new LineVertex[phys.Count * linesize];
                    System.Threading.Tasks.Parallel.For(0, phys.Count, (index) =>
                      {
                          int counter = 0;
                          var verts = (GenerateLine(phys[index], false));
                          foreach (var vert in verts)
                          {
                              physverts[index * linesize + counter++] = vert;
                          }
                      });
                    _physlines = _physvbo.AddLines(
                        phys,
                        physverts);
                    _bluedecorator.Initialize(phys);
                }
                if (accel.Count != 0)
                {
                    LineVertex[] accelverts = new LineVertex[accel.Count * linesize];
                    System.Threading.Tasks.Parallel.For(0, accel.Count, (index) =>
                    {
                        int counter = 0;
                        var verts = (GenerateLine(accel[index], false));
                        foreach (var vert in verts)
                        {
                            accelverts[index * linesize + counter++] = vert;
                        }
                    });
                    _accellines = _accelvbo.AddLines(
                        accel,
                        accelverts);
                    _acceldecorator.Initialize(accel);
                }
                RequiresUpdate = true;
            }
        }
        public void AddLine(GameLine line)
        {
            RequiresUpdate = true;
            using (_sync.AcquireWrite())
            {
                _lineactions.Enqueue(
                    new Tuple<LineActionType, GameLine>(
                        LineActionType.Add,
                        line));
            }
        }
        public void LineChanged(GameLine line)
        {
            RequiresUpdate = true;
            using (_sync.AcquireWrite())
            {
                _lineactions.Enqueue(
                    new Tuple<LineActionType, GameLine>(
                        LineActionType.Change,
                        line));
            }
        }
        public void RemoveLine(GameLine line)
        {
            RequiresUpdate = true;
            using (_sync.AcquireWrite())
            {
                _lineactions.Enqueue(
                    new Tuple<LineActionType, GameLine>(
                        LineActionType.Remove,
                        line));
            }
        }
        private void UpdateBuffers()
        {
            using (_sync.AcquireWrite())
            {
                RequiresUpdate = false;
                while (_lineactions.Count != 0)
                {
                    var dequeued = _lineactions.Dequeue();
                    var line = dequeued.Item2;
                    switch (dequeued.Item1)
                    {
                        case LineActionType.Add:
                            if (line.Type == LineType.Scenery)
                            {
                                AddLine(
                                    line,
                                    _sceneryvbo,
                                    _scenerylines);
                            }
                            else if (line.Type == LineType.Blue)
                            {
                                AddLine(
                                    line,
                                    _physvbo,
                                    _physlines);
                                _bluedecorator.AddLine((StandardLine)line);
                            }
                            else
                            {
                                AddLine(
                                    line,
                                    _accelvbo,
                                    _accellines);
                                _acceldecorator.AddLine((StandardLine)line);
                            }
                            break;
                        case LineActionType.Remove:
                            if (line.Type == LineType.Scenery)
                            {
                                RemoveLine(
                                    line,
                                    _sceneryvbo,
                                    _scenerylines);
                            }
                            else if (line.Type == LineType.Blue)
                            {
                                RemoveLine(
                                    line,
                                    _physvbo,
                                    _physlines);
                                _bluedecorator.RemoveLine((StandardLine)line);
                            }
                            else
                            {
                                RemoveLine(
                                    line,
                                    _accelvbo,
                                    _accellines);
                                _acceldecorator.RemoveLine((StandardLine)line);
                            }
                            break;
                        case LineActionType.Change:
                            if (line.Type == LineType.Scenery)
                            {
                                LineChanged(
                                    line,
                                    _sceneryvbo,
                                    _scenerylines,
                                    false);
                            }
                            else if (line.Type == LineType.Blue)
                            {
                                bool hit = Settings.Editor.HitTest
                                    ? game.Track.Timeline.IsLineHit(line.ID)
                                    : false;
                                LineChanged(
                                    line,
                                    _physvbo,
                                    _physlines,
                                    hit);
                                _bluedecorator.LineChanged((StandardLine)line, hit);
                            }
                            else
                            {
                                bool hit = Settings.Editor.HitTest
                                  ? game.Track.Timeline.IsLineHit(line.ID)
                                  : false;
                                LineChanged(
                                    line,
                                    _accelvbo,
                                    _accellines,
                                    hit);
                                _acceldecorator.LineChanged((StandardLine)line, hit);
                            }
                            break;
                    }
                }
                _lineactions.Clear();
            }
        }
        private void AddLine(
            GameLine line,
            LineRenderer renderer,
            Dictionary<int, int> lookup)
        {
            if (lookup.ContainsKey(line.ID))
            {
                LineChanged(line, renderer, lookup, false);
                return;
            }
            var lineverts = GenerateLine(line, false);
            int start = renderer.AddLine(lineverts);
            lookup.Add(line.ID, start);
        }
        private void LineChanged(
            GameLine line,
            LineRenderer renderer,
            Dictionary<int, int> lookup,
            bool hit)
        {
            var lineverts = GenerateLine(line, hit);
            renderer.ChangeLine(lookup[line.ID], lineverts);
        }
        private void RemoveLine(
            GameLine line,
            LineRenderer renderer,
            Dictionary<int, int> lookup)
        {
            int start = lookup[line.ID];
            renderer.RemoveLine(start);
            // preserve the id in the lookup in event of undo
        }
        private static LineVertex[] GenerateLine(GameLine line, bool hit)
        {
            int color = 0;
            if (line is StandardLine stl)
            {
                if (hit)
                {
                    color = Utility.ColorToRGBA_LE(line.Color);
                }
                else if (stl.Trigger != null)
                {
                    var trigger = Utility.ColorToRGBA_LE(
                        Constants.TriggerLineColor);
                    color = Utility.ChangeAlpha(trigger, 254);
                }
            }
            var lineverts = LineRenderer.CreateTrackLine(
                line.Position,
                line.Position2,
                2 * line.Width,
                color,
                (byte)line.SelectionState);
            return lineverts;
        }
    }
}