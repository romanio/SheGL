using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace SheGL
{
    public partial class Form1 : Form
    {
        GLControl glControl = null;

        string VertexShader =
            "#version 330 core\n" +
            "\n" +
            "layout (location=0) in vec3 position;\n" +
            "layout (location=1) in float  U;\n" +
            "out float Ux;\n" +
            "uniform mat4 mvp;\n"+
            "void main(void)\n" +
            "{\n" +
            "gl_Position = mvp * vec4(position, 1.0f);\n" +
            "Ux = U;\n"+
            "}\n";

        string FragmentShader =
            "#version 330 core\n" +
            "in float Ux;\n" +
            "out vec4 color;\n" +
            "uniform sampler1D tSampler;\n" +
            "\n" +
            "void main(void)\n" +
            "{\n" +
            "   color = texture(tSampler, Ux).rgba;\n" +
            "}\n";

        string GeometryShader =
            "#version 330 core\n" +
            "layout (triangles) in;\n"+
            "layout (triangle_strip, max_vertices = 3) out;\n"+
            "void main(void)\n" +
            "{\n" +
            "gl_Position = gl_in[0].gl_Position;\n" +
            "EmitVertex();\n"+
            "gl_Position = gl_in[1].gl_Position;\n" +
            "EmitVertex();\n" +
            "gl_Position = gl_in[2].gl_Position;\n" +
            "EmitVertex();\n" +
            "EndPrimitive();\n"+
            "}\n";

        public Form1()
        {
            InitializeComponent();

            glControl = new GLControl(GraphicsMode.Default, 3, 3, GraphicsContextFlags.Default);
            glControl.Paint += GlControl_Paint;
            glControl.Load += GlControl_Load;
            glControl.Resize += GlControl_Resize;
            glControl.Dock = DockStyle.Fill;
            plOpenGL.Controls.Add(glControl);
            
        }
        private int CompileShaders()
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShader);
            GL.CompileShader(vertexShader);
            var info = GL.GetShaderInfoLog(vertexShader);

            System.Diagnostics.Debug.WriteLine("Vertex Shader : " + info);

            //
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, FragmentShader);
            GL.CompileShader(fragmentShader);
            info = GL.GetShaderInfoLog(fragmentShader);
            System.Diagnostics.Debug.WriteLine("Fragment Shader : " + info);
            //
            /*
            var geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometryShader, GeometryShader);
            GL.CompileShader(geometryShader);
            info = GL.GetShaderInfoLog(geometryShader);
            System.Diagnostics.Debug.WriteLine("Geometry Shader : " + info);
    */
    //
            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
     //       GL.AttachShader(program, geometryShader);
            GL.LinkProgram(program);

            info = GL.GetProgramInfoLog(program);
            System.Diagnostics.Debug.WriteLine("Program Shader : " + info);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
//GL.DetachShader(program, geometryShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
           // GL.DeleteShader(geometryShader);

            return program;
        }

        Matrix4 projection;
        Matrix4 modelview;
        Matrix4 mvp;

        private void GlControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(glControl.Size);

            float aspect = (float)glControl.Width / (float)glControl.Height;
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1f, 10000);
            modelview = Matrix4.LookAt(new Vector3(4, 1, -6), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            mvp = modelview * projection;
            GL.UseProgram(_program);

            int loc_mvp = GL.GetUniformLocation(_program, "mvp");
            GL.UniformMatrix4(loc_mvp, false, ref mvp);

            GlControl_Paint(null, null);
        }

        int _program;

        int VBO, IBO, VAO;

        float[] points = {
            -1.0f,  1.0f,  1.0f,  0.0f,
            1.0f, 1.0f,  1.0f, 0,2f,
            1.0f, -1.0f,  1.0f, 0.3f,
            -1.0f, -1.0f, 1.0f, 1.0f
        };

        uint[] indices = {
            //
            0, 1, 2,
            0, 2, 3};

        private void GlControl_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Before  " + GL.GetError().ToString());

            //
            VAO = GL.GenVertexArray();

            System.Diagnostics.Debug.WriteLine("Gen VAO  " + GL.GetError().ToString());

            GL.BindVertexArray(VAO);

            System.Diagnostics.Debug.WriteLine("VAO  " + GL.GetError().ToString());

            //
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, points.Length * sizeof(float), points, BufferUsageHint.StaticDraw);

            System.Diagnostics.Debug.WriteLine("VBO  " + GL.GetError().ToString());

            //
            IBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            System.Diagnostics.Debug.WriteLine("IBO  " + GL.GetError().ToString());

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindVertexArray(0);

            GL.EnableVertexAttribArray(1);
            System.Diagnostics.Debug.WriteLine("Vertex  1 Enable " + GL.GetError().ToString());

            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 4 * sizeof(float), 3*sizeof(float));

            System.Diagnostics.Debug.WriteLine("Vertex  1 Attrib  " + GL.GetError().ToString());

1            _program = CompileShaders();

            System.Diagnostics.Debug.WriteLine("After Compile   " + GL.GetError().ToString());

            Text = "OpenGL Version " + GL.GetString(StringName.Version) + " " + _program;

            GlControl_Resize(null, null);

            // Generate 1D Color Texture
            int texture;

            Bitmap bmp = new Bitmap(256, 1);
            for (int istep = 0; istep < 256; ++istep)
                bmp.SetPixel(istep, 0, Color.FromArgb(255, 255 - istep, 0, istep));

            pictureBox1.BackgroundImage = bmp;

            System.Diagnostics.Debug.WriteLine("Enable Cap  " + GL.GetError().ToString());

            texture = GL.GenTexture();

            System.Diagnostics.Debug.WriteLine("Gen " + GL.GetError().ToString());


            System.Diagnostics.Debug.WriteLine("Bind " + GL.GetError().ToString());

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            System.Diagnostics.Debug.WriteLine("Store " + GL.GetError().ToString());

            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage1D(TextureTarget.Texture1D, 0, PixelInternalFormat.Rgba, 256, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            System.Diagnostics.Debug.WriteLine("Tex " + GL.GetError().ToString());

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, 256, 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, 256, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            System.Diagnostics.Debug.WriteLine("Sub" + GL.GetError().ToString());

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture1D, texture);

        }

        private void plOpenGL_MouseClick(object sender, MouseEventArgs e)
        {
            GlControl_Paint(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(IBO);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_program);
            GL.BindVertexArray(VAO);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            glControl.SwapBuffers();
        }
    }
}
