using OpenTK;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace AsteroidFieldSlow
{
    public class Mesh
    {
        const int MAX_BONE_INFLUENCE = 4;
        public struct Vertex
        {
            // position
            public Vector3 Position;
            // normal
            public Vector3 Normal;
            // texCoords
            public Vector2 TexCoords;
            // tangent
            public Vector3 Tangent;
            // bitangent
            public Vector3 Bitangent;
            //bone indexes which will influence this vertex
            //int m_BoneIDs[MAX_BONE_INFLUENCE];
            //int[] m_BoneIDs;
            //weights from each bone
            //  float[] m_Weights;
        };

        // constructor
        public Mesh(Vertex[] vertices, int[] indices, Texture[] textures)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.textures = textures;

            // now that we have all the required data, set the vertex buffers and its attribute pointers.
            setupMesh();
        }

        // mesh Data
        Vertex[] vertices;
        int[] indices;
        Texture[] textures;
        int VAO;
        public void Draw(Shader shader)
        {
            // bind appropriate textures
            int diffuseNr = 1;
            int specularNr = 1;
            int normalNr = 1;
            int heightNr = 1;
            for (int i = 0; i < textures.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i); // active proper texture unit before binding
                                                            // retrieve texture number (the N in diffuse_textureN)
                string number = "";
                string name = textures[i].type;
                if (name == "texture_diffuse")
                    number = (diffuseNr++).ToString();
                else if (name == "texture_specular")
                    number = (specularNr++).ToString(); // transfer unsigned int to string
                else if (name == "texture_normal")
                    number = (normalNr++).ToString(); // transfer unsigned int to string
                else if (name == "texture_height")
                    number = (heightNr++).ToString(); // transfer unsigned int to string

                // now set the sampler to the correct texture unit
                GL.Uniform1(GL.GetUniformLocation(shader.ID, (name + number).ToString()), i);
                // and finally bind the texture
                GL.BindTexture(TextureTarget.Texture2D, textures[i].id);
            }

            // draw mesh
            GL.BindVertexArray(VAO);
            GL.DrawElements(BeginMode.Triangles, (indices.Length), DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            // always good practice to set everything back to defaults once configured.
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        // render data 
        int VBO, EBO;

        // initializes all the buffer objects/arrays
        void setupMesh()
        {
            // create buffers/arrays
            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);
            GL.GenBuffers(1, out EBO);

            GL.BindVertexArray(VAO);
            // load data into vertex buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            // A great thing about structs is that their memory layout is sequential for all its items.
            // The effect is that we can simply pass a pointer to the struct and it translates perfectly to a glm::vec3/2 array which
            // again translates to 3/2 floats which translates to a byte array.
            int vertexStructSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vertex));

            //var verticesArray = vertices.ToArray();
            //var indicesArray = indices.ToArray();

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count() * vertexStructSize, vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            // set the vertex attribute pointers
            // vertex Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexStructSize, 0);
            // vertex normals
            GL.EnableVertexAttribArray(1);

            var offsetNormal = System.Runtime.InteropServices.Marshal.OffsetOf(typeof(Vertex), "Normal");

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexStructSize, offsetNormal);
            // vertex texture coords
            GL.EnableVertexAttribArray(2);
            var offsetTexCoords = System.Runtime.InteropServices.Marshal.OffsetOf(typeof(Vertex), "TexCoords");

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertexStructSize, offsetTexCoords);
            // vertex tangent
            GL.EnableVertexAttribArray(3);
            var offsetTangent = System.Runtime.InteropServices.Marshal.OffsetOf(typeof(Vertex), "Tangent");

            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, vertexStructSize, offsetTangent);
            // vertex bitangent
            GL.EnableVertexAttribArray(4);
            var offsetBitangent = System.Runtime.InteropServices.Marshal.OffsetOf(typeof(Vertex), "Bitangent");

            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, vertexStructSize, offsetBitangent);
            // ids
            //GL.EnableVertexAttribArray(5);
            // GL.VertexAttribIPointer(5, 4, VertexAttribPointerType.Int, vertexStructSize, (void*)offsetof(Vertex, m_BoneIDs));

            // weights
            // GL.EnableVertexAttribArray(6);
            //var offset1 = System.Runtime.InteropServices.Marshal.OffsetOf(typeof(Vertex), "m_Weights");
            // GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, vertexStructSize, offset1);
            //GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (6 * sizeof(float)));

            GL.BindVertexArray(0);
        }
    }
}

