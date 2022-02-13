// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Windows.Threading;
using System.Windows.Media;
using Transformations;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_airplane;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        private float m_zRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 5.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private float m_eyeX = 0.0f;
        private float m_eyeY = 0.0f;
        private float m_eyeZ = 5.0f;

        private float m_centerX = 0.0f;
        private float m_centerY = 0.0f;
        private float m_centerZ = 0.0f;

        private float m_upX = 0.0f;
        private float m_upY = 1.0f;
        private float m_upZ = 0.0f;

        private float m_airplaneX;
        private float m_airplaneY;
        private float m_airplaneZ;

        private float pivot = 1.0f;

        private bool m_animationInProgress = false;

        private float m_scaleAirplane = 0.001f;
        private double m_runwayLength = -20.0;
        private double m_airplaneSpeed;

        private float[] light0pos;
        private float[] light1pos;

        private float[] m_baseNormals;
        private float[] m_baseVertices;

        ///	 Identifikatori tekstura za jednostavniji pristup teksturama
        private enum TextureObjects { Grass = 0, Asphalt };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        ///	 Identifikatori OpenGL tekstura
        private uint[] m_textures = null;

        ///	 Putanje do slika koje se koriste za teksture
        private string[] m_textureFiles = { "..//..//Textures//grass.jpg", "..//..//Textures//asphalt.jpg" };

        public DispatcherTimer timer1;
        public DispatcherTimer timer2;
        public DispatcherTimer timer3;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_airplane; }
            set { m_airplane = value; }
        }

        public bool AnimationInProgress
        {
            get { return m_animationInProgress;  }
            set { m_animationInProgress = value; }
        }

        public float AirplaneScaleFactor
        {
            get { return m_scaleAirplane;  }
            set { m_scaleAirplane = value; }
        }

        public double RunwayLength
        {
            get { return m_runwayLength;  }
            set { m_runwayLength = value; }
        }

        public double AirplaneSpeed
        {
            get { return m_airplaneSpeed;  }
            set { m_airplaneSpeed = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        public float RotationZ 
        {
            get { return m_zRotation;  } 
            set { m_zRotation = value; } 
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_airplane = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            this.m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.68f, 0.85f, 0.9f, 1.0f);
            m_airplane.LoadScene();
            m_airplane.Initialize();
        
            gl.Enable(OpenGL.GL_COLOR_MATERIAL); //Color tracking mehanizam
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE); // ambijentalna i difuzna komponenta materijala
            SetUpLighting(gl);

            SetupTextures(gl);

            timer1 = new DispatcherTimer();
            timer2 = new DispatcherTimer();
            timer3 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(m_airplaneSpeed);
            timer1.Tick += new EventHandler(StartAnimation);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, m_width, m_height);
            gl.FrontFace(OpenGL.GL_CCW);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            gl.Perspective(45f, (double)m_width / m_height, 0.5f, 50f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            // pozicioniranje svjetlosnog izvora koji je neosjetljiv na transformacije
            light0pos = new float[] { 0.0f, 500.0f, -250.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);

            gl.PushMatrix();

            //Definisanje kamere
            //gl.LookAt(m_eyeX, m_eyeY, m_eyeZ, m_centerX, m_centerY, m_centerZ, m_upX, m_upY, m_upZ);

            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            // Rotacija scene
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
         
            DrawBase(gl);
            DrawRunway(gl);
            DrawLightSigns(gl);
            DrawAirplane(gl);
            DrawText(gl);

            gl.PopMatrix();
            gl.Flush();
        }

        private void SetUpLighting(OpenGL gl)
        {
            //Definisanje tackastog izvora svjetlosti, bijele boje
            light0pos = new float[] { 0.0f, 500.0f, -250.0f, 1.0f };           
            float[] light0ambient = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };     
            float[] light0diffuse = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] light0specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);            
             
            //Definisanje reflektorskog izvora svjetlosti plave boje
            light1pos = new float[] { 10.0f, 0.0f, 5.0f, 1.0f};                   
            float[] light1ambient = new float[] { 0.0f, 0.5f, 1.0f, 1.0f };       
            float[] light1diffuse = new float[] { 0.0f, 0.0f, 1.0f, 1.0f };
            float[] light1specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 30.0f);       

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);
            m_baseVertices = new float[] { -10.0f, -1.0f, 2.0f, 10.0f, -1.0f, 2.0f, 10.0f, -1.0f, (float)m_runwayLength, -10.0f, -1.0f, (float)m_runwayLength };
            m_baseNormals = LightingUtilities.ComputeVertexNormals(m_baseVertices);
            gl.Enable(OpenGL.GL_NORMALIZE); //automatsko generisanje normala
        }

        private void SetupTextures(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
   
            // Ucitaj slike i kreiraj teksture
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                if (i == 1)
                {
                    gl.MatrixMode(OpenGL.GL_TEXTURE);
                    gl.Scale(1.0f, 0.5f, 0.5f);
                }
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);		// Nearest Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);		// Nearest Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_REPLACE); // nacin stapanja teksture sa materijalom

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        public void StartAnimation(object sender, EventArgs e)
        {
            m_yRotation += 10;
            while(m_yRotation < 60)
                m_yRotation += 10;

            timer1.Stop();
            timer2.Interval = TimeSpan.FromMilliseconds(m_airplaneSpeed);
            timer2.Tick += new EventHandler(TranslateAirplane);
            //timer2.Start();
        }

        public void TranslateAirplane(object sender, EventArgs e)
        {
            timer2.Start();

            while(m_airplaneZ > -20.0f)
                m_airplaneZ -= 1;

            timer2.Stop();
            StopAnimation();
            AnimationInProgress = false;
        }

        private void StopAnimation()
        {
            m_yRotation = 0.0f;
            m_airplaneZ = 3.5f;
        }

        private void DrawBase(OpenGL gl)
        {
           // gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            //gl.Scale(0.8f, 0.8f, 0.8f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            gl.Translate(0.0f, -1.0f, 0.0f);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, m_baseNormals);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.2f, .8f, 0.2f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-10.0, -1.0, 2.0);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(10.0, -1.0, 2.0);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(10.0, -1.0, m_runwayLength);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-10.0, -1.0, m_runwayLength);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, IntPtr.Zero);
            gl.End();
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void DrawRunway(OpenGL gl)
        {
            //gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            //gl.Scale(0.8f, 0.8f, 0.8f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Asphalt]);
            gl.Translate(0.0f, -0.9f, 0.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.7, 0.7, 0.7);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-2.0f, -1.0f, 2.0f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(2.0f, -1.0f, 2.0f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(2.0f, -1.0f, m_runwayLength);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-2.0f, -1.0f, m_runwayLength);
            gl.End();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            //Iscrtavanje linija po pisti
            float temp = 0.6f;
            for (int i = 0; i < Math.Round((-m_runwayLength) /2); i++)
            {
                gl.PushMatrix();
                gl.Translate(0.0f, 0.1f, -0.5f);
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(1.0, 1.0, 1.0);
                gl.Vertex(-0.06f, -1.0f, temp);
                gl.Vertex(0.06f, -1.0f, temp);
                gl.Vertex(0.06f, -1.0f, temp - 1.0f);
                gl.Vertex(-0.06f, -1.0f, temp - 1.0f);

                temp -= 2.0f;
                gl.End();
                gl.PopMatrix();
            }
            gl.PopMatrix();
        }

        private void DrawLightSigns(OpenGL gl)
        {
            //gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            //gl.Scale(0.8f, 0.8f, 0.8f);
            gl.Color(1.0f, 1.0f, 1.0f);

            gl.Translate(2.0f, -1.7f, -0.5f);
            Sphere sphere = new Sphere();
            sphere.Radius = 0.1f;
            sphere.QuadricDrawStyle = DrawStyle.Fill;
            sphere.CreateInContext(gl);
            // Emisiona komponenta zute boje
            sphere.Material = new SharpGL.SceneGraph.Assets.Material();
            sphere.Material.Emission = System.Drawing.Color.Yellow;
            sphere.Material.Bind(gl);
            sphere.Render(gl, RenderMode.Render);

            gl.Translate(-4.0f, 0.0f, 0.0f);
            sphere.Render(gl, RenderMode.Render);

            for (int i = 0; i < Math.Round((-m_runwayLength) / 5); i++)
            {
                gl.Translate(0.0f, 0.0f, -4.0f);
                sphere.Render(gl, RenderMode.Render);

                gl.Translate(4.0f * pivot, 0.0f, 0.0f);
                sphere.Render(gl, RenderMode.Render);

                pivot = -1.0f * pivot;
            }
            sphere.Material.Emission = System.Drawing.Color.Black;
            sphere.Material.Bind(gl);
            //sphere.Material = null;
            gl.PopMatrix();
            //sphere.Material = new SharpGL.SceneGraph.Assets.Material();
            //sphere.Material = null;
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void DrawAirplane(OpenGL gl)
        {
            //gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            //gl.Scale(0.8f, 0.8f, 0.8f);
            gl.Scale(0.8f, 0.8f, 0.8f);
            gl.Translate(4.0f, 0.0f, 3.5f);
            gl.Rotate(90.0f, 0.0f, 0.0f, 1.0f);
            gl.Rotate(90.0f, 0.0f, 1.0f, 0.0f);
            gl.Translate(4.0f, 4.0f, -1.0f);
            gl.Scale(m_scaleAirplane, m_scaleAirplane, m_scaleAirplane);
            m_airplane.Draw();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            light1pos = new float[] { 5.0f, 0.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, light1pos);
            gl.PopMatrix();

            gl.PopMatrix();
        }

        private void DrawText(OpenGL gl)
        {
            gl.PushMatrix();
            gl.FrontFace(OpenGL.GL_CW);
            gl.Viewport(2 * m_width / 3, 0, m_width / 3, m_height / 3);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-10.0f, 10.0f, -10.0f, 10.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.Translate(-3.0f, -4.0f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "Predmet: Racunarska grafika");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -4.1f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "_______________________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -5.0f, 0.0f);
            gl.DrawText3D("Tahoma", 10f, 1f, 0.1f, "Sk.god: 2021/22.");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -5.2f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "_____________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -6.0f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1f, 0.1f, "Ime: Marija");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -6.1f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "_________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -7.0f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1f, 0.1f, "Prezime: Kljestan");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -7.1f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "______________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -8.0f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "Sifra zad: 13.1");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-3.0f, -8.2f, 0.0f);
            gl.DrawText3D("Tahoma", 10.0f, 1.0f, 0.1f, "____________");
            gl.PopMatrix();

            gl.PopMatrix();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;

            // Ukljuci testiranje dubine, sakrivanje nevidljivih povrsina i orijentaciju poligona lica
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_CCW);

            // Viewport preko cijelog prozora
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);     
            gl.LoadIdentity();

            // Definisanje projekcije u perspektivi
            gl.Perspective(45f, (double)width / height, 0.5f, 50f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                      
        }


        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_airplane.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
