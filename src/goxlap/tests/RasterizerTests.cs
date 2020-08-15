using System;
using HWVec = System.Numerics;
using Goxlap.src.Goxlap.rasterizer;

namespace Goxlap.src.Goxlap.tests{
    using global::Goxlap.src.Goxlap.utils;
    /// <summary>
    /// Class used for benchmarking and unit testing Rasterizer functionality
    /// </summary>
    class RasterizerTests{
        public RasterizerPipeline rasterPipeline;
        public PND3d pND;

        public RasterizerTests(RasterizerPipeline pipeline, PND3d pnd){
            this.pND = pnd;
            this.rasterPipeline = pipeline;
        }
        public void TestDriver(){
            //Setup our test matrices
            HWVec.Matrix4x4 viewMatrix = new HWVec.Matrix4x4( 1.0f,  -0.0f,  0.0f,  -0.0f, 
                                                                0.0f,  1.0f,  0.0f,  -0.0f, 
                                                                -0.0f,  -0.0f,  1.0f,  -10.0f, 
                                                                0.0f,  0.0f,  0.0f,  1.0f );
            HWVec.Matrix4x4 projMatrix = new HWVec.Matrix4x4( 1.8106601f,  0.0f,  0.0f,  0.0f, 
                                                                0.0f,  2.4142134f,  0.0f,  0.0f, 
                                                                0.0f,  0.0f,  -1.002002f,  -2.002002f, 
                                                                0.0f,  0.0f,  -1.0f,  -0.0f );
            HWVec.Matrix4x4 result = new HWVec.Matrix4x4( 1.8106601f,  0.0f,  0.0f,  0.0f, 
                                                            0.0f,  2.4142134f,  0.0f,  0.0f, 
                                                            0.0f,  0.0f,  -1.002002f,  8.018018f, 
                                                            0.0f,  0.0f,  -1.0f,  10.0f);
            //Run our first test:
            _testMatrixMultiply(projMatrix,viewMatrix,result);

            //Setup the result matrix for our multiplication
            HWVec.Vector4 multRes = new HWVec.Vector4( 5.0f, 3.0f, -3.0f, 1.0f);
            HWVec.Vector4 vec4 = new HWVec.Vector4(5,3,7,1);
            _testMatrixVectorMult(viewMatrix,multRes,vec4);

            //Test BBox
            rasterPipeline.UpdateViewParameters(80f);
            rasterPipeline.SetupRasterizerProperties();
            utils.AABB aabb = new AABB(new Godot.Vector3(0,3,0), new Godot.Vector3(1,1,1));
            _testAABBValidity(aabb);
        }
        private void _testMatrixMultiply(HWVec.Matrix4x4 mat1, HWVec.Matrix4x4 mat2, HWVec.Matrix4x4 resComp){
            HWVec.Matrix4x4 res = mat1.multiplyColMaj(mat2);
            Console.WriteLine("------------------------------Test Matrix multiplication: ");
            Console.WriteLine($"Mat1: {mat1} \nMat2: {mat2}\nResult {res}");
            Console.WriteLine($"Result Compare {resComp}");
            Console.WriteLine($"Are results equal: {resComp == res} ?");
        }
        private void _testMatrixVectorMult(HWVec.Matrix4x4 mat1, HWVec.Vector4 resComp,HWVec.Vector4 vec4){
            Console.WriteLine("-------------------------------Test Matrix Vector4: ");
            HWVec.Vector4 result = GDExtension.multProj(mat1,vec4);
            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Are results equal: {resComp == result}");
            
        }
        private void _testAABBValidity(utils.AABB aabb){
            Console.WriteLine("-------------------------------Test AABB Validity");
            BoundingRect regularRect = pND.boundSquare(aabb);
            BoundingRect fastRect = rasterPipeline.CreateBoundingRect(aabb);
            Console.WriteLine($"Regular Bounding Rect: {regularRect}");
            Console.WriteLine($"Fast Bounding Rect: {fastRect}");
            Console.WriteLine($"Are the results equal: {fastRect == regularRect}");
            _testAABBPartialTrace(aabb,fastRect);
        }

        private void _testAABBPartialTrace(utils.AABB aabb, BoundingRect rect){
            Console.WriteLine("-------------------------------Test Partial Trace");
            Godot.BitMap map = rasterPipeline.CreateScreenToVoxelMapping(rect,aabb);
            Console.WriteLine($"Test Cam Matrix: {rasterPipeline.Cam.GetCameraTransform()}");
            Console.WriteLine($"Test Projection Matrix: {rasterPipeline.ProjMatrix}");
            var data = map.Data["data"];
            
        }

        

    }

}