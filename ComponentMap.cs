using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SurvivalCraftMap
{

    public class ComponentMap : Component, IDrawable, IUpdateable
    {
        /// <summary>
        /// SCMap.[Version].@[Num]
        /// @A=alpha
        /// @B=Beta
        /// @C=Release
        /// @D=Debug
        /// Suceru {*_*}
        /// fidax@qq.com
        /// </summary>
        string Version = "SCMap.C.C1";
        int VersionNum = 84550;
        // 地图方块数半径
        private int mapRadius;
        //地图缩放
        private float MapScale;
        //地图边界半径
        private float RmapRadius;
        //切换地图按钮
        private ButtonWidget mapButton;
        //juesezuobiao
        private Point2 Point;
        //juesezaiUIshang
        private Vector2 Center;
        private Vector2 LookVector;
        private Texture2D MapTexture;
        public Camera camera1;
        //地形系统
        public ComponentPlayer m_componentPlayer;
        public SubsystemTerrain subsystemTerrain;
        public PrimitivesRenderer2D primitivesRenderer2DUp1 = new PrimitivesRenderer2D();
        public PrimitivesRenderer2D primitivesRenderer2DUp2 = new PrimitivesRenderer2D();
        public PrimitivesRenderer2D primitivesRenderer2DDown1 = new PrimitivesRenderer2D();
        public PrimitivesRenderer2D primitivesRenderer2DDown2 = new PrimitivesRenderer2D();
        public PrimitivesRenderer2D primitivesRenderer2D3 = new PrimitivesRenderer2D();

        public static int[] m_drawOrders = new int[] { /*1101*/1246 };
        /*
        FlatBatch2D FlatBatch2DSurface1;
        FontBatch2D FontBatch2DSurface1;
        FlatBatch2D FlatBatch2DSurface2;
        FontBatch2D FontBatch2DSurface2;*/
        TexturedBatch2D DrawTexturedBatch2DUp;
        TexturedBatch2D DrawTexturedBatch2DDown;
        TexturedBatch2D DoubleBufferedUp1;
        TexturedBatch2D DoubleBufferedUp2;
        TexturedBatch2D DoubleBufferedDown1;
        TexturedBatch2D DoubleBufferedDown2;
        bool DoubleBuffered = true;
        bool jisuan = false;
        public bool MapType = false;
        public UpdateOrder UpdateOrder => UpdateOrder.Default;
        public void HandleInput()
        {
            switch (m_componentPlayer.GameWidget.Input.IsKeyDownOnce(Engine.Input.Key.M) || this.mapButton.IsClicked)
            {
                case true:
                    Log.Information("Mini-MapMod Version:"+VersionNum);
                    MapType = !MapType;
                    return;
            }
        }
        public void HandleRoundMap(TexturedBatch2D texturedBatch2DUp, TexturedBatch2D texturedBatch2DDown)
        {
            camera1 = m_componentPlayer.GameWidget.ActiveCamera;
            Point = Terrain.ToCell(this.m_componentPlayer.ComponentBody.Position.XZ);
            //界面尺寸
            Vector2 ScreenSize = camera1.ViewportSize;
            //小地图的中心
            Center = new Vector2(ScreenSize.X * (1f - this.RmapRadius / 2), ScreenSize.X * this.RmapRadius / 2);
            texturedBatch2DUp.Clear();
            texturedBatch2DDown.Clear();
                for (int Y = -mapRadius; Y <= mapRadius; Y++)
                {
                    for (int X = (int)-MathUtils.Round(MathUtils.Sqrt(MathUtils.Sqr(mapRadius) - MathUtils.Sqr(Y))); X <= (int)MathUtils.Round(MathUtils.Sqrt(MathUtils.Sqr(mapRadius) - MathUtils.Sqr(Y))); X++)
                    {
                        Tool.CalculateSlotTexCoordTables();
                        Vector4 slotTexCoord = Tool.SlotTexCoords[BlocksManager.Blocks[GetTopContent(Point.X + X, Point.Y + Y)].DefaultTextureSlot];
                        Vector2 corner1_2 = new Vector2(ScreenSize.X / 2 + X, ScreenSize.Y / 2 + Y);
                        Vector2 corner2_2 = new Vector2(ScreenSize.X / 2 + X + 1, ScreenSize.Y / 2 + Y + 1);
                        Color color;
                        switch (BlocksManager.Blocks[GetTopContent(Point.X + X, Point.Y + Y)])
                        {
                            case CrossBlock _:
                                color = Color.MintGreen;
                                break;
                            case WoodBlock _:
                                color = Color.White;
                                break;
                            case GrassBlock _:
                            case LeavesBlock _:
                            case IvyBlock _:
                                color = Color.Green;
                                break;
                            case BottomSuckerBlock _:
                                color = Color.DarkBlue;
                                break;
                            case FluidBlock _:
                                color = Color.Blue;
                                break;
                            default:
                                color = Color.White;
                                break;
                        }
                        switch (Y < 0)
                        {
                            case true:
                                texturedBatch2DUp.QueueQuad(corner1_2, corner2_2, 0.0f, new Vector2(slotTexCoord.X, slotTexCoord.W), new Vector2(slotTexCoord.Z, slotTexCoord.Y), color);

                                break;
                            default:
                                texturedBatch2DDown.QueueQuad(corner1_2, corner2_2, 0.0f, new Vector2(slotTexCoord.X, slotTexCoord.W), new Vector2(slotTexCoord.Z, slotTexCoord.Y), color);

                                break;
                        }

                    }

                }
            Matrix matrix = Matrix.CreateTranslation(-(ScreenSize.X / 2), -(ScreenSize.Y / 2), 0) * Matrix.CreateRotationZ(-Vector2.Angle(-Vector2.UnitY, m_componentPlayer.ComponentBody.Matrix.Forward.XZ)) * Matrix.CreateScale(MapScale) * Matrix.CreateTranslation(Center.X, Center.Y, 0);
            texturedBatch2DUp.TransformTriangles(matrix);
            texturedBatch2DDown.TransformTriangles(matrix);
            DrawTexturedBatch2DUp = texturedBatch2DUp;
            DrawTexturedBatch2DDown = texturedBatch2DDown;
            DoubleBuffered = !DoubleBuffered;
        }
        public void Update(float dt)
        {
            HandleInput();
            switch (MapType)
            {
                case true:
                    return;
                default:
                    if (jisuan == false)
                    {
                        if (DoubleBuffered)
                        {
                            jisuan = true;
                            Task.Run(() =>
                            {
                                try
                                {
                                    HandleRoundMap(DoubleBufferedUp1, DoubleBufferedDown1);
                                }
                                catch (Exception e)
                                {

                                    Log.Warning("Waring:"+e);
                                }
                                    
                            }).ContinueWith(t =>
                            {
                                jisuan = false;


                            });
                        }
                        else
                        {
                            jisuan = true;
                            Task.Run(() =>
                            {
                                try
                                {
                                    HandleRoundMap(DoubleBufferedUp2, DoubleBufferedDown2);
                                }
                                catch (Exception e)
                                {
                                    Log.Warning("Waring:" + e);
                                }
                                   
                            }).ContinueWith(t =>
                            {
                                jisuan = false;
                            });

                        }


                    }
                    return;
            }
        }
        public void Draw(Camera camera, int drawOrder)
        {/*
            if (camera1 == null) camera1 = camera;*/
            switch (MapType)
            {
                case true:
                    return;
                default:
                    DrawMap(DrawTexturedBatch2DUp, DrawTexturedBatch2DDown, Center);
                    return;
            }

        }

        public void DrawMap(TexturedBatch2D DrawUp, TexturedBatch2D DrawDown, Vector2 center)
        {
            Matrix matrix = Matrix.CreateTranslation(-(Center.X), -(Center.Y), 0) * Matrix.CreateRotationZ(-Vector2.Angle(LookVector, m_componentPlayer.ComponentBody.Matrix.Forward.XZ)) * Matrix.CreateTranslation(Center.X, Center.Y, 0);
            DrawUp.TransformTriangles(matrix);
            DrawDown.TransformTriangles(matrix);
            TexturedBatch2D texturedBatch2D2 = this.primitivesRenderer2D3.TexturedBatch((Texture2D)ContentManager.Get<Texture2D>("Textures/Gui/SoftwareMouseCursor"), depthStencilState: DepthStencilState.None, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointWrap);
            Matrix matrix1 = Matrix.CreateTranslation(-center.X, -center.Y, 0) * Matrix.CreateRotationZ(0.5633f) * Matrix.CreateTranslation(center.X, center.Y, 0);

            //四边形队列
            texturedBatch2D2.QueueQuad(new Vector2(center.X - 4, center.Y - 4), new Vector2(center.X + 4, center.Y + 4), 1f, Vector2.Zero, Vector2.One, Color.LightYellow);
            texturedBatch2D2.TransformTriangles(matrix1, texturedBatch2D2.TriangleVertices.Count);
            DrawUp.Flush(false);
            DrawDown.Flush(false);

            texturedBatch2D2.Flush();
            LookVector = m_componentPlayer.ComponentBody.Matrix.Forward.XZ;
        }
        public override void Load(TemplatesDatabase.ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            
            this.m_componentPlayer = (ComponentPlayer)this.Entity.FindComponent<ComponentPlayer>(true);
            this.MapTexture = (Texture2D)ContentManager.Get<Texture2D>("Textures/Blocks");
            camera1 = m_componentPlayer.GameWidget.ActiveCamera;
            StackPanelWidget stackPanelWidget = this.m_componentPlayer.GameWidget.Children.Find<StackPanelWidget>("MoreContents", true);
            this.mapButton = new BevelledButtonWidget
            {

                Name = "MapButton",
                Text = "M",
                Size = new Vector2(68f, 64f),
                Margin = new Vector2(4f, 0f),
                CenterColor = new Color(127, 127, 127, 180)
            };
            LookVector = m_componentPlayer.ComponentBody.Matrix.Forward.XZ;
            foreach (var item in stackPanelWidget.Children.m_widgets)
            {
                switch (item.Name == mapButton.Name)
                {
                    case true:
                        stackPanelWidget.Children.Remove(item);
                        break;
                }
            }
            stackPanelWidget.Children.Add(this.mapButton);
            subsystemTerrain = this.Project.FindSubsystem<SubsystemTerrain>(true);
            DoubleBufferedUp1 = primitivesRenderer2DUp1.TexturedBatch(this.MapTexture, depthStencilState: DepthStencilState.None, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);
            DoubleBufferedUp2 = primitivesRenderer2DUp2.TexturedBatch(this.MapTexture, depthStencilState: DepthStencilState.None, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);
            DoubleBufferedDown1 = primitivesRenderer2DDown1.TexturedBatch(this.MapTexture, depthStencilState: DepthStencilState.None, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);
            DoubleBufferedDown2 = primitivesRenderer2DDown2.TexturedBatch(this.MapTexture, depthStencilState: DepthStencilState.None, blendState: BlendState.Opaque, samplerState: SamplerState.LinearClamp);


            switch (Environment.OSVersion.Platform.ToString())
            {
                case "Windows":
                case "windows":
                    mapRadius = 100;
                    MapScale = 0.5f;
                    RmapRadius = 0.30f;
                    break;
                case "Android":
                case "android":
                case "Unix":
                case "unix":
                    mapRadius = 100;
                    MapScale = 1.5f;
                    RmapRadius = 0.30f;
                    break;
                default:
                    mapRadius = 100;
                    MapScale = 1.5f;
                    RmapRadius = 0.30f;
                    break;
            }
        }
        public int GetTopContent(int pointX, int pointY)
        {
            return subsystemTerrain.Terrain.GetCellContents(pointX, subsystemTerrain.Terrain.GetTopHeight(pointX, pointY), pointY);
        }

        public ComponentMap() : base()
        {
        }
        public int[] DrawOrders
        {
            get
            {
                return m_drawOrders;
            }
        }
    }
}