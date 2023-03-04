using OpenTK;
using System;
using OpenTK.Graphics.OpenGL;
using System.Reflection;

namespace stencil
{
    public class Shader
    {
        public Shader()
        {

        }

        public Shader(string vs, string fs)
        {
            Init(vs, fs, null);
        }

        public int shaderProgram;

        public void SetFloat(string nm, float v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.Uniform1(loc, v);
        }
        public void SetMat4(string nm, Matrix4 v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.UniformMatrix4(loc, false, ref v);
        }

        public void SetVec3(string nm, Vector3 v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.Uniform3(loc, v);
        }
        public void SetInt(string nm, int v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.Uniform1(loc, v);
        }

        //public void Init(string nm1 = "vertexshader1", string nm2 = "fragmentshader1")
        protected void Init(string vs, string fs, Assembly _asm)
        {
            // build and compile our shader program
            // ------------------------------------
            // vertex shader

            var asm = Assembly.GetAssembly(typeof(Shader));
            if (_asm != null)
            {
                asm = _asm;
            }
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            var vertexShaderSource = ResourceFile.GetFileText(vs, asm);
            var fragmentShaderSource = ResourceFile.GetFileText(fs, asm);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            // check for shader compile errors
            int success;
            string infoLog;

            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(vertexShader, out infoLog);
                throw new Exception(infoLog);
                //std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
            }
            // fragment shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            // check for shader compile errors
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(fragmentShader, out infoLog);
                throw new Exception(infoLog);
                // glGetShaderInfoLog(fragmentShader, 512, NULL, infoLog);
                //std::cout << "ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" << infoLog << std::endl;
            }
            // link shaders
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            // check for linking errors
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                GL.GetProgramInfoLog(shaderProgram, out infoLog);
                throw new Exception(infoLog);
            }
            //glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
            //if (!success)
            {
                // glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
                //std::cout << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << infoLog << std::endl;
            }
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);


        }

        
        public void use()
        {
            GL.UseProgram(shaderProgram);
        }

        public virtual void SetUniforms()
        {

        }
    }
}
