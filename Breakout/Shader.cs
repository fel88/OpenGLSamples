﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Breakout
{
    public class Shader
    {
        public static string ReadResourceTxt(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public Shader(string vShaderFile, string fShaderFile)
        {
            // 2. compile shaders
            int vertex, fragment;
            // vertex shader
            string vShaderCode = ReadResourceTxt(vShaderFile);
            string fShaderCode = ReadResourceTxt(fShaderFile);
            vertex = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertex, vShaderCode);
            GL.CompileShader(vertex);
            checkCompileErrors(vertex, "VERTEX");
            // fragment Shader
            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fShaderCode);
            GL.CompileShader(fragment);
            checkCompileErrors(fragment, "FRAGMENT");
            // shader Program
            ID = GL.CreateProgram();
            GL.AttachShader(ID, vertex);
            GL.AttachShader(ID, fragment);
            GL.LinkProgram(ID);
            checkCompileErrors(ID, "PROGRAM");
            // delete the shaders as they're linked into our program now and no longer necessary
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }
        public readonly int ID;
        void checkCompileErrors(int shader, string type)
        {
            int success;
            string infoLog;
            if (type != "PROGRAM")
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    GL.GetShaderInfoLog(shader, out infoLog);
                    //std::cout << "ERROR::SHADER_COMPILATION_ERROR of type: " << type << "\n" << infoLog << "\n -- --------------------------------------------------- -- " << std::endl;
                }
            }
            else
            {
                GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out success);
                if (success == 0)
                {
                    GL.GetProgramInfoLog(shader, out infoLog);
                    //  std::cout << "ERROR::PROGRAM_LINKING_ERROR of type: " << type << "\n" << infoLog << "\n -- --------------------------------------------------- -- " << std::endl;
                }
            }
        }
        internal void setInt(string v1, int v2)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, v1), v2);
        }

        internal void setMat4(string v, Matrix4 projection)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(ID, v), false, ref projection);
        }

        internal void setVec3(string v, Vector3 newPos)
        {
            GL.Uniform3(GL.GetUniformLocation(ID, v), ref newPos);
        }
        internal void setVec4(string v, Vector4 newPos)
        {
            GL.Uniform4(GL.GetUniformLocation(ID, v), ref newPos);
        }
        internal void setVec2(string v, Vector2 newPos)
        {
            GL.Uniform2(GL.GetUniformLocation(ID, v), ref newPos);
        }

        internal Shader use()
        {
            GL.UseProgram(ID);
            return this;
        }

        internal void SetVector3f(string v, Vector3 color)
        {
            setVec3(v, color);
        }

        internal void SetMatrix4(string v, Matrix4 projection, bool useShader = false)
        {
            if (useShader)
                use();
            setMat4(v, projection);
        }

        internal void SetInteger(string v1, int v2, bool useShader = false)
        {
            if (useShader)
                use();
            setInt(v1, v2);
        }

        internal void SetVector2f(string v, Vector2 position)
        {
            setVec2(v, position);
        }

        internal void SetVector4f(string v, Vector4 color)
        {
            setVec4(v, color);
        }

        internal void SetInteger(string v, bool chaos, bool useShader = false)
        {
            SetInteger(v, chaos ? 1 : 0, useShader);
        }

        internal void SetFloat(string v, float time)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, v), time); 
        }
    }
}

