﻿//#define debuggrid
//#define debugcamera
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Gwen;
using Gwen.Controls;
using linerider.Audio;
using linerider.Drawing;
using linerider.Rendering;
using linerider.IO;
using linerider.Tools;
using linerider.UI;
using linerider.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Key = OpenTK.Input.Key;
using Label = Gwen.Controls.Label;
using Menu = Gwen.Controls.Menu;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using MessageBox = Gwen.Controls.MessageBox;

namespace linerider
{
    public class MainWindow : OpenTK.GameWindow
    {
        public Dictionary<string, MouseCursor> Cursors = new Dictionary<string, MouseCursor>();
        public MsaaFbo MSAABuffer;
        public GameCanvas Canvas;
        public bool ReversePlayback = false;
        public Size RenderSize
        {
            get
            {
                if (TrackRecorder.Recording)
                {
                    return TrackRecorder.Recording1080p ? new Size(1920, 1080) : new Size(1280, 720);
                }
                return ClientSize;
            }
            set
            {
                ClientSize = value;
            }
        }
        public Vector2d ScreenTranslation => -ScreenPosition;
        public Vector2d ScreenPosition
            => Track.Camera.GetViewport(
                Track.Zoom,
                RenderSize.Width,
                RenderSize.Height).Vector;

        public Editor Track { get; }
        private bool _uicursor = false;
        private Gwen.Input.OpenTK _input;
        private bool _dragRider;
        private bool _invalidated;
        private readonly Stopwatch _autosavewatch = Stopwatch.StartNew();
        public MainWindow()
            : base(
                1280,
                720,
                new GraphicsMode(new ColorFormat(24), 0, 0, 0, ColorFormat.Empty),
                   "Line Rider: Advanced",
                   GameWindowFlags.Default,
                   DisplayDevice.Default,
                   2,
                   0,
                   GraphicsContextFlags.Default)
        {
            SafeFrameBuffer.Initialize();
            Track = new Editor();
            VSync = VSyncMode.On;
            Context.ErrorChecking = false;
            Track.UpdateScarf(Settings.SelectedScarf);
            WindowBorder = WindowBorder.Resizable;
            RenderFrame += (o, e) => { Render(); };
            UpdateFrame += (o, e) => { GameUpdate(); };
            GameService.Initialize(this);
            RegisterHotkeys();
        }

        public override void Dispose()
        {
            if (Canvas != null)
            {
                Canvas.Dispose();
                Canvas.Skin.Dispose();
                Canvas.Skin.DefaultFont.Dispose();
                Canvas.Renderer.Dispose();
                Canvas = null;
            }
            base.Dispose();
        }

        public bool ShouldXySnap()
        {
            return Settings.Editor.ForceXySnap || InputUtils.CheckPressed(Hotkey.ToolXYSnap);
        }
        public void Render(float blend = 1)
        {
            bool shouldrender = _invalidated ||
             Canvas.NeedsRedraw ||
            (Track.Playing) ||
            Canvas.Loading ||
            Track.NeedsDraw ||
            CurrentTools.SelectedTool.NeedsRender;
            if (shouldrender)
            {
                _invalidated = false;
                BeginOrtho();
                if (blend == 1 && Settings.SmoothPlayback && Track.Playing && !Canvas.Scrubbing)
                {
                    blend = Math.Min(1, (float)Track.Scheduler.ElapsedPercent);
                    if (ReversePlayback)
                        blend = 1 - blend;
                    Track.Camera.BeginFrame(blend, Track.Zoom);
                }
                else
                {
                    Track.Camera.BeginFrame(blend, Track.Zoom);
                }
                if (Track.Playing && CurrentTools.PencilTool.Active)
                {
                    CurrentTools.PencilTool.OnMouseMoved(InputUtils.GetMouse());
                }
                GL.ClearColor(Settings.NightMode
                   ? Constants.ColorNightMode
                   : (Settings.WhiteBG ? Constants.ColorWhite : (Settings.ColoredBG ? Constants.ColorColored : Constants.ColorOffwhite)));
                MSAABuffer.Use(RenderSize.Width, RenderSize.Height);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Enable(EnableCap.Blend);

#if debuggrid
                if (this.Keyboard.GetState().IsKeyDown(Key.C))
                    GameRenderer.DbgDrawGrid();
#endif
                Track.Render(blend);
//#if debugcamera
                if (this.Keyboard.GetState().IsKeyDown(Key.C))
                    GameRenderer.DbgDrawCamera();
//#endif
                Canvas.RenderCanvas();
                MSAABuffer.End();

                if (Settings.NightMode)
                {
                    StaticRenderer.RenderRect(new FloatRect(0, 0, RenderSize.Width, RenderSize.Height), Color.FromArgb(40, 0, 0, 0));
                }
                SwapBuffers();
                //there are machines and cases where a refresh may not hit the screen without calling glfinish...
                GL.Finish();
                var seconds = Track.FramerateWatch.Elapsed.TotalSeconds;
                Track.FramerateCounter.AddFrame(seconds);
                Track.FramerateWatch.Restart();
            }
            if (!Focused && !TrackRecorder.Recording)
            {
                Thread.Sleep(16);
            }
            else
            if (!Track.Playing &&
                    !Canvas.NeedsRedraw &&
                    !Track.NeedsDraw &&
                    !CurrentTools.SelectedTool.Active)
            {
                Thread.Sleep(10);
            }
        }
        private void GameUpdateHandleInput()
        {
            if (InputUtils.HandleMouseMove(out int x, out int y) && !Canvas.IsModalOpen)
            {
                CurrentTools.SelectedTool.OnMouseMoved(new Vector2d(x, y));
            }
        }
        public void GameUpdate()
        {
            GameUpdateHandleInput();
            var updates = Track.Scheduler.UnqueueUpdates();
            if (updates > 0)
            {
                Invalidate();
                if (Track.Playing)
                {
                    if (InputUtils.Check(Hotkey.PlaybackZoom))
                        Track.ZoomBy(0.08f);
                    else if (InputUtils.Check(Hotkey.PlaybackUnzoom))
                        Track.ZoomBy(-0.08f);
                }
            }
            if (_autosavewatch.Elapsed.TotalMinutes >= 5)
            {
                _autosavewatch.Restart();
                new Thread(() => { Track.BackupTrack(false); }).Start();
            }


            if (Track.Playing)
            {
                if (ReversePlayback)
                {
                    for (int i = 0; i < updates; i++)
                    {
                        Track.PreviousFrame();
                        Track.UpdateCamera(true);
                    }
                }
                else
                {
                    Track.Update(updates);
                }
            }
            AudioService.EnsureSync();
            if (Program.NewVersion != null)
            {
                Canvas.ShowOutOfDate();
            }
        }
        public void Invalidate()
        {
            _invalidated = true;
        }
        public void UpdateCursor()
        {
            MouseCursor cursor;

            if (_uicursor)
                cursor = Canvas.Platform.CurrentCursor;
            else if (Track.Playing || _dragRider)
                cursor = Cursors["default"];
            else if (CurrentTools.SelectedTool != null)
                cursor = CurrentTools.SelectedTool.Cursor;
            else
            {
                cursor = MouseCursor.Default;
                Debug.Fail("Improperly handled UpdateCursor");
            }
            if (cursor != Cursor)
            {
                Cursor = cursor;
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            Shaders.Load();
            MSAABuffer = new MsaaFbo();
            var renderer = new Gwen.Renderer.OpenTK();

            var skinpng = renderer.CreateTexture(GameResources.DefaultSkin);

            var fontpng = renderer.CreateTexture(GameResources.liberation_sans_15_png);
            var fontpngbold = renderer.CreateTexture(GameResources.liberation_sans_15_bold_png);

            var gamefont_15 = new Gwen.Renderer.BitmapFont(
                renderer,
                GameResources.liberation_sans_15_fnt,
                fontpng);


            var gamefont_15_bold = new Gwen.Renderer.BitmapFont(
                renderer,
                GameResources.liberation_sans_15_bold_fnt,
                fontpngbold);

            var skin = new Gwen.Skin.TexturedBase(renderer,
            skinpng,
            GameResources.DefaultColors
            )
            { DefaultFont = gamefont_15 };

            Fonts f = new Fonts(gamefont_15, gamefont_15_bold);
            Canvas = new GameCanvas(skin,
            this,
            renderer,
            f);

            _input = new Gwen.Input.OpenTK(this);
            _input.Initialize(Canvas);
            Canvas.ShouldDrawBackground = false;

            try   { Models.LoadModels(Settings.SelectedBoshSkin); }
            catch { Models.LoadModels(); }

            AddCursor("pencil", GameResources.cursor_pencil, 6, 25);
            AddCursor("line", GameResources.cursor_line, 11, 11);
            AddCursor("eraser", GameResources.cursor_eraser, 8, 8);
            AddCursor("hand", GameResources.cursor_move, 16, 16);
            AddCursor("hand_point", GameResources.cursor_hand, 14, 8);
            AddCursor("closed_hand", GameResources.cursor_dragging, 16, 16);
            AddCursor("adjustline", GameResources.cursor_select, 4, 4);
            AddCursor("size_nesw", GameResources.cursor_size_nesw, 16, 16);
            AddCursor("size_nwse", GameResources.cursor_size_nwse, 16, 16);
            AddCursor("size_hor", GameResources.cursor_size_horz, 16, 16);
            AddCursor("size_ver", GameResources.cursor_size_vert, 16, 16);
            AddCursor("size_all", GameResources.cursor_size_all, 16, 16);
            AddCursor("default", GameResources.cursor_default, 7, 4);
            AddCursor("zoom", GameResources.cursor_zoom_in, 11, 10);
            AddCursor("ibeam", GameResources.cursor_ibeam, 11, 11);
            Program.UpdateCheck();
            Track.AutoLoadPrevious();
            linerider.Tools.CurrentTools.Init();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Track.Camera.OnResize();
            try
            {
                Canvas.SetCanvasSize(RenderSize.Width, RenderSize.Height);
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (linerider.IO.TrackRecorder.Recording)
                    return;
                var r = _input.ProcessMouseMessage(e);
                _uicursor = _input.MouseCaptured;
                if (Canvas.GetOpenWindows().Count != 0)
                {
                    UpdateCursor();
                    return;
                }
                if (!r)
                {
                    InputUtils.ProcessMouseHotkeys();
                    if (!Track.Playing)
                    {
                        bool dragstart = false;
                        if (Track.Offset == 0 &&
                         e.Button == MouseButton.Left &&
                        InputUtils.Check(Hotkey.EditorMoveStart))
                        {
                            var gamepos = ScreenPosition + (new Vector2d(e.X, e.Y) / Track.Zoom);
                            dragstart = Game.Rider.GetBounds(
                                Track.GetStart()).Contains(
                                    gamepos.X,
                                    gamepos.Y);
                            if (dragstart)
                            {
                                // 5 is arbitrary, but i assume that's a decent
                                // place to assume the user has done "work"
                                if (!Track.MoveStartWarned && Track.LineCount > 5)
                                {
                                    var popup = MessageBox.Show(Canvas,
                                        "You're about to move the start position of the rider." +
                                        " This cannot be undone, and may drastically change how your track plays." +
                                        "\nAre you sure you want to do this?", "Warning", MessageBox.ButtonType.OkCancel);
                                    popup.RenameButtons("I understand");
                                    popup.Dismissed += (o, args) =>
                                    {
                                        if (popup.Result == Gwen.DialogResult.OK)
                                        {
                                            Track.MoveStartWarned = true;
                                        }
                                    };
                                }
                                else
                                {
                                    _dragRider = dragstart;
                                }
                            }
                        }
                        if (!_dragRider && !dragstart)
                        {
                            if (e.Button == MouseButton.Left)
                            {
                                CurrentTools.SelectedTool.OnMouseDown(new Vector2d(e.X, e.Y));
                            }
                            else if (e.Button == MouseButton.Right)
                            {
                                CurrentTools.SelectedTool.OnMouseRightDown(new Vector2d(e.X, e.Y));
                            }
                        }
                    }
                    else if (CurrentTools.SelectedTool == CurrentTools.PencilTool && CurrentTools.PencilTool.DrawingScenery)
                    {
                        if (e.Button == MouseButton.Left)
                        {
                            CurrentTools.PencilTool.OnMouseDown(new Vector2d(e.X, e.Y));
                        }
                    }
                }
                UpdateCursor();
                Invalidate();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (linerider.IO.TrackRecorder.Recording)
                    return;
                _dragRider = false;
                var r = _input.ProcessMouseMessage(e);
                _uicursor = _input.MouseCaptured;
                InputUtils.CheckCurrentHotkey();
                if (!r || CurrentTools.SelectedTool.IsMouseButtonDown)
                {
                    if (!CurrentTools.SelectedTool.IsMouseButtonDown &&
                        Canvas.GetOpenWindows().Count != 0)
                    {
                        UpdateCursor();
                        return;
                    }
                    if (e.Button == MouseButton.Left)
                    {
                        CurrentTools.SelectedTool.OnMouseUp(new Vector2d(e.X, e.Y));
                    }
                    else if (e.Button == MouseButton.Right)
                    {
                        CurrentTools.SelectedTool.OnMouseRightUp(new Vector2d(e.X, e.Y));
                    }
                }
                UpdateCursor();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (linerider.IO.TrackRecorder.Recording)
                    return;
                var r = _input.ProcessMouseMessage(e);
                _uicursor = _input.MouseCaptured;
                if (Canvas.GetOpenWindows().Count != 0)
                {
                    UpdateCursor();
                    return;
                }
                if (_dragRider)
                {
                    var pos = new Vector2d(e.X, e.Y);
                    var gamepos = ScreenPosition + (pos / Track.Zoom);
                    Track.Stop();
                    using (var trk = Track.CreateTrackWriter())
                    {
                        trk.Track.StartOffset = gamepos;
                        Track.Reset();
                        Track.NotifyTrackChanged();
                    }
                    Invalidate();
                }
                if (CurrentTools.SelectedTool.RequestsMousePrecision)
                {
                    CurrentTools.SelectedTool.OnMouseMoved(new Vector2d(e.X, e.Y));
                }

                if (r)
                {
                    Invalidate();
                }
                UpdateCursor();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (linerider.IO.TrackRecorder.Recording)
                    return;
                if (_input.ProcessMouseMessage(e))
                    return;
                if (Canvas.GetOpenWindows().Count != 0)
                {
                    UpdateCursor();
                    return;
                }
                var delta = (float.IsNaN(e.DeltaPrecise) ? e.Delta : e.DeltaPrecise);
                delta *= Settings.ScrollSensitivity;
                Track.ZoomBy(delta / 6);
                UpdateCursor();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            try
            {
                if (!e.IsRepeat)
                {
                    InputUtils.KeyDown(e.Key);
                }
                InputUtils.UpdateKeysDown(e.Keyboard, e.Modifiers);
                if (linerider.IO.TrackRecorder.Recording)
                    return;
                var mod = e.Modifiers;
                if (_input.ProcessKeyDown(e))
                {
                    return;
                }
                if (e.Key == Key.Escape && !e.IsRepeat)
                {
                    var openwindows = Canvas.GetOpenWindows();
                    if (openwindows != null && openwindows.Count >= 1)
                    {
                        foreach (var v in openwindows)
                        {
                            ((WindowControl)v).Close();
                            Invalidate();
                        }
                        return;
                    }
                }
                if (
                    Canvas.IsModalOpen ||
                    (!Track.Playing && CurrentTools.SelectedTool.OnKeyDown(e.Key)) ||
                    _dragRider)
                {
                    UpdateCursor();
                    Invalidate();
                    return;
                }
                InputUtils.ProcessKeyboardHotkeys();
                UpdateCursor();
                Invalidate();
                var input = e.Keyboard;
                if (!input.IsAnyKeyDown)
                    return;
                if (input.IsKeyDown(Key.AltLeft) || input.IsKeyDown(Key.AltRight))
                {
                    if (input.IsKeyDown(Key.Enter))
                    {
                        if (WindowBorder == WindowBorder.Resizable)
                        {
                            WindowBorder = WindowBorder.Hidden;
                            X = 0;
                            Y = 0;
                            var area = Screen.PrimaryScreen.Bounds;
                            RenderSize = area.Size;
                        }
                        else
                        {
                            WindowBorder = WindowBorder.Resizable;
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            try
            {
                InputUtils.UpdateKeysDown(e.Keyboard, e.Modifiers);
                if (linerider.IO.TrackRecorder.Recording)
                    return;
                InputUtils.CheckCurrentHotkey();
                CurrentTools.SelectedTool.OnKeyUp(e.Key);
                _input.ProcessKeyUp(e);
                UpdateCursor();
                Invalidate();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }


        public void StopTools()
        {
            CurrentTools.SelectedTool.Stop();
        }
        public void StopHandTool()
        {
            if (CurrentTools.SelectedTool == CurrentTools.HandTool)
            {
                CurrentTools.HandTool.Stop();
            }
        }

        private void BeginOrtho()
        {
            if (RenderSize.Height > 0 && RenderSize.Width > 0)
            {
                GL.Viewport(new Rectangle(0, 0, RenderSize.Width, RenderSize.Height));
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, RenderSize.Width, RenderSize.Height, 0, 0, 1);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
            }
        }

        private void AddCursor(string name, Bitmap image, int hotx, int hoty)
        {
            var data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);
            Cursors[name] = new MouseCursor(hotx, hoty, image.Width, image.Height, data.Scan0);
            image.UnlockBits(data);
        }
        private void RegisterHotkeys()
        {
            RegisterPlaybackHotkeys();
            RegisterEditorHotkeys();
            RegisterSettingHotkeys();
            RegisterPopupHotkeys();
        }
        private void RegisterSettingHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.PreferenceOnionSkinning, () => true, () =>
            {
                Settings.OnionSkinning = !Settings.OnionSkinning;
                Settings.Save();
                Track.Invalidate();
            });
        }
        private void RegisterPlaybackHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.PlaybackStartSlowmo, () => true, () =>
            {
                StopTools();
                Track.StartFromFlag();
                Track.Scheduler.UpdatesPerSecond = Settings.SlowmoSpeed;
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackStartIgnoreFlag, () => true, () =>
            {
                StopTools();
                Track.StartIgnoreFlag();
                Track.ResetSpeedDefault();
            });
            // InputUtils.RegisterHotkey(Hotkey.PlaybackStartGhostFlag, () => true, () =>
            // {
            //     StopTools();
            //     Track.ResumeFromFlag();
            //     Track.ResetSpeedDefault();
            // });
            InputUtils.RegisterHotkey(Hotkey.PlaybackStart, () => true, () =>
            {
                StopTools();
                Track.StartFromFlag();
                Track.ResetSpeedDefault();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackStop, () => true, () =>
            {
                StopTools();
                Track.Stop();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackFlag, () => true, () =>
            {
                Track.Flag(Track.Offset);
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackForward, () => true, () =>
            {
                StopTools();
                if (Track.Paused)
                    Track.TogglePause();
                ReversePlayback = false;
                UpdateCursor();
            },
            () =>
            {
                if (!Track.Paused)
                    Track.TogglePause();
                ReversePlayback = false;
                Track.UpdateCamera();
                UpdateCursor();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackBackward, () => true, () =>
            {
                StopTools();
                if (Track.Paused)
                    Track.TogglePause();
                ReversePlayback = true;
                UpdateCursor();
            },
            () =>
            {
                if (!Track.Paused)
                    Track.TogglePause();
                ReversePlayback = false;
                Track.UpdateCamera();
                UpdateCursor();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackFrameNext, () => true, () =>
            {
                StopHandTool();
                if (!Track.Paused)
                    Track.TogglePause();
                Track.NextFrame();
                Invalidate();
                Track.UpdateCamera();
                if (CurrentTools.SelectedTool.IsMouseButtonDown)
                {
                    CurrentTools.SelectedTool.OnMouseMoved(InputUtils.GetMouse());
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackFramePrev, () => true, () =>
            {
                StopHandTool();
                if (!Track.Paused)
                    Track.TogglePause();
                Track.PreviousFrame();
                Invalidate();
                Track.UpdateCamera(true);
                if (CurrentTools.SelectedTool.IsMouseButtonDown)
                {
                    CurrentTools.SelectedTool.OnMouseMoved(InputUtils.GetMouse());
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackSpeedUp, () => true, () =>
            {
                Track.PlaybackSpeedUp();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackSpeedDown, () => true, () =>
            {
                Track.PlaybackSpeedDown();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackSlowmo, () => true, () =>
            {
                if (Track.Scheduler.UpdatesPerSecond !=
                Settings.SlowmoSpeed)
                {
                    Track.Scheduler.UpdatesPerSecond = Settings.SlowmoSpeed;
                }
                else
                {
                    Track.ResetSpeedDefault(false);
                }
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackTogglePause, () => true, () =>
            {
                StopTools();
                Track.TogglePause();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.PlaybackIterationNext, () => !Track.Playing, () =>
            {
                StopTools();
                if (!Track.Paused)
                    Track.TogglePause();
                if (Track.IterationsOffset != 6)
                {
                    Track.IterationsOffset++;
                }
                else
                {
                    Track.NextFrame();
                    Track.IterationsOffset = 0;
                    Track.UpdateCamera();
                }
                Track.InvalidateRenderRider();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackIterationPrev, () => !Track.Playing, () =>
            {
                if (Track.Offset != 0)
                {
                    StopTools();
                    if (Track.IterationsOffset > 0)
                    {
                        Track.IterationsOffset--;
                    }
                    else
                    {
                        Track.PreviousFrame();
                        Track.IterationsOffset = 6;
                        Invalidate();
                        Track.UpdateCamera();
                    }
                    Track.InvalidateRenderRider();
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackResetCamera, () => true, () =>
            {
                Track.Zoom = Track.Timeline.GetFrameZoom(Track.Offset);
                Track.UseUserZoom = false;
                Track.UpdateCamera();
            });
        }
        private void RegisterPopupHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.LoadWindow, () => true, () =>
            {
                Canvas.ShowLoadDialog();
            });

            InputUtils.RegisterHotkey(Hotkey.PreferencesWindow,
            () => !CurrentTools.SelectedTool.Active,
            () =>
            {
                Canvas.ShowPreferencesDialog();
            });
            InputUtils.RegisterHotkey(Hotkey.GameMenuWindow,
            () => !CurrentTools.SelectedTool.Active,
            () =>
            {
                Canvas.ShowGameMenuWindow();
            });
            InputUtils.RegisterHotkey(Hotkey.TrackPropertiesWindow,
            () => !CurrentTools.SelectedTool.Active,
            () =>
            {
                Canvas.ShowTrackPropertiesDialog();
            });
            InputUtils.RegisterHotkey(Hotkey.Quicksave, () => true, () =>
               {
                   Track.QuickSave();
               });
        }
        private void RegisterEditorHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.ToggleAll, () => !Track.Playing, () =>
            {
                if (Settings.ToggleConfig)
                {
                    if (Settings.ConfigTContactPoints)
                    {
                        Settings.Editor.DrawContactPoints = true;
                    }
                    if (Settings.ConfigTGravityWells)
                    {
                        Settings.Editor.RenderGravityWells = true;
                    }
                    if (Settings.ConfigTHitTest)
                    {
                        Settings.Editor.HitTest = true;
                    }
                    if (Settings.ConfigTMomentumVectors)
                    {
                        Settings.Editor.MomentumVectors = true;
                    }
                    if (Settings.ConfigTOnionSkinning)
                    {
                        Settings.OnionSkinning = true;
                    }
                    Settings.ToggleConfig = false;
                }
                else
                {
                    if (Settings.ConfigTContactPoints)
                    {
                        Settings.Editor.DrawContactPoints = false;
                    }
                    if (Settings.ConfigTGravityWells)
                    {
                        Settings.Editor.RenderGravityWells = false;
                    }
                    if (Settings.ConfigTHitTest)
                    {
                        Settings.Editor.HitTest = false;
                    }
                    if (Settings.ConfigTMomentumVectors)
                    {
                        Settings.Editor.MomentumVectors = false;
                    }
                    if (Settings.ConfigTOnionSkinning)
                    {
                        Settings.OnionSkinning = false;
                    }
                    Settings.ToggleConfig = true;
                }
            });

            InputUtils.RegisterHotkey(Hotkey.ScenerySetSmall, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.GreenMultiplier = 0.1f;
            });
            InputUtils.RegisterHotkey(Hotkey.SceneryIncreaseBig, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.GreenMultiplier = (float)Math.Round(Math.Min(25.5f, toolswatch.GreenMultiplier + 0.10f), 2);
            });
            InputUtils.RegisterHotkey(Hotkey.SceneryDecreaseBig, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.GreenMultiplier = (float)Math.Round(Math.Max(0.1f, toolswatch.GreenMultiplier - 0.10f), 2);
            });

            InputUtils.RegisterHotkey(Hotkey.AccelerationSetSmall, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.RedMultiplier = 1;
            });
            InputUtils.RegisterHotkey(Hotkey.AccelerationSetMedium, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.RedMultiplier = 100;
            });
            InputUtils.RegisterHotkey(Hotkey.AccelerationSetLarge, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.RedMultiplier = 200;
            });
            InputUtils.RegisterHotkey(Hotkey.AccelerationSetMax, () => !Track.Playing, () =>
            {
                var toolswatch = CurrentTools.SelectedTool.Swatch;
                toolswatch.RedMultiplier = 255;
            });

            InputUtils.RegisterHotkey(Hotkey.EditorPencilTool, () => !Track.Playing, () =>
            {
                CurrentTools.SetTool(CurrentTools.PencilTool);
            });
            InputUtils.RegisterHotkey(Hotkey.EditorLineTool, () => !Track.Playing, () =>
            {
                CurrentTools.SetTool(CurrentTools.LineTool);
            });
            InputUtils.RegisterHotkey(Hotkey.EditorEraserTool, () => !Track.Playing, () =>
            {
                CurrentTools.SetTool(CurrentTools.EraserTool);
            });
            InputUtils.RegisterHotkey(Hotkey.EditorSelectTool, () => !Track.Playing, () =>
            {
                CurrentTools.SetTool(CurrentTools.MoveTool);
            });
            InputUtils.RegisterHotkey(Hotkey.EditorPanTool, () => !Track.Playing, () =>
            {
                CurrentTools.SetTool(CurrentTools.HandTool);
            });
            InputUtils.RegisterHotkey(Hotkey.EditorQuickPan, () => !Track.Playing && !Canvas.IsModalOpen, () =>
            {
                CurrentTools.QuickPan = true;
                Invalidate();
                UpdateCursor();
            },
            () =>
            {
                CurrentTools.QuickPan = false;
                Invalidate();
                UpdateCursor();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorDragCanvas, () => !Track.Playing && !Canvas.IsModalOpen, () =>
            {
                var mouse = InputUtils.GetMouse();
                CurrentTools.QuickPan = true;
                CurrentTools.HandTool.OnMouseDown(new Vector2d(mouse.X, mouse.Y));
            },
            () =>
            {
                if (CurrentTools.QuickPan)
                {
                    var mouse = InputUtils.GetMouse();
                    CurrentTools.HandTool.OnMouseUp(new Vector2d(mouse.X, mouse.Y));
                    CurrentTools.QuickPan = false;
                }
            });

            InputUtils.RegisterHotkey(Hotkey.EditorUndo, () => !Track.Playing, () =>
            {
                CurrentTools.SelectedTool.Cancel();
                var hint = Track.UndoManager.Undo();
                CurrentTools.SelectedTool.OnUndoRedo(true, hint);
                Invalidate();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.EditorRedo, () => !Track.Playing, () =>
            {
                CurrentTools.SelectedTool.Cancel();
                var hint = Track.UndoManager.Redo();
                CurrentTools.SelectedTool.OnUndoRedo(false, hint);
                Invalidate();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.EditorRemoveLatestLine, () => !Track.Playing, () =>
            {
                if (!Track.Playing)
                {
                    StopTools();
                    using (var trk = Track.CreateTrackWriter())
                    {
                        CurrentTools.SelectedTool.Stop();
                        var l = trk.GetNewestLine();
                        if (l != null)
                        {
                            Track.UndoManager.BeginAction();
                            trk.RemoveLine(l);
                            Track.UndoManager.EndAction();
                        }

                        Track.NotifyTrackChanged();
                        Invalidate();
                    }
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.EditorFocusStart, () => !Track.Playing, () =>
            {
                using (var trk = Track.CreateTrackReader())
                {
                    var l = trk.GetOldestLine();
                    if (l != null)
                    {
                        Track.Camera.SetFrameCenter(l.Position);
                        Invalidate();
                    }
                }
            });
            InputUtils.RegisterHotkey(Hotkey.EditorFocusLastLine, () => !Track.Playing, () =>
            {
                using (var trk = Track.CreateTrackReader())
                {
                    var l = trk.GetNewestLine();
                    if (l != null)
                    {
                        Track.Camera.SetFrameCenter(l.Position);
                        Invalidate();
                    }
                }
            });
            InputUtils.RegisterHotkey(Hotkey.EditorCycleToolSetting, () => !Track.Playing, () =>
            {
                if (CurrentTools.SelectedTool.ShowSwatch)
                {
                    CurrentTools.SelectedTool.Swatch.IncrementSelectedMultiplier();
                    Invalidate();
                }
            });
            InputUtils.RegisterHotkey(Hotkey.ToolToggleOverlay, () => !Track.Playing, () =>
            {
                Settings.Local.TrackOverlay = !Settings.Local.TrackOverlay;
            });
            InputUtils.RegisterHotkey(Hotkey.EditorToolColor1, () => !Track.Playing, () =>
            {
                var swatch = CurrentTools.SelectedTool.Swatch;
                if (swatch != null)
                {
                    swatch.Selected = LineType.Blue;
                }
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorToolColor2, () => !Track.Playing, () =>
            {
                var swatch = CurrentTools.SelectedTool.Swatch;
                if (swatch != null)
                {
                    swatch.Selected = LineType.Red;
                }
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorToolColor3, () => !Track.Playing, () =>
            {
                var swatch = CurrentTools.SelectedTool.Swatch;
                if (swatch != null)
                {
                    swatch.Selected = LineType.Scenery;
                }
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorFocusFlag, () => !Track.Playing, () =>
            {
                var flag = Track.GetFlag();
                if (flag != null)
                {
                    Track.Camera.SetFrameCenter(flag.State.CalculateCenter());
                    Invalidate();
                }
            });
            InputUtils.RegisterHotkey(Hotkey.EditorFocusRider, () => !Track.Playing, () =>
            {
                Track.Camera.SetFrameCenter(Track.RenderRider.CalculateCenter());
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorCancelTool,
            () => CurrentTools.SelectedTool.Active,
            () =>
            {
                var tool = CurrentTools.SelectedTool;
                var selecttool = CurrentTools.SelectTool;
                if (tool == selecttool)
                {
                    selecttool.CancelSelection();
                }
                else
                {
                    tool.Cancel();
                }
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.ToolCopy, () => !Track.Playing &&
            CurrentTools.SelectedTool == CurrentTools.SelectTool, () =>
            {
                CurrentTools.SelectTool.Copy();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolCut, () => !Track.Playing &&
            CurrentTools.SelectedTool == CurrentTools.SelectTool, () =>
            {
                CurrentTools.SelectTool.Cut();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolPaste, () => !Track.Playing &&
            (CurrentTools.SelectedTool == CurrentTools.SelectTool ||
            CurrentTools.SelectedTool == CurrentTools.MoveTool), () =>
            {
                CurrentTools.SelectTool.Paste();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolDelete, () => !Track.Playing &&
            CurrentTools.SelectedTool == CurrentTools.SelectTool, () =>
            {
                CurrentTools.SelectTool.Delete();
                Invalidate();
            },
            null,
            repeat: false);
        }
    }
}