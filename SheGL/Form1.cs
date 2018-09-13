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
            "layout (location=1) in float U;\n" +
            "layout (location=2) in vec3 bary_pos;\n"+
            "out float Ux;\n"+
            "out vec3 Bx;\n"+
            "uniform mat4 mvp;\n"+
            "void main(void)\n" +
            "{\n" +
            "  gl_Position = mvp * vec4(position, 1.0f);\n" +
            "  Ux = U;\n"+
            "  Bx = bary_pos;\n"+
            "}\n";

        string FragmentShader =
            "#version 330 core\n" +
            "in float Ux;\n" +
            "in vec3 Bx;\n"+
            "out vec4 color;\n" +
            "uniform sampler1D tSampler; \n" +
            "\n" +
            "float egde(){ \n"+
            "vec3 d = fwidth(Bx);\n"+
            "vec3 a3 = smoothstep(vec3(0.0), d * 1.5, Bx);\n"+
            "return min(min(a3.x, a3.y), a3.z);\n"+
            "}\n"+
            "void main(void)\n" +
            "{\n" +
            " color.rgb = mix(vec3(0.0), vec3(0.5), edge());\n"+
            " color.a = 1.0;\n"+
            //"   color = texture(tSampler, Ux).rgba;\n" +
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
            //GL.AttachShader(program, geometryShader);
            GL.LinkProgram(program);

            info = GL.GetProgramInfoLog(program);
            System.Diagnostics.Debug.WriteLine("Program Shader : " + info);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            //GL.DetachShader(program, geometryShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            //GL.DeleteShader(geometryShader);

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
            modelview = Matrix4.LookAt(new Vector3(+4, +4, -6), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            mvp = modelview * projection;
            GL.UseProgram(_program);

            int loc_mvp = GL.GetUniformLocation(_program, "mvp");
            GL.UniformMatrix4(loc_mvp, false, ref mvp);

            GlControl_Paint(null, null);
        }

        int _program;

        int VBO, IBO, VAO;
        int texture;


        float[] points = {
            // front
            -1.0f, -1.0f, 1.0f, 1f, 1, 0, 0,
            1.0f, -1.0f,  1.0f, 1f, 0, 1, 0,
            1.0f,  1.0f,  1.0f, 1f, 0, 0, 1,
            -1.0f,  1.0f,  1.0f, 1f, 0, 1, 0,
            // top
            -1.0f,  1.0f,  1.0f, 0f,1, 0, 0,
            1.0f,  1.0f,  1.0f, 0f,0, 1, 0,
            1.0f,  1.0f, -1.0f, 0f, 0, 0, 1,
            -1.0f,  1.0f, -1.0f, 0f,0, 1, 0,
            // back
            1.0f, -1.0f, -1.0f, 0.6f,1, 0, 0,
            -1.0f, -1.0f, -1.0f, 0.7f, 0, 1, 0,
            -1.0f,  1.0f, -1.0f, 0.72f, 0, 0, 1,
            1.0f,  1.0f, -1.0f, 0.76f, 0, 1, 0,
            // bottom
            -1.0f, -1.0f, -1.0f, 0.4f,1, 0, 0,
            1.0f, -1.0f, -1.0f, 0.2f, 0, 1, 0,
            1.0f, -1.0f,  1.0f, 0.5f, 0, 0, 1,
            -1.0f, -1.0f,  1.0f, 0.6f, 0, 1, 0,
            // left
            -1.0f, -1.0f, -1.0f, 0.2f,1, 0, 0,
            -1.0f, -1.0f,  1.0f, 0.1f, 0, 1, 0,
            -1.0f,  1.0f,  1.0f, 0.02f,  0, 0, 1,
            -1.0f,  1.0f, -1.0f, 0.14f, 0, 1, 0,
            // right
            1.0f, -1.0f,  1.0f, 0.8f,1, 0, 0,
            1.0f, -1.0f, -1.0f, 0.9f, 0, 1, 0,
            1.0f,  1.0f, -1.0f, 0.82f, 0, 0, 1,
            1.0f,  1.0f,  1.0f, 0.2f, 0, 1, 0
        };

        uint[] indices = {
            //
            0, 1, 2,
            2, 3, 0,
            4, 5, 6,
            6, 7, 4,
            8, 9, 10,
            10, 11, 8,
            12, 13, 14,
            14, 15, 12,
            16, 17, 18,
            18, 19, 16,
            20, 21, 22,
            22, 23, 20
        };

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.FrontAndBack);

            Text = "OpenGL Version " + GL.GetString(StringName.Version);

            System.Diagnostics.Debug.WriteLine("Remove " + GL.GetError().ToString());

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            //
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, points.Length * sizeof(float), points, BufferUsageHint.StaticDraw);
            //
            IBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3*sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 4 * sizeof(float));

            _program = CompileShaders();

            Bitmap bmp = new Bitmap(256, 1);
            for (int istep = 0; istep < 256; ++istep)
                bmp.SetPixel(istep, 0, Color.FromArgb(255, 255 - istep, 0, istep));

            pictureBox1.BackgroundImage = bmp;

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture1D, texture);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage1D(TextureTarget.Texture1D, 0, PixelInternalFormat.Rgba, 255, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            System.Diagnostics.Debug.WriteLine("Tex " + GL.GetError().ToString());

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, 255, 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, 255, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            System.Diagnostics.Debug.WriteLine("Sub" + GL.GetError().ToString());

            GL.ActiveTexture(TextureUnit.Texture0);

            GlControl_Resize(null, null);

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
            GL.DeleteTexture(texture);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_program);
            GL.BindVertexArray(VAO);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawElements(BeginMode.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            glControl.SwapBuffers();
        }
    }
}
