using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using DotNetPerls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XInput = Microsoft.Xna.Framework.Input;


// todo: save file dialog for exporter
// export as labeled pieces and comment in the output

namespace ForzaStudio
{
    public partial class ForzaStudioForm : Form
    {
        #region Fields
        private Car car;
        private Camera Camera;
        private GraphicsDevice Device;
        private BasicEffect be;
        private BasicEffect color;  // use for colored vertices without normals

        private bool RefreshViewport = true;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize the form.
        /// </summary>
        public ForzaStudioForm()
        {
            InitializeComponent();
            ManuallyUpdateComponent();
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs immediately after the form is displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            //this.statusStrip.Focus(); // give something focus so the mouse wheel event gets fired...

            // start the main loop
            MainLoop();
        }

        void Form1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.statusStrip.Focused)
            {
                if (e.Delta < 0)
                    Camera.ApplyZoomForce(0.02f);
                else if (e.Delta > 0)
                    Camera.ApplyZoomForce(-0.02f);
            }
        }

        /// <summary>
        /// Loads a car model file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    car = new Car(ofd.FileName);
                    Text = "Forza Studio - " + Path.GetFileName(ofd.FileName);
                    tvCarStructure.Nodes.Clear();

                    // get base name without lod information
                    string carName = Path.GetFileName(ofd.FileName).Replace(".carbin", "").Replace("_lod0", "");

                    // todo: separate models by lod
                    for (int i = 0; i < car.Sections.Length; i++)
                    {
                        CarSection section = car.Sections[i];
                        for (int j = 0; j < section.Pieces.Length; j++)
                        {
                            string cName = carName + "_lod" + section.Pieces[j].Lod;

                            // check if main node has been added
                            bool mainNodeExists = false;
                            foreach (TreeNode n in tvCarStructure.Nodes)
                            {
                                if (n.Text == cName)
                                {
                                    mainNodeExists = true;
                                    break;
                                }
                            }
                            if (!mainNodeExists)
                            {
                                tvCarStructure.Nodes.Add(cName, cName);

                                // todo: account for different lods
                                tvCarStructure.Nodes[cName].ToolTipText = "Vertex Count: " + car.VertexCount + " Face Count: " + car.FaceCount;
                            }

                            // check if section node exists
                            bool sectionNodeExists = false;
                            foreach (TreeNode n in tvCarStructure.Nodes[cName].Nodes)
                            {
                                if (n.Text == section.Name)
                                {
                                    sectionNodeExists = true;
                                    break;
                                }
                            }

                            // add section
                            if (!sectionNodeExists)
                            {
                                tvCarStructure.Nodes[cName].Nodes.Add("Section" + i.ToString(), section.Name);
                                tvCarStructure.Nodes[cName].Nodes["Section" + i.ToString()].ToolTipText = GetSectionDescription(section.Name) + " - Vertex Count: " + section.VertexCount + " Face Count: " + section.FaceCount;
                            }

                            // add piece
                            tvCarStructure.Nodes[cName].Nodes["Section" + i.ToString()].Nodes.Add("Section" + i + "_Piece" + j.ToString(), section.Pieces[j].Name);
                            tvCarStructure.Nodes[cName].Nodes["Section" + i.ToString()].Nodes["Section" + i + "_Piece" + j.ToString()].ToolTipText = "Vertex Count: " + section.Pieces[j].VertexCount + " Face Count: " + section.Pieces[j].FaceCount;

                        }
                    }

                    // check root node which will end up checking everything else
                    tvCarStructure.Nodes[0].Checked = true;

                    // todo: either reset custom options like vertex color, or re-apply them to new import
                    // most likely re-apply...

                    Camera.MoveTo(new Vector3(2.1f, 1.25f, 1.1f));
                    Camera.LookTo(new Vector2(-2.75f, -0.4f));
                    RefreshViewport = true;
                }
            }

        }

        private void tvCarStructure_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // fixes bug where right clicks don't update selection
            tvCarStructure.SelectedNode = e.Node;

            ctxMenuSelectiveExport.Visible = !e.Node.Name.Contains("Piece");    // assume you dont need a selective export on a single piece...

            // todo: only show selective export if more than one child is checked and not every child is checked...
        }


        DateTime lastCheckTime = DateTime.Now;
        private void tvCarStructure_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            //TimeSpan elapse = DateTime.Now - lastCheckTime;
            //if (elapse.TotalMilliseconds > 1000)
            //{
            //    tvCarStructure.AfterCheck += new TreeViewEventHandler(tvCarStructure_AfterCheck);
            //}
            //else
            //{
            //    tvCarStructure.AfterCheck -= new TreeViewEventHandler(tvCarStructure_AfterCheck);
            //}
        }

        private void tvCarStructure_AfterCheck(object sender, TreeViewEventArgs e)
        {
            tvCarStructure.AfterCheck -= new TreeViewEventHandler(tvCarStructure_AfterCheck);
            CheckChildNode(e.Node);
            CheckParentNode(e.Node);
            tvCarStructure.AfterCheck += new TreeViewEventHandler(tvCarStructure_AfterCheck);
            RefreshViewport = true;
        }

        private void tvCarStructure_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tabCtrl.SelectedTab.Text == "Information")
            {
                if (e.Node.Name.Contains("Section") && e.Node.Name.Contains("Piece"))
                {
                    string[] str = e.Node.Name.Split('_');

                    // todo: display piece information in information tab
                    int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                    int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Piece Header" + Environment.NewLine);
                    sb.Append("===============================" + Environment.NewLine);
                    sb.Append("unk1: " + car.Sections[sectionIndex].Pieces[pieceIndex].Lod + Environment.NewLine);
                    sb.Append("unk2: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk2 + Environment.NewLine);
                    sb.Append("unk3: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk3 + Environment.NewLine);
                    sb.Append("unk4: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk4 + Environment.NewLine);
                    sb.Append("unk5: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk5 + Environment.NewLine);
                    sb.Append("unk6: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk6 + Environment.NewLine);
                    sb.Append("unk7: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk7 + Environment.NewLine);
                    sb.Append("unk8: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk8 + Environment.NewLine);
                    sb.Append("unk9: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk9 + Environment.NewLine);
                    sb.Append("unk10: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk10 + Environment.NewLine);
                    sb.Append("unk11: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk11 + Environment.NewLine);
                    sb.Append("unk12: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk12 + Environment.NewLine);
                    sb.Append("unk13: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk13 + Environment.NewLine);
                    sb.Append("unk14: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk14 + Environment.NewLine);
                    sb.Append("unk15: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk15 + Environment.NewLine);
                    sb.Append("unk16: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk16 + Environment.NewLine);
                    sb.Append("unk17: " + car.Sections[sectionIndex].Pieces[pieceIndex].unk17 + Environment.NewLine);

                    txtInformation.Text = sb.ToString();
                    // indices drop down list
                }
                else if (e.Node.Name.Contains("Section"))
                {
                    int sectionIndex = Convert.ToInt32(e.Node.Name.Replace("Section", ""));

                    if (sectionIndex >= 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Section Header" + Environment.NewLine);
                        sb.Append("===============================" + Environment.NewLine);
                        sb.Append("y shift: " + car.Sections[sectionIndex].xOffset + Environment.NewLine);
                        sb.Append("x shift: " + car.Sections[sectionIndex].yOffset + Environment.NewLine);
                        sb.Append("z shift: " + car.Sections[sectionIndex].zOffset + Environment.NewLine);
                        sb.Append("unk4: " + car.Sections[sectionIndex].flt4 + Environment.NewLine);
                        sb.Append("unk5: " + car.Sections[sectionIndex].flt5 + Environment.NewLine);
                        sb.Append("unk6: " + car.Sections[sectionIndex].flt6 + Environment.NewLine);
                        sb.Append("unk7: " + car.Sections[sectionIndex].flt7 + Environment.NewLine);
                        sb.Append("unk8: " + car.Sections[sectionIndex].flt8 + Environment.NewLine);
                        sb.Append("unk9: " + car.Sections[sectionIndex].flt9 + Environment.NewLine);
                        sb.Append("unk10: " + car.Sections[sectionIndex].flt10 + Environment.NewLine);
                        sb.Append("unk11: " + car.Sections[sectionIndex].flt11 + Environment.NewLine);
                        sb.Append("unk12: " + car.Sections[sectionIndex].flt12 + Environment.NewLine);
                        sb.Append("unk13: " + car.Sections[sectionIndex].flt13 + Environment.NewLine);
                        sb.Append("unk14: " + car.Sections[sectionIndex].flt14 + Environment.NewLine);

                        // vertices (todo: doesnt work with lod because those are in LodVertices)
                        sb.Append(Environment.NewLine + Environment.NewLine + "Vertices (first 100 out of " + car.Sections[sectionIndex].Vertices.Length + ")" + Environment.NewLine);
                        sb.Append("===============================" + Environment.NewLine);
                        int i = 0;
                        foreach (CarVertex v in car.Sections[sectionIndex].Vertices)
                        {
                            sb.Append("v" + i.ToString().PadLeft(2, '0') + "\tx:" + FloatFormat(v.Position.X) + "\ty:" + FloatFormat(v.Position.Y) + "\tz:" + FloatFormat(v.Position.Z));// + "\ts:" + FloatFormat(v.s));
                            sb.Append("\tu0:" + FloatFormat(v.Texture0Coordinate.X) + "\tv0:" + FloatFormat(v.Texture0Coordinate.Y) + "\tu1:" + FloatFormat(v.Texture1Coordinate.X) + "\tv1:" + FloatFormat(v.Texture1Coordinate.Y));
                            sb.Append("\tnx: " + FloatFormat(v.Normal.X) + "\tny:" + FloatFormat(v.Normal.Y) + "\tnz:" + FloatFormat(v.Normal.Z));
                            sb.Append("\tu1x: " + FloatFormat(v.unk1.X) + "\tu1y:" + FloatFormat(v.unk1.Y) + "\tu1z:" + FloatFormat(v.unk1.Z));
                            sb.Append("\tu2x: " + FloatFormat(v.unk2.X) + "\tu2y:" + FloatFormat(v.unk2.Y) + "\tu2z:" + FloatFormat(v.unk2.Z));
                            sb.Append("\tu3x: " + FloatFormat(v.unk3.X) + "\tu3y:" + FloatFormat(v.unk3.Y) + "\tu3z:" + FloatFormat(v.unk3.Z));
                            sb.Append("\tu4x: " + FloatFormat(v.unk4.X) + "\tu4y:" + FloatFormat(v.unk4.Y) + "\tu4z:" + FloatFormat(v.unk4.Z));
                            sb.Append("\tu5x: " + FloatFormat(v.unk5.X) + "\tu5y:" + FloatFormat(v.unk5.Y) + "\tu5z:" + FloatFormat(v.unk5.Z));

                            sb.Append(Environment.NewLine);
                            i++;

                            if (i == 100) break; // only display the first hundred
                        }

                        txtInformation.Text = sb.ToString();
                    }
                    else // header values
                    {
                        StringBuilder sb2 = new StringBuilder();
                        sb2.Append("Header" + Environment.NewLine);
                        sb2.Append("===============================" + Environment.NewLine);
                        sb2.Append("unk1: " + FloatFormat(car.unk1) + Environment.NewLine);
                        sb2.Append("unk2: " + FloatFormat(car.unk2) + Environment.NewLine);


                        //sb.Append(Environment.NewLine + Environment.NewLine + "Unknown Lists" + Environment.NewLine);
                        //sb.Append("===============================" + Environment.NewLine);

                        txtInformation.Text = sb2.ToString();
                    }
                }
            }
        }

        private void ctxMenuExport_Click(object sender, EventArgs e)
        {
            Export(false);
        }


        private void ctxMenuSelectiveExport_Click(object sender, EventArgs e)
        {
            Export(true);
        }


        private void Export(bool selective)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Wavefront Obj (*.obj)|*.obj";

                if (tvCarStructure.SelectedNode.Name.Contains(car.Name))
                {
                    // entire model
                    string name = tvCarStructure.Nodes[tvCarStructure.SelectedNode.Name].Text;
                    sfd.FileName = name + ".obj";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream wObj = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(wObj))
                        {
                            car.Export(sw, selective);
                        }
                    }
                }
                else if (!tvCarStructure.SelectedNode.Name.Contains('_'))
                {
                    // entire section
                    int sectionIndex = Convert.ToInt32(tvCarStructure.SelectedNode.Name.Replace("Section", ""));
                    sfd.FileName = tvCarStructure.SelectedNode.Parent.Text + "_" + tvCarStructure.SelectedNode.Text + ".obj";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream wObj = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(wObj))
                        {
                            car.Sections[sectionIndex].Export(sw, selective);
                        }
                    }
                }
                else
                {
                    // individual piece
                    string[] str = tvCarStructure.SelectedNode.Name.Split('_');
                    int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                    int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));
                    sfd.FileName = car.Name + "_" + tvCarStructure.SelectedNode.Parent.Text + "_" + tvCarStructure.SelectedNode.Text + ".obj";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream wObj = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(wObj))
                        {
                            car.Sections[sectionIndex].Pieces[pieceIndex].Export(sw);
                        }
                    }
                }
            }
        }

        private void pnlVisual_Resize(object sender, EventArgs e)
        {
            ResetDevice();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        private void chkNormals_Click(object sender, EventArgs e)
        {
            RefreshViewport = true;
        }

        private void chkWireframe_Click(object sender, EventArgs e)
        {
            RefreshViewport = true;
        }

        private void cmbFillMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshViewport = true;
        }

        private void cmbCullMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshViewport = true;
        }

        private void chkGrid_Click(object sender, EventArgs e)
        {
            RefreshViewport = true;
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ForzaStudioForm));
            Image img = ((Icon)(resources.GetObject("$this.Icon"))).ToBitmap();

            string credits = "The following people have either directly or indirectly contributed to the development of this application..." + Environment.NewLine + Environment.NewLine +
                "- Veegie (Nate Hawbaker)" + Environment.NewLine + "- ajmiles" + Environment.NewLine + "- revelation" + Environment.NewLine + "- Dan Frederiksen" + Environment.NewLine + "- sommergemuese" + Environment.NewLine + Environment.NewLine +
                "A special thanks also goes out to everyone else at XeNTaX for putting together one hell of a site." + Environment.NewLine + Environment.NewLine + "Cheers  :-)";

            BetterDialog.ShowDialog("About Forza Studio", "Credits", credits, null, "Close", img);
        }

        private void cmbModelColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                cmbModelColor.BackColor = cd.Color;
                foreach (CarSection section in car.Sections)
                {
                    foreach (CarPiece piece in section.Pieces)
                    {
                        for (int i = 0; i < piece.Vertices.Length; i++)
                        {
                            piece.Vertices[i].color = new Microsoft.Xna.Framework.Graphics.Color(cd.Color.R, cd.Color.G, cd.Color.B, cd.Color.A);
                        }
                    }
                }
                RefreshViewport = true;
            }
        }

        // make sure to refresh the viewport if the panel ever needs repainting
        private void pnlVisual_Paint(object sender, PaintEventArgs e)
        {
            RefreshViewport = true;
        }

        private void pnlVisual_Click(object sender, EventArgs e)
        {
            // give some hidden panel focus and check on that to determine if input is for camera
            this.statusStrip.Focus();


        }

        #endregion

        #region Methods

        private void MainLoop()
        {
            Camera = new Camera(pnlVisual);
            Camera.MoveTo(new Vector3(2.1f, 1.25f, 1.1f));
            Camera.LookTo(new Vector2(-2.75f, -0.4f));

            CreateDevice();

            while (!IsDisposed)
            {
                UpdateCamera();
                UpdateScene();
                statusRenderInfo.Text = GetRenderInformation();
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);
            }
        }

        private string GetRenderInformation()
        {
            if (car != null)
            {
                int vertexCount = 0;
                int faceCount = 0;
                foreach (CarSection section in car.Sections)
                {
                    foreach (CarPiece piece in section.Pieces)
                    {
                        if (piece.Visible)
                        {
                            vertexCount += piece.VertexCount;
                            faceCount += piece.FaceCount;
                        }
                    }
                }
                return string.Format("Rendering {0} vertices and {1} faces.", vertexCount, faceCount);
            }
            else return string.Empty;
        }

        private string FloatFormat(float f)
        {
            return String.Format("{0:000.00000}", f);
        }

        private void CreateDevice()
        {
            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferCount = 1;
            pp.IsFullScreen = false;
            pp.SwapEffect = SwapEffect.Discard;
            pp.BackBufferWidth = pnlVisual.Width;
            pp.BackBufferHeight = pnlVisual.Height;
            pp.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8;
            pp.EnableAutoDepthStencil = true;
            pp.PresentationInterval = PresentInterval.Default;
            pp.BackBufferFormat = SurfaceFormat.Unknown;
            pp.MultiSampleType = MultiSampleType.TwoSamples;

            Device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.Hardware, pnlVisual.Handle, pp);
            Device.RenderState.MultiSampleAntiAlias = true;

            be = new BasicEffect(Device, null);

            be.Alpha = 1.0f;
            be.DiffuseColor = new Vector3(0.75f, 0.75f, 0.75f);
            be.SpecularColor = new Vector3(0.75f, 0.75f, 0.75f);
            be.SpecularPower = 25.0f;
            be.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);
            be.LightingEnabled = true;
            be.EnableDefaultLighting();
            be.VertexColorEnabled = true;

            // todo: have directional lights relative to camera
            be.DirectionalLight0.Enabled = true;
            be.DirectionalLight0.DiffuseColor = Vector3.One;
            be.DirectionalLight0.SpecularColor = new Vector3(0.75f, 0.75f, 0.75f);


            // need to disable lighting to use colored vertices without normal values
            color = new BasicEffect(Device, null);
            color.Alpha = 1.0f;
            color.DiffuseColor = Vector3.One;
            color.AmbientLightColor = Vector3.One;
            color.VertexColorEnabled = true;


            UpdateCamera();
            RefreshViewport = true;
        }
        private void DestroyDevice()
        {
            if (Device != null) Device.Dispose();
            if (be != null) be.Dispose();
        }
        private void ResetDevice()
        {
            DestroyDevice();
            CreateDevice();
        }

        private void UpdateCamera()
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();
            GamePadState gamepad = GamePad.GetState(PlayerIndex.One);

            // apply movement
            if (keyboard.IsKeyDown(XInput.Keys.W))
            {
                Camera.ApplyForce(Camera.ForwardDirection);
            }
            if (keyboard.IsKeyDown(XInput.Keys.A))
            {
                Camera.ApplyForce(-Camera.HorizontalDirection);
            }
            if (keyboard.IsKeyDown(XInput.Keys.S))
            {
                Camera.ApplyForce(-Camera.ForwardDirection);
            }
            if (keyboard.IsKeyDown(XInput.Keys.D))
            {
                Camera.ApplyForce(Camera.HorizontalDirection);
            }
            if (keyboard.IsKeyDown(XInput.Keys.Q))
            {
                Camera.ApplyForce(new Vector3(0, -1, 0));
            }
            if (keyboard.IsKeyDown(XInput.Keys.E))
            {
                Camera.ApplyForce(new Vector3(0, 1, 0));
            }
            if (keyboard.IsKeyDown(XInput.Keys.Up))
            {
                Camera.ApplyLookForce(new Vector2(0, 0.1f));
            }
            if (keyboard.IsKeyDown(XInput.Keys.Left))
            {
                Camera.ApplyLookForce(new Vector2(-0.1f, 0));
            }
            if (keyboard.IsKeyDown(XInput.Keys.Down))
            {
                Camera.ApplyLookForce(new Vector2(0, -0.1f));
            }
            if (keyboard.IsKeyDown(XInput.Keys.Right))
            {
                Camera.ApplyLookForce(new Vector2(0.1f, 0));
            }

            //if (mouse.ScrollWheelValue < 0)
            //    Camera.ApplyZoomForce(0.02f);
            //else if (mouse.ScrollWheelValue > 0)
            //    Camera.ApplyZoomForce(-0.02f);

            //if (mouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            //{
            //    Camera.ApplyLookForce(new Vector2(-mouse.
            //}
            // if windowed, you must click the left mouse button to look around
            //if (DXUTIsWindowed())
            //{
            //    if (mouseState->rgbButtons[0] > 0)
            //        cam->ApplyLookForce(Vector2(-mouseState->lX * .01f, -mouseState->lY * .01f));
            //}
            //else
            //    cam->ApplyLookForce(Vector2(-mouseState->lX * .01f, -mouseState->lY * .01f));

            // do the calculations
            Camera.Update();

            // update views
            be.View = Camera.View;
            be.Projection = Camera.Projection;
            be.World = Camera.World;

            color.View = Camera.View;
            color.Projection = Camera.Projection;
            color.World = Camera.World;

            be.DirectionalLight0.Direction = Camera.ForwardDirection;
        }
        private void UpdateScene()
        {
            if (Camera.HasChanged || RefreshViewport)
            {
                Device.Clear(Microsoft.Xna.Framework.Graphics.Color.SteelBlue);

                DrawGrid(10, 1.0f);

                if (car != null)
                {
                    ChooseCullMode();
                    DrawSelectedPieces();
                    DrawNormals();
                    DrawSelectedPiecesWireframe();
                }

                Device.Present();
                RefreshViewport = false;
            }

            // todo: catch device lost exception and reset
        }

        private void DrawCarPiece(CarPiece piece)
        {
            Device.VertexDeclaration = new VertexDeclaration(Device, ForzaVertex.VertexElements);
            if (piece.Lod == 0)
            {
                Device.DrawUserIndexedPrimitives<ForzaVertex>(PrimitiveType.TriangleList, piece.Vertices, 0, piece.Vertices.Length, piece.IndexBuffer, 0, piece.FaceCount);
            }
            else
            {
                Device.DrawUserIndexedPrimitives<ForzaVertex>(PrimitiveType.TriangleList, piece.Vertices, 0, piece.Vertices.Length, piece.IndexBuffer, 0, piece.FaceCount);
            }            
        }

        private void DrawSelectedPieces()
        {
            ChooseFillMode();

            // must edit these before calling begin on basic effect
            be.VertexColorEnabled = true;
            be.DiffuseColor = new Vector3(0.7f, 0.7f, 0.7f);

            be.Begin();
            foreach (EffectPass pass in be.CurrentTechnique.Passes)
            {
                pass.Begin();
                foreach (TreeNode n in tvCarStructure.Nodes)
                {
                    foreach (TreeNode nodeSection in n.Nodes)
                    {
                        foreach (TreeNode nodePiece in nodeSection.Nodes)
                        {
                            if (nodePiece.Checked)
                            {
                                string[] str = nodePiece.Name.Split('_');
                                int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                                int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));
                                DrawCarPiece(car.Sections[sectionIndex].Pieces[pieceIndex]);
                            }
                        }
                    }
                }
                pass.End();
            }
            be.End();
        }


        private void DrawNormals()
        {
            if (chkNormals.Checked)
            {
                Device.RenderState.FillMode = FillMode.Solid;

                color.Begin();
                foreach (EffectPass pass in color.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    foreach (TreeNode n in tvCarStructure.Nodes)
                    {
                        foreach (TreeNode nodeSection in n.Nodes)
                        {
                            foreach (TreeNode nodePiece in nodeSection.Nodes)
                            {
                                if (nodePiece.Checked)
                                {
                                    string[] str = nodePiece.Name.Split('_');
                                    int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                                    int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));

                                    CarPiece piece = car.Sections[sectionIndex].Pieces[pieceIndex];

                                    // draw normals
                                    VertexPositionColor[] normals = new VertexPositionColor[piece.VertexCount * 2];
                                    for (int i = 0; i < piece.VertexCount - 1; i++)
                                    {
                                        float x = piece.Vertices[i].position.X;
                                        float y = piece.Vertices[i].position.Y;
                                        float z = piece.Vertices[i].position.Z;
                                        Vector3 normal = piece.Vertices[i].normal;

                                        normals[i * 2] = new VertexPositionColor(new Vector3(x, y, z), Microsoft.Xna.Framework.Graphics.Color.Yellow);
                                        normals[i * 2 + 1] = new VertexPositionColor(new Vector3(x, y, z) + normal * 0.01f, Microsoft.Xna.Framework.Graphics.Color.Yellow);
                                    }
                                    Device.VertexDeclaration = new VertexDeclaration(Device, VertexPositionColor.VertexElements);
                                    Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, normals, 0, piece.VertexCount);
                                    be.LightingEnabled = true;
                                }
                            }
                        }
                    }
                    pass.End();
                }
                color.End();
            }
        }

        private void DrawSelectedPiecesWireframe()
        {
            if (chkWireframe.Checked)
            {
                Device.RenderState.FillMode = FillMode.WireFrame;
                // must edit these before calling begin on basic effect
                be.VertexColorEnabled = false;
                be.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);

                be.Begin();
                foreach (EffectPass pass in be.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    foreach (TreeNode n in tvCarStructure.Nodes)
                    {
                        foreach (TreeNode nodeSection in n.Nodes)
                        {
                            foreach (TreeNode nodePiece in nodeSection.Nodes)
                            {
                                if (nodePiece.Checked)
                                {
                                    string[] str = nodePiece.Name.Split('_');
                                    int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                                    int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));
                                    DrawCarPiece(car.Sections[sectionIndex].Pieces[pieceIndex]);
                                }
                            }
                        }
                    }
                    pass.End();
                }
                be.End();
            }
        }

        private void DrawGrid(int size, float spacing)
        {
            if (chkGrid.Checked)
            {
                color.Begin();
                foreach (EffectPass pass in color.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    float lineSize = size * spacing;
                    float posHalfSize = lineSize / 2;
                    float negHalfSize = -lineSize / 2;
                    int lineCount = (size + 1) * 4;

                    // create vertex buffer
                    VertexPositionColor[] vertices = new VertexPositionColor[lineCount + 2];
                    for (int i = 0; i < lineCount; i += 4)
                    {
                        float index = negHalfSize + (i / 4) * spacing;

                        // x axis
                        vertices[i] = new VertexPositionColor(new Vector3(negHalfSize, 0, index), Microsoft.Xna.Framework.Graphics.Color.DarkGreen);
                        vertices[i + 1] = new VertexPositionColor(new Vector3(posHalfSize, 0, index), Microsoft.Xna.Framework.Graphics.Color.White);

                        // y axis
                        vertices[i + 2] = new VertexPositionColor(new Vector3(index, 0, negHalfSize), Microsoft.Xna.Framework.Graphics.Color.DarkBlue);
                        vertices[i + 3] = new VertexPositionColor(new Vector3(index, 0, posHalfSize), Microsoft.Xna.Framework.Graphics.Color.White);
                    }

                    // create x axis
                    //vertices[lineCount] = new VertexPositionColor(new Vector3(negHalfSize, 0, 0), Microsoft.Xna.Framework.Graphics.Color.Green);
                    //vertices[lineCount + 1] = new VertexPositionColor(new Vector3(posHalfSize, 0, 0), Microsoft.Xna.Framework.Graphics.Color.Green);

                    // create y axis
                    vertices[lineCount] = new VertexPositionColor(new Vector3(0, negHalfSize, 0), Microsoft.Xna.Framework.Graphics.Color.DarkRed);
                    vertices[lineCount + 1] = new VertexPositionColor(new Vector3(0, posHalfSize, 0), Microsoft.Xna.Framework.Graphics.Color.White);

                    // create z axis
                    //vertices[lineCount + 4] = new VertexPositionColor(new Vector3(0, 0, negHalfSize), Microsoft.Xna.Framework.Graphics.Color.Blue);
                    //vertices[lineCount + 5] = new VertexPositionColor(new Vector3(0, 0, posHalfSize), Microsoft.Xna.Framework.Graphics.Color.Blue);

                    // render grid
                    Device.VertexDeclaration = new VertexDeclaration(Device, VertexPositionColor.VertexElements);
                    Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, vertices.Length / 2);

                    pass.End();
                }
                color.End();
            }
        }

        private void CheckChildNode(TreeNode node)
        {
            bool checkStatus = node.Checked;

            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    // update piece checked status
                    if (n.Name.Contains("_"))
                    {
                        string[] str = n.Name.Split('_');
                        int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                        int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));
                        car.Sections[sectionIndex].Pieces[pieceIndex].Visible = checkStatus;
                    }

                    n.Checked = checkStatus;
                    CheckChildNode(n);
                }
            }
            else
            {
                // update individual piece check status
                if (node.Name.Contains("_"))
                {
                    string[] str = node.Name.Split('_');
                    int sectionIndex = Convert.ToInt32(str[0].Replace("Section", ""));
                    int pieceIndex = Convert.ToInt32(str[1].Replace("Piece", ""));
                    car.Sections[sectionIndex].Pieces[pieceIndex].Visible = checkStatus;
                }
            }
        }

        private void CheckParentNode(TreeNode node)
        {
            TreeNode parent = node.Parent;
            if (parent != null)
            {
                parent.Checked = true;
                foreach (TreeNode n in parent.Nodes)
                {
                    if (!node.Checked)
                    {
                        parent.Checked = false;
                    }
                }
                CheckParentNode(parent);
            }
        }

        //private bool updatingChildNodes = false;
        //private void UpdateChildNodes(TreeNode node, bool checkStatus)
        //{
        //    if (node != null && node.Nodes != null && node.Nodes.Count > 0)
        //    {
        //        foreach (TreeNode n in node.Nodes)
        //        {
        //            updatingChildNodes = true;
        //            n.Checked = checkStatus;
        //            updatingChildNodes = false;
        //            UpdateChildNodes(n, checkStatus);
        //        }
        //    }

        //}



        private void ChooseFillMode()
        {
            if ((string)cmbFillMode.SelectedItem == "Solid")
            {
                Device.RenderState.FillMode = FillMode.Solid;
            }
            else if ((string)cmbFillMode.SelectedItem == "Wireframe")
            {
                Device.RenderState.FillMode = FillMode.WireFrame;
            }
            else if ((string)cmbFillMode.SelectedItem == "Point")
            {
                Device.RenderState.FillMode = FillMode.Point;
            }
        }

        private void ChooseCullMode()
        {
            if ((string)cmbCullMode.SelectedItem == "None")
            {
                Device.RenderState.CullMode = CullMode.None;
            }
            else if ((string)cmbCullMode.SelectedItem == "Clockwise")
            {
                Device.RenderState.CullMode = CullMode.CullClockwiseFace;
            }
            else if ((string)cmbCullMode.SelectedItem == "Counterclockwise")
            {
                Device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            }
        }

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            return true;// return base.IsInputKey(keyData);
        }
        protected override bool IsInputChar(char charCode)
        {
            return true;// base.IsInputChar(charCode);
        }





        private string GetSectionDescription(string name)
        {
            string description = GetSectionName(name, "bumperF", "Front Bumper");
            if (description != null) return description;
            description = GetSectionName(name, "bumperR", "Rear Bumper");
            if (description != null) return description;
            description = GetSectionName(name, "bumperFrameF", "Front Bumper Frame");
            if (description != null) return description;
            description = GetSectionName(name, "bumperFrameR", "Rear Bumper Frame");
            if (description != null) return description;
            description = GetSectionName(name, "wing", "Rear Spoiler");
            if (description != null) return description;
            description = GetSectionName(name, "skirtL", "Left Side Skirt");
            if (description != null) return description;
            description = GetSectionName(name, "skirtR", "Right Side Skirt");
            if (description != null) return description;
            description = GetSectionName(name, "hood", "Hood");
            if (description != null) return description;
            description = GetSectionName(name, "glassLTL", "Left Taillight Glass");
            if (description != null) return description;
            description = GetSectionName(name, "glassRTL", "Right Taillight Glass");
            if (description != null) return description;
            description = GetSectionName(name, "taillightL", "Left Taillight");
            if (description != null) return description;
            description = GetSectionName(name, "taillightR", "Right Taillight");
            if (description != null) return description;
            description = GetSectionName(name, "exhaust", "Exhaust");
            if (description != null) return description;
            description = GetSectionName(name, "exhaustR", "Right Exhaust");
            if (description != null) return description;
            description = GetSectionName(name, "exhaustL", "Left Exhaust");
            if (description != null) return description;
            description = GetSectionName(name, "glassR", "Rear Window");
            if (description != null) return description;
            description = GetSectionName(name, "trunk", "Trunk");
            if (description != null) return description;
            description = GetSectionName(name, "glassLR", "Left Rear Window");
            if (description != null) return description;
            description = GetSectionName(name, "glassRR", "Right Rear Window");
            if (description != null) return description;
            description = GetSectionName(name, "wheel", "Wheel");
            if (description != null) return description;
            description = GetSectionName(name, "headlightL", "Left Headlight");
            if (description != null) return description;
            description = GetSectionName(name, "headlightR", "Right Headlight");
            if (description != null) return description;
            description = GetSectionName(name, "glassLHL", "Left Headlight Glass");
            if (description != null) return description;
            description = GetSectionName(name, "glassRHL", "Right Headlight Glass");
            if (description != null) return description;
            description = GetSectionName(name, "body", "Body");
            if (description != null) return description;
            description = GetSectionName(name, "undercarriage", "Undercarriage");
            if (description != null) return description;
            description = GetSectionName(name, "glassLF", "Left Front Window");
            if (description != null) return description;
            description = GetSectionName(name, "glassRF", "Right Front Window");
            if (description != null) return description;
            description = GetSectionName(name, "mirrorL", "Left Mirror");
            if (description != null) return description;
            description = GetSectionName(name, "mirrorR", "Right Mirror");
            if (description != null) return description;
            description = GetSectionName(name, "cagerace", "Roll Cage");
            if (description != null) return description;
            description = GetSectionName(name, "interior", "Interior");
            if (description != null) return description;
            description = GetSectionName(name, "glassF", "Windshield");
            if (description != null) return description;
            description = GetSectionName(name, "a_pillar", "Body"); // partial car body around cockpit
            if (description != null) return description;
            description = GetSectionName(name, "steering_wheel", "Steering Wheel");
            if (description != null) return description;
            description = GetSectionName(name, "seatR", "Passenger Seat");
            if (description != null) return description;
            description = GetSectionName(name, "seatL", "Driver Seat");
            if (description != null) return description;

            // todo: more...

            return "Unknown";
        }

        private string GetSectionName(string name, string code, string description)
        {
            if (name.Contains(code))
            {
                string version = name.Replace(code, "");
                if (version.Length == 0) return description;
                else if (version.Length == 1 && version[0] >= 'a' && version[0] <= 'z') return description + " " + (version[0] - 'a' + 1);
                else if (version == "race") return description + " Race";
                else return null;
            }
            else return null;
        }




        #endregion
 
    }
}
