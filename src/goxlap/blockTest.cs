using System;
using Godot;

namespace FPSGame.src.Common.goxlap
{
    class blockTest : MeshInstance
    {

        float length = 0.25f;
        float height = 0.25f;
        float width = 0.25f;
        public override void _Ready(){
            var surfTool = new SurfaceTool();
            surfTool.Begin(Mesh.PrimitiveType.Triangles);
            Vector3[] vertices = new Vector3[]{new Vector3(0,0,0),
                                                new Vector3(length,0,0),
                                                new Vector3(length,0,width),
                                                new Vector3(0,0,width),

                                                new Vector3(0,height,0),
                                                new Vector3(length,height,0),
                                                new Vector3(length,height,width),
                                                new Vector3(0,height,width)};

            // surfTool.AddNormal(new Vector3(0.0f,0.0f,-1.0f));
            // surfTool.AddVertex(new Vector3(length, -height, -width));
            // surfTool.AddVertex(new Vector3(-length, -height, -width));
            // surfTool.AddVertex(new Vector3(-length, height, -width));
            // surfTool.AddVertex(new Vector3(length, height, -width));

            // // surfTool.AddNormal(new Vector3(0.0f, 0.0f, 1.0f));
            // surfTool.AddVertex(new Vector3(-length, -height, width));
            // surfTool.AddVertex(new Vector3(length, -height, width));
            // surfTool.AddVertex(new Vector3(length, height, width));
            // surfTool.AddVertex(new Vector3(-length, height, width));

            // // surfTool.AddNormal(new Vector3(1.0f, 0.0f, 0.0f));
            // surfTool.AddVertex(new Vector3(length, -height, width));
            // surfTool.AddVertex(new Vector3(length, -height, -width));
            // surfTool.AddVertex(new Vector3(length, height, -width));
            // surfTool.AddVertex(new Vector3(length, height, width));

            // surfTool.AddNormal(new Vector3(-1.0f, 0.0f, 0.0f));
            // surfTool.AddVertex(new Vector3(-length, -height, -width));
            // surfTool.AddVertex(new Vector3(-length, -height, width));
            // surfTool.AddVertex(new Vector3(-length, height, width));
            // surfTool.AddVertex(new Vector3(-length, height, -width));

            //Bottom Face
            surfTool.AddNormal(new Vector3(0.0f, -1.0f, 0.0f));
            surfTool.AddVertex(vertices[1]);
            surfTool.AddVertex(vertices[3]);
            surfTool.AddVertex(vertices[2]);

            surfTool.AddVertex(vertices[1]);
            surfTool.AddVertex(vertices[0]);
            surfTool.AddVertex(vertices[3]);
            
            surfTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));
            surfTool.AddVertex(vertices[4]);
            surfTool.AddVertex(vertices[5]);
            surfTool.AddVertex(vertices[7]);

            surfTool.AddVertex(vertices[5]);
            surfTool.AddVertex(vertices[6]);
            surfTool.AddVertex(vertices[7]);

            surfTool.AddNormal(new Vector3(0.0f, 0.0f, 1.0f));
            surfTool.AddVertex(vertices[0]);
            surfTool.AddVertex(vertices[1]);
            surfTool.AddVertex(vertices[5]);

            surfTool.AddVertex(vertices[5]);
            surfTool.AddVertex(vertices[4]);
            surfTool.AddVertex(vertices[0]);

            surfTool.AddNormal(new Vector3(0.0f, 0.0f, -1.0f));
            surfTool.AddVertex(vertices[3]);
            surfTool.AddVertex(vertices[6]);
            surfTool.AddVertex(vertices[2]);

            surfTool.AddVertex(vertices[3]);
            surfTool.AddVertex(vertices[7]);
            surfTool.AddVertex(vertices[6]);


            surfTool.AddNormal(new Vector3(-1.0f,0.0f,0.0f));
            surfTool.AddVertex(vertices[0]);
            surfTool.AddVertex(vertices[7]);
            surfTool.AddVertex(vertices[3]);

            surfTool.AddVertex(vertices[0]);
            surfTool.AddVertex(vertices[4]);
            surfTool.AddVertex(vertices[7]);

            surfTool.AddNormal(new Vector3(1.0f,0.0f,0.0f));
            surfTool.AddVertex(vertices[2]);
            surfTool.AddVertex(vertices[5]);
            surfTool.AddVertex(vertices[1]);

            surfTool.AddVertex(vertices[2]);
            surfTool.AddVertex(vertices[6]);
            surfTool.AddVertex(vertices[5]);


            // // surfTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));
            // surfTool.AddVertex(new Vector3(length, height, -width));
            // surfTool.AddVertex(new Vector3(-length, height, -width));
            // surfTool.AddVertex(new Vector3(-length, height, width));
            // surfTool.AddVertex(new Vector3(length, height, width));

            surfTool.Index();
            // surfTool.GenerateNormals();
            
            var mesh = surfTool.Commit();
            this.SetMesh(mesh);
        }


    }

}